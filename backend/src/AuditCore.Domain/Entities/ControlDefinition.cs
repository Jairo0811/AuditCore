using AuditCore.Domain.Common;

namespace AuditCore.Domain.Entities;

public sealed class ControlDefinition : BaseAuditableEntity
{
    private ControlDefinition() { }

    public ControlDefinition(Guid frameworkId, string code, string title, string domain, decimal weight, string? description = null)
    {
        if (frameworkId == Guid.Empty) throw new ArgumentException("El marco es obligatorio.", nameof(frameworkId));
        if (weight <= 0 || weight > 100) throw new ArgumentOutOfRangeException(nameof(weight), "El peso debe estar entre 0 y 100.");

        FrameworkId = frameworkId;
        SetCode(code);
        SetTitle(title);
        SetDomain(domain);
        Weight = weight;
        Description = Optional(description);
        IsActive = true;
    }

    public Guid FrameworkId { get; private set; }
    public ControlFramework Framework { get; private set; } = null!;
    public string Code { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Domain { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Weight { get; private set; }
    public bool IsActive { get; private set; }

    public void Update(string code, string title, string domain, decimal weight, string? description, bool isActive)
    {
        if (weight <= 0 || weight > 100) throw new ArgumentOutOfRangeException(nameof(weight), "El peso debe estar entre 0 y 100.");
        SetCode(code);
        SetTitle(title);
        SetDomain(domain);
        Weight = weight;
        Description = Optional(description);
        IsActive = isActive;
    }

    private void SetCode(string value) { ArgumentException.ThrowIfNullOrWhiteSpace(value); Code = value.Trim().ToUpperInvariant(); }
    private void SetTitle(string value) { ArgumentException.ThrowIfNullOrWhiteSpace(value); Title = value.Trim(); }
    private void SetDomain(string value) { ArgumentException.ThrowIfNullOrWhiteSpace(value); Domain = value.Trim(); }
    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
