using AuditCore.Application;
using AuditCore.Infrastructure;

namespace AuditCore.Api.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddAuditCoreApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddAuditCoreModules(configuration);

        return services;
    }

    private static IServiceCollection AddAuditCoreModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);

        return services;
    }
}
