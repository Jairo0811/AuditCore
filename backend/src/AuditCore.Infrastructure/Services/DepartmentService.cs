using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Features.Departments;
using AuditCore.Application.Features.Departments.Models;
using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Services;

public sealed class DepartmentService : IDepartmentService
{
    private readonly IAuditCoreDbContext _dbContext;

    public DepartmentService(
        IAuditCoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<DepartmentDto>> GetAllAsync(
        Guid? organizationId = null,
        Guid? branchId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Departments
            .AsNoTracking()
            .AsQueryable();

        if (organizationId.HasValue)
        {
            query = query.Where(
                x => x.OrganizationId ==
                     organizationId.Value);
        }

        if (branchId.HasValue)
        {
            query = query.Where(
                x => x.BranchId ==
                     branchId.Value);
        }

        return await query
            .OrderBy(x => x.Name)
            .Select(x => new DepartmentDto(
                x.Id,
                x.OrganizationId,
                x.Organization.Name,
                x.BranchId,
                x.Branch != null
                    ? x.Branch.Name
                    : null,
                x.Name,
                x.Code,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<DepartmentDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new DepartmentDto(
                x.Id,
                x.OrganizationId,
                x.Organization.Name,
                x.BranchId,
                x.Branch != null
                    ? x.Branch.Name
                    : null,
                x.Name,
                x.Code,
                x.IsActive))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<DepartmentDto> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidateOrganizationAndBranchAsync(
            request.OrganizationId,
            request.BranchId,
            cancellationToken);

        var code = NormalizeCode(request.Code);

        var duplicate =
            await _dbContext.Departments
                .AnyAsync(
                    x => x.OrganizationId ==
                         request.OrganizationId &&
                         x.Code == code,
                    cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Ya existe un departamento con el código '{code}' en esta organización.");
        }

        var department =
            new Department(
                request.OrganizationId,
                request.Name,
                code,
                request.BranchId);

        _dbContext.Departments.Add(department);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await GetByIdAsync(
                   department.Id,
                   cancellationToken)
               ?? throw new InvalidOperationException(
                   "No fue posible recuperar el departamento creado.");
    }

    public async Task<DepartmentDto?> UpdateAsync(
        Guid id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var department =
            await _dbContext.Departments
                .SingleOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

        if (department is null)
        {
            return null;
        }

        await ValidateOrganizationAndBranchAsync(
            department.OrganizationId,
            request.BranchId,
            cancellationToken);

        var code = NormalizeCode(request.Code);

        var duplicate =
            await _dbContext.Departments
                .AnyAsync(
                    x => x.Id != id &&
                         x.OrganizationId ==
                         department.OrganizationId &&
                         x.Code == code,
                    cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Ya existe un departamento con el código '{code}' en esta organización.");
        }

        department.Update(
            request.Name,
            code,
            request.BranchId,
            request.IsActive);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await GetByIdAsync(
            id,
            cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var department =
            await _dbContext.Departments
                .SingleOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

        if (department is null)
        {
            return false;
        }

        _dbContext.Departments.Remove(
            department);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private async Task ValidateOrganizationAndBranchAsync(
        Guid organizationId,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        var organizationExists =
            await _dbContext.Organizations
                .AnyAsync(
                    x => x.Id == organizationId,
                    cancellationToken);

        if (!organizationExists)
        {
            throw new InvalidOperationException(
                "La organización indicada no existe.");
        }

        if (!branchId.HasValue)
        {
            return;
        }

        var branch =
            await _dbContext.Branches
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == branchId.Value,
                    cancellationToken);

        if (branch is null)
        {
            throw new InvalidOperationException(
                "La sucursal indicada no existe.");
        }

        if (branch.OrganizationId != organizationId)
        {
            throw new InvalidOperationException(
                "La sucursal no pertenece a la organización del departamento.");
        }
    }

    private static string NormalizeCode(
        string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return code.Trim().ToUpperInvariant();
    }
}
