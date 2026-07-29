using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Services;
using JewelryHub.Domain.Catalog;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Catalog.Products.Commands.UpdateProductStatus;

public record UpdateProductStatusCommand(Guid ProductId, ProductStatus NewStatus, string? Reason) : IRequest;

public class UpdateProductStatusCommandValidator : AbstractValidator<UpdateProductStatusCommand>
{
    public UpdateProductStatusCommandValidator()
    {
        RuleFor(x => x.NewStatus).IsInEnum();
        RuleFor(x => x.Reason).NotEmpty().When(x => x.NewStatus == ProductStatus.Rejected)
            .WithMessage("A reason is required when rejecting a product.");
    }
}

public class UpdateProductStatusCommandHandler : IRequestHandler<UpdateProductStatusCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public UpdateProductStatusCommandHandler(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task Handle(UpdateProductStatusCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.QueryTracking()
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        product.Status = request.NewStatus;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.NewStatus == ProductStatus.Rejected)
        {
            await _notificationService.NotifyAsync(
                product.Seller.UserId, "Product", "Listing rejected",
                $"'{product.Name}' was rejected: {request.Reason}",
                linkUrl: $"/seller/products/{product.Id}", cancellationToken: cancellationToken);
        }
    }
}
