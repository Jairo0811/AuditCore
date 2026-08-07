using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Features.Findings;
using AuditCore.Application.Features.Findings.Models;
using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Services;

public sealed class FindingService : IFindingService
{
    private readonly IAuditCoreDbContext _dbContext;
    private readonly TenantGuard _tenantGuard;

    public FindingService(IAuditCoreDbContext dbContext, TenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<IReadOnlyCollection<FindingDto>> GetAllAsync(Guid? auditId = null, FindingStatus? status = null, FindingSeverity? severity = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Findings.AsNoTracking().AsQueryable();
        var restricted = _tenantGuard.RestrictedOrganizationId;
        if (restricted.HasValue) query = query.Where(x => x.Audit.OrganizationId == restricted.Value);
        if (auditId.HasValue) query = query.Where(x => x.AuditId == auditId.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (severity.HasValue) query = query.Where(x => x.Severity == severity.Value);

        return await query.OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<FindingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Findings.AsNoTracking().Where(x => x.Id == id);
        var restricted = _tenantGuard.RestrictedOrganizationId;
        if (restricted.HasValue) query = query.Where(x => x.Audit.OrganizationId == restricted.Value);
        return await query.Select(x => Map(x)).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<FindingDto> CreateAsync(CreateFindingRequest request, CancellationToken cancellationToken = default)
    {
        var audit = await _dbContext.Audits.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.AuditId, cancellationToken)
            ?? throw new InvalidOperationException("La auditoría indicada no existe.");
        _tenantGuard.EnsureOrganization(audit.OrganizationId);
        await ValidateRiskAsync(request.RiskId, request.AuditId, cancellationToken);
        await ValidateResponsibleUserAsync(request.ResponsibleUserId, audit.OrganizationId, cancellationToken);
        var code = NormalizeCode(request.Code);
        if (await _dbContext.Findings.AnyAsync(x => x.AuditId == request.AuditId && x.Code == code, cancellationToken))
            throw new InvalidOperationException($"Ya existe un hallazgo con el código '{code}' en esta auditoría.");

        var finding = new Finding(request.AuditId, code, request.Title, request.Condition, request.Criteria,
            request.Cause, request.Effect, request.Recommendation, request.Severity,
            request.RiskId, request.ResponsibleUserId, request.DueDateUtc);
        _dbContext.Findings.Add(finding);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(finding.Id, cancellationToken) ?? throw new InvalidOperationException("No fue posible recuperar el hallazgo creado.");
    }

    public async Task<FindingDto?> UpdateAsync(Guid id, UpdateFindingRequest request, CancellationToken cancellationToken = default)
    {
        var finding = await _dbContext.Findings.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (finding is null) return null;
        var audit = await _dbContext.Audits.AsNoTracking().SingleAsync(x => x.Id == finding.AuditId, cancellationToken);
        _tenantGuard.EnsureOrganization(audit.OrganizationId);
        await ValidateRiskAsync(request.RiskId, finding.AuditId, cancellationToken);
        await ValidateResponsibleUserAsync(request.ResponsibleUserId, audit.OrganizationId, cancellationToken);
        var code = NormalizeCode(request.Code);
        if (await _dbContext.Findings.AnyAsync(x => x.Id != id && x.AuditId == finding.AuditId && x.Code == code, cancellationToken))
            throw new InvalidOperationException($"Ya existe otro hallazgo con el código '{code}' en esta auditoría.");

        finding.Update(code, request.Title, request.Condition, request.Criteria, request.Cause, request.Effect,
            request.Recommendation, request.Severity, request.RiskId, request.ResponsibleUserId, request.DueDateUtc);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public Task<bool> SendToReviewAsync(Guid id, CancellationToken cancellationToken = default) => ChangeStateAsync(id, x => x.SendToReview(), cancellationToken);
    public Task<bool> AcceptAsync(Guid id, CancellationToken cancellationToken = default) => ChangeStateAsync(id, x => x.Accept(), cancellationToken);
    public Task<bool> ResolveAsync(Guid id, CancellationToken cancellationToken = default) => ChangeStateAsync(id, x => x.Resolve(), cancellationToken);
    public Task<bool> CloseAsync(Guid id, CancellationToken cancellationToken = default) => ChangeStateAsync(id, x => x.Close(), cancellationToken);

    private async Task<bool> ChangeStateAsync(Guid id, Action<Finding> changeState, CancellationToken cancellationToken)
    {
        var finding = await _dbContext.Findings.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (finding is null) return false;
        var organizationId = await _dbContext.Audits.Where(x => x.Id == finding.AuditId).Select(x => x.OrganizationId).SingleAsync(cancellationToken);
        _tenantGuard.EnsureOrganization(organizationId);
        changeState(finding);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidateRiskAsync(Guid? riskId, Guid auditId, CancellationToken cancellationToken)
    {
        if (!riskId.HasValue) return;
        var risk = await _dbContext.Risks.AsNoTracking().SingleOrDefaultAsync(x => x.Id == riskId.Value, cancellationToken)
            ?? throw new InvalidOperationException("El riesgo indicado no existe.");
        if (risk.AuditId != auditId) throw new InvalidOperationException("El riesgo debe pertenecer a la misma auditoría del hallazgo.");
    }

    private async Task ValidateResponsibleUserAsync(Guid? userId, Guid organizationId, CancellationToken cancellationToken)
    {
        if (!userId.HasValue) return;
        var user = await _dbContext.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId.Value, cancellationToken)
            ?? throw new InvalidOperationException("El responsable indicado no existe.");
        if (user.OrganizationId != organizationId || !user.IsActive || user.IsLocked)
            throw new InvalidOperationException("El responsable debe pertenecer a la organización y estar activo/desbloqueado.");
    }

    private static FindingDto Map(Finding x) => new(
        x.Id, x.AuditId, x.Audit.Code, x.RiskId, x.Risk != null ? x.Risk.Code : null,
        x.Code, x.Title, x.Condition, x.Criteria, x.Cause, x.Effect, x.Recommendation,
        x.Severity, x.ResponsibleUserId,
        x.ResponsibleUser != null ? x.ResponsibleUser.FirstName + " " + x.ResponsibleUser.LastName : null,
        x.DueDateUtc, x.Status, x.IsActive);

    private static string NormalizeCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return code.Trim().ToUpperInvariant();
    }
}
