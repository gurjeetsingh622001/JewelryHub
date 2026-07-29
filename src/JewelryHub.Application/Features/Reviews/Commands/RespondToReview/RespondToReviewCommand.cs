using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Reviews;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Reviews.Commands.RespondToReview;

public record RespondToReviewCommand(Guid ReviewId, string Response) : IRequest;

public class RespondToReviewCommandValidator : AbstractValidator<RespondToReviewCommand>
{
    public RespondToReviewCommandValidator()
    {
        RuleFor(x => x.Response).NotEmpty().MaximumLength(2000);
    }
}

public class RespondToReviewCommandHandler : IRequestHandler<RespondToReviewCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RespondToReviewCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(RespondToReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _unitOfWork.Reviews.QueryTracking()
            .Include(r => r.Seller)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken)
            ?? throw new NotFoundException(nameof(Review), request.ReviewId);

        if (review.Seller.UserId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException("You can only respond to reviews left on your own listings.");
        }

        review.SellerResponse = request.Response.Trim();
        review.SellerRespondedAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
