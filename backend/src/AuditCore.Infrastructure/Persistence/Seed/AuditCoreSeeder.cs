using AuditCore.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AuditCore.Infrastructure.Persistence.Seed;

public sealed class AuditCoreSeeder
{
    private const string DefaultOrganizationCode = "AUDITCORE";
    private const string DefaultAdminEmail = "admin@auditcore.local";

    private readonly AuditCoreDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuditCoreSeeder(
        AuditCoreDbContext dbContext,
        IConfiguration configuration,
        IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(cancellationToken);
        await SeedPermissionsAsync(cancellationToken);
        await SeedSuperAdminPermissionsAsync(cancellationToken);

        var organization = await SeedOrganizationAsync(cancellationToken);

        await SeedAdministratorAsync(
            organization,
            cancellationToken);
    }

    private async Task SeedRolesAsync(
        CancellationToken cancellationToken)
    {
        var existingCodes = await _dbContext.Roles
            .IgnoreQueryFilters()
            .Select(role => role.Code)
            .ToListAsync(cancellationToken);

        foreach (var item in DefaultRoles.All)
        {
            if (existingCodes.Contains(item.Code))
            {
                continue;
            }

            _dbContext.Roles.Add(
                new Role(
                    item.Name,
                    item.Code,
                    item.Description));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedPermissionsAsync(
        CancellationToken cancellationToken)
    {
        var existingCodes = await _dbContext.Permissions
            .IgnoreQueryFilters()
            .Select(permission => permission.Code)
            .ToListAsync(cancellationToken);

        foreach (var item in DefaultPermissions.All)
        {
            if (existingCodes.Contains(item.Code))
            {
                continue;
            }

            _dbContext.Permissions.Add(
                new Permission(
                    item.Name,
                    item.Code));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedSuperAdminPermissionsAsync(
        CancellationToken cancellationToken)
    {
        var superAdminRole = await _dbContext.Roles
            .SingleAsync(
                role => role.Code == DefaultRoles.SuperAdmin,
                cancellationToken);

        var permissions = await _dbContext.Permissions
            .Where(permission =>
                DefaultPermissions.SuperAdmin.Contains(permission.Code))
            .ToListAsync(cancellationToken);

        var existingPermissionIds = await _dbContext.RolePermissions
            .Where(item => item.RoleId == superAdminRole.Id)
            .Select(item => item.PermissionId)
            .ToListAsync(cancellationToken);

        foreach (var permission in permissions)
        {
            if (existingPermissionIds.Contains(permission.Id))
            {
                continue;
            }

            _dbContext.RolePermissions.Add(
                new RolePermission(
                    superAdminRole.Id,
                    permission.Id));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Organization> SeedOrganizationAsync(
        CancellationToken cancellationToken)
    {
        var existingOrganization =
            await _dbContext.Organizations
                .SingleOrDefaultAsync(
                    organization =>
                        organization.Code == DefaultOrganizationCode,
                    cancellationToken);

        if (existingOrganization is not null)
        {
            return existingOrganization;
        }

        var organization = new Organization(
            "AuditCore",
            DefaultOrganizationCode,
            "Organización inicial de administración de AuditCore.");

        _dbContext.Organizations.Add(organization);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return organization;
    }

    private async Task SeedAdministratorAsync(
        Organization organization,
        CancellationToken cancellationToken)
    {
        var email =
            _configuration["SeedData:AdminEmail"]
            ?? DefaultAdminEmail;

        var existingUser = await _dbContext.Users
            .SingleOrDefaultAsync(
                user =>
                    user.OrganizationId == organization.Id &&
                    user.Email == email.ToLowerInvariant(),
                cancellationToken);

        if (existingUser is not null)
        {
            return;
        }

        var password =
            _configuration["SeedData:AdminPassword"];

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "SeedData:AdminPassword no está configurada. " +
                "Defínela mediante una variable de entorno antes de iniciar AuditCore.");
        }

        var user = new User(
            organization.Id,
            "Administrador",
            "AuditCore",
            email,
            "TEMPORARY_HASH");

        var passwordHash =
            _passwordHasher.HashPassword(user, password);

        user.ChangePassword(passwordHash);

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var superAdminRole = await _dbContext.Roles
            .SingleAsync(
                role => role.Code == DefaultRoles.SuperAdmin,
                cancellationToken);

        _dbContext.UserRoles.Add(
            new UserRole(
                user.Id,
                superAdminRole.Id));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
