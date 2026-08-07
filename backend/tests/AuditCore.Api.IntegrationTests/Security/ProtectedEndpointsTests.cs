using System.Net;
using AuditCore.Api.IntegrationTests.Infrastructure;

namespace AuditCore.Api.IntegrationTests.Security;

public sealed class ProtectedEndpointsTests : IClassFixture<AuditCoreWebApplicationFactory>
{
    private readonly HttpClient _client;
    public ProtectedEndpointsTests(AuditCoreWebApplicationFactory factory) => _client = factory.CreateClient();

    [Theory]
    [InlineData("/api/audits")]
    [InlineData("/api/risks")]
    [InlineData("/api/findings")]
    [InlineData("/api/evidence")]
    [InlineData("/api/action-plans")]
    [InlineData("/api/frameworks")]
    [InlineData("/api/reports/dashboard")]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturnUnauthorized(string url)
    {
        var response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Health_ShouldBePublicAndHealthy()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
