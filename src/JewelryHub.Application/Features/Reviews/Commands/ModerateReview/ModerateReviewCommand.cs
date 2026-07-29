using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Reviews;
using MediatR;

namespace JewelryHub.Application.Features.Reviews.Commands.ModerateReview;

public record ModerateReviewCommand(Guid ReviewId, bool IsApproved) : IRequest;

public class ModerateReviewCommandHandler : IRequestHandler<ModerateReviewCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ModerateReviewCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(ModerateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(request.ReviewId, cancellationToken)
            ?? throw new NotFoundException(nameof(Review), request.ReviewId);

        review.IsApproved = request.IsApproved;
        review.IsFlagged = !request.IsApproved;

        _unitOfWork.Reviews.Update(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Note: hiding a review deliberately does NOT roll back the
        // Product/Seller rolling average — that average is a lightweight
        // denormalized signal, not an audited figure, and unwinding a
        // single rating from it accurately would need the full review
        // history, not just the current count/average pair.
    }
}
