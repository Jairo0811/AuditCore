using AuditCore.Domain.Common;

namespace AuditCore.Domain.Entities;

public sealed class Branch : BaseAuditableEntity
{
    private Branch()
    {
    }

    public Branch(
        Guid organizationId,
        string name,
        string code,
        string? address = null)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException(
                "La organización es obligatoria.",
                nameof(organizationId));
        }

        OrganizationId = organizationId;

        SetName(name);
        SetCode(code);

        Address = NormalizeOptionalText(address);
        IsActive = true;
    }

    public Guid OrganizationId { get; private set; }

    public Organization Organization { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public string? Address { get; private set; }

    public bool IsActive { get; private set; }

    public ICollection<Department> Departments { get; private set; } = [];

    public void Update(
        string name,
        string code,
        string? address,
        bool isActive)
    {
        SetName(name);
        SetCode(code);

        Address = NormalizeOptionalText(address);
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
