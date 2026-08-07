using AuditCore.Domain.Common;

namespace AuditCore.Domain.Entities;

public sealed class Role : BaseAuditableEntity
{
    private Role()
    {
    }

    public Role(string name, string code, string? description = null)
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

    public ICollection<UserRole> UserRoles { get; private set; } = [];

    public ICollection<RolePermission> RolePermissions { get; private set; } = [];

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
