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
        // Cada módulo funcional registrará aquí su capa Application e Infrastructure.
        // Ejemplos futuros: Identity, Organizations, Audits, Risks y Reporting.
        return services;
    }
}
