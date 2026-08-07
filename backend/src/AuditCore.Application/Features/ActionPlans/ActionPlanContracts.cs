using AuditCore.Domain.Entities;

namespace AuditCore.Application.Features.ActionPlans;

public sealed record ActionPlanDto(
    Guid Id,
    Guid FindingId,
    string FindingCode,
    string Title,
    string? Description,
    Guid ResponsibleUserId,
    string ResponsibleName,
    DateTime DueDateUtc,
    int ProgressPercent,
    ActionPlanStatus Status,
    string? CompletionNotes,
    DateTime? CompletedAtUtc,
    bool IsActive);

public sealed record CreateActionPlanRequest(
    Guid FindingId,
    string Title,
    string? Description,
    Guid ResponsibleUserId,
    DateTime DueDateUtc);

public sealed record UpdateActionPlanRequest(
    string Title,
    string? Description,
    Guid ResponsibleUserId,
    DateTime DueDateUtc);

public sealed record SetActionPlanProgressRequest(int ProgressPercent);
public sealed record CompleteActionPlanRequest(string? Notes);

public interface IActionPlanService
{
    Task<IReadOnlyCollection<ActionPlanDto>> GetAllAsync(Guid? findingId = null, ActionPlanStatus? status = null, CancellationToken cancellationToken = default);
    Task<ActionPlanDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ActionPlanDto> CreateAsync(CreateActionPlanRequest request, CancellationToken cancellationToken = default);
    Task<ActionPlanDto?> UpdateAsync(Guid id, UpdateActionPlanRequest request, CancellationToken cancellationToken = default);
    Task<bool> SetProgressAsync(Guid id, int progressPercent, CancellationToken cancellationToken = default);
    Task<bool> CompleteAsync(Guid id, string? notes, CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> MarkOverdueAsync(CancellationToken cancellationToken = default);
}
