using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Features.Risks;
using AuditCore.Application.Features.Risks.Models;
using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Services;

public sealed class RiskService : IRiskService
{
    private readonly IAuditCoreDbContext _dbContext;
    private readonly TenantGuard _tenantGuard;

    public RiskService(IAuditCoreDbContext dbContext, TenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<IReadOnlyCollection<RiskDto>> GetAllAsync(Guid? auditId = null, RiskStatus? status = null, RiskLevel? level = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Risks.AsNoTracking().AsQueryable();
        var restricted = _tenantGuard.RestrictedOrganizationId;
        if (restricted.HasValue) query = query.Where(x => x.Audit.OrganizationId == restricted.Value);
        if (auditId.HasValue) query = query.Where(x => x.AuditId == auditId.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);

        var risks = await query.OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new RiskDto(
                x.Id, x.AuditId, x.Audit.Code, x.Code, x.Title, x.Description,
                x.Probability, x.Impact, x.Probability * x.Impact,
                CalculateLevel(x.Probability * x.Impact), x.Treatment, x.OwnerUserId,
                x.OwnerUser != null ? x.OwnerUser.FirstName + " " + x.OwnerUser.LastName : null,
                x.Status, x.IsActive))
            .ToListAsync(cancellationToken);
        return level.HasValue ? risks.Where(x => x.Level == level.Value).ToArray() : risks;
    }

    public async Task<RiskDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Risks.AsNoTracking().Where(x => x.Id == id);
        var restricted = _tenantGuard.RestrictedOrganizationId;
        if (restricted.HasValue) query = query.Where(x => x.Audit.OrganizationId == restricted.Value);
        return await query.Select(x => new RiskDto(
            x.Id, x.AuditId, x.Audit.Code, x.Code, x.Title, x.Description,
            x.Probability, x.Impact, x.Probability * x.Impact,
            CalculateLevel(x.Probability * x.Impact), x.Treatment, x.OwnerUserId,
            x.OwnerUser != null ? x.OwnerUser.FirstName + " " + x.OwnerUser.LastName : null,
            x.Status, x.IsActive)).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<RiskDto> CreateAsync(CreateRiskRequest request, CancellationToken cancellationToken = default)
    {
        var audit = await _dbContext.Audits.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.AuditId, cancellationToken)
            ?? throw new InvalidOperationException("La auditoría indicada no existe.");
        _tenantGuard.EnsureOrganization(audit.OrganizationId);
        await ValidateOwnerAsync(request.OwnerUserId, audit.OrganizationId, cancellationToken);
        var code = await new SequentialCodeGenerator(_dbContext).NextRiskCodeAsync(request.AuditId, cancellationToken);
        var risk = new Risk(request.AuditId, code, request.Title, request.Description, request.Probability, request.Impact, request.Treatment, request.OwnerUserId);
        _dbContext.Risks.Add(risk);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(risk.Id, cancellationToken) ?? throw new InvalidOperationException("No fue posible recuperar el riesgo creado.");
    }

    public async Task<RiskDto?> UpdateAsync(Guid id, UpdateRiskRequest request, CancellationToken cancellationToken = default)
    {
        var risk = await _dbContext.Risks.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (risk is null) return null;
        var organizationId = await _dbContext.Audits.Where(x => x.Id == risk.AuditId).Select(x => x.OrganizationId).SingleAsync(cancellationToken);
        _tenantGuard.EnsureOrganization(organizationId);
        await ValidateOwnerAsync(request.OwnerUserId, organizationId, cancellationToken);
        risk.Update(risk.Code, request.Title, request.Description, request.Probability, request.Impact, request.Treatment, request.OwnerUserId);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public Task<bool> StartTreatmentAsync(Guid id, CancellationToken cancellationToken = default) => ChangeStateAsync(id, x => x.StartTreatment(), cancellationToken);
    public Task<bool> AcceptAsync(Guid id, CancellationToken cancellationToken = default) => ChangeStateAsync(id, x => x.Accept(), cancellationToken);
    public Task<bool> MitigateAsync(Guid id, CancellationToken cancellationToken = default) => ChangeStateAsync(id, x => x.Mitigate(), cancellationToken);
    public Task<bool> CloseAsync(Guid id, CancellationToken cancellationToken = default) => ChangeStateAsync(id, x => x.Close(), cancellationToken);

    private async Task<bool> ChangeStateAsync(Guid id, Action<Risk> changeState, CancellationToken cancellationToken)
    {
        var risk = await _dbContext.Risks.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (risk is null) return false;
        var organizationId = await _dbContext.Audits.Where(x => x.Id == risk.AuditId).Select(x => x.OrganizationId).SingleAsync(cancellationToken);
        _tenantGuard.EnsureOrganization(organizationId);
        changeState(risk);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidateOwnerAsync(Guid? ownerUserId, Guid organizationId, CancellationToken cancellationToken)
    {
        if (!ownerUserId.HasValue) return;
        var owner = await _dbContext.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == ownerUserId.Value, cancellationToken)
            ?? throw new InvalidOperationException("El responsable del riesgo no existe.");
        if (owner.OrganizationId != organizationId || !owner.IsActive || owner.IsLocked)
            throw new InvalidOperationException("El responsable del riesgo debe pertenecer a la organización y estar activo/desbloqueado.");
    }

    private static RiskLevel CalculateLevel(int score) => score switch
    {
        <= 4 => RiskLevel.Low,
        <= 9 => RiskLevel.Medium,
        <= 16 => RiskLevel.High,
        _ => RiskLevel.Critical
    };
}
