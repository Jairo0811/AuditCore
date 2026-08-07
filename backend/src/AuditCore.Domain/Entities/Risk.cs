using AuditCore.Domain.Common;

namespace AuditCore.Domain.Entities;

public sealed class Risk : BaseAuditableEntity
{
    private Risk()
    {
    }

    public Risk(
        Guid auditId,
        string code,
        string title,
        string? description,
        int probability,
        int impact,
        string? treatment = null,
        Guid? ownerUserId = null)
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

        Description = NormalizeOptionalText(description);
        Treatment = NormalizeOptionalText(treatment);
        OwnerUserId = ownerUserId;

        SetAssessment(
            probability,
            impact);

        Status = RiskStatus.Identified;
        IsActive = true;
    }

    public Guid AuditId { get; private set; }

    public Audit Audit { get; private set; } = null!;

    public string Code { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int Probability { get; private set; }

    public int Impact { get; private set; }

    public int Score => Probability * Impact;

    public RiskLevel Level => CalculateLevel(Score);

    public string? Treatment { get; private set; }

    public Guid? OwnerUserId { get; private set; }

    public User? OwnerUser { get; private set; }

    public RiskStatus Status { get; private set; }

    public bool IsActive { get; private set; }

    public void Update(
        string code,
        string title,
        string? description,
        int probability,
        int impact,
        string? treatment,
        Guid? ownerUserId)
    {
        EnsureEditable();

        SetCode(code);
        SetTitle(title);

        Description = NormalizeOptionalText(description);
        Treatment = NormalizeOptionalText(treatment);
        OwnerUserId = ownerUserId;

        SetAssessment(
            probability,
            impact);
    }

    public void StartTreatment()
    {
        if (Status != RiskStatus.Identified)
        {
            throw new InvalidOperationException(
                "Solo un riesgo identificado puede iniciar tratamiento.");
        }

        Status = RiskStatus.UnderTreatment;
    }

    public void Accept()
    {
        if (Status == RiskStatus.Closed)
        {
            throw new InvalidOperationException(
                "Un riesgo cerrado no puede aceptarse.");
        }

        Status = RiskStatus.Accepted;
    }

    public void Mitigate()
    {
        if (Status is not RiskStatus.Identified and
            not RiskStatus.UnderTreatment)
        {
            throw new InvalidOperationException(
                "El riesgo no puede marcarse como mitigado en su estado actual.");
        }

        Status = RiskStatus.Mitigated;
    }

    public void Close()
    {
        if (Status is not RiskStatus.Accepted and
            not RiskStatus.Mitigated)
        {
            throw new InvalidOperationException(
                "Solo un riesgo aceptado o mitigado puede cerrarse.");
        }

        Status = RiskStatus.Closed;
        IsActive = false;
    }

    private void EnsureEditable()
    {
        if (Status == RiskStatus.Closed)
        {
            throw new InvalidOperationException(
                "Un riesgo cerrado no puede modificarse.");
        }
    }

    private void SetAssessment(
        int probability,
        int impact)
    {
        if (probability is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(
                nameof(probability),
                "La probabilidad debe estar entre 1 y 5.");
        }

        if (impact is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(
                nameof(impact),
                "El impacto debe estar entre 1 y 5.");
        }

        Probability = probability;
        Impact = impact;
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

    private static RiskLevel CalculateLevel(
        int score)
    {
        return score switch
        {
            <= 4 => RiskLevel.Low,
            <= 9 => RiskLevel.Medium,
            <= 16 => RiskLevel.High,
            _ => RiskLevel.Critical
        };
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
