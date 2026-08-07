using AuditCore.Domain.Common;

namespace AuditCore.Domain.Entities;

public sealed class ControlQuestion : BaseAuditableEntity
{
    private ControlQuestion() { }

    public ControlQuestion(Guid controlId, string text, decimal weight, int order, bool isRequired = true)
    {
        if (controlId == Guid.Empty) throw new ArgumentException("El control es obligatorio.", nameof(controlId));
        if (weight <= 0 || weight > 100) throw new ArgumentOutOfRangeException(nameof(weight), "El peso debe estar entre 0 y 100.");
        if (order < 1) throw new ArgumentOutOfRangeException(nameof(order), "El orden debe ser mayor que cero.");
        ControlId = controlId;
        SetText(text);
        Weight = weight;
        Order = order;
        IsRequired = isRequired;
        IsActive = true;
    }

    public Guid ControlId { get; private set; }
    public ControlDefinition Control { get; private set; } = null!;
    public string Text { get; private set; } = string.Empty;
    public decimal Weight { get; private set; }
    public int Order { get; private set; }
    public bool IsRequired { get; private set; }
    public bool IsActive { get; private set; }

    public void Update(string text, decimal weight, int order, bool isRequired, bool isActive)
    {
        if (weight <= 0 || weight > 100) throw new ArgumentOutOfRangeException(nameof(weight));
        if (order < 1) throw new ArgumentOutOfRangeException(nameof(order));
        SetText(text);
        Weight = weight;
        Order = order;
        IsRequired = isRequired;
        IsActive = isActive;
    }

    private void SetText(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        Text = text.Trim();
    }
}
