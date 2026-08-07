using System.Security.Cryptography;
using System.Text;
using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Auth;
using AuditCore.Application.Features.Auth.Models;
using AuditCore.Domain.Entities;
using AuditCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuditCore.Infrastructure.Identity;

public sealed class AuthService : IAuthService
{
    private readonly AuditCoreDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        AuditCoreDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Password);

        var email = request.Email.Trim().ToLowerInvariant();

        var users = await _dbContext.Users
            .Where(user => user.Email == email)
            .Take(2)
            .ToListAsync(cancellationToken);

        if (users.Count != 1)
        {
            throw new UnauthorizedAccessException(
                "Credenciales inválidas.");
        }

        var user = users[0];

        if (!user.IsActive || user.IsLocked)
        {
            throw new UnauthorizedAccessException(
                "Credenciales inválidas.");
        }

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

        if (verificationResult ==
            PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException(
                "Credenciales inválidas.");
        }

        user.RegisterLogin();

        return await CreateSessionAsync(
            user,
            cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(
        RefreshRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.RefreshToken);

        var currentHash =
            HashRefreshToken(request.RefreshToken);

        var currentToken =
            await _dbContext.RefreshTokens
                .Include(token => token.User)
                .SingleOrDefaultAsync(
                    token => token.TokenHash == currentHash,
                    cancellationToken);

        if (currentToken is null ||
            !currentToken.IsActive ||
            !currentToken.User.IsActive ||
            currentToken.User.IsLocked)
        {
            throw new UnauthorizedAccessException(
                "Refresh token inválido o expirado.");
        }

        var newRawRefreshToken =
            GenerateRefreshToken();

        var newTokenHash =
            HashRefreshToken(newRawRefreshToken);

        currentToken.Revoke(newTokenHash);

        var replacementToken =
            new RefreshToken(
                currentToken.UserId,
                newTokenHash,
                DateTime.UtcNow.AddDays(
                    _jwtOptions.RefreshTokenDays));

        _dbContext.RefreshTokens.Add(replacementToken);

        var authorization =
            await LoadAuthorizationAsync(
                currentToken.UserId,
                cancellationToken);

        var accessToken =
            _jwtTokenService.GenerateAccessToken(
                currentToken.User,
                authorization.Roles,
                authorization.Permissions);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new AuthResponse(
            accessToken,
            newRawRefreshToken,
            DateTime.UtcNow.AddMinutes(
                _jwtOptions.AccessTokenMinutes),
            currentToken.User.Email,
            currentToken.User.FullName,
            authorization.Roles,
            authorization.Permissions);
    }

    public async Task LogoutAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.RefreshToken);

        var tokenHash =
            HashRefreshToken(request.RefreshToken);

        var refreshToken =
            await _dbContext.RefreshTokens
                .SingleOrDefaultAsync(
                    token => token.TokenHash == tokenHash,
                    cancellationToken);

        if (refreshToken is null)
        {
            return;
        }

        refreshToken.Revoke();

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<AuthResponse> CreateSessionAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var authorization =
            await LoadAuthorizationAsync(
                user.Id,
                cancellationToken);

        var accessToken =
            _jwtTokenService.GenerateAccessToken(
                user,
                authorization.Roles,
                authorization.Permissions);

        var rawRefreshToken =
            GenerateRefreshToken();

        var tokenHash =
            HashRefreshToken(rawRefreshToken);

        var refreshToken =
            new RefreshToken(
                user.Id,
                tokenHash,
                DateTime.UtcNow.AddDays(
                    _jwtOptions.RefreshTokenDays));

        _dbContext.RefreshTokens.Add(refreshToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new AuthResponse(
            accessToken,
            rawRefreshToken,
            DateTime.UtcNow.AddMinutes(
                _jwtOptions.AccessTokenMinutes),
            user.Email,
            user.FullName,
            authorization.Roles,
            authorization.Permissions);
    }

    private async Task<AuthorizationData> LoadAuthorizationAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var roles = await _dbContext.UserRoles
            .Where(item => item.UserId == userId)
            .Select(item => item.Role.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        var permissions =
            await _dbContext.RolePermissions
                .Where(item =>
                    roles.Contains(item.Role.Code))
                .Select(item => item.Permission.Code)
                .Distinct()
                .ToListAsync(cancellationToken);

        return new AuthorizationData(
            roles,
            permissions);
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));
    }

    private static string HashRefreshToken(
        string refreshToken)
    {
        var bytes =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(refreshToken));

        return Convert.ToHexString(bytes);
    }

    private sealed record AuthorizationData(
        IReadOnlyCollection<string> Roles,
        IReadOnlyCollection<string> Permissions);
}
