using AuditCore.Domain.Common;

namespace AuditCore.Domain.Entities;

public sealed class Department : BaseAuditableEntity
{
    private Department()
    {
    }

    public Department(
        Guid organizationId,
        string name,
        string code,
        Guid? branchId = null)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException(
                "La organización es obligatoria.",
                nameof(organizationId));
        }

        OrganizationId = organizationId;
        BranchId = branchId;

        SetName(name);
        SetCode(code);

        IsActive = true;
    }

    public Guid OrganizationId { get; private set; }

    public Organization Organization { get; private set; } = null!;

    public Guid? BranchId { get; private set; }

    public Branch? Branch { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public void Update(
        string name,
        string code,
        Guid? branchId,
        bool isActive)
    {
        SetName(name);
        SetCode(code);

        BranchId = branchId;
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
}
