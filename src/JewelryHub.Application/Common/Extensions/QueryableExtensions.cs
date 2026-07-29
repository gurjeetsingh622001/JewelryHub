using JewelryHub.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Common.Extensions;

public static class QueryableExtensions
{
    private const int MaxPageSize = 100;

    /// <summary>Clamps page size so a client can't request an unbounded result set, then materializes count + page in two queries.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > MaxPageSize ? Math.Min(Math.Max(pageSize, 1), MaxPageSize) : pageSize;

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }
}
