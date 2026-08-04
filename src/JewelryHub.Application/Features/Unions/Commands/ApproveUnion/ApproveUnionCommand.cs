using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Services;
using JewelryHub.Domain.Unions;
using MediatR;

namespace JewelryHub.Application.Features.Unions.Commands.ApproveUnion;

public record ApproveUnionCommand(Guid UnionId) : IRequest;

public class ApproveUnionCommandHandler : IRequestHandler<ApproveUnionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public ApproveUnionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task Handle(ApproveUnionCommand request, CancellationToken cancellationToken)
    {
        var union = await _unitOfWork.Unions.GetByIdAsync(request.UnionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Union), request.UnionId);

        if (union.IsApprovedByAdmin)
        {
            throw new BusinessRuleException("This union is already approved.");
        }

        union.IsApprovedByAdmin = true;
        union.ApprovedByAdminId = _currentUser.UserId;
        union.ApprovedAtUtc = DateTime.UtcNow;

        _unitOfWork.Unions.Update(union);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var creator = await _unitOfWork.Sellers.GetByIdAsync(union.CreatedBySellerId, cancellationToken);
        if (creator is not null)
        {
            await _notificationService.NotifyAsync(
                creator.UserId, "Union", "Your union has been approved",
                $"'{union.Name}' is now approved and visible on JewelryHub.", cancellationToken: cancellationToken);
        }
    }
}
