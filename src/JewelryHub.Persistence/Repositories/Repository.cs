using System.Linq.Expressions;
using JewelryHub.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly JewelryHubDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(JewelryHubDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbSet.FindAsync(new object[] { id }, cancellationToken);

    // AsNoTracking by default: read-heavy paths (listings, search) never
    // pay for EF's change tracker. Handlers that need to mutate an entity
    // should GetByIdAsync it (tracked) rather than pull it from Query().
    public IQueryable<T> Query() => _dbSet.AsNoTracking();

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        _dbSet.AnyAsync(predicate, cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await _dbSet.AddAsync(entity, cancellationToken);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Remove(T entity) => _dbSet.Remove(entity);
}
