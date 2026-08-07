using AuditCore.Domain.Common;

namespace AuditCore.Domain.Entities;

public sealed class ControlEvaluation : BaseAuditableEntity
{
    private ControlEvaluation() { }

    public ControlEvaluation(Guid auditId, Guid controlId)
    {
        if (auditId == Guid.Empty) throw new ArgumentException("La auditoría es obligatoria.", nameof(auditId));
        if (controlId == Guid.Empty) throw new ArgumentException("El control es obligatorio.", nameof(controlId));

        AuditId = auditId;
        ControlId = controlId;
        Status = ComplianceStatus.NotEvaluated;
        IsActive = true;
    }

    public Guid AuditId { get; private set; }
    public Audit Audit { get; private set; } = null!;
    public Guid ControlId { get; private set; }
    public ControlDefinition Control { get; private set; } = null!;
    public int? Score { get; private set; }
    public ComplianceStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public Guid? EvaluatedByUserId { get; private set; }
    public User? EvaluatedByUser { get; private set; }
    public DateTime? EvaluatedAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    public void Evaluate(int? score, ComplianceStatus status, string? notes, Guid evaluatedByUserId)
    {
        if (evaluatedByUserId == Guid.Empty) throw new ArgumentException("El evaluador es obligatorio.", nameof(evaluatedByUserId));
        if (status == ComplianceStatus.NotEvaluated) throw new ArgumentException("Debe indicar un estado de cumplimiento evaluado.", nameof(status));
        if (status == ComplianceStatus.NotApplicable)
        {
            score = null;
        }
        else if (score is null or < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(score), "El puntaje debe estar entre 0 y 100.");
        }

        Score = score;
        Status = status;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        EvaluatedByUserId = evaluatedByUserId;
        EvaluatedAtUtc = DateTime.UtcNow;
    }
}
