using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Cart;
using JewelryHub.Domain.Catalog;
using JewelryHub.Domain.Customers;
using JewelryHub.Domain.Identity;
using JewelryHub.Domain.Orders;
using JewelryHub.Domain.Payments;
using JewelryHub.Domain.Reviews;
using JewelryHub.Domain.Sellers;
using JewelryHub.Domain.Tax;
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
    private IRepository<Seller>? _sellers;
    private IRepository<SellerDocument>? _sellerDocuments;
    private IRepository<Category>? _categories;
    private IRepository<Product>? _products;
    private IRepository<Inventory>? _inventory;
    private IRepository<Cart>? _carts;
    private IRepository<Wishlist>? _wishlists;
    private IRepository<Order>? _orders;
    private IRepository<Payment>? _payments;
    private IRepository<TaxRate>? _taxRates;
    private IRepository<Review>? _reviews;

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
    public IRepository<Seller> Sellers => _sellers ??= new Repository<Seller>(_context);
    public IRepository<SellerDocument> SellerDocuments => _sellerDocuments ??= new Repository<SellerDocument>(_context);
    public IRepository<Category> Categories => _categories ??= new Repository<Category>(_context);
    public IRepository<Product> Products => _products ??= new Repository<Product>(_context);
    public IRepository<Inventory> Inventory => _inventory ??= new Repository<Inventory>(_context);
    public IRepository<Cart> Carts => _carts ??= new Repository<Cart>(_context);
    public IRepository<Wishlist> Wishlists => _wishlists ??= new Repository<Wishlist>(_context);
    public IRepository<Order> Orders => _orders ??= new Repository<Order>(_context);
    public IRepository<Payment> Payments => _payments ??= new Repository<Payment>(_context);
    public IRepository<TaxRate> TaxRates => _taxRates ??= new Repository<TaxRate>(_context);
    public IRepository<Review> Reviews => _reviews ??= new Repository<Review>(_context);

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
