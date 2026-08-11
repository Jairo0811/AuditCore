namespace AuditCore.Application.Features.Organizations.Models;

public sealed record OrganizationDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive);

public sealed record CreateOrganizationRequest(
    string Name,
    string? Description);

public sealed record UpdateOrganizationRequest(
    string Name,
    string? Description,
    bool IsActive);
