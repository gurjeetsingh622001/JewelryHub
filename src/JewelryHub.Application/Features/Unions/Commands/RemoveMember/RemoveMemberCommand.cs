using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Services;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Unions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Commands.RemoveMember;

public record RemoveMemberCommand(Guid MembershipId) : IRequest;

public class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public RemoveMemberCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        var membership = await _unitOfWork.UnionMembers.QueryTracking().Include(m => m.Seller).Include(m => m.Union)
            .FirstOrDefaultAsync(m => m.Id == request.MembershipId, cancellationToken)
            ?? throw new NotFoundException(nameof(UnionMember), request.MembershipId);

        await UnionAuthorization.EnsureOfficerOrAdminAsync(_unitOfWork, _currentUser, membership.UnionId, cancellationToken);

        if (membership.Role == UnionMemberRole.President)
        {
            var otherActivePresidents = await _unitOfWork.UnionMembers.Query().AnyAsync(m =>
                m.UnionId == membership.UnionId && m.Id != membership.Id &&
                m.Role == UnionMemberRole.President && m.Status == UnionMembershipStatus.Active,
                cancellationToken);

            if (!otherActivePresidents)
            {
                throw new BusinessRuleException("Cannot remove the union's only President — assign a new President first.");
            }
        }

        membership.Status = UnionMembershipStatus.Removed;
        _unitOfWork.UnionMembers.Update(membership);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyAsync(
            membership.Seller.UserId, "Union", "Your union membership was ended",
            $"You have been removed from '{membership.Union.Name}'.", cancellationToken: cancellationToken);
    }
}
