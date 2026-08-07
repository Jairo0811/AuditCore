using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Features.Roles;
using AuditCore.Application.Features.Roles.Models;
using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Services;

public sealed class RoleService : IRoleService
{
    private readonly IAuditCoreDbContext _dbContext;

    public RoleService(
        IAuditCoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<RoleDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new RoleDto(
                x.Id,
                x.Name,
                x.Code,
                x.Description,
                x.IsActive,
                x.RolePermissions
                    .Select(rp => rp.Permission.Code)
                    .OrderBy(code => code)
                    .ToArray()))
            .ToListAsync(cancellationToken);
    }

    public async Task<RoleDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new RoleDto(
                x.Id,
                x.Name,
                x.Code,
                x.Description,
                x.IsActive,
                x.RolePermissions
                    .Select(rp => rp.Permission.Code)
                    .OrderBy(code => code)
                    .ToArray()))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<RoleDto> CreateAsync(
        CreateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = NormalizeCode(request.Code);

        var duplicate =
            await _dbContext.Roles.AnyAsync(
                x => x.Code == code,
                cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Ya existe un rol con el código '{code}'.");
        }

        var role = new Role(
            request.Name,
            code,
            request.Description);

        _dbContext.Roles.Add(role);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
                   role.Id,
                   cancellationToken)
               ?? throw new InvalidOperationException(
                   "No fue posible recuperar el rol creado.");
    }

    public async Task<RoleDto?> UpdateAsync(
        Guid id,
        UpdateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = await _dbContext.Roles
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (role is null)
        {
            return null;
        }

        var code = NormalizeCode(request.Code);

        var duplicate =
            await _dbContext.Roles.AnyAsync(
                x => x.Id != id &&
                     x.Code == code,
                cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Ya existe otro rol con el código '{code}'.");
        }

        role.Update(
            request.Name,
            code,
            request.Description,
            request.IsActive);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> SetPermissionsAsync(
        Guid id,
        SetRolePermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var roleExists =
            await _dbContext.Roles.AnyAsync(
                x => x.Id == id,
                cancellationToken);

        if (!roleExists)
        {
            return false;
        }

        var permissionIds = request.PermissionIds
            .Distinct()
            .ToArray();

        var existingPermissionIds =
            await _dbContext.Permissions
                .Where(x => permissionIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

        if (existingPermissionIds.Count != permissionIds.Length)
        {
            throw new InvalidOperationException(
                "Uno o más permisos indicados no existen.");
        }

        var currentPermissions =
            await _dbContext.RolePermissions
                .Where(x => x.RoleId == id)
                .ToListAsync(cancellationToken);

        _dbContext.RolePermissions.RemoveRange(
            currentPermissions);

        foreach (var permissionId in permissionIds)
        {
            _dbContext.RolePermissions.Add(
                new RolePermission(
                    id,
                    permissionId));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string NormalizeCode(
        string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return code.Trim().ToUpperInvariant();
    }
}
