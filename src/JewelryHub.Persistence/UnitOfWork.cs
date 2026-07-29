using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Customers;
using JewelryHub.Domain.Identity;
using JewelryHub.Domain.Sellers;
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
