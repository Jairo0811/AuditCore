using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Application.Common.Interfaces;

public interface IAuditCoreDbContext
{
    DbSet<Organization> Organizations { get; }
    DbSet<Branch> Branches { get; }
    DbSet<Department> Departments { get; }
    DbSet<Audit> Audits { get; }
    DbSet<Risk> Risks { get; }
    DbSet<Finding> Findings { get; }
    DbSet<Evidence> Evidences { get; }
    DbSet<ActionPlan> ActionPlans { get; }
    DbSet<ControlFramework> ControlFrameworks { get; }
    DbSet<ControlDefinition> ControlDefinitions { get; }
    DbSet<ControlEvaluation> ControlEvaluations { get; }
    DbSet<ControlQuestion> ControlQuestions { get; }
    DbSet<ControlAnswer> ControlAnswers { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
