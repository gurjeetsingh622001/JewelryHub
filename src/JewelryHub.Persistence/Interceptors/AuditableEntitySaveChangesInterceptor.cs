using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace JewelryHub.Persistence.Interceptors;

/// <summary>
/// Centralizes two cross-cutting rules so individual services never have
/// to remember them:
///  1. Stamp CreatedAtUtc/CreatedBy on insert, ModifiedAtUtc/ModifiedBy on update.
///  2. Turn a tracked Delete into a soft-delete (IsDeleted = true) for any
///     entity implementing ISoftDelete, instead of issuing a DELETE.
/// The current user id comes from ICurrentUserService (Application layer
/// abstraction, implemented in Infrastructure from the JWT claims) so this
/// interceptor has no dependency on ASP.NET Core's HttpContext.
/// </summary>
public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    public AuditableEntitySaveChangesInterceptor(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAuditRulesAndSoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyAuditRulesAndSoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditRulesAndSoftDelete(DbContext? context)
    {
        if (context is null) return;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                    entry.Entity.CreatedBy = _currentUser.UserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedAtUtc = DateTime.UtcNow;
                    entry.Entity.ModifiedBy = _currentUser.UserId;
                    break;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State != EntityState.Deleted) continue;

            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAtUtc = DateTime.UtcNow;
            entry.Entity.DeletedBy = _currentUser.UserId;
        }
    }
}

