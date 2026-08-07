using System.Text;
using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Auth;
using AuditCore.Domain.Entities;
using AuditCore.Infrastructure.Identity;
using AuditCore.Infrastructure.Persistence;
using AuditCore.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AuditCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "La cadena de conexión 'DefaultConnection' no está configurada.");

        services.AddDbContext<AuditCoreDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlServerOptions =>
                    sqlServerOptions.EnableRetryOnFailure()));

        services.AddScoped<IAuditCoreDbContext>(
            serviceProvider =>
                serviceProvider.GetRequiredService<AuditCoreDbContext>());

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        services.AddScoped<AuditCoreSeeder>();
        services.AddScoped<DatabaseInitializer>();

        services.Configure<JwtOptions>(
            configuration.GetSection(
                JwtOptions.SectionName));

        var jwtOptions =
            configuration
                .GetSection(JwtOptions.SectionName)
                .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "La configuración JWT no existe.");

        if (string.IsNullOrWhiteSpace(jwtOptions.Key) ||
            jwtOptions.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key debe tener al menos 32 caracteres.");
        }

        services.AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    jwtOptions.Key)),
                        ClockSkew = TimeSpan.Zero
                    };
            });

        services.AddAuthorization();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
