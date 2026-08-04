using JewelryHub.Domain.Unions;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Common;

public static class PollMapper
{
    /// <summary>Caller must have loaded CreatedByMember.Seller and Options.Votes (.Include(p => p.CreatedByMember).ThenInclude(m => m.Seller).Include(p => p.Options).ThenInclude(o => o.Votes)).</summary>
    public static IQueryable<UnionPoll> WithFullDetails(this IQueryable<UnionPoll> query) => query
        .Include(p => p.CreatedByMember).ThenInclude(m => m.Seller)
        .Include(p => p.Options).ThenInclude(o => o.Votes);

    public static PollDto ToDto(UnionPoll p)
    {
        var options = p.Options.OrderBy(o => o.DisplayOrder)
            .Select(o => new PollOptionResultDto(o.Id, o.Text, o.DisplayOrder, o.Votes.Count))
            .ToList();

        return new PollDto(
            p.Id, p.UnionId, p.Question, p.AllowMultipleSelections, p.Status,
            p.CreatedByMemberId, p.CreatedByMember.Seller.BusinessName, p.ClosesAtUtc, p.CreatedAtUtc,
            options, options.Sum(o => o.VoteCount));
    }

    public static PollSummaryDto ToSummaryDto(UnionPoll p) => new(p.Id, p.UnionId, p.Question, p.Status, p.ClosesAtUtc);
}
