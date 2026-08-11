using FluentValidation;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Uploads.Common;
using MediatR;

namespace JewelryHub.Application.Features.Uploads.Commands.UploadFile;

/// <summary>Which storage sub-folder (and therefore which extension whitelist) a file belongs to.</summary>
public enum UploadKind
{
    ProductImage,
    SellerDocument,
}

public record UploadFileCommand(byte[] Content, string FileName, UploadKind Kind) : IRequest<UploadedFileDto>;

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] DocumentExtensions = [".jpg", ".jpeg", ".png", ".webp", ".pdf"];
    private const long MaxSizeBytes = 5 * 1024 * 1024; // 5 MB — generous for a product photo or a scanned document, small enough to keep local disk usage sane

    public UploadFileCommandValidator()
    {
        RuleFor(x => x.Content)
            .Must(c => c.Length > 0).WithMessage("The file is empty.")
            .Must(c => c.Length <= MaxSizeBytes).WithMessage("The file must be 5 MB or smaller.");

        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);

        RuleFor(x => x)
            .Must(x => ImageExtensions.Contains(Path.GetExtension(x.FileName).ToLowerInvariant()))
            .When(x => x.Kind == UploadKind.ProductImage)
            .WithMessage("Product images must be JPG, PNG, or WEBP.");

        RuleFor(x => x)
            .Must(x => DocumentExtensions.Contains(Path.GetExtension(x.FileName).ToLowerInvariant()))
            .When(x => x.Kind == UploadKind.SellerDocument)
            .WithMessage("Documents must be JPG, PNG, WEBP, or PDF.");
    }
}

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, UploadedFileDto>
{
    private readonly IFileStorageService _fileStorage;

    public UploadFileCommandHandler(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    public async Task<UploadedFileDto> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var folder = request.Kind == UploadKind.ProductImage ? "products" : "documents";
        var url = await _fileStorage.SaveAsync(request.Content, request.FileName, folder, cancellationToken);
        return new UploadedFileDto(url, request.FileName, request.Content.Length);
    }
}
