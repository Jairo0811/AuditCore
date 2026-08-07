using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuditCore.Api.IntegrationTests.Infrastructure;
using AuditCore.Application.Features.Auth.Models;

namespace AuditCore.Api.IntegrationTests.Auth;

public sealed class AuthEndpointsTests
    : IClassFixture<AuditCoreWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(
        AuditCoreWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        var response = await LoginAsync();

        Assert.Equal(
            TestCredentials.Email,
            response.Email);

        Assert.False(
            string.IsNullOrWhiteSpace(
                response.AccessToken));

        Assert.False(
            string.IsNullOrWhiteSpace(
                response.RefreshToken));

        Assert.Contains(
            "SUPER_ADMIN",
            response.Roles);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        var request =
            new LoginRequest(
                TestCredentials.Email,
                "incorrect-password");

        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutAccessToken_ShouldReturnUnauthorized()
    {
        var response =
            await _client.GetAsync(
                "/api/auth/me");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidAccessToken_ShouldReturnCurrentUser()
    {
        var login = await LoginAsync();

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "/api/auth/me");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        var response =
            await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var currentUser =
            await response.Content
                .ReadFromJsonAsync<CurrentUserTestResponse>();

        Assert.NotNull(currentUser);

        Assert.Equal(
            TestCredentials.Email,
            currentUser.Email);

        Assert.Contains(
            "SUPER_ADMIN",
            currentUser.Roles);

        Assert.Contains(
            "USERS.VIEW",
            currentUser.Permissions);
    }

    [Fact]
    public async Task Refresh_ShouldRotateRefreshToken()
    {
        var login = await LoginAsync();

        var refreshed =
            await RefreshAsync(
                login.RefreshToken);

        Assert.NotEqual(
            login.RefreshToken,
            refreshed.RefreshToken);

        Assert.False(
            string.IsNullOrWhiteSpace(
                refreshed.AccessToken));
    }

    [Fact]
    public async Task Refresh_ReusingRotatedToken_ShouldReturnUnauthorized()
    {
        var login = await LoginAsync();

        await RefreshAsync(
            login.RefreshToken);

        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/refresh",
                new RefreshRequest(
                    login.RefreshToken));

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Logout_ShouldRevokeRefreshToken()
    {
        var login = await LoginAsync();

        var logoutResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/logout",
                new LogoutRequest(
                    login.RefreshToken));

        Assert.Equal(
            HttpStatusCode.NoContent,
            logoutResponse.StatusCode);

        var refreshResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/refresh",
                new RefreshRequest(
                    login.RefreshToken));

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            refreshResponse.StatusCode);
    }

    private async Task<AuthTestResponse> LoginAsync()
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest(
                    TestCredentials.Email,
                    TestCredentials.Password));

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<AuthTestResponse>()
            ?? throw new InvalidOperationException(
                "La respuesta del login está vacía.");
    }

    private async Task<AuthTestResponse> RefreshAsync(
        string refreshToken)
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/refresh",
                new RefreshRequest(
                    refreshToken));

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<AuthTestResponse>()
            ?? throw new InvalidOperationException(
                "La respuesta de refresh está vacía.");
    }
}
