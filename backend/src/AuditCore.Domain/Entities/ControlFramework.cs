using AuditCore.Domain.Common;

namespace AuditCore.Domain.Entities;

public sealed class ControlFramework : BaseAuditableEntity
{
    private ControlFramework() { }

    public ControlFramework(string name, string code, string version, string? description = null)
    {
        SetName(name);
        SetCode(code);
        SetVersion(version);
        Description = Optional(description);
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Version { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public ICollection<ControlDefinition> Controls { get; private set; } = [];

    public void Update(string name, string code, string version, string? description, bool isActive)
    {
        SetName(name);
        SetCode(code);
        SetVersion(version);
        Description = Optional(description);
        IsActive = isActive;
    }

    private void SetName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Name = value.Trim();
    }

    private void SetCode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Code = value.Trim().ToUpperInvariant();
    }

    private void SetVersion(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Version = value.Trim();
    }

    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
