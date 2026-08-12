namespace AuditCore.Application.Features.Users.Models;

public sealed record UserDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    bool IsActive,
    bool IsLocked,
    DateTime? LastLoginAtUtc,
    IReadOnlyCollection<string> Roles);

public sealed record CreateUserRequest(
    Guid OrganizationId,
    string FirstName,
    string LastName,
    string Email,
    string Password,
    IReadOnlyCollection<Guid>? RoleIds);

public sealed record UpdateUserRequest(
    string FirstName,
    string LastName,
    string Email);

public sealed record ChangeUserPasswordRequest(
    string Password);

public sealed record SetUserRolesRequest(
    IReadOnlyCollection<Guid> RoleIds);
