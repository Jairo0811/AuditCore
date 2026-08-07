using AuditCore.Application.Common.Interfaces;
using AuditCore.Domain.Common;
using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Persistence;

public sealed class AuditCoreDbContext : DbContext, IAuditCoreDbContext
{
    public AuditCoreDbContext(DbContextOptions<AuditCoreDbContext> options) : base(options) { }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Audit> Audits => Set<Audit>();
    public DbSet<Risk> Risks => Set<Risk>();
    public DbSet<Finding> Findings => Set<Finding>();
    public DbSet<Evidence> Evidences => Set<Evidence>();
    public DbSet<ActionPlan> ActionPlans => Set<ActionPlan>();
    public DbSet<ControlFramework> ControlFrameworks => Set<ControlFramework>();
    public DbSet<ControlDefinition> ControlDefinitions => Set<ControlDefinition>();
    public DbSet<ControlEvaluation> ControlEvaluations => Set<ControlEvaluation>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditCoreDbContext).Assembly);
        ApplySoftDeleteFilters(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Property(nameof(BaseAuditableEntity.CreatedAtUtc)).CurrentValue = now;
            if (entry.State == EntityState.Modified)
                entry.Property(nameof(BaseAuditableEntity.UpdatedAtUtc)).CurrentValue = now;
        }
    }

    private static void ApplySoftDeleteFilters(ModelBuilder modelBuilder)
    {
        var auditableTypes = modelBuilder.Model.GetEntityTypes()
            .Where(entityType => typeof(BaseAuditableEntity).IsAssignableFrom(entityType.ClrType));

        foreach (var entityType in auditableTypes)
        {
            var method = typeof(AuditCoreDbContext)
                .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);
            method.Invoke(null, new object[] { modelBuilder });
        }
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : BaseAuditableEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(entity => !entity.IsDeleted);
    }
}
