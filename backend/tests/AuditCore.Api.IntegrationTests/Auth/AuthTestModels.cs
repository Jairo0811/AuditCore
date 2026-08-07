namespace AuditCore.Api.IntegrationTests.Auth;

public sealed record AuthTestResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);

public sealed record CurrentUserTestResponse(
    string? UserId,
    string? Email,
    string? Name,
    string? OrganizationId,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
