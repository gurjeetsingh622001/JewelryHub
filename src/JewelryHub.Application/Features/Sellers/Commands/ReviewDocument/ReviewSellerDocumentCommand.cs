using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Sellers;
using MediatR;

namespace JewelryHub.Application.Features.Sellers.Commands.ReviewDocument;

public record ReviewSellerDocumentCommand(Guid DocumentId, DocumentVerificationStatus Decision, string? ReviewerNote) : IRequest;

public class ReviewSellerDocumentCommandValidator : AbstractValidator<ReviewSellerDocumentCommand>
{
    public ReviewSellerDocumentCommandValidator()
    {
        RuleFor(x => x.Decision).Must(d => d is DocumentVerificationStatus.Verified or DocumentVerificationStatus.Rejected)
            .WithMessage("Decision must be either Verified or Rejected.");
        RuleFor(x => x.ReviewerNote).NotEmpty().When(x => x.Decision == DocumentVerificationStatus.Rejected)
            .WithMessage("A note is required when rejecting a document.");
    }
}

public class ReviewSellerDocumentCommandHandler : IRequestHandler<ReviewSellerDocumentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ReviewSellerDocumentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(ReviewSellerDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.SellerDocuments.GetByIdAsync(request.DocumentId, cancellationToken)
            ?? throw new NotFoundException(nameof(SellerDocument), request.DocumentId);

        document.Status = request.Decision;
        document.ReviewerNote = request.ReviewerNote;
        document.ReviewedByAdminId = _currentUser.UserId;
        document.ReviewedAtUtc = DateTime.UtcNow;

        _unitOfWork.SellerDocuments.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
