using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Catalog;
using JewelryHub.Domain.Enums;
using MediatR;

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

    public UpdateProductStatusCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(UpdateProductStatusCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        product.Status = request.NewStatus;
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO once the Notifications vertical slice exists: notify the
        // seller of the status change (and Reason, if rejected).
    }
}
