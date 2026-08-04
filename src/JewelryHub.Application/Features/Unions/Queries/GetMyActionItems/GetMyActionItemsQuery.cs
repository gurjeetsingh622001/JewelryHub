using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Queries.GetMyActionItems;

/// <summary>The member dashboard's core query — every open/in-progress action item assigned to the caller across every union they belong to.</summary>
public record GetMyActionItemsQuery : IRequest<IReadOnlyList<ActionItemDto>>;

public class GetMyActionItemsQueryHandler : IRequestHandler<GetMyActionItemsQuery, IReadOnlyList<ActionItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMyActionItemsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ActionItemDto>> Handle(GetMyActionItemsQuery request, CancellationToken cancellationToken)
    {
        var myMemberIds = await _unitOfWork.UnionMembers.Query()
            .Where(m => m.Seller.UserId == _currentUser.UserId)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        var items = await _unitOfWork.ActionItems.Query()
            .Include(ai => ai.ResponsibleMember).ThenInclude(m => m.Seller)
            .Where(ai => myMemberIds.Contains(ai.ResponsibleMemberId) && ai.Status != ActionItemStatus.Completed)
            .OrderBy(ai => ai.DueDateUtc)
            .ToListAsync(cancellationToken);

        return items.Select(ai => new ActionItemDto(
            ai.Id, ai.Description, ai.ResponsibleMemberId, ai.ResponsibleMember.Seller.BusinessName, ai.DueDateUtc, ai.Status))
            .ToList();
    }
}
