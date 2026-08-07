using AuditCore.Application.Common.Security;
using AuditCore.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AuditCore.Infrastructure.Persistence;

public sealed class AuditActorInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserContext _currentUser;

    public AuditActorInterceptor(ICurrentUserContext currentUser) => _currentUser = currentUser;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null || !_currentUser.UserId.HasValue) return;
        var userId = _currentUser.UserId.Value;

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Property(nameof(BaseAuditableEntity.CreatedByUserId)).CurrentValue = userId;
            else if (entry.State == EntityState.Modified)
                entry.Property(nameof(BaseAuditableEntity.UpdatedByUserId)).CurrentValue = userId;
        }
    }
}
