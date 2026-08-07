using AuditCore.Api.Configuration;

namespace AuditCore.Api;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuditCoreApi(
            builder.Configuration);

        var app = builder.Build();

        if (!app.Environment.IsEnvironment("Testing"))
        {
            await app.InitializeAuditCoreDatabaseAsync();
        }

        app.UseAuditCorePipeline();

        await app.RunAsync();
    }
}