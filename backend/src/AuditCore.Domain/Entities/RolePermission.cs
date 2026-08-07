namespace AuditCore.Domain.Entities;

public sealed class RolePermission
{
    private RolePermission()
    {
    }

    public RolePermission(Guid roleId, Guid permissionId)
    {
        if (roleId == Guid.Empty)
        {
            throw new ArgumentException(
                "El rol es obligatorio.",
                nameof(roleId));
        }

        if (permissionId == Guid.Empty)
        {
            throw new ArgumentException(
                "El permiso es obligatorio.",
                nameof(permissionId));
        }

        RoleId = roleId;
        PermissionId = permissionId;
    }

    public Guid RoleId { get; private set; }

    public Role Role { get; private set; } = null!;

    public Guid PermissionId { get; private set; }

    public Permission Permission { get; private set; } = null!;
}
