using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuditCore.Api.IntegrationTests.Auth;
using AuditCore.Application.Features.Auth.Models;
using AuditCore.Domain.Entities;
using AuditCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuditCore.Api.IntegrationTests.Infrastructure;

public sealed record TestUser(Guid Id, Guid OrganizationId, string Email, string Password);

/// <summary>
/// Utilidades para preparar escenarios multiempresa y RBAC directamente en la base en memoria.
/// </summary>
public static class TestData
{
    public const string DefaultPassword = "Integration-Test-2026";

    public static async Task<T> WithDbAsync<T>(
        this AuditCoreWebApplicationFactory factory,
        Func<AuditCoreDbContext, IServiceProvider, Task<T>> action)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditCoreDbContext>();
        return await action(dbContext, scope.ServiceProvider);
    }

    public static Task<Organization> CreateOrganizationAsync(
        this AuditCoreWebApplicationFactory factory,
        string? name = null) =>
        factory.WithDbAsync(async (db, _) =>
        {
            var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            var organization = new Organization(name ?? $"Organización {suffix}", $"ORG-{suffix}");
            db.Organizations.Add(organization);
            await db.SaveChangesAsync();
            return organization;
        });

    /// <summary>
    /// Crea (o reutiliza) un rol con el código indicado, le agrega los permisos solicitados y crea un usuario con ese rol.
    /// </summary>
    public static Task<TestUser> CreateUserAsync(
        this AuditCoreWebApplicationFactory factory,
        Guid organizationId,
        string roleCode,
        IEnumerable<string> permissions,
        Guid? branchId = null,
        Guid? departmentId = null) =>
        factory.WithDbAsync(async (db, services) =>
        {
            var role = await EnsureRoleAsync(db, roleCode, permissions);

            var email = $"user-{Guid.NewGuid():N}@tests.auditcore.local";
            var user = new User(organizationId, "Usuario", roleCode, email, "TEMPORARY", branchId, departmentId);
            var hasher = services.GetRequiredService<IPasswordHasher<User>>();
            user.ChangePassword(hasher.HashPassword(user, DefaultPassword));
            db.Users.Add(user);
            db.UserRoles.Add(new UserRole(user.Id, role.Id));
            await db.SaveChangesAsync();

            return new TestUser(user.Id, organizationId, email, DefaultPassword);
        });

    public static Task<Role> EnsureRoleAsync(
        this AuditCoreWebApplicationFactory factory,
        string roleCode,
        IEnumerable<string> permissions) =>
        factory.WithDbAsync((db, _) => EnsureRoleAsync(db, roleCode, permissions));

    public static Task<Audit> CreateAuditAsync(
        this AuditCoreWebApplicationFactory factory,
        Guid organizationId) =>
        factory.WithDbAsync(async (db, _) =>
        {
            var audit = new Audit(organizationId, $"AUD-{Guid.NewGuid():N}"[..20], "Auditoría de prueba");
            db.Audits.Add(audit);
            await db.SaveChangesAsync();
            return audit;
        });

    public static async Task<HttpClient> CreateClientForAsync(
        this AuditCoreWebApplicationFactory factory,
        TestUser user)
    {
        var auth = await factory.LoginAsync(user.Email, user.Password);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }

    public static async Task<AuthTestResponse> LoginAsync(
        this AuditCoreWebApplicationFactory factory,
        string email,
        string password)
    {
        using var anonymous = factory.CreateClient();
        var response = await anonymous.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthTestResponse>())!;
    }

    private static async Task<Role> EnsureRoleAsync(
        AuditCoreDbContext db,
        string roleCode,
        IEnumerable<string> permissions)
    {
        var role = await db.Roles.SingleOrDefaultAsync(x => x.Code == roleCode);
        if (role is null)
        {
            role = new Role(roleCode, roleCode);
            db.Roles.Add(role);
            await db.SaveChangesAsync();
        }

        foreach (var code in permissions.Distinct(StringComparer.Ordinal))
        {
            var permission = await db.Permissions.SingleOrDefaultAsync(x => x.Code == code);
            if (permission is null)
            {
                permission = new Permission(code, code);
                db.Permissions.Add(permission);
                await db.SaveChangesAsync();
            }

            var assigned = await db.RolePermissions.AnyAsync(x => x.RoleId == role.Id && x.PermissionId == permission.Id);
            if (!assigned)
            {
                db.RolePermissions.Add(new RolePermission(role.Id, permission.Id));
                await db.SaveChangesAsync();
            }
        }

        return role;
    }
}
