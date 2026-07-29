using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Sellers.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Sellers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Sellers.Commands.SubmitDocument;

/// <summary>
/// FileUrl is expected to already point at the uploaded file (Azure Blob
/// Storage) — this command only records the metadata and puts it in the
/// review queue. The actual byte upload happens via a separate
/// pre-signed-URL flow (Infrastructure's file storage service, not built
/// yet), so this handler never touches file contents directly.
/// </summary>
public record SubmitSellerDocumentCommand(string DocumentType, string FileUrl, string? FileName) : IRequest<SellerDocumentDto>;

public class SubmitSellerDocumentCommandValidator : AbstractValidator<SubmitSellerDocumentCommand>
{
    public SubmitSellerDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FileUrl).NotEmpty().MaximumLength(500);
    }
}

public class SubmitSellerDocumentCommandHandler : IRequestHandler<SubmitSellerDocumentCommand, SellerDocumentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public SubmitSellerDocumentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<SellerDocumentDto> Handle(SubmitSellerDocumentCommand request, CancellationToken cancellationToken)
    {
        var seller = await _unitOfWork.Sellers.Query().FirstOrDefaultAsync(s => s.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new BusinessRuleException("No seller profile is associated with this account.");

        if (seller.VerificationStatus is SellerVerificationStatus.Suspended)
        {
            throw new BusinessRuleException("This seller account is suspended and cannot submit new documents.");
        }

        var document = new SellerDocument
        {
            SellerId = seller.Id,
            DocumentType = request.DocumentType.Trim(),
            FileUrl = request.FileUrl,
            FileName = request.FileName,
            Status = DocumentVerificationStatus.Pending,
        };

        await _unitOfWork.SellerDocuments.AddAsync(document, cancellationToken);

        // Submitting a fresh document while previously rejected re-opens
        // the seller for admin review rather than leaving them stuck.
        if (seller.VerificationStatus == SellerVerificationStatus.Rejected)
        {
            seller.VerificationStatus = SellerVerificationStatus.UnderReview;
            seller.RejectionReason = null;
            _unitOfWork.Sellers.Update(seller);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SellerDocumentDto(document.Id, document.DocumentType, document.FileUrl, document.FileName,
            document.Status, document.ReviewerNote, document.CreatedAtUtc);
    }
}
