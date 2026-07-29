using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Services;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Sellers;
using MediatR;

namespace JewelryHub.Application.Features.Sellers.Commands.RejectSeller;

public record RejectSellerCommand(Guid SellerId, string Reason) : IRequest;

public class RejectSellerCommandValidator : AbstractValidator<RejectSellerCommand>
{
    public RejectSellerCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

public class RejectSellerCommandHandler : IRequestHandler<RejectSellerCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public RejectSellerCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task Handle(RejectSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await _unitOfWork.Sellers.GetByIdAsync(request.SellerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Seller), request.SellerId);

        seller.VerificationStatus = SellerVerificationStatus.Rejected;
        seller.RejectionReason = request.Reason;
        seller.VerifiedAtUtc = null;
        seller.VerifiedByAdminId = _currentUser.UserId;

        _unitOfWork.Sellers.Update(seller);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyAsync(
            seller.UserId, "Seller", "Your seller application needs attention",
            $"Your application was not approved: {request.Reason}",
            cancellationToken: cancellationToken);
    }
}
