using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Features.Organizations;
using AuditCore.Application.Features.Organizations.Models;
using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Services;

public sealed class OrganizationService : IOrganizationService
{
    private readonly IAuditCoreDbContext _dbContext;

    public OrganizationService(
        IAuditCoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<OrganizationDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Organizations
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new OrganizationDto(
                x.Id,
                x.Name,
                x.Code,
                x.Description,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<OrganizationDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Organizations
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new OrganizationDto(
                x.Id,
                x.Name,
                x.Code,
                x.Description,
                x.IsActive))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<OrganizationDto> CreateAsync(
        CreateOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = NormalizeCode(request.Code);

        var exists = await _dbContext.Organizations
            .AnyAsync(
                x => x.Code == code,
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                $"Ya existe una organización con el código '{code}'.");
        }

        var organization =
            new Organization(
                request.Name,
                code,
                request.Description);

        _dbContext.Organizations.Add(organization);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Map(organization);
    }

    public async Task<OrganizationDto?> UpdateAsync(
        Guid id,
        UpdateOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var organization =
            await _dbContext.Organizations
                .SingleOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

        if (organization is null)
        {
            return null;
        }

        var code = NormalizeCode(request.Code);

        var duplicate =
            await _dbContext.Organizations
                .AnyAsync(
                    x => x.Id != id &&
                         x.Code == code,
                    cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Ya existe una organización con el código '{code}'.");
        }

        organization.Update(
            request.Name,
            code,
            request.Description,
            request.IsActive);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Map(organization);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var organization =
            await _dbContext.Organizations
                .SingleOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

        if (organization is null)
        {
            return false;
        }

        var hasBranches =
            await _dbContext.Branches
                .AnyAsync(
                    x => x.OrganizationId == id,
                    cancellationToken);

        var hasDepartments =
            await _dbContext.Departments
                .AnyAsync(
                    x => x.OrganizationId == id,
                    cancellationToken);

        var hasUsers =
            await _dbContext.Users
                .AnyAsync(
                    x => x.OrganizationId == id,
                    cancellationToken);

        if (hasBranches ||
            hasDepartments ||
            hasUsers)
        {
            throw new InvalidOperationException(
                "La organización no puede eliminarse porque posee registros relacionados.");
        }

        _dbContext.Organizations.Remove(
            organization);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private static string NormalizeCode(
        string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return code.Trim().ToUpperInvariant();
    }

    private static OrganizationDto Map(
        Organization organization)
    {
        return new OrganizationDto(
            organization.Id,
            organization.Name,
            organization.Code,
            organization.Description,
            organization.IsActive);
    }
}
