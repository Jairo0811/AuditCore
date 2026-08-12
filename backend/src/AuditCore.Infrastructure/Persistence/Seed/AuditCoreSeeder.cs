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
        await SeedRolePermissionsAsync(cancellationToken);

        var organization =
            await SeedOrganizationAsync(cancellationToken);

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

    private async Task SeedRolePermissionsAsync(
        CancellationToken cancellationToken)
    {
        var roles = await _dbContext.Roles
            .IgnoreQueryFilters()
            .Where(role => DefaultRolePermissions.Matrix.Keys.Contains(role.Code))
            .ToDictionaryAsync(role => role.Code, cancellationToken);

        var permissionCodes = DefaultRolePermissions.Matrix.Values
            .SelectMany(codes => codes)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissions = await _dbContext.Permissions
            .IgnoreQueryFilters()
            .Where(permission => permissionCodes.Contains(permission.Code))
            .ToDictionaryAsync(permission => permission.Code, cancellationToken);

        var existingAssignments = await _dbContext.RolePermissions
            .Select(item => new { item.RoleId, item.PermissionId })
            .ToListAsync(cancellationToken);

        var existing = existingAssignments
            .Select(item => (item.RoleId, item.PermissionId))
            .ToHashSet();

        foreach (var (roleCode, requiredPermissionCodes) in DefaultRolePermissions.Matrix)
        {
            if (!roles.TryGetValue(roleCode, out var role))
            {
                continue;
            }

            foreach (var permissionCode in requiredPermissionCodes)
            {
                if (!permissions.TryGetValue(permissionCode, out var permission))
                {
                    throw new InvalidOperationException(
                        $"El permiso requerido '{permissionCode}' no está registrado.");
                }

                if (existing.Contains((role.Id, permission.Id)))
                {
                    continue;
                }

                _dbContext.RolePermissions.Add(
                    new RolePermission(role.Id, permission.Id));
            }
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
                        organization.Code ==
                        DefaultOrganizationCode,
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

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return organization;
    }

    private async Task SeedAdministratorAsync(
        Organization organization,
        CancellationToken cancellationToken)
    {
        var email =
            (_configuration["SeedData:AdminEmail"]
             ?? DefaultAdminEmail)
            .Trim()
            .ToLowerInvariant();

        var password =
            _configuration["SeedData:AdminPassword"];

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "SeedData:AdminPassword no está configurada. " +
                "Defínela mediante configuración segura antes " +
                "de iniciar AuditCore.");
        }

        var resetPassword =
            _configuration.GetValue<bool>(
                "SeedData:ResetAdminPasswordOnStartup");

        var existingUser = await _dbContext.Users
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                user =>
                    user.OrganizationId == organization.Id &&
                    user.Email == email,
                cancellationToken);

        if (existingUser is not null)
        {
            var changed = false;

            if (resetPassword)
            {
                var passwordHash =
                    _passwordHasher.HashPassword(
                        existingUser,
                        password);

                existingUser.ChangePassword(passwordHash);
                changed = true;
            }

            if (!existingUser.IsActive)
            {
                existingUser.Activate();
                changed = true;
            }

            if (existingUser.IsLocked)
            {
                existingUser.Unlock();
                changed = true;
            }

            if (changed)
            {
                await _dbContext.SaveChangesAsync(
                    cancellationToken);
            }

            await EnsureSuperAdminRoleAsync(
                existingUser,
                cancellationToken);

            return;
        }

        var user = new User(
            organization.Id,
            "Administrador",
            "AuditCore",
            email,
            "TEMPORARY_HASH");

        var hash =
            _passwordHasher.HashPassword(
                user,
                password);

        user.ChangePassword(hash);

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await EnsureSuperAdminRoleAsync(
            user,
            cancellationToken);
    }

    private async Task EnsureSuperAdminRoleAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var superAdminRole =
            await _dbContext.Roles
                .SingleAsync(
                    role =>
                        role.Code == DefaultRoles.SuperAdmin,
                    cancellationToken);

        var hasRole =
            await _dbContext.UserRoles
                .AnyAsync(
                    item =>
                        item.UserId == user.Id &&
                        item.RoleId == superAdminRole.Id,
                    cancellationToken);

        if (hasRole)
        {
            return;
        }

        _dbContext.UserRoles.Add(
            new UserRole(
                user.Id,
                superAdminRole.Id));

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
