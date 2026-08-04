using FluentValidation;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Unions;
using MediatR;

namespace JewelryHub.Application.Features.Unions.Commands.UploadDocument;

/// <summary>
/// Any active member can share a document (bylaws, financial reports,
/// meeting records) — unlike announcements/meetings/polls this isn't
/// restricted to officers. FileUrl is expected to already point at an
/// uploaded file, same convention as SubmitSellerDocumentCommand.
/// </summary>
public record UploadDocumentCommand(Guid UnionId, string Title, string FileUrl, string? Category) : IRequest<UnionDocumentDto>;

public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.FileUrl).NotEmpty().MaximumLength(500);
    }
}

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, UnionDocumentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UploadDocumentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<UnionDocumentDto> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var member = await UnionAuthorization.GetActiveMembershipAsync(_unitOfWork, _currentUser, request.UnionId, cancellationToken);

        var document = new UnionDocument
        {
            UnionId = request.UnionId,
            Title = request.Title.Trim(),
            FileUrl = request.FileUrl,
            Category = request.Category,
            UploadedByMemberId = member.Id,
        };
        await _unitOfWork.UnionDocuments.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        document.UploadedByMember = member;
        return UnionMapper.ToDto(document);
    }
}
