using AuditCore.Application.Features.Users.Models;

namespace AuditCore.Application.Features.Users;

public interface IUserService
{
    Task<IReadOnlyCollection<UserDto>> GetAllAsync(
        Guid? organizationId = null,
        CancellationToken cancellationToken = default);

    Task<UserDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<UserDto> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<UserDto?> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ChangePasswordAsync(
        Guid id,
        ChangeUserPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> SetRolesAsync(
        Guid id,
        SetUserRolesRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(
        Guid id,
        bool active,
        CancellationToken cancellationToken = default);

    Task<bool> SetLockedAsync(
        Guid id,
        bool locked,
        CancellationToken cancellationToken = default);
}
