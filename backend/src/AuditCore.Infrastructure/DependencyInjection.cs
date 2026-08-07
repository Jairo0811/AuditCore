using System.Text;
using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.ActionPlans;
using AuditCore.Application.Features.Audits;
using AuditCore.Application.Features.Auth;
using AuditCore.Application.Features.Branches;
using AuditCore.Application.Features.Departments;
using AuditCore.Application.Features.Evidence;
using AuditCore.Application.Features.Findings;
using AuditCore.Application.Features.Frameworks;
using AuditCore.Application.Features.Organizations;
using AuditCore.Application.Features.Permissions;
using AuditCore.Application.Features.Reports;
using AuditCore.Application.Features.Risks;
using AuditCore.Application.Features.Roles;
using AuditCore.Application.Features.Users;
using AuditCore.Domain.Entities;
using AuditCore.Infrastructure.Identity;
using AuditCore.Infrastructure.Persistence;
using AuditCore.Infrastructure.Persistence.Seed;
using AuditCore.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AuditCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");

        services.AddDbContext<AuditCoreDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

        services.AddScoped<IAuditCoreDbContext>(sp => sp.GetRequiredService<AuditCoreDbContext>());
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<AuditCoreSeeder>();
        services.AddScoped<DatabaseInitializer>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("La configuración JWT no existe.");
        if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
            throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            foreach (var permission in PermissionCodes.All)
                options.AddPolicy(permission, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("permission", permission);
                });
        });

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IRiskService, RiskService>();
        services.AddScoped<IFindingService, FindingService>();
        services.AddScoped<IEvidenceService, EvidenceService>();
        services.AddScoped<IActionPlanService, ActionPlanService>();
        services.AddScoped<IFrameworkService, FrameworkService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
