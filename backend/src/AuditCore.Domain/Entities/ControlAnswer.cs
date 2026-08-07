using AuditCore.Domain.Common;

namespace AuditCore.Domain.Entities;

public sealed class ControlAnswer : BaseAuditableEntity
{
    private ControlAnswer() { }

    public ControlAnswer(Guid evaluationId, Guid questionId, int? score, string? notes = null)
    {
        if (evaluationId == Guid.Empty) throw new ArgumentException("La evaluación es obligatoria.", nameof(evaluationId));
        if (questionId == Guid.Empty) throw new ArgumentException("La pregunta es obligatoria.", nameof(questionId));
        EvaluationId = evaluationId;
        QuestionId = questionId;
        SetAnswer(score, notes);
    }

    public Guid EvaluationId { get; private set; }
    public ControlEvaluation Evaluation { get; private set; } = null!;
    public Guid QuestionId { get; private set; }
    public ControlQuestion Question { get; private set; } = null!;
    public int? Score { get; private set; }
    public string? Notes { get; private set; }

    public void Update(int? score, string? notes) => SetAnswer(score, notes);

    private void SetAnswer(int? score, string? notes)
    {
        if (score is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(score), "El puntaje debe estar entre 0 y 100.");
        Score = score;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
