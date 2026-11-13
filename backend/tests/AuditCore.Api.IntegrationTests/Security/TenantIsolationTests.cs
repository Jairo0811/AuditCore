using System.Net;
using System.Net.Http.Json;
using AuditCore.Api.IntegrationTests.Infrastructure;
using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Audits.Models;
using AuditCore.Application.Features.Users.Models;
using AuditCore.Domain.Entities;

namespace AuditCore.Api.IntegrationTests.Security;

/// <summary>
/// Escenario con dos organizaciones. El actor pertenece a la organización A y tiene permisos amplios,
/// pero no es superadministrador; ningún recurso de la organización B debe ser accesible (IDOR/BOLA).
/// </summary>
public sealed class TwoTenantFixture : IAsyncLifetime
{
    public AuditCoreWebApplicationFactory Factory { get; } = new();
    public HttpClient ClientA { get; private set; } = null!;
    public HttpClient SuperAdminClient { get; private set; } = null!;
    public Guid OrganizationA { get; private set; }
    public Guid OrganizationB { get; private set; }
    public Guid AuditA { get; private set; }
    public Guid AuditB { get; private set; }
    public Guid RiskB { get; private set; }
    public Guid FindingB { get; private set; }
    public Guid ActionPlanB { get; private set; }
    public Guid EvidenceB { get; private set; }
    public Guid BranchB { get; private set; }
    public Guid DepartmentB { get; private set; }
    public Guid UserB { get; private set; }
    public Guid UserA { get; private set; }

    public static readonly string[] BroadPermissions =
    [
        PermissionCodes.OrganizationsView, PermissionCodes.OrganizationsManage,
        PermissionCodes.UsersView, PermissionCodes.UsersManage,
        PermissionCodes.AuditsView, PermissionCodes.AuditsCreate, PermissionCodes.AuditsUpdate,
        PermissionCodes.AuditsExecute, PermissionCodes.AuditsClose,
        PermissionCodes.RisksView, PermissionCodes.RisksManage,
        PermissionCodes.FindingsView, PermissionCodes.FindingsManage,
        PermissionCodes.EvidenceView, PermissionCodes.EvidenceManage,
        PermissionCodes.ActionPlansView, PermissionCodes.ActionPlansManage,
        PermissionCodes.ReportsView, PermissionCodes.ReportsExport
    ];

    public async Task InitializeAsync()
    {
        await Factory.InitializeAsync();

        OrganizationA = (await Factory.CreateOrganizationAsync("Organización A")).Id;
        OrganizationB = (await Factory.CreateOrganizationAsync("Organización B")).Id;

        var actorA = await Factory.CreateUserAsync(OrganizationA, "TENANT_MANAGER", BroadPermissions);
        UserA = actorA.Id;
        var actorB = await Factory.CreateUserAsync(OrganizationB, "TENANT_MANAGER", BroadPermissions);
        UserB = actorB.Id;
        var superAdmin = await Factory.CreateUserAsync(OrganizationA, "SUPER_ADMIN", BroadPermissions);

        AuditA = (await Factory.CreateAuditAsync(OrganizationA)).Id;
        AuditB = (await Factory.CreateAuditAsync(OrganizationB)).Id;

        (RiskB, FindingB, ActionPlanB, EvidenceB, BranchB, DepartmentB) = await Factory.WithDbAsync(async (db, _) =>
        {
            var risk = new Risk(AuditB, "RSK-B-001", "Riesgo B", null, 3, 3, null, UserB);
            db.Risks.Add(risk);
            var finding = new Finding(AuditB, "FND-B-001", "Hallazgo B", "Condición", "Criterio", null, null, null, FindingSeverity.High, risk.Id, UserB);
            db.Findings.Add(finding);
            var plan = new ActionPlan(finding.Id, "Plan B", null, UserB, DateTime.UtcNow.AddDays(10));
            db.ActionPlans.Add(plan);
            var evidence = new Evidence(AuditB, "b.txt", "text/plain", 4, $"{AuditB:N}/b.txt", new string('B', 64), null, finding.Id, UserB);
            db.Evidences.Add(evidence);
            var branch = new Branch(OrganizationB, "Sucursal B", "SUC-B-001");
            db.Branches.Add(branch);
            var department = new Department(OrganizationB, "Departamento B", "DEP-B-001", branch.Id);
            db.Departments.Add(department);
            await db.SaveChangesAsync();
            return (risk.Id, finding.Id, plan.Id, evidence.Id, branch.Id, department.Id);
        });

        ClientA = await Factory.CreateClientForAsync(actorA);
        SuperAdminClient = await Factory.CreateClientForAsync(superAdmin);
    }

    public async Task DisposeAsync()
    {
        ClientA.Dispose();
        SuperAdminClient.Dispose();
        await Factory.DisposeAsync();
    }
}

public sealed class TenantIsolationTests : IClassFixture<TwoTenantFixture>
{
    private readonly TwoTenantFixture _fixture;

    public TenantIsolationTests(TwoTenantFixture fixture) => _fixture = fixture;

    public static TheoryData<string> ForeignDetailEndpoints => new()
    {
        "/api/audits/{AuditB}",
        "/api/risks/{RiskB}",
        "/api/findings/{FindingB}",
        "/api/action-plans/{ActionPlanB}",
        "/api/evidence/{EvidenceB}",
        "/api/evidence/{EvidenceB}/download",
        "/api/branches/{BranchB}",
        "/api/departments/{DepartmentB}",
        "/api/users/{UserB}"
    };

    [Theory]
    [MemberData(nameof(ForeignDetailEndpoints))]
    public async Task ForeignResource_ShouldNotBeReadable(string template)
    {
        var response = await _fixture.ClientA.GetAsync(Resolve(template));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public static TheoryData<string> ForeignFilteredQueries => new()
    {
        "/api/audits?organizationId={OrganizationB}",
        "/api/users?organizationId={OrganizationB}",
        "/api/users/lookup?organizationId={OrganizationB}",
        "/api/branches?organizationId={OrganizationB}",
        "/api/departments?organizationId={OrganizationB}",
        "/api/organizations/{OrganizationB}",
        "/api/reports/dashboard?organizationId={OrganizationB}",
        "/api/reports/audits/export?organizationId={OrganizationB}&format=Csv"
    };

    [Theory]
    [MemberData(nameof(ForeignFilteredQueries))]
    public async Task ExplicitForeignOrganizationFilter_ShouldBeForbidden(string template)
    {
        var response = await _fixture.ClientA.GetAsync(Resolve(template));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Collections_ShouldOnlyContainOwnOrganizationData()
    {
        var audits = await _fixture.ClientA.GetFromJsonAsync<List<AuditDto>>("/api/audits");
        Assert.Contains(audits!, x => x.Id == _fixture.AuditA);
        Assert.All(audits!, x => Assert.Equal(_fixture.OrganizationA, x.OrganizationId));

        var lookup = await _fixture.ClientA.GetFromJsonAsync<List<UserLookupDto>>("/api/users/lookup");
        Assert.Contains(lookup!, x => x.Id == _fixture.UserA);
        Assert.All(lookup!, x => Assert.Equal(_fixture.OrganizationA, x.OrganizationId));

        foreach (var url in new[] { "/api/risks", "/api/findings", "/api/action-plans", "/api/evidence", "/api/branches", "/api/departments", "/api/users" })
        {
            var body = await _fixture.ClientA.GetStringAsync(url);
            Assert.DoesNotContain(_fixture.OrganizationB.ToString(), body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(_fixture.AuditB.ToString(), body, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static TheoryData<string, string> ForeignMutations => new()
    {
        { "PUT", "/api/audits/{AuditB}" },
        { "PUT", "/api/audits/{AuditB}/start" },
        { "PUT", "/api/audits/{AuditB}/cancel" },
        { "PUT", "/api/risks/{RiskB}/accept" },
        { "PUT", "/api/findings/{FindingB}/review" },
        { "PUT", "/api/action-plans/{ActionPlanB}/cancel" },
        { "DELETE", "/api/evidence/{EvidenceB}" },
        { "DELETE", "/api/branches/{BranchB}" },
        { "DELETE", "/api/departments/{DepartmentB}" },
        { "PUT", "/api/users/{UserB}/lock" },
        { "PUT", "/api/users/{UserB}/deactivate" }
    };

    [Theory]
    [MemberData(nameof(ForeignMutations))]
    public async Task ForeignResource_ShouldNotBeModifiable(string method, string template)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), Resolve(template));
        if (template == "/api/audits/{AuditB}")
            request.Content = JsonContent.Create(new { title = "Intento", objective = (string?)null, scope = (string?)null });

        var response = await _fixture.ClientA.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Upload_ToForeignAudit_ShouldBeForbidden()
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(_fixture.AuditB.ToString()), "auditId" },
            { new ByteArrayContent("hola"u8.ToArray()) { Headers = { ContentType = new("text/plain") } }, "file", "x.txt" }
        };

        var response = await _fixture.ClientA.PostAsync("/api/evidence", form);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SuperAdmin_ShouldKeepGlobalAccess()
    {
        Assert.Equal(HttpStatusCode.OK, (await _fixture.SuperAdminClient.GetAsync($"/api/audits/{_fixture.AuditB}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _fixture.SuperAdminClient.GetAsync($"/api/audits?organizationId={_fixture.OrganizationB}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _fixture.SuperAdminClient.GetAsync($"/api/reports/audits/export?organizationId={_fixture.OrganizationB}&format=Csv")).StatusCode);
    }

    private string Resolve(string template) => template
        .Replace("{OrganizationB}", _fixture.OrganizationB.ToString())
        .Replace("{AuditB}", _fixture.AuditB.ToString())
        .Replace("{RiskB}", _fixture.RiskB.ToString())
        .Replace("{FindingB}", _fixture.FindingB.ToString())
        .Replace("{ActionPlanB}", _fixture.ActionPlanB.ToString())
        .Replace("{EvidenceB}", _fixture.EvidenceB.ToString())
        .Replace("{BranchB}", _fixture.BranchB.ToString())
        .Replace("{DepartmentB}", _fixture.DepartmentB.ToString())
        .Replace("{UserB}", _fixture.UserB.ToString());
}
