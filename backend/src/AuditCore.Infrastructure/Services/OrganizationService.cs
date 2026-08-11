using System.Globalization;
using System.Text;
using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Features.Organizations;
using AuditCore.Application.Features.Organizations.Models;
using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Services;

public sealed class OrganizationService : IOrganizationService
{
    private static readonly HashSet<string> IgnoredCodeWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "DE", "DEL", "LA", "LAS", "EL", "LOS", "Y", "E", "OF", "THE", "AND"
    };

    private readonly IAuditCoreDbContext _dbContext;
    private readonly TenantGuard _tenantGuard;

    public OrganizationService(IAuditCoreDbContext dbContext, TenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<IReadOnlyCollection<OrganizationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Organizations.AsNoTracking().AsQueryable();
        var restricted = _tenantGuard.RestrictedOrganizationId;
        if (restricted.HasValue) query = query.Where(x => x.Id == restricted.Value);
        return await query.OrderBy(x => x.Name)
            .Select(x => new OrganizationDto(x.Id, x.Name, x.Code, x.Description, x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _tenantGuard.EnsureOrganization(id);
        return await _dbContext.Organizations.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new OrganizationDto(x.Id, x.Name, x.Code, x.Description, x.IsActive))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<OrganizationDto> CreateAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        if (_tenantGuard.RestrictedOrganizationId.HasValue)
            throw new UnauthorizedAccessException("Solo un superadministrador puede crear organizaciones.");

        var code = await GenerateUniqueCodeAsync(request.Name, cancellationToken);
        var organization = new Organization(request.Name, code, request.Description);

        _dbContext.Organizations.Add(organization);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(organization);
    }

    public async Task<OrganizationDto?> UpdateAsync(Guid id, UpdateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        _tenantGuard.EnsureOrganization(id);
        var organization = await _dbContext.Organizations.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (organization is null) return null;

        organization.Update(request.Name, organization.Code, request.Description, request.IsActive);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(organization);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_tenantGuard.RestrictedOrganizationId.HasValue)
            throw new UnauthorizedAccessException("Solo un superadministrador puede eliminar organizaciones.");
        var organization = await _dbContext.Organizations.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (organization is null) return false;
        var related = await _dbContext.Branches.AnyAsync(x => x.OrganizationId == id, cancellationToken)
            || await _dbContext.Departments.AnyAsync(x => x.OrganizationId == id, cancellationToken)
            || await _dbContext.Users.AnyAsync(x => x.OrganizationId == id, cancellationToken)
            || await _dbContext.Audits.AnyAsync(x => x.OrganizationId == id, cancellationToken);
        if (related) throw new InvalidOperationException("La organización no puede eliminarse porque posee registros relacionados.");
        _dbContext.Organizations.Remove(organization);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<string> GenerateUniqueCodeAsync(string name, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var baseCode = BuildCodeFromName(name);
        var candidate = baseCode;
        var suffix = 2;

        while (await _dbContext.Organizations.AnyAsync(x => x.Code == candidate, cancellationToken))
        {
            candidate = $"{baseCode}-{suffix}";
            suffix++;
        }

        return candidate;
    }

    private static string BuildCodeFromName(string name)
    {
        var normalized = RemoveDiacritics(name).ToUpperInvariant();
        var words = normalized
            .Split([' ', '-', '_', '.', ',', '/', '\\', '(', ')'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(KeepLettersAndDigits)
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .ToArray();

        var meaningfulWords = words
            .Where(word => !IgnoredCodeWords.Contains(word))
            .ToArray();

        if (meaningfulWords.Length >= 2)
        {
            return string.Concat(meaningfulWords.Select(word => word[0]))[..Math.Min(10, meaningfulWords.Length)];
        }

        var source = meaningfulWords.FirstOrDefault() ?? words.FirstOrDefault() ?? "ORG";
        return source[..Math.Min(12, source.Length)];
    }

    private static string RemoveDiacritics(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(character);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string KeepLettersAndDigits(string value) =>
        new(value.Where(char.IsLetterOrDigit).ToArray());

    private static OrganizationDto Map(Organization organization) =>
        new(organization.Id, organization.Name, organization.Code, organization.Description, organization.IsActive);
}
