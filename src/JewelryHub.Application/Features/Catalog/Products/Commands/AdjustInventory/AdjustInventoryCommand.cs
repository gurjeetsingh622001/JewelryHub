using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Catalog;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Catalog.Products.Commands.AdjustInventory;

/// <summary>QuantityDelta can be negative (correcting an overcount, writing off damaged stock) as well as positive (restocking).</summary>
public record AdjustInventoryCommand(Guid ProductId, int QuantityDelta, string? Reason) : IRequest;

public class AdjustInventoryCommandValidator : AbstractValidator<AdjustInventoryCommand>
{
    public AdjustInventoryCommandValidator()
    {
        RuleFor(x => x.QuantityDelta).NotEqual(0);
    }
}

public class AdjustInventoryCommandHandler : IRequestHandler<AdjustInventoryCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AdjustInventoryCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(AdjustInventoryCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.Query()
            .Include(p => p.Seller).Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        if (product.Seller.UserId != _currentUser.UserId && !_currentUser.IsInRole("Admin"))
        {
            throw new ForbiddenAccessException("You can only adjust stock for your own products.");
        }

        if (product.Inventory is null)
        {
            throw new BusinessRuleException("This product does not track inventory.");
        }

        var newQuantity = product.Inventory.QuantityAvailable + request.QuantityDelta;
        if (newQuantity < 0)
        {
            throw new BusinessRuleException("This adjustment would take stock below zero.", nameof(request.QuantityDelta));
        }

        product.Inventory.QuantityAvailable = newQuantity;
        if (newQuantity == 0 && product.Status == ProductStatus.Active)
        {
            product.Status = ProductStatus.OutOfStock;
        }
        else if (newQuantity > 0 && product.Status == ProductStatus.OutOfStock)
        {
            product.Status = ProductStatus.Active;
        }

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
