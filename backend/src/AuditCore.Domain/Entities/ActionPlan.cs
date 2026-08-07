using AuditCore.Domain.Common;

namespace AuditCore.Domain.Entities;

public sealed class ActionPlan : BaseAuditableEntity
{
    private ActionPlan() { }

    public ActionPlan(
        Guid findingId,
        string title,
        string? description,
        Guid responsibleUserId,
        DateTime dueDateUtc)
    {
        if (findingId == Guid.Empty) throw new ArgumentException("El hallazgo es obligatorio.", nameof(findingId));
        if (responsibleUserId == Guid.Empty) throw new ArgumentException("El responsable es obligatorio.", nameof(responsibleUserId));

        FindingId = findingId;
        ResponsibleUserId = responsibleUserId;
        SetTitle(title);
        Description = Optional(description);
        DueDateUtc = dueDateUtc;
        Status = ActionPlanStatus.Pending;
        ProgressPercent = 0;
        IsActive = true;
    }

    public Guid FindingId { get; private set; }
    public Finding Finding { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid ResponsibleUserId { get; private set; }
    public User ResponsibleUser { get; private set; } = null!;
    public DateTime DueDateUtc { get; private set; }
    public int ProgressPercent { get; private set; }
    public ActionPlanStatus Status { get; private set; }
    public string? CompletionNotes { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    public void Update(string title, string? description, Guid responsibleUserId, DateTime dueDateUtc)
    {
        if (Status is ActionPlanStatus.Completed or ActionPlanStatus.Cancelled)
            throw new InvalidOperationException("El plan ya no puede modificarse.");
        if (responsibleUserId == Guid.Empty)
            throw new ArgumentException("El responsable es obligatorio.", nameof(responsibleUserId));

        SetTitle(title);
        Description = Optional(description);
        ResponsibleUserId = responsibleUserId;
        DueDateUtc = dueDateUtc;
    }

    public void SetProgress(int progressPercent)
    {
        if (progressPercent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(progressPercent), "El progreso debe estar entre 0 y 100.");
        if (Status is ActionPlanStatus.Completed or ActionPlanStatus.Cancelled)
            throw new InvalidOperationException("El plan ya no admite cambios de progreso.");

        ProgressPercent = progressPercent;
        Status = progressPercent == 0 ? ActionPlanStatus.Pending : ActionPlanStatus.InProgress;
    }

    public void Complete(string? notes = null)
    {
        if (Status == ActionPlanStatus.Cancelled)
            throw new InvalidOperationException("Un plan cancelado no puede completarse.");

        ProgressPercent = 100;
        Status = ActionPlanStatus.Completed;
        CompletionNotes = Optional(notes);
        CompletedAtUtc = DateTime.UtcNow;
        IsActive = false;
    }

    public void MarkOverdue()
    {
        if (Status is ActionPlanStatus.Completed or ActionPlanStatus.Cancelled)
            return;
        Status = ActionPlanStatus.Overdue;
    }

    public void Cancel()
    {
        if (Status == ActionPlanStatus.Completed)
            throw new InvalidOperationException("Un plan completado no puede cancelarse.");
        Status = ActionPlanStatus.Cancelled;
        IsActive = false;
    }

    private void SetTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Title = title.Trim();
    }

    private static string? Optional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
