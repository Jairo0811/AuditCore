using AuditCore.Api;
using AuditCore.Application.Common.Interfaces;
using AuditCore.Domain.Entities;
using AuditCore.Infrastructure.Persistence;
using AuditCore.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AuditCore.Api.IntegrationTests.Infrastructure;

public sealed class AuditCoreWebApplicationFactory
    : WebApplicationFactory<Program>,
      IAsyncLifetime
{
    private readonly string _databaseName =
        $"AuditCoreIntegrationTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            RemoveProductionDatabaseServices(services);

            services.AddDbContext<AuditCoreDbContext>(
                options =>
                    options.UseInMemoryDatabase(
                        _databaseName));

            services.AddScoped<IAuditCoreDbContext>(
                serviceProvider =>
                    serviceProvider.GetRequiredService<
                        AuditCoreDbContext>());
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<AuditCoreDbContext>();

        await dbContext.Database.EnsureCreatedAsync();

        await SeedAsync(
            dbContext,
            scope.ServiceProvider);
    }

    public new async Task DisposeAsync()
    {
        using (var scope = Services.CreateScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AuditCoreDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
        }

        await base.DisposeAsync();
    }

    private static void RemoveProductionDatabaseServices(
        IServiceCollection services)
    {
        services.RemoveAll<
            DbContextOptions<AuditCoreDbContext>>();

        services.RemoveAll<
            IDbContextOptionsConfiguration<AuditCoreDbContext>>();

        services.RemoveAll<AuditCoreDbContext>();

        services.RemoveAll<IAuditCoreDbContext>();
    }

    private static async Task SeedAsync(
        AuditCoreDbContext dbContext,
        IServiceProvider serviceProvider)
    {
        var organization =
            new Organization(
                "AuditCore Tests",
                "AUDITCORE_TEST",
                "Organización utilizada por pruebas de integración.");

        dbContext.Organizations.Add(organization);

        var role =
            new Role(
                "Super Administrador",
                DefaultRoles.SuperAdmin,
                "Rol utilizado por las pruebas de integración.");

        dbContext.Roles.Add(role);

        var permission =
            new Permission(
                "Ver usuarios",
                "USERS.VIEW");

        dbContext.Permissions.Add(permission);

        await dbContext.SaveChangesAsync();

        var user =
            new User(
                organization.Id,
                "Administrador",
                "Pruebas",
                TestCredentials.Email,
                "TEMPORARY");

        var passwordHasher =
            serviceProvider.GetRequiredService<
                IPasswordHasher<User>>();

        user.ChangePassword(
            passwordHasher.HashPassword(
                user,
                TestCredentials.Password));

        dbContext.Users.Add(user);

        dbContext.UserRoles.Add(
            new UserRole(
                user.Id,
                role.Id));

        dbContext.RolePermissions.Add(
            new RolePermission(
                role.Id,
                permission.Id));

        await dbContext.SaveChangesAsync();
    }
}

public static class TestCredentials
{
    public const string Email =
        "admin.integration@auditcore.local";

    public const string Password =
        "AuditCore-Test-Password-2026!";
}
