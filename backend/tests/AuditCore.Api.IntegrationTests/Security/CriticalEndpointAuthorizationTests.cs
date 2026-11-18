using System.Net;
using System.Net.Http.Json;
using AuditCore.Api.IntegrationTests.Infrastructure;
using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Auth.Models;

namespace AuditCore.Api.IntegrationTests.Security;

/// <summary>
/// Un usuario con la matriz de permisos de VIEWER puede consultar, pero ninguna mutación ni exportación debe estar autorizada.
/// </summary>
public sealed class CriticalEndpointAuthorizationTests : IClassFixture<AuditCoreWebApplicationFactory>
{
    private static readonly string[] ViewerPermissions =
    [
        PermissionCodes.OrganizationsView, PermissionCodes.AuditsView, PermissionCodes.FrameworksView,
        PermissionCodes.RisksView, PermissionCodes.FindingsView, PermissionCodes.EvidenceView,
        PermissionCodes.ActionPlansView, PermissionCodes.ReportsView
    ];

    private readonly AuditCoreWebApplicationFactory _factory;

    public CriticalEndpointAuthorizationTests(AuditCoreWebApplicationFactory factory) => _factory = factory;

    public static TheoryData<string, string> ForbiddenOperations => new()
    {
        { "POST", "/api/audits" },
        { "PUT", "/api/audits/{id}/start" },
        { "PUT", "/api/audits/{id}/close" },
        { "POST", "/api/risks" },
        { "POST", "/api/findings" },
        { "POST", "/api/action-plans" },
        { "DELETE", "/api/evidence/{id}" },
        { "POST", "/api/organizations" },
        { "DELETE", "/api/branches/{id}" },
        { "GET", "/api/users" },
        { "GET", "/api/users/lookup" },
        { "PUT", "/api/users/{id}/roles" },
        { "GET", "/api/roles" },
        { "PUT", "/api/roles/{id}/permissions" },
        { "POST", "/api/frameworks" },
        { "GET", "/api/reports/audits/export?format=Csv" }
    };

    [Theory]
    [MemberData(nameof(ForbiddenOperations))]
    public async Task Viewer_ShouldBeForbiddenFromMutationsAndAdministration(string method, string template)
    {
        using var client = await CreateViewerClientAsync();
        using var request = new HttpRequestMessage(new HttpMethod(method), template.Replace("{id}", Guid.NewGuid().ToString()));
        if (method is "POST" or "PUT") request.Content = JsonContent.Create(new { });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/audits")]
    [InlineData("/api/risks")]
    [InlineData("/api/findings")]
    [InlineData("/api/evidence")]
    [InlineData("/api/action-plans")]
    [InlineData("/api/frameworks")]
    [InlineData("/api/reports/dashboard")]
    public async Task Viewer_ShouldReadPermittedModules(string url)
    {
        using var client = await CreateViewerClientAsync();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ForLockedUser_ShouldReturnUnauthorized()
    {
        var organization = await _factory.CreateOrganizationAsync();
        var user = await _factory.CreateUserAsync(organization.Id, "REFRESH_VIEWER", ViewerPermissions);
        var login = await _factory.LoginAsync(user.Email, user.Password);

        await _factory.WithDbAsync(async (db, _) =>
        {
            var entity = await db.Users.FindAsync(user.Id);
            entity!.Lock();
            return await db.SaveChangesAsync();
        });

        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(login.RefreshToken));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<HttpClient> CreateViewerClientAsync()
    {
        var organization = await _factory.CreateOrganizationAsync();
        var viewer = await _factory.CreateUserAsync(organization.Id, "VIEWER_MATRIX", ViewerPermissions);
        return await _factory.CreateClientForAsync(viewer);
    }
}
