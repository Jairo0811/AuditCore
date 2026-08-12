namespace AuditCore.Application.Features.Departments.Models;

public sealed record DepartmentDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    Guid? BranchId,
    string? BranchName,
    string Name,
    string Code,
    bool IsActive);

public sealed record CreateDepartmentRequest(
    Guid OrganizationId,
    string Name,
    Guid? BranchId);

public sealed record UpdateDepartmentRequest(
    string Name,
    Guid? BranchId,
    bool IsActive);
