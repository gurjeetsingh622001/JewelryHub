using JewelryHub.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using JewelryHub.Domain.Cart;
using JewelryHub.Domain.Catalog;
using JewelryHub.Domain.Customers;
using JewelryHub.Domain.Identity;
using JewelryHub.Domain.Notifications;
using JewelryHub.Domain.Orders;
using JewelryHub.Domain.Payments;
using JewelryHub.Domain.Reviews;
using JewelryHub.Domain.Sellers;
using JewelryHub.Domain.Tax;
using JewelryHub.Domain.Unions;
using JewelryHub.Domain.Wishlist;
using JewelryHub.Persistence.Repositories;

namespace JewelryHub.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly JewelryHubDbContext _context;

    private IRepository<User>? _users;
    private IRepository<Role>? _roles;
    private IRepository<RefreshToken>? _refreshTokens;
    private IRepository<Customer>? _customers;
    private IRepository<CustomerAddress>? _customerAddresses;
    private IRepository<Seller>? _sellers;
    private IRepository<SellerDocument>? _sellerDocuments;
    private IRepository<Category>? _categories;
    private IRepository<Product>? _products;
    private IRepository<Inventory>? _inventory;
    private IRepository<Cart>? _carts;
    private IRepository<Wishlist>? _wishlists;
    private IRepository<Order>? _orders;
    private IRepository<Shipment>? _shipments;
    private IRepository<Payment>? _payments;
    private IRepository<TaxRate>? _taxRates;
    private IRepository<Review>? _reviews;
    private IRepository<Notification>? _notifications;
    private IRepository<Union>? _unions;
    private IRepository<UnionMember>? _unionMembers;
    private IRepository<UnionAnnouncement>? _unionAnnouncements;
    private IRepository<UnionDocument>? _unionDocuments;
    private IRepository<UnionEvent>? _unionEvents;
    private IRepository<UnionPoll>? _unionPolls;
    private IRepository<PollOption>? _pollOptions;
    private IRepository<PollVote>? _pollVotes;
    private IRepository<Meeting>? _meetings;
    private IRepository<MeetingAgendaItem>? _meetingAgendaItems;
    private IRepository<MeetingAttendee>? _meetingAttendees;
    private IRepository<MeetingMinute>? _meetingMinutes;
    private IRepository<ActionItem>? _actionItems;

    public UnitOfWork(JewelryHubDbContext context)
    {
        _context = context;
    }

    // Lazily instantiated so a unit of work that only touches Users, say,
    // never pays for repositories it doesn't use.
    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<Role> Roles => _roles ??= new Repository<Role>(_context);
    public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);
    public IRepository<Customer> Customers => _customers ??= new Repository<Customer>(_context);
    public IRepository<CustomerAddress> CustomerAddresses => _customerAddresses ??= new Repository<CustomerAddress>(_context);
    public IRepository<Seller> Sellers => _sellers ??= new Repository<Seller>(_context);
    public IRepository<SellerDocument> SellerDocuments => _sellerDocuments ??= new Repository<SellerDocument>(_context);
    public IRepository<Category> Categories => _categories ??= new Repository<Category>(_context);
    public IRepository<Product> Products => _products ??= new Repository<Product>(_context);
    public IRepository<Inventory> Inventory => _inventory ??= new Repository<Inventory>(_context);
    public IRepository<Cart> Carts => _carts ??= new Repository<Cart>(_context);
    public IRepository<Wishlist> Wishlists => _wishlists ??= new Repository<Wishlist>(_context);
    public IRepository<Order> Orders => _orders ??= new Repository<Order>(_context);
    public IRepository<Shipment> Shipments => _shipments ??= new Repository<Shipment>(_context);
    public IRepository<Payment> Payments => _payments ??= new Repository<Payment>(_context);
    public IRepository<TaxRate> TaxRates => _taxRates ??= new Repository<TaxRate>(_context);
    public IRepository<Review> Reviews => _reviews ??= new Repository<Review>(_context);
    public IRepository<Notification> Notifications => _notifications ??= new Repository<Notification>(_context);
    public IRepository<Union> Unions => _unions ??= new Repository<Union>(_context);
    public IRepository<UnionMember> UnionMembers => _unionMembers ??= new Repository<UnionMember>(_context);
    public IRepository<UnionAnnouncement> UnionAnnouncements => _unionAnnouncements ??= new Repository<UnionAnnouncement>(_context);
    public IRepository<UnionDocument> UnionDocuments => _unionDocuments ??= new Repository<UnionDocument>(_context);
    public IRepository<UnionEvent> UnionEvents => _unionEvents ??= new Repository<UnionEvent>(_context);
    public IRepository<UnionPoll> UnionPolls => _unionPolls ??= new Repository<UnionPoll>(_context);
    public IRepository<PollOption> PollOptions => _pollOptions ??= new Repository<PollOption>(_context);
    public IRepository<PollVote> PollVotes => _pollVotes ??= new Repository<PollVote>(_context);
    public IRepository<Meeting> Meetings => _meetings ??= new Repository<Meeting>(_context);
    public IRepository<MeetingAgendaItem> MeetingAgendaItems => _meetingAgendaItems ??= new Repository<MeetingAgendaItem>(_context);
    public IRepository<MeetingAttendee> MeetingAttendees => _meetingAttendees ??= new Repository<MeetingAttendee>(_context);
    public IRepository<MeetingMinute> MeetingMinutes => _meetingMinutes ??= new Repository<MeetingMinute>(_context);
    public IRepository<ActionItem> ActionItems => _actionItems ??= new Repository<ActionItem>(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        // Uses the DbContext's execution strategy so this composes safely
        // with EnableRetryOnFailure (a bare BeginTransaction would throw
        // when retry-on-failure is also enabled).
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            await operation();
            await transaction.CommitAsync(cancellationToken);
        });
    }
}
