using System.Linq.Expressions;

namespace JewelryHub.Application.Common.Interfaces;

/// <summary>
/// Generic repository for straightforward CRUD. Query() returns an
/// IQueryable (no-tracking) so Application-layer handlers can compose
/// filters/paging/sorting/Include chains without every possible query
/// shape needing its own repository method — the alternative is a
/// repository interface that balloons to dozens of methods per aggregate.
/// Writes always go through AddAsync/Update/Remove so the Unit of Work
/// stays the single place changes are actually persisted.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    IQueryable<T> Query();

    /// <summary>
    /// Same as Query() but change-tracked. Needed whenever a handler must
    /// mutate a loaded aggregate's child collection directly (e.g.
    /// cart.Items.Remove(item)) — EF Core only picks up collection
    /// add/remove as an INSERT/DELETE when the graph was tracked to begin
    /// with; doing that against a no-tracking Query() result silently
    /// loses the removal. Everything else should keep using Query().
    /// </summary>
    IQueryable<T> QueryTracking();

    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}

/// <summary>
/// Coordinates repositories that participate in one logical transaction
/// and commits them together via a single SaveChangesAsync — per spec:
/// "await unitOfWork.SaveChangesAsync(); instead of independently saving
/// each repository operation when a transaction involves multiple entities."
/// Only lists repositories actually consumed by an Application feature so
/// far; grows as new modules get their vertical slices built out.
/// </summary>
public interface IUnitOfWork
{
    IRepository<Domain.Identity.User> Users { get; }
    IRepository<Domain.Identity.Role> Roles { get; }
    IRepository<Domain.Identity.RefreshToken> RefreshTokens { get; }
    IRepository<Domain.Customers.Customer> Customers { get; }
    IRepository<Domain.Sellers.Seller> Sellers { get; }
    IRepository<Domain.Sellers.SellerDocument> SellerDocuments { get; }
    IRepository<Domain.Catalog.Category> Categories { get; }
    IRepository<Domain.Catalog.Product> Products { get; }
    IRepository<Domain.Catalog.Inventory> Inventory { get; }
    IRepository<Domain.Cart.Cart> Carts { get; }
    IRepository<Domain.Wishlist.Wishlist> Wishlists { get; }
    IRepository<Domain.Orders.Order> Orders { get; }
    IRepository<Domain.Payments.Payment> Payments { get; }
    IRepository<Domain.Tax.TaxRate> TaxRates { get; }
    IRepository<Domain.Reviews.Review> Reviews { get; }
    IRepository<Domain.Notifications.Notification> Notifications { get; }
    IRepository<Domain.Unions.Union> Unions { get; }
    IRepository<Domain.Unions.UnionMember> UnionMembers { get; }
    IRepository<Domain.Unions.UnionAnnouncement> UnionAnnouncements { get; }
    IRepository<Domain.Unions.UnionDocument> UnionDocuments { get; }
    IRepository<Domain.Unions.UnionEvent> UnionEvents { get; }
    IRepository<Domain.Unions.UnionPoll> UnionPolls { get; }
    IRepository<Domain.Unions.PollOption> PollOptions { get; }
    IRepository<Domain.Unions.PollVote> PollVotes { get; }
    IRepository<Domain.Unions.Meeting> Meetings { get; }
    IRepository<Domain.Unions.MeetingAgendaItem> MeetingAgendaItems { get; }
    IRepository<Domain.Unions.MeetingAttendee> MeetingAttendees { get; }
    IRepository<Domain.Unions.MeetingMinute> MeetingMinutes { get; }
    IRepository<Domain.Unions.ActionItem> ActionItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Wraps a multi-step operation (e.g. checkout: decrement stock + create order + create payment) in a single DB transaction.</summary>
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}
