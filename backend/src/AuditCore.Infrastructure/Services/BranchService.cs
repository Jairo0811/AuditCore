using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Features.Branches;
using AuditCore.Application.Features.Branches.Models;
using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Services;

public sealed class BranchService : IBranchService
{
    private readonly IAuditCoreDbContext _dbContext;
    private readonly TenantGuard _tenantGuard;

    public BranchService(IAuditCoreDbContext dbContext, TenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<IReadOnlyCollection<BranchDto>> GetAllAsync(Guid? organizationId = null, CancellationToken cancellationToken = default)
    {
        var restricted = _tenantGuard.RestrictedOrganizationId;
        if (restricted.HasValue)
        {
            if (organizationId.HasValue && organizationId.Value != restricted.Value)
                throw new UnauthorizedAccessException("No tiene acceso a otra organización.");
            organizationId = restricted.Value;
        }
        var query = _dbContext.Branches.AsNoTracking().AsQueryable();
        if (organizationId.HasValue) query = query.Where(x => x.OrganizationId == organizationId.Value);
        return await query.OrderBy(x => x.Name)
            .Select(x => new BranchDto(x.Id, x.OrganizationId, x.Organization.Name, x.Name, x.Code, x.Address, x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<BranchDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Branches.AsNoTracking().Where(x => x.Id == id);
        var restricted = _tenantGuard.RestrictedOrganizationId;
        if (restricted.HasValue) query = query.Where(x => x.OrganizationId == restricted.Value);
        return await query.Select(x => new BranchDto(x.Id, x.OrganizationId, x.Organization.Name, x.Name, x.Code, x.Address, x.IsActive))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<BranchDto> CreateAsync(CreateBranchRequest request, CancellationToken cancellationToken = default)
    {
        _tenantGuard.EnsureOrganization(request.OrganizationId);
        if (!await _dbContext.Organizations.AnyAsync(x => x.Id == request.OrganizationId, cancellationToken))
            throw new InvalidOperationException("La organización indicada no existe.");
        var code = NormalizeCode(request.Code);
        if (await _dbContext.Branches.AnyAsync(x => x.OrganizationId == request.OrganizationId && x.Code == code, cancellationToken))
            throw new InvalidOperationException($"Ya existe una sucursal con el código '{code}' en esta organización.");
        var branch = new Branch(request.OrganizationId, request.Name, code, request.Address);
        _dbContext.Branches.Add(branch);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(branch.Id, cancellationToken) ?? throw new InvalidOperationException("No fue posible recuperar la sucursal creada.");
    }

    public async Task<BranchDto?> UpdateAsync(Guid id, UpdateBranchRequest request, CancellationToken cancellationToken = default)
    {
        var branch = await _dbContext.Branches.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (branch is null) return null;
        _tenantGuard.EnsureOrganization(branch.OrganizationId);
        var code = NormalizeCode(request.Code);
        if (await _dbContext.Branches.AnyAsync(x => x.Id != id && x.OrganizationId == branch.OrganizationId && x.Code == code, cancellationToken))
            throw new InvalidOperationException($"Ya existe una sucursal con el código '{code}' en esta organización.");
        branch.Update(request.Name, code, request.Address, request.IsActive);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var branch = await _dbContext.Branches.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (branch is null) return false;
        _tenantGuard.EnsureOrganization(branch.OrganizationId);
        if (await _dbContext.Departments.AnyAsync(x => x.BranchId == id, cancellationToken))
            throw new InvalidOperationException("La sucursal no puede eliminarse porque posee departamentos relacionados.");
        _dbContext.Branches.Remove(branch);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string NormalizeCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return code.Trim().ToUpperInvariant();
    }
}
