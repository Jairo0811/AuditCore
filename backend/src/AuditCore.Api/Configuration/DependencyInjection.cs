using System.Threading.RateLimiting;
using AuditCore.Api.Services;
using AuditCore.Application;
using AuditCore.Application.Common.Security;
using AuditCore.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;

namespace AuditCore.Api.Configuration;

public static class DependencyInjection
{
    public const string CorsPolicyName = "AuditCoreFrontend";

    public static IServiceCollection AddAuditCoreApi(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHealthChecks();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5173", "https://localhost:5173"];

        services.AddCors(options =>
            options.AddPolicy(CorsPolicyName, policy =>
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));

        var authPermitLimit = configuration.GetValue<int?>("RateLimiting:AuthPermitLimit") ?? 10;

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("auth", limiter =>
            {
                limiter.PermitLimit = authPermitLimit;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.QueueLimit = 0;
                limiter.AutoReplenishment = true;
            });
        });

        services.AddApplication();
        services.AddInfrastructure(configuration);
        return services;
    }
}
