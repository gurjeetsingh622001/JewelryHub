using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Queries.GetUnionMembers;

/// <summary>Active roster of a union, ordered senior-officer-first. For the pending-approval queue, see GetPendingMembershipsQuery.</summary>
public record GetUnionMembersQuery(Guid UnionId, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<UnionMemberDto>>;

public class GetUnionMembersQueryHandler : IRequestHandler<GetUnionMembersQuery, PagedResult<UnionMemberDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUnionMembersQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<UnionMemberDto>> Handle(GetUnionMembersQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.UnionMembers.Query().Include(m => m.Seller)
            .Where(m => m.UnionId == request.UnionId && m.Status == UnionMembershipStatus.Active)
            .OrderByDescending(m => m.Role).ThenBy(m => m.Seller.BusinessName);

        var paged = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = paged.Items.Select(UnionMapper.ToDto).ToList();
        return new PagedResult<UnionMemberDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
