using AuditCore.Domain.Common;

namespace AuditCore.Domain.Entities;

public sealed class Audit : BaseAuditableEntity
{
    private Audit()
    {
    }

    public Audit(
        Guid organizationId,
        string code,
        string title,
        string? objective = null,
        string? scope = null)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException(
                "La organización es obligatoria.",
                nameof(organizationId));
        }

        OrganizationId = organizationId;

        SetCode(code);
        SetTitle(title);

        Objective = NormalizeOptionalText(objective);
        Scope = NormalizeOptionalText(scope);

        Status = AuditStatus.Draft;
        IsActive = true;
    }

    public Guid OrganizationId { get; private set; }

    public Organization Organization { get; private set; } = null!;

    public string Code { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string? Objective { get; private set; }

    public string? Scope { get; private set; }

    public Guid? LeadAuditorUserId { get; private set; }

    public User? LeadAuditorUser { get; private set; }

    public DateTime? StartDateUtc { get; private set; }

    public DateTime? EndDateUtc { get; private set; }

    public AuditStatus Status { get; private set; }

    public bool IsActive { get; private set; }

    public void Update(
        string code,
        string title,
        string? objective,
        string? scope)
    {
        EnsureEditable();

        SetCode(code);
        SetTitle(title);

        Objective = NormalizeOptionalText(objective);
        Scope = NormalizeOptionalText(scope);
    }

    public void Plan(
        Guid leadAuditorUserId,
        DateTime startDateUtc,
        DateTime endDateUtc)
    {
        if (Status != AuditStatus.Draft)
        {
            throw new InvalidOperationException(
                "Solo una auditoría en borrador puede planificarse.");
        }

        if (leadAuditorUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "El auditor principal es obligatorio.",
                nameof(leadAuditorUserId));
        }

        ValidateDates(startDateUtc, endDateUtc);

        LeadAuditorUserId = leadAuditorUserId;
        StartDateUtc = startDateUtc;
        EndDateUtc = endDateUtc;
        Status = AuditStatus.Planned;
    }

    public void Start()
    {
        if (Status != AuditStatus.Planned)
        {
            throw new InvalidOperationException(
                "Solo una auditoría planificada puede iniciarse.");
        }

        Status = AuditStatus.InProgress;
    }

    public void Complete()
    {
        if (Status != AuditStatus.InProgress)
        {
            throw new InvalidOperationException(
                "Solo una auditoría en ejecución puede completarse.");
        }

        Status = AuditStatus.Completed;
    }

    public void Close()
    {
        if (Status != AuditStatus.Completed)
        {
            throw new InvalidOperationException(
                "Solo una auditoría completada puede cerrarse.");
        }

        Status = AuditStatus.Closed;
        IsActive = false;
    }

    public void Cancel()
    {
        if (Status is AuditStatus.Closed or AuditStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "La auditoría ya está cerrada o cancelada.");
        }

        Status = AuditStatus.Cancelled;
        IsActive = false;
    }

    private void EnsureEditable()
    {
        if (Status is not AuditStatus.Draft and
            not AuditStatus.Planned)
        {
            throw new InvalidOperationException(
                "La auditoría ya no puede modificarse en su estado actual.");
        }
    }

    private void SetCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        Code = code.Trim().ToUpperInvariant();
    }

    private void SetTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Title = title.Trim();
    }

    private static void ValidateDates(
        DateTime startDateUtc,
        DateTime endDateUtc)
    {
        if (endDateUtc < startDateUtc)
        {
            throw new ArgumentException(
                "La fecha final no puede ser anterior a la fecha inicial.");
        }
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
