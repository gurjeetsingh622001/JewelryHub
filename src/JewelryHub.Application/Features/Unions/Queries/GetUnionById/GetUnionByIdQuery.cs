using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Unions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Queries.GetUnionById;

public record GetUnionByIdQuery(Guid UnionId) : IRequest<UnionDto>;

public class GetUnionByIdQueryHandler : IRequestHandler<GetUnionByIdQuery, UnionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetUnionByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<UnionDto> Handle(GetUnionByIdQuery request, CancellationToken cancellationToken)
    {
        var union = await _unitOfWork.Unions.Query().Include(u => u.CreatedBySeller)
            .FirstOrDefaultAsync(u => u.Id == request.UnionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Union), request.UnionId);

        // An unapproved union isn't public yet — only its founder or an
        // Admin can look it up by id; everyone else gets a 404 as if it
        // doesn't exist, rather than leaking that it's pending review.
        if (!union.IsApprovedByAdmin && !_currentUser.IsInRole("Admin") && union.CreatedBySeller.UserId != _currentUser.UserId)
        {
            throw new NotFoundException(nameof(Union), request.UnionId);
        }

        var activeMemberCount = await _unitOfWork.UnionMembers.Query()
            .CountAsync(m => m.UnionId == request.UnionId && m.Status == UnionMembershipStatus.Active, cancellationToken);

        return UnionMapper.ToDto(union, activeMemberCount);
    }
}
