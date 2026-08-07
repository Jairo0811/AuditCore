using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Features.ActionPlans;
using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Services;

public sealed class ActionPlanService : IActionPlanService
{
    private readonly IAuditCoreDbContext _dbContext;

    public ActionPlanService(IAuditCoreDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyCollection<ActionPlanDto>> GetAllAsync(Guid? findingId = null, ActionPlanStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ActionPlans.AsNoTracking().AsQueryable();
        if (findingId.HasValue) query = query.Where(x => x.FindingId == findingId.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);

        return await query.OrderBy(x => x.DueDateUtc)
            .Select(x => MapProjection(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<ActionPlanDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.ActionPlans.AsNoTracking().Where(x => x.Id == id).Select(x => MapProjection(x)).SingleOrDefaultAsync(cancellationToken);

    public async Task<ActionPlanDto> CreateAsync(CreateActionPlanRequest request, CancellationToken cancellationToken = default)
    {
        var finding = await _dbContext.Findings.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.FindingId, cancellationToken)
            ?? throw new InvalidOperationException("El hallazgo indicado no existe.");
        var audit = await _dbContext.Audits.AsNoTracking().SingleAsync(x => x.Id == finding.AuditId, cancellationToken);
        await ValidateResponsibleAsync(request.ResponsibleUserId, audit.OrganizationId, cancellationToken);

        var entity = new ActionPlan(request.FindingId, request.Title, request.Description, request.ResponsibleUserId, request.DueDateUtc);
        _dbContext.ActionPlans.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return (await GetByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<ActionPlanDto?> UpdateAsync(Guid id, UpdateActionPlanRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ActionPlans.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return null;
        var organizationId = await _dbContext.Findings.Where(x => x.Id == entity.FindingId)
            .Select(x => x.Audit.OrganizationId).SingleAsync(cancellationToken);
        await ValidateResponsibleAsync(request.ResponsibleUserId, organizationId, cancellationToken);
        entity.Update(request.Title, request.Description, request.ResponsibleUserId, request.DueDateUtc);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public Task<bool> SetProgressAsync(Guid id, int progressPercent, CancellationToken cancellationToken = default) =>
        ChangeAsync(id, x => x.SetProgress(progressPercent), cancellationToken);

    public Task<bool> CompleteAsync(Guid id, string? notes, CancellationToken cancellationToken = default) =>
        ChangeAsync(id, x => x.Complete(notes), cancellationToken);

    public Task<bool> CancelAsync(Guid id, CancellationToken cancellationToken = default) =>
        ChangeAsync(id, x => x.Cancel(), cancellationToken);

    public async Task<int> MarkOverdueAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var entities = await _dbContext.ActionPlans.Where(x => x.DueDateUtc < now && x.Status != ActionPlanStatus.Completed && x.Status != ActionPlanStatus.Cancelled).ToListAsync(cancellationToken);
        foreach (var entity in entities) entity.MarkOverdue();
        if (entities.Count > 0) await _dbContext.SaveChangesAsync(cancellationToken);
        return entities.Count;
    }

    private async Task<bool> ChangeAsync(Guid id, Action<ActionPlan> action, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.ActionPlans.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return false;
        action(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidateResponsibleAsync(Guid userId, Guid organizationId, CancellationToken cancellationToken)
    {
        var valid = await _dbContext.Users.AsNoTracking().AnyAsync(x => x.Id == userId && x.OrganizationId == organizationId && x.IsActive && !x.IsLocked, cancellationToken);
        if (!valid) throw new InvalidOperationException("El responsable no es válido para la organización del hallazgo.");
    }

    private static ActionPlanDto MapProjection(ActionPlan x) =>
        new(x.Id, x.FindingId, x.Finding.Code, x.Title, x.Description, x.ResponsibleUserId,
            x.ResponsibleUser.FirstName + " " + x.ResponsibleUser.LastName,
            x.DueDateUtc, x.ProgressPercent, x.Status, x.CompletionNotes, x.CompletedAtUtc, x.IsActive);
}
