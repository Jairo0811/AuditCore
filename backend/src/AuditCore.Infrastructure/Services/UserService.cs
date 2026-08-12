using System.Linq.Expressions;
using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Features.Users;
using AuditCore.Application.Features.Users.Models;
using AuditCore.Domain.Entities;
using AuditCore.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Services;

public sealed class UserService : IUserService
{
    private static readonly Expression<Func<User, UserDto>> Projection = x => new UserDto(
        x.Id, x.OrganizationId, x.Organization.Name,
        x.BranchId, x.Branch != null ? x.Branch.Name : null,
        x.DepartmentId, x.Department != null ? x.Department.Name : null,
        x.FirstName, x.LastName,
        x.FirstName + " " + x.LastName, x.Email, x.IsActive, x.IsLocked, x.LastLoginAtUtc,
        x.UserRoles.Select(ur => ur.Role.Code).OrderBy(code => code).ToArray());

    private readonly IAuditCoreDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly TenantGuard _tenantGuard;

    public UserService(IAuditCoreDbContext dbContext, IPasswordHasher<User> passwordHasher, TenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _tenantGuard = tenantGuard;
    }

    public async Task<IReadOnlyCollection<UserDto>> GetAllAsync(Guid? organizationId = null, CancellationToken cancellationToken = default)
    {
        var restricted = _tenantGuard.RestrictedOrganizationId;
        if (restricted.HasValue)
        {
            if (organizationId.HasValue && organizationId.Value != restricted.Value)
                throw new UnauthorizedAccessException("No tiene acceso a otra organización.");
            organizationId = restricted.Value;
        }
        var query = _dbContext.Users.AsNoTracking().AsQueryable();
        if (organizationId.HasValue) query = query.Where(x => x.OrganizationId == organizationId.Value);
        return await query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName).Select(Projection).ToListAsync(cancellationToken);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users.AsNoTracking().Where(x => x.Id == id);
        var restricted = _tenantGuard.RestrictedOrganizationId;
        if (restricted.HasValue) query = query.Where(x => x.OrganizationId == restricted.Value);
        return await query.Select(Projection).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        _tenantGuard.EnsureOrganization(request.OrganizationId);
        if (!await _dbContext.Organizations.AnyAsync(x => x.Id == request.OrganizationId, cancellationToken))
            throw new InvalidOperationException("La organización indicada no existe.");

        await ValidateStructureAsync(request.OrganizationId, request.BranchId, request.DepartmentId, cancellationToken);

        var email = NormalizeEmail(request.Email);
        if (await _dbContext.Users.AnyAsync(x => x.OrganizationId == request.OrganizationId && x.Email == email, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con este correo en la organización.");
        await ValidateRoleAssignmentAsync(request.RoleIds ?? [], cancellationToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Password);
        var user = new User(request.OrganizationId, request.FirstName, request.LastName, email, "TEMPORARY_HASH", request.BranchId, request.DepartmentId);
        user.ChangePassword(_passwordHasher.HashPassword(user, request.Password));
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        if (request.RoleIds is { Count: > 0 }) await ReplaceRolesAsync(user.Id, request.RoleIds, cancellationToken);
        return await GetByIdAsync(user.Id, cancellationToken) ?? throw new InvalidOperationException("No fue posible recuperar el usuario creado.");
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null) return null;
        _tenantGuard.EnsureOrganization(user.OrganizationId);
        await ValidateStructureAsync(user.OrganizationId, request.BranchId, request.DepartmentId, cancellationToken);

        var email = NormalizeEmail(request.Email);
        if (await _dbContext.Users.AnyAsync(x => x.Id != id && x.OrganizationId == user.OrganizationId && x.Email == email, cancellationToken))
            throw new InvalidOperationException("Ya existe otro usuario con este correo en la organización.");
        user.UpdateProfile(request.FirstName, request.LastName, email);
        user.AssignStructure(request.BranchId, request.DepartmentId);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> ChangePasswordAsync(Guid id, ChangeUserPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await GetEditableUserAsync(id, cancellationToken);
        if (user is null) return false;
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Password);
        user.ChangePassword(_passwordHasher.HashPassword(user, request.Password));
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetRolesAsync(Guid id, SetUserRolesRequest request, CancellationToken cancellationToken = default)
    {
        var user = await GetEditableUserAsync(id, cancellationToken);
        if (user is null) return false;
        await ValidateRoleAssignmentAsync(request.RoleIds, cancellationToken);
        await ReplaceRolesAsync(id, request.RoleIds, cancellationToken);
        return true;
    }

    public async Task<bool> SetActiveAsync(Guid id, bool active, CancellationToken cancellationToken = default)
    {
        var user = await GetEditableUserAsync(id, cancellationToken);
        if (user is null) return false;
        if (active) user.Activate(); else user.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetLockedAsync(Guid id, bool locked, CancellationToken cancellationToken = default)
    {
        var user = await GetEditableUserAsync(id, cancellationToken);
        if (user is null) return false;
        if (locked) user.Lock(); else user.Unlock();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<User?> GetEditableUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is not null) _tenantGuard.EnsureOrganization(user.OrganizationId);
        return user;
    }

    private async Task ValidateStructureAsync(Guid organizationId, Guid? branchId, Guid? departmentId, CancellationToken cancellationToken)
    {
        if (branchId.HasValue)
        {
            var branch = await _dbContext.Branches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == branchId.Value, cancellationToken)
                ?? throw new InvalidOperationException("La sucursal indicada no existe.");
            if (branch.OrganizationId != organizationId || !branch.IsActive)
                throw new InvalidOperationException("La sucursal debe pertenecer a la organización y estar activa.");
        }

        if (!departmentId.HasValue) return;

        var department = await _dbContext.Departments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == departmentId.Value, cancellationToken)
            ?? throw new InvalidOperationException("El departamento indicado no existe.");
        if (department.OrganizationId != organizationId || !department.IsActive)
            throw new InvalidOperationException("El departamento debe pertenecer a la organización y estar activo.");
        if (branchId.HasValue && department.BranchId.HasValue && department.BranchId != branchId)
            throw new InvalidOperationException("El departamento seleccionado no pertenece a la sucursal indicada.");
    }

    private async Task ValidateRoleAssignmentAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken)
    {
        if (!_tenantGuard.RestrictedOrganizationId.HasValue || roleIds.Count == 0) return;
        var assignsSuperAdmin = await _dbContext.Roles.AnyAsync(x => roleIds.Contains(x.Id) && x.Code == DefaultRoles.SuperAdmin, cancellationToken);
        if (assignsSuperAdmin) throw new UnauthorizedAccessException("Solo un superadministrador puede asignar el rol SUPER_ADMIN.");
    }

    private async Task ReplaceRolesAsync(Guid userId, IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken)
    {
        var distinctRoleIds = roleIds.Distinct().ToArray();
        var existingRoleIds = await _dbContext.Roles.Where(x => distinctRoleIds.Contains(x.Id)).Select(x => x.Id).ToListAsync(cancellationToken);
        if (existingRoleIds.Count != distinctRoleIds.Length) throw new InvalidOperationException("Uno o más roles indicados no existen.");
        var currentRoles = await _dbContext.UserRoles.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
        _dbContext.UserRoles.RemoveRange(currentRoles);
        foreach (var roleId in distinctRoleIds) _dbContext.UserRoles.Add(new UserRole(userId, roleId));
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return email.Trim().ToLowerInvariant();
    }
}
