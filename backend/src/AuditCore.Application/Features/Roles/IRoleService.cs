using AuditCore.Application.Features.Roles.Models;

namespace AuditCore.Application.Features.Roles;

public interface IRoleService
{
    Task<IReadOnlyCollection<RoleDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<RoleDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<RoleDto> CreateAsync(
        CreateRoleRequest request,
        CancellationToken cancellationToken = default);

    Task<RoleDto?> UpdateAsync(
        Guid id,
        UpdateRoleRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> SetPermissionsAsync(
        Guid id,
        SetRolePermissionsRequest request,
        CancellationToken cancellationToken = default);
}
