using System.Net;
using System.Net.Http.Json;
using AuditCore.Api.IntegrationTests.Infrastructure;
using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Users.Models;
using AuditCore.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Api.IntegrationTests.Security;

public sealed class UserAdministrationSecurityTests : IClassFixture<AuditCoreWebApplicationFactory>
{
    private static readonly string[] OrganizationAdminPermissions =
        [PermissionCodes.UsersView, PermissionCodes.UsersManage, PermissionCodes.RolesView];

    private readonly AuditCoreWebApplicationFactory _factory;

    public UserAdministrationSecurityTests(AuditCoreWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task OrganizationAdmin_ShouldNotAssignSuperAdminRole()
    {
        var scenario = await ArrangeAsync();

        var response = await scenario.OrgAdminClient.PutAsJsonAsync(
            $"/api/users/{scenario.PlainUser.Id}/roles",
            new SetUserRolesRequest([scenario.SuperAdminRoleId]));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var roles = await RoleCodesAsync(scenario.PlainUser.Id);
        Assert.DoesNotContain(DefaultRoles.SuperAdmin, roles);
    }

    [Fact]
    public async Task OrganizationAdmin_ShouldReplaceRolesOfRegularUser()
    {
        var scenario = await ArrangeAsync();
        var auditorRole = await _factory.EnsureRoleAsync("AUDITOR_TEST", [PermissionCodes.AuditsView]);

        var response = await scenario.OrgAdminClient.PutAsJsonAsync(
            $"/api/users/{scenario.PlainUser.Id}/roles",
            new SetUserRolesRequest([scenario.ViewerRoleId, auditorRole.Id]));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var user = await scenario.OrgAdminClient.GetFromJsonAsync<UserDto>($"/api/users/{scenario.PlainUser.Id}");
        Assert.Equal(new[] { "AUDITOR_TEST", "VIEWER_TEST" }, user!.Roles.OrderBy(x => x).ToArray());
    }

    [Theory]
    [InlineData("password")]
    [InlineData("roles")]
    [InlineData("lock")]
    [InlineData("deactivate")]
    [InlineData("profile")]
    public async Task OrganizationAdmin_ShouldNotModifySuperAdminAccount(string operation)
    {
        var scenario = await ArrangeAsync();
        var target = scenario.SuperAdminUser.Id;

        var response = operation switch
        {
            "password" => await scenario.OrgAdminClient.PutAsJsonAsync($"/api/users/{target}/password", new ChangeUserPasswordRequest("Tomado-Por-Admin-2026")),
            "roles" => await scenario.OrgAdminClient.PutAsJsonAsync($"/api/users/{target}/roles", new SetUserRolesRequest([scenario.ViewerRoleId])),
            "profile" => await scenario.OrgAdminClient.PutAsJsonAsync($"/api/users/{target}", new UpdateUserRequest(null, null, "Otro", "Nombre", "otro@tests.auditcore.local")),
            _ => await scenario.OrgAdminClient.PutAsync($"/api/users/{target}/{operation}", null)
        };

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SuperAdmin_ShouldAssignSuperAdminRole()
    {
        var scenario = await ArrangeAsync();

        var response = await scenario.SuperAdminClient.PutAsJsonAsync(
            $"/api/users/{scenario.PlainUser.Id}/roles",
            new SetUserRolesRequest([scenario.SuperAdminRoleId]));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Contains(DefaultRoles.SuperAdmin, await RoleCodesAsync(scenario.PlainUser.Id));
    }

    [Theory]
    [InlineData("corta1")]
    [InlineData("solamenteletras")]
    [InlineData("1234567890123")]
    [InlineData("          ")]
    public async Task CreateUser_WithWeakPassword_ShouldReturnBadRequest(string password)
    {
        var scenario = await ArrangeAsync();

        var response = await scenario.OrgAdminClient.PostAsJsonAsync("/api/users", NewUser(scenario.OrganizationId, password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("10", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task CreateUser_WithPolicyCompliantPassword_ShouldSucceed()
    {
        var scenario = await ArrangeAsync();

        var response = await scenario.OrgAdminClient.PostAsJsonAsync("/api/users", NewUser(scenario.OrganizationId, "Valida-Segura-2026"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_ShouldEnforcePolicy()
    {
        var scenario = await ArrangeAsync();
        var url = $"/api/users/{scenario.PlainUser.Id}/password";

        Assert.Equal(HttpStatusCode.BadRequest, (await scenario.OrgAdminClient.PutAsJsonAsync(url, new ChangeUserPasswordRequest("abc"))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await scenario.OrgAdminClient.PutAsJsonAsync(url, new ChangeUserPasswordRequest("Nueva-Clave-2026"))).StatusCode);

        var login = await _factory.LoginAsync(scenario.PlainUser.Email, "Nueva-Clave-2026");
        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
    }

    [Fact]
    public async Task UserLookup_ShouldRequireAssignmentPermissionAndExcludeInactiveUsers()
    {
        var scenario = await ArrangeAsync();
        var viewer = await _factory.CreateUserAsync(scenario.OrganizationId, "LOOKUP_VIEWER", [PermissionCodes.AuditsView]);
        var riskManager = await _factory.CreateUserAsync(scenario.OrganizationId, "LOOKUP_RISK", [PermissionCodes.RisksView, PermissionCodes.RisksManage]);

        using var viewerClient = await _factory.CreateClientForAsync(viewer);
        Assert.Equal(HttpStatusCode.Forbidden, (await viewerClient.GetAsync("/api/users/lookup")).StatusCode);

        await scenario.OrgAdminClient.PutAsync($"/api/users/{scenario.PlainUser.Id}/lock", null);

        using var riskClient = await _factory.CreateClientForAsync(riskManager);
        var lookup = await riskClient.GetFromJsonAsync<List<UserLookupDto>>("/api/users/lookup");
        Assert.Contains(lookup!, x => x.Id == riskManager.Id);
        Assert.DoesNotContain(lookup!, x => x.Id == scenario.PlainUser.Id);
    }

    private async Task<Scenario> ArrangeAsync()
    {
        var organization = await _factory.CreateOrganizationAsync();
        var orgAdmin = await _factory.CreateUserAsync(organization.Id, "ORGANIZATION_ADMIN_TEST", OrganizationAdminPermissions);
        var superAdmin = await _factory.CreateUserAsync(organization.Id, DefaultRoles.SuperAdmin, OrganizationAdminPermissions);
        var plain = await _factory.CreateUserAsync(organization.Id, "VIEWER_TEST", [PermissionCodes.AuditsView]);
        var viewerRole = await _factory.EnsureRoleAsync("VIEWER_TEST", [PermissionCodes.AuditsView]);
        var superAdminRole = await _factory.EnsureRoleAsync(DefaultRoles.SuperAdmin, []);

        return new Scenario(
            organization.Id,
            await _factory.CreateClientForAsync(orgAdmin),
            await _factory.CreateClientForAsync(superAdmin),
            superAdmin,
            plain,
            viewerRole.Id,
            superAdminRole.Id);
    }

    private Task<string[]> RoleCodesAsync(Guid userId) =>
        _factory.WithDbAsync((db, _) => db.UserRoles.Where(x => x.UserId == userId).Select(x => x.Role.Code).ToArrayAsync());

    private static CreateUserRequest NewUser(Guid organizationId, string password) =>
        new(organizationId, null, null, "Nuevo", "Usuario", $"nuevo-{Guid.NewGuid():N}@tests.auditcore.local", password, null);

    private sealed record Scenario(
        Guid OrganizationId,
        HttpClient OrgAdminClient,
        HttpClient SuperAdminClient,
        TestUser SuperAdminUser,
        TestUser PlainUser,
        Guid ViewerRoleId,
        Guid SuperAdminRoleId);
}
