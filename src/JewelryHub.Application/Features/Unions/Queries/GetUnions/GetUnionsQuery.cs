using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Queries.GetUnions;

/// <summary>Public directory of admin-approved unions. Unapproved unions never show up here — see GetPendingUnionsQuery for the admin review queue.</summary>
public record GetUnionsQuery(string? Search, string? City, string? State, int PageNumber = 1, int PageSize = 20)
    : IRequest<PagedResult<UnionDto>>;

public class GetUnionsQueryHandler : IRequestHandler<GetUnionsQuery, PagedResult<UnionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUnionsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<UnionDto>> Handle(GetUnionsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Unions.Query().Include(u => u.CreatedBySeller).Where(u => u.IsApprovedByAdmin);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(u => u.Name.Contains(request.Search));
        }
        if (!string.IsNullOrWhiteSpace(request.City))
        {
            query = query.Where(u => u.City == request.City);
        }
        if (!string.IsNullOrWhiteSpace(request.State))
        {
            query = query.Where(u => u.State == request.State);
        }

        query = query.OrderBy(u => u.Name);

        var paged = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = await MapWithMemberCountsAsync(_unitOfWork, paged.Items, cancellationToken);
        return new PagedResult<UnionDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }

    /// <summary>
    /// Batches the active-member count for a page of unions into a single
    /// GROUP BY instead of one COUNT query per union.
    /// </summary>
    internal static async Task<List<UnionDto>> MapWithMemberCountsAsync(
        IUnitOfWork unitOfWork, IReadOnlyList<Domain.Unions.Union> unions, CancellationToken cancellationToken)
    {
        var unionIds = unions.Select(u => u.Id).ToList();
        var counts = await unitOfWork.UnionMembers.Query()
            .Where(m => unionIds.Contains(m.UnionId) && m.Status == UnionMembershipStatus.Active)
            .GroupBy(m => m.UnionId)
            .Select(g => new { UnionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UnionId, x => x.Count, cancellationToken);

        return unions.Select(u => UnionMapper.ToDto(u, counts.GetValueOrDefault(u.Id))).ToList();
    }
}
