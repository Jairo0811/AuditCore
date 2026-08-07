namespace AuditCore.Application.Features.Permissions.Models;

public sealed record PermissionDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive);
