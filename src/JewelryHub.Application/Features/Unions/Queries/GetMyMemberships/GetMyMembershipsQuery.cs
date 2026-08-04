using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Queries.GetMyMemberships;

/// <summary>Every union the authenticated seller belongs to (or is awaiting approval for) — not paged, since one seller belonging to dozens of unions isn't a realistic scale.</summary>
public record GetMyMembershipsQuery : IRequest<IReadOnlyList<UnionMemberDto>>;

public class GetMyMembershipsQueryHandler : IRequestHandler<GetMyMembershipsQuery, IReadOnlyList<UnionMemberDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMyMembershipsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<UnionMemberDto>> Handle(GetMyMembershipsQuery request, CancellationToken cancellationToken)
    {
        var memberships = await _unitOfWork.UnionMembers.Query().Include(m => m.Seller)
            .Where(m => m.Seller.UserId == _currentUser.UserId)
            .OrderByDescending(m => m.JoinedAtUtc)
            .ToListAsync(cancellationToken);

        return memberships.Select(UnionMapper.ToDto).ToList();
    }
}
