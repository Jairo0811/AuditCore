using AuditCore.Application.Features.Auth.Models;

namespace AuditCore.Application.Features.Auth;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> RefreshAsync(
        RefreshRequest request,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default);
}
