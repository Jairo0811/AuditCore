using System.Net;
using AuditCore.Api.IntegrationTests.Infrastructure;
using AuditCore.Application.Common.Security;
using AuditCore.Domain.Entities;

namespace AuditCore.Api.IntegrationTests.Administration;

public sealed class AdministrativeDeletionTests : IClassFixture<AuditCoreWebApplicationFactory>
{
    private static readonly string[] AdminPermissions =
        [PermissionCodes.OrganizationsView, PermissionCodes.OrganizationsManage];

    private readonly AuditCoreWebApplicationFactory _factory;

    public AdministrativeDeletionTests(AuditCoreWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task DeleteBranch_WithAssignedUsers_ShouldReturnBusinessConflict()
    {
        var (client, organizationId) = await ArrangeAsync();
        var branch = await CreateBranchAsync(organizationId);
        await _factory.CreateUserAsync(organizationId, "BRANCH_MEMBER", [PermissionCodes.AuditsView], branchId: branch.Id);

        var response = await client.DeleteAsync($"/api/branches/{branch.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("usuarios asignados", await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/branches/{branch.Id}")).StatusCode);
    }

    [Fact]
    public async Task DeleteDepartment_WithAssignedUsers_ShouldReturnBusinessConflict()
    {
        var (client, organizationId) = await ArrangeAsync();
        var department = await _factory.WithDbAsync(async (db, _) =>
        {
            var entity = new Department(organizationId, "Finanzas", $"DEP-{Guid.NewGuid():N}"[..12]);
            db.Departments.Add(entity);
            await db.SaveChangesAsync();
            return entity;
        });
        await _factory.CreateUserAsync(organizationId, "DEPARTMENT_MEMBER", [PermissionCodes.AuditsView], departmentId: department.Id);

        var response = await client.DeleteAsync($"/api/departments/{department.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("usuarios asignados", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task DeleteBranch_WithDepartments_ShouldReturnBusinessConflict()
    {
        var (client, organizationId) = await ArrangeAsync();
        var branch = await CreateBranchAsync(organizationId);
        await _factory.WithDbAsync(async (db, _) =>
        {
            db.Departments.Add(new Department(organizationId, "Operaciones", $"DEP-{Guid.NewGuid():N}"[..12], branch.Id));
            return await db.SaveChangesAsync();
        });

        var response = await client.DeleteAsync($"/api/branches/{branch.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBranch_WithoutRelations_ShouldSucceed()
    {
        var (client, organizationId) = await ArrangeAsync();
        var branch = await CreateBranchAsync(organizationId);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/branches/{branch.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/branches/{branch.Id}")).StatusCode);
    }

    [Fact]
    public async Task DeleteOrganization_WithRelatedRecords_ShouldReturnBusinessConflict()
    {
        var organization = await _factory.CreateOrganizationAsync();
        var actorOrganization = await _factory.CreateOrganizationAsync();
        var superAdmin = await _factory.CreateUserAsync(actorOrganization.Id, "SUPER_ADMIN", AdminPermissions);
        await _factory.CreateAuditAsync(organization.Id);
        using var client = await _factory.CreateClientForAsync(superAdmin);

        var response = await client.DeleteAsync($"/api/organizations/{organization.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private async Task<(HttpClient Client, Guid OrganizationId)> ArrangeAsync()
    {
        var organization = await _factory.CreateOrganizationAsync();
        var admin = await _factory.CreateUserAsync(organization.Id, "STRUCTURE_ADMIN", AdminPermissions);
        return (await _factory.CreateClientForAsync(admin), organization.Id);
    }

    private Task<Branch> CreateBranchAsync(Guid organizationId) =>
        _factory.WithDbAsync(async (db, _) =>
        {
            var branch = new Branch(organizationId, "Sucursal Norte", $"SUC-{Guid.NewGuid():N}"[..12]);
            db.Branches.Add(branch);
            await db.SaveChangesAsync();
            return branch;
        });
}
