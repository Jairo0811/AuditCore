using AuditCore.Api.Configuration;

namespace AuditCore.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuditCoreApi(builder.Configuration);

        var app = builder.Build();

        app.UseAuditCorePipeline();

        app.Run();
    }
}
