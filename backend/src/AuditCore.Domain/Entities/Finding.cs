using AuditCore.Domain.Common;

namespace AuditCore.Domain.Entities;

public sealed class Finding : BaseAuditableEntity
{
    private Finding()
    {
    }

    public Finding(
        Guid auditId,
        string code,
        string title,
        string condition,
        string criteria,
        string? cause,
        string? effect,
        string? recommendation,
        FindingSeverity severity,
        Guid? riskId = null,
        Guid? responsibleUserId = null,
        DateTime? dueDateUtc = null)
    {
        if (auditId == Guid.Empty)
        {
            throw new ArgumentException(
                "La auditoría es obligatoria.",
                nameof(auditId));
        }

        AuditId = auditId;

        SetCode(code);
        SetTitle(title);
        SetCondition(condition);
        SetCriteria(criteria);

        Cause = NormalizeOptionalText(cause);
        Effect = NormalizeOptionalText(effect);
        Recommendation = NormalizeOptionalText(recommendation);

        Severity = severity;
        RiskId = riskId;
        ResponsibleUserId = responsibleUserId;
        DueDateUtc = dueDateUtc;

        Status = FindingStatus.Open;
        IsActive = true;
    }

    public Guid AuditId { get; private set; }

    public Audit Audit { get; private set; } = null!;

    public Guid? RiskId { get; private set; }

    public Risk? Risk { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Condition { get; private set; } = string.Empty;

    public string Criteria { get; private set; } = string.Empty;

    public string? Cause { get; private set; }

    public string? Effect { get; private set; }

    public string? Recommendation { get; private set; }

    public FindingSeverity Severity { get; private set; }

    public Guid? ResponsibleUserId { get; private set; }

    public User? ResponsibleUser { get; private set; }

    public DateTime? DueDateUtc { get; private set; }

    public FindingStatus Status { get; private set; }

    public bool IsActive { get; private set; }

    public void Update(
        string code,
        string title,
        string condition,
        string criteria,
        string? cause,
        string? effect,
        string? recommendation,
        FindingSeverity severity,
        Guid? riskId,
        Guid? responsibleUserId,
        DateTime? dueDateUtc)
    {
        EnsureEditable();

        SetCode(code);
        SetTitle(title);
        SetCondition(condition);
        SetCriteria(criteria);

        Cause = NormalizeOptionalText(cause);
        Effect = NormalizeOptionalText(effect);
        Recommendation = NormalizeOptionalText(recommendation);

        Severity = severity;
        RiskId = riskId;
        ResponsibleUserId = responsibleUserId;
        DueDateUtc = dueDateUtc;
    }

    public void SendToReview()
    {
        if (Status != FindingStatus.Open)
        {
            throw new InvalidOperationException(
                "Solo un hallazgo abierto puede enviarse a revisión.");
        }

        Status = FindingStatus.InReview;
    }

    public void Accept()
    {
        if (Status != FindingStatus.InReview)
        {
            throw new InvalidOperationException(
                "Solo un hallazgo en revisión puede aceptarse.");
        }

        Status = FindingStatus.Accepted;
    }

    public void Resolve()
    {
        if (Status != FindingStatus.Accepted)
        {
            throw new InvalidOperationException(
                "Solo un hallazgo aceptado puede resolverse.");
        }

        Status = FindingStatus.Resolved;
    }

    public void Close()
    {
        if (Status != FindingStatus.Resolved)
        {
            throw new InvalidOperationException(
                "Solo un hallazgo resuelto puede cerrarse.");
        }

        Status = FindingStatus.Closed;
        IsActive = false;
    }

    private void EnsureEditable()
    {
        if (Status == FindingStatus.Closed)
        {
            throw new InvalidOperationException(
                "Un hallazgo cerrado no puede modificarse.");
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

    private void SetCondition(string condition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(condition);

        Condition = condition.Trim();
    }

    private void SetCriteria(string criteria)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(criteria);

        Criteria = criteria.Trim();
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
