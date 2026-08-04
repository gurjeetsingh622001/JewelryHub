using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Queries.GetPendingMemberships;

/// <summary>An officer/Admin's review queue of sellers asking to join a specific union.</summary>
public record GetPendingMembershipsQuery(Guid UnionId, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<UnionMemberDto>>;

public class GetPendingMembershipsQueryHandler : IRequestHandler<GetPendingMembershipsQuery, PagedResult<UnionMemberDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetPendingMembershipsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<UnionMemberDto>> Handle(GetPendingMembershipsQuery request, CancellationToken cancellationToken)
    {
        await UnionAuthorization.EnsureOfficerOrAdminAsync(_unitOfWork, _currentUser, request.UnionId, cancellationToken);

        var query = _unitOfWork.UnionMembers.Query().Include(m => m.Seller)
            .Where(m => m.UnionId == request.UnionId && m.Status == UnionMembershipStatus.PendingApproval)
            .OrderBy(m => m.JoinedAtUtc);

        var paged = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = paged.Items.Select(UnionMapper.ToDto).ToList();
        return new PagedResult<UnionMemberDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
