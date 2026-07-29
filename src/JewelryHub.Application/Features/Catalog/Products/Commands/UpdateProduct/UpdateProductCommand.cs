using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Catalog.Products.Common;
using JewelryHub.Domain.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Catalog.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal GrossWeightGrams,
    decimal NetWeightGrams,
    decimal MetalRatePerGramAtListing,
    string? Size,
    string? SizeUnit,
    decimal MakingCharges,
    bool MakingChargesArePercentage,
    decimal WastageCharges,
    decimal? DiscountPercentage) : IRequest<ProductDto>;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.GrossWeightGrams).GreaterThan(0);
        RuleFor(x => x.NetWeightGrams).GreaterThan(0).LessThanOrEqualTo(x => x.GrossWeightGrams);
        RuleFor(x => x.MetalRatePerGramAtListing).GreaterThan(0);
        RuleFor(x => x.MakingCharges).GreaterThanOrEqualTo(0);
        RuleFor(x => x.WastageCharges).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DiscountPercentage).InclusiveBetween(0, 100).When(x => x.DiscountPercentage.HasValue);
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.Query()
            .Include(p => p.Seller).Include(p => p.Category)
            .Include(p => p.Images).Include(p => p.Gemstones).Include(p => p.Certificates).Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        if (product.Seller.UserId != _currentUser.UserId && !_currentUser.IsInRole("Admin"))
        {
            throw new ForbiddenAccessException("You can only edit your own products.");
        }

        var gemstoneValue = product.Gemstones.Sum(g => g.Value * g.Quantity);
        var metalValue = request.NetWeightGrams * request.MetalRatePerGramAtListing;
        var makingChargesAmount = request.MakingChargesArePercentage
            ? Math.Round(metalValue * request.MakingCharges / 100m, 2)
            : request.MakingCharges;

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.GrossWeightGrams = request.GrossWeightGrams;
        product.NetWeightGrams = request.NetWeightGrams;
        product.MetalRatePerGramAtListing = request.MetalRatePerGramAtListing;
        product.Size = request.Size;
        product.SizeUnit = request.SizeUnit;
        product.MetalValue = metalValue;
        product.MakingCharges = makingChargesAmount;
        product.MakingChargesArePercentage = request.MakingChargesArePercentage;
        product.WastageCharges = request.WastageCharges;
        product.BasePrice = metalValue + makingChargesAmount + gemstoneValue + request.WastageCharges;
        product.DiscountPercentage = request.DiscountPercentage;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductMapper.ToDto(product);
    }
}
