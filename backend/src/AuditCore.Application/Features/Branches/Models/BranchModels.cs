namespace AuditCore.Application.Features.Branches.Models;

public sealed record BranchDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    string Name,
    string Code,
    string? Address,
    bool IsActive);

public sealed record CreateBranchRequest(
    Guid OrganizationId,
    string Name,
    string Code,
    string? Address);

public sealed record UpdateBranchRequest(
    string Name,
    string Code,
    string? Address,
    bool IsActive);
