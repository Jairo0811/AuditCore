using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Features.Frameworks;
using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Services;

public sealed class FrameworkService : IFrameworkService
{
    private readonly IAuditCoreDbContext _dbContext;
    private readonly TenantGuard _tenantGuard;

    public FrameworkService(IAuditCoreDbContext dbContext, TenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<IReadOnlyCollection<FrameworkDto>> GetFrameworksAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.ControlFrameworks.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new FrameworkDto(x.Id, x.Name, x.Code, x.Version, x.Description, x.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<FrameworkDto> CreateFrameworkAsync(CreateFrameworkRequest request, CancellationToken cancellationToken = default)
    {
        EnsureGlobalConfigurationAccess();
        var code = NormalizeCode(request.Code);
        var version = request.Version.Trim();
        if (await _dbContext.ControlFrameworks.AnyAsync(x => x.Code == code && x.Version == version, cancellationToken))
            throw new InvalidOperationException("Ya existe esta versión del marco de control.");
        var entity = new ControlFramework(request.Name, code, version, request.Description);
        _dbContext.ControlFrameworks.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new FrameworkDto(entity.Id, entity.Name, entity.Code, entity.Version, entity.Description, entity.IsActive);
    }

    public async Task<FrameworkDto?> UpdateFrameworkAsync(Guid id, UpdateFrameworkRequest request, CancellationToken cancellationToken = default)
    {
        EnsureGlobalConfigurationAccess();
        var entity = await _dbContext.ControlFrameworks.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return null;
        var code = NormalizeCode(request.Code);
        var version = request.Version.Trim();
        if (await _dbContext.ControlFrameworks.AnyAsync(x => x.Id != id && x.Code == code && x.Version == version, cancellationToken))
            throw new InvalidOperationException("Ya existe esta versión del marco de control.");
        entity.Update(request.Name, code, version, request.Description, request.IsActive);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new FrameworkDto(entity.Id, entity.Name, entity.Code, entity.Version, entity.Description, entity.IsActive);
    }

    public async Task<IReadOnlyCollection<ControlDto>> GetControlsAsync(Guid? frameworkId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ControlDefinitions.AsNoTracking().AsQueryable();
        if (frameworkId.HasValue) query = query.Where(x => x.FrameworkId == frameworkId.Value);
        return await query.OrderBy(x => x.Domain).ThenBy(x => x.Code)
            .Select(x => new ControlDto(x.Id, x.FrameworkId, x.Code, x.Title, x.Domain, x.Description, x.Weight, x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<ControlDto> CreateControlAsync(CreateControlRequest request, CancellationToken cancellationToken = default)
    {
        EnsureGlobalConfigurationAccess();
        if (!await _dbContext.ControlFrameworks.AnyAsync(x => x.Id == request.FrameworkId, cancellationToken))
            throw new InvalidOperationException("El marco de control no existe.");
        var code = NormalizeCode(request.Code);
        if (await _dbContext.ControlDefinitions.AnyAsync(x => x.FrameworkId == request.FrameworkId && x.Code == code, cancellationToken))
            throw new InvalidOperationException("El código de control ya existe en el marco.");
        var entity = new ControlDefinition(request.FrameworkId, code, request.Title, request.Domain, request.Weight, request.Description);
        _dbContext.ControlDefinitions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new ControlDto(entity.Id, entity.FrameworkId, entity.Code, entity.Title, entity.Domain, entity.Description, entity.Weight, entity.IsActive);
    }

    public async Task<ControlDto?> UpdateControlAsync(Guid id, UpdateControlRequest request, CancellationToken cancellationToken = default)
    {
        EnsureGlobalConfigurationAccess();
        var entity = await _dbContext.ControlDefinitions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return null;
        var code = NormalizeCode(request.Code);
        if (await _dbContext.ControlDefinitions.AnyAsync(x => x.Id != id && x.FrameworkId == entity.FrameworkId && x.Code == code, cancellationToken))
            throw new InvalidOperationException("El código de control ya existe en el marco.");
        entity.Update(code, request.Title, request.Domain, request.Weight, request.Description, request.IsActive);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new ControlDto(entity.Id, entity.FrameworkId, entity.Code, entity.Title, entity.Domain, entity.Description, entity.Weight, entity.IsActive);
    }

    public async Task<IReadOnlyCollection<EvaluationDto>> GetEvaluationsAsync(Guid auditId, CancellationToken cancellationToken = default)
    {
        var organizationId = await GetAuditOrganizationAsync(auditId, cancellationToken);
        _tenantGuard.EnsureOrganization(organizationId);
        return await _dbContext.ControlEvaluations.AsNoTracking().Where(x => x.AuditId == auditId).OrderBy(x => x.Control.Code)
            .Select(x => new EvaluationDto(x.Id, x.AuditId, x.ControlId, x.Control.Code, x.Score, x.Status, x.Notes, x.EvaluatedByUserId, x.EvaluatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<EvaluationDto> EvaluateAsync(Guid auditId, Guid controlId, EvaluateControlRequest request, CancellationToken cancellationToken = default)
    {
        var audit = await _dbContext.Audits.AsNoTracking().SingleOrDefaultAsync(x => x.Id == auditId, cancellationToken)
            ?? throw new InvalidOperationException("La auditoría no existe.");
        _tenantGuard.EnsureOrganization(audit.OrganizationId);
        if (!await _dbContext.ControlDefinitions.AnyAsync(x => x.Id == controlId && x.IsActive, cancellationToken))
            throw new InvalidOperationException("El control no existe o está inactivo.");
        if (!await _dbContext.Users.AnyAsync(x => x.Id == request.EvaluatedByUserId && x.OrganizationId == audit.OrganizationId && x.IsActive && !x.IsLocked, cancellationToken))
            throw new InvalidOperationException("El evaluador no es válido para esta organización.");

        var entity = await _dbContext.ControlEvaluations.SingleOrDefaultAsync(x => x.AuditId == auditId && x.ControlId == controlId, cancellationToken);
        if (entity is null)
        {
            entity = new ControlEvaluation(auditId, controlId);
            _dbContext.ControlEvaluations.Add(entity);
        }
        entity.Evaluate(request.Score, request.Status, request.Notes, request.EvaluatedByUserId);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var controlCode = await _dbContext.ControlDefinitions.Where(x => x.Id == controlId).Select(x => x.Code).SingleAsync(cancellationToken);
        return new EvaluationDto(entity.Id, entity.AuditId, entity.ControlId, controlCode, entity.Score, entity.Status, entity.Notes, entity.EvaluatedByUserId, entity.EvaluatedAtUtc);
    }

    private async Task<Guid> GetAuditOrganizationAsync(Guid auditId, CancellationToken cancellationToken) =>
        await _dbContext.Audits.Where(x => x.Id == auditId).Select(x => x.OrganizationId).SingleOrDefaultAsync(cancellationToken) is var id && id != Guid.Empty
            ? id
            : throw new InvalidOperationException("La auditoría no existe.");

    private void EnsureGlobalConfigurationAccess()
    {
        if (_tenantGuard.RestrictedOrganizationId.HasValue)
            throw new UnauthorizedAccessException("Solo un superadministrador puede modificar marcos globales.");
    }

    private static string NormalizeCode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim().ToUpperInvariant();
    }
}
