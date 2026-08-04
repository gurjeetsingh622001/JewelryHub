using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Services;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Unions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Commands.ReviewMembership;

/// <summary>An officer (President/VicePresident/Secretary) or Admin approves or rejects a pending join request.</summary>
public record ReviewMembershipCommand(Guid MembershipId, bool Approve) : IRequest;

public class ReviewMembershipCommandHandler : IRequestHandler<ReviewMembershipCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public ReviewMembershipCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task Handle(ReviewMembershipCommand request, CancellationToken cancellationToken)
    {
        var membership = await _unitOfWork.UnionMembers.QueryTracking().Include(m => m.Seller).Include(m => m.Union)
            .FirstOrDefaultAsync(m => m.Id == request.MembershipId, cancellationToken)
            ?? throw new NotFoundException(nameof(UnionMember), request.MembershipId);

        await UnionAuthorization.EnsureOfficerOrAdminAsync(_unitOfWork, _currentUser, membership.UnionId, cancellationToken);

        if (membership.Status != UnionMembershipStatus.PendingApproval)
        {
            throw new BusinessRuleException("This membership request has already been reviewed.");
        }

        membership.Status = request.Approve ? UnionMembershipStatus.Active : UnionMembershipStatus.Removed;
        _unitOfWork.UnionMembers.Update(membership);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyAsync(
            membership.Seller.UserId, "Union",
            request.Approve ? "Your union membership was approved" : "Your union membership request was declined",
            request.Approve
                ? $"You are now a member of '{membership.Union.Name}'."
                : $"Your request to join '{membership.Union.Name}' was declined.",
            cancellationToken: cancellationToken);
    }
}
