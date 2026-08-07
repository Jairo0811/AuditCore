using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Application.Common.Interfaces;

public interface IAuditCoreDbContext
{
    DbSet<Organization> Organizations { get; }

    DbSet<Branch> Branches { get; }

    DbSet<Department> Departments { get; }

    DbSet<User> Users { get; }

    DbSet<Role> Roles { get; }

    DbSet<Permission> Permissions { get; }

    DbSet<UserRole> UserRoles { get; }

    DbSet<RolePermission> RolePermissions { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
