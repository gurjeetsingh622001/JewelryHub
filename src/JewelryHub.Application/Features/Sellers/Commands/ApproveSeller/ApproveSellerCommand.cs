using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Sellers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Sellers.Commands.ApproveSeller;

public record ApproveSellerCommand(Guid SellerId) : IRequest;

public class ApproveSellerCommandHandler : IRequestHandler<ApproveSellerCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ApproveSellerCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(ApproveSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await _unitOfWork.Sellers.Query()
            .Include(s => s.Documents)
            .FirstOrDefaultAsync(s => s.Id == request.SellerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Seller), request.SellerId);

        if (seller.Documents.Count == 0)
        {
            throw new BusinessRuleException("Cannot approve a seller who has not submitted any KYC documents.");
        }

        var unverified = seller.Documents.Where(d => d.Status != DocumentVerificationStatus.Verified).ToList();
        if (unverified.Count != 0)
        {
            throw new BusinessRuleException(
                $"{unverified.Count} submitted document(s) are not yet marked Verified. Review every document before approving the seller.");
        }

        seller.VerificationStatus = SellerVerificationStatus.Approved;
        seller.RejectionReason = null;
        seller.VerifiedAtUtc = DateTime.UtcNow;
        seller.VerifiedByAdminId = _currentUser.UserId;

        _unitOfWork.Sellers.Update(seller);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO once Notifications exist: notify the seller their account is live.
    }
}
