namespace AuditCore.Application.Features.Roles.Models;

public sealed record RoleDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    IReadOnlyCollection<string> Permissions);

public sealed record CreateRoleRequest(
    string Name,
    string Code,
    string? Description);

public sealed record UpdateRoleRequest(
    string Name,
    string Code,
    string? Description,
    bool IsActive);

public sealed record SetRolePermissionsRequest(
    IReadOnlyCollection<Guid> PermissionIds);
