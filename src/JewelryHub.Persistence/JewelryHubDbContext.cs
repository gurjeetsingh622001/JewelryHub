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
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Persistence;

/// <summary>
/// The audit/soft-delete interceptor is attached via
/// DbContextOptionsBuilder.AddInterceptors (see
/// ServiceCollectionExtensions.AddPersistence), not injected into this
/// constructor — that keeps the standard `DbContextOptions<T>`-only
/// constructor EF Core's tooling (migrations, scaffolding) expects.
/// </summary>
public class JewelryHubDbContext : DbContext
{
    public JewelryHubDbContext(DbContextOptions<JewelryHubDbContext> options)
        : base(options)
    {
    }

    // --- Identity ---
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // --- Customers / Sellers ---
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<Seller> Sellers => Set<Seller>();
    public DbSet<SellerDocument> SellerDocuments => Set<SellerDocument>();

    // --- Catalog ---
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductGemstone> ProductGemstones => Set<ProductGemstone>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductCertificate> ProductCertificates => Set<ProductCertificate>();
    public DbSet<Inventory> Inventory => Set<Inventory>();

    // --- Cart / Wishlist ---
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    // --- Orders / Payments / Tax ---
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderTax> OrderTaxes => Set<OrderTax>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();

    // --- Reviews / Notifications ---
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // --- Unions ---
    public DbSet<Union> Unions => Set<Union>();
    public DbSet<UnionMember> UnionMembers => Set<UnionMember>();
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<MeetingAgendaItem> MeetingAgendaItems => Set<MeetingAgendaItem>();
    public DbSet<MeetingAttendee> MeetingAttendees => Set<MeetingAttendee>();
    public DbSet<MeetingMinute> MeetingMinutes => Set<MeetingMinute>();
    public DbSet<ActionItem> ActionItems => Set<ActionItem>();
    public DbSet<UnionAnnouncement> UnionAnnouncements => Set<UnionAnnouncement>();
    public DbSet<UnionDocument> UnionDocuments => Set<UnionDocument>();
    public DbSet<UnionEvent> UnionEvents => Set<UnionEvent>();
    public DbSet<UnionPoll> UnionPolls => Set<UnionPoll>();
    public DbSet<PollOption> PollOptions => Set<PollOption>();
    public DbSet<PollVote> PollVotes => Set<PollVote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Picks up every IEntityTypeConfiguration<T> in this assembly —
        // new entities only need a config class added, never a manual
        // registration here.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JewelryHubDbContext).Assembly);

        // Global default: every decimal not given explicit precision by a
        // configuration still gets a sane one instead of SQL Server's
        // silently-truncating default.
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(4);
        }
    }
}
