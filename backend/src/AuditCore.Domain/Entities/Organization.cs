using AuditCore.Domain.Common;

namespace AuditCore.Domain.Entities;

public sealed class Organization : BaseAuditableEntity
{
    private Organization()
    {
    }

    public Organization(
        string name,
        string code,
        string? description = null)
    {
        SetName(name);
        SetCode(code);

        Description = NormalizeOptionalText(description);
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public ICollection<Branch> Branches { get; private set; } = [];

    public ICollection<Department> Departments { get; private set; } = [];

    public void Update(
        string name,
        string code,
        string? description,
        bool isActive)
    {
        SetName(name);
        SetCode(code);

        Description = NormalizeOptionalText(description);
        IsActive = isActive;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }

    private void SetCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code.Trim().ToUpperInvariant();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
