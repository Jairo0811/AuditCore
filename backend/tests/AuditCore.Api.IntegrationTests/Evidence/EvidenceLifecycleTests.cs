using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using AuditCore.Api.IntegrationTests.Infrastructure;
using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Evidence;
using Microsoft.EntityFrameworkCore;
using EvidenceEntity = AuditCore.Domain.Entities.Evidence;

namespace AuditCore.Api.IntegrationTests.Evidence;

public sealed class EvidenceLifecycleTests : IClassFixture<AuditCoreWebApplicationFactory>
{
    private readonly AuditCoreWebApplicationFactory _factory;

    public EvidenceLifecycleTests(AuditCoreWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Delete_ShouldRemoveEvidenceFromCollectionDetailAndDownload()
    {
        var (client, user, auditId) = await ArrangeAsync();
        const string text = "Contenido de evidencia para prueba de ciclo completo.";

        var upload = await client.PostAsync("/api/evidence", BuildForm(auditId, "acta.txt", "text/plain", Encoding.UTF8.GetBytes(text)));
        Assert.Equal(HttpStatusCode.Created, upload.StatusCode);
        var evidence = (await upload.Content.ReadFromJsonAsync<EvidenceDto>())!;
        Assert.Equal(user.Id, evidence.UploadedByUserId);

        var download = await client.GetAsync($"/api/evidence/{evidence.Id}/download");
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.Equal(text, await download.Content.ReadAsStringAsync());

        var delete = await client.DeleteAsync($"/api/evidence/{evidence.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var collection = await client.GetFromJsonAsync<List<EvidenceDto>>($"/api/evidence?auditId={auditId}");
        Assert.DoesNotContain(collection!, x => x.Id == evidence.Id);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/evidence/{evidence.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/evidence/{evidence.Id}/download")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/evidence/{evidence.Id}")).StatusCode);

        var stored = await _factory.WithDbAsync((db, _) =>
            db.Evidences.IgnoreQueryFilters().AsNoTracking().SingleAsync(x => x.Id == evidence.Id));
        Assert.True(stored.IsDeleted);
        Assert.False(stored.IsActive);
        Assert.False(File.Exists(Path.Combine(_factory.EvidenceStoragePath, stored.StorageKey)));
    }

    [Fact]
    public async Task Upload_ShouldIgnoreClientProvidedUploader()
    {
        var (client, user, auditId) = await ArrangeAsync();
        var otherUser = await _factory.CreateUserAsync(user.OrganizationId, "EVIDENCE_OTHER", [PermissionCodes.EvidenceView]);

        var form = BuildForm(auditId, "nota.txt", "text/plain", "texto"u8.ToArray());
        form.Add(new StringContent(otherUser.Id.ToString()), "uploadedByUserId");

        var upload = await client.PostAsync("/api/evidence", form);
        Assert.Equal(HttpStatusCode.Created, upload.StatusCode);
        var evidence = (await upload.Content.ReadFromJsonAsync<EvidenceDto>())!;
        Assert.Equal(user.Id, evidence.UploadedByUserId);
    }

    [Theory]
    [InlineData("reporte.pdf", "application/pdf", "esto no es un PDF")]
    [InlineData("script.exe", "application/octet-stream", "MZ binario")]
    [InlineData("imagen.png", "text/plain", "texto plano")]
    public async Task Upload_WithInconsistentFile_ShouldBeRejected(string fileName, string contentType, string content)
    {
        var (client, _, auditId) = await ArrangeAsync();

        var upload = await client.PostAsync("/api/evidence", BuildForm(auditId, fileName, contentType, Encoding.UTF8.GetBytes(content)));

        Assert.Equal(HttpStatusCode.Conflict, upload.StatusCode);
    }

    [Fact]
    public async Task LegacyDeactivatedEvidence_ShouldNotBeVisibleOrDownloadable()
    {
        var (client, _, auditId) = await ArrangeAsync();
        var legacyId = await _factory.WithDbAsync(async (db, _) =>
        {
            var legacy = new EvidenceEntity(auditId, "antigua.txt", "text/plain", 10, $"{auditId:N}/legacy.txt", new string('A', 64));
            legacy.Deactivate();
            db.Evidences.Add(legacy);
            await db.SaveChangesAsync();
            return legacy.Id;
        });

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/evidence/{legacyId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/evidence/{legacyId}/download")).StatusCode);
        var collection = await client.GetFromJsonAsync<List<EvidenceDto>>($"/api/evidence?auditId={auditId}");
        Assert.DoesNotContain(collection!, x => x.Id == legacyId);
    }

    private async Task<(HttpClient Client, TestUser User, Guid AuditId)> ArrangeAsync()
    {
        var organization = await _factory.CreateOrganizationAsync();
        var user = await _factory.CreateUserAsync(
            organization.Id,
            "EVIDENCE_MANAGER",
            [PermissionCodes.EvidenceView, PermissionCodes.EvidenceManage]);
        var audit = await _factory.CreateAuditAsync(organization.Id);
        return (await _factory.CreateClientForAsync(user), user, audit.Id);
    }

    private static MultipartFormDataContent BuildForm(Guid auditId, string fileName, string contentType, byte[] content)
    {
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent
        {
            { new StringContent(auditId.ToString()), "auditId" },
            { new StringContent("Evidencia de prueba"), "description" },
            { file, "file", fileName }
        };
    }
}
