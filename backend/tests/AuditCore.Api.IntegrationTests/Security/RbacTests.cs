using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuditCore.Api.IntegrationTests.Auth;
using AuditCore.Api.IntegrationTests.Infrastructure;
using AuditCore.Application.Features.Auth.Models;

namespace AuditCore.Api.IntegrationTests.Security;

public sealed class RbacTests : IClassFixture<AuditCoreWebApplicationFactory>
{
    private readonly HttpClient _client;
    public RbacTests(AuditCoreWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task AuthenticatedUser_WithoutRequiredPermission_ShouldReturnForbidden()
    {
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(TestCredentials.Email, TestCredentials.Password));
        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<AuthTestResponse>();
        Assert.NotNull(login);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/reports/dashboard");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedUser_WithRequiredPermission_ShouldSucceed()
    {
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(TestCredentials.Email, TestCredentials.Password));
        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<AuthTestResponse>();
        Assert.NotNull(login);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
