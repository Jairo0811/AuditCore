using AuditCore.Infrastructure.Persistence.Seed;

namespace AuditCore.Api.Configuration;

public static class DatabaseInitialization
{
    public static async Task<WebApplication> InitializeAuditCoreDatabaseAsync(
        this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        await using var scope =
            app.Services.CreateAsyncScope();

        var initializer =
            scope.ServiceProvider
                .GetRequiredService<DatabaseInitializer>();

        await initializer.InitializeAsync();

        return app;
    }
}
