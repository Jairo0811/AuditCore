using System.Net;
using System.Net.Http.Json;
using AuditCore.Api.IntegrationTests.Infrastructure;
using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Permissions.Models;
using AuditCore.Application.Features.Roles.Models;

namespace AuditCore.Api.IntegrationTests.Roles;

/// <summary>
/// Reproduce el flujo de la pantalla de Roles: el rol expone sus permisos por código y el guardado trabaja con IDs.
/// Convertir los códigos actuales a IDs y guardar sin cambios debe conservar exactamente la asignación original.
/// </summary>
public sealed class RolePermissionsTests : IClassFixture<AuditCoreWebApplicationFactory>
{
    private readonly AuditCoreWebApplicationFactory _factory;

    public RolePermissionsTests(AuditCoreWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task SavingCurrentPermissionsMappedFromCodes_ShouldPreserveAssignment()
    {
        var client = await CreateRoleAdminClientAsync();
        var target = await _factory.EnsureRoleAsync(
            "ROLE_UNDER_TEST",
            [PermissionCodes.AuditsView, PermissionCodes.RisksView, PermissionCodes.ReportsView]);

        var before = (await client.GetFromJsonAsync<RoleDto>($"/api/roles/{target.Id}"))!;
        var catalog = (await client.GetFromJsonAsync<List<PermissionDto>>("/api/permissions"))!;
        var ids = catalog.Where(p => before.Permissions.Contains(p.Code)).Select(p => p.Id).ToArray();
        Assert.Equal(before.Permissions.Count, ids.Length);

        var response = await client.PutAsJsonAsync($"/api/roles/{target.Id}/permissions", new SetRolePermissionsRequest(ids));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var after = (await client.GetFromJsonAsync<RoleDto>($"/api/roles/{target.Id}"))!;
        Assert.Equal(before.Permissions.OrderBy(x => x), after.Permissions.OrderBy(x => x));
    }

    [Fact]
    public async Task SavingUnknownPermission_ShouldReturnConflictAndKeepAssignment()
    {
        var client = await CreateRoleAdminClientAsync();
        var target = await _factory.EnsureRoleAsync("ROLE_UNKNOWN_TEST", [PermissionCodes.AuditsView]);

        var response = await client.PutAsJsonAsync(
            $"/api/roles/{target.Id}/permissions",
            new SetRolePermissionsRequest([Guid.NewGuid()]));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var after = (await client.GetFromJsonAsync<RoleDto>($"/api/roles/{target.Id}"))!;
        Assert.Equal(new[] { PermissionCodes.AuditsView }, after.Permissions.ToArray());
    }

    private async Task<HttpClient> CreateRoleAdminClientAsync()
    {
        var organization = await _factory.CreateOrganizationAsync();
        var admin = await _factory.CreateUserAsync(
            organization.Id,
            "SUPER_ADMIN",
            [PermissionCodes.RolesView, PermissionCodes.RolesManage]);
        return await _factory.CreateClientForAsync(admin);
    }
}
