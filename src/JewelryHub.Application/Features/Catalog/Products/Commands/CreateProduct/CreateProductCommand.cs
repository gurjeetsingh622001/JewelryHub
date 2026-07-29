using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Utilities;
using JewelryHub.Application.Features.Catalog.Products.Common;
using JewelryHub.Domain.Catalog;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Sellers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Catalog.Products.Commands.CreateProduct;

public record CreateProductCommand(
    Guid CategoryId,
    string Name,
    string Sku,
    string? Description,
    ProductType ProductType,
    MetalType MetalType,
    PurityType Purity,
    decimal GrossWeightGrams,
    decimal NetWeightGrams,
    decimal MetalRatePerGramAtListing,
    string? Size,
    string? SizeUnit,
    decimal MakingCharges,
    bool MakingChargesArePercentage,
    decimal WastageCharges,
    decimal? DiscountPercentage,
    bool IsHallmarked,
    string? HallmarkUniqueId,
    string? CertificationAuthority,
    int InitialQuantity,
    IReadOnlyList<CreateProductGemstoneRequest> Gemstones,
    IReadOnlyList<CreateProductImageRequest> Images) : IRequest<ProductDto>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(80);
        RuleFor(x => x.GrossWeightGrams).GreaterThan(0);
        RuleFor(x => x.NetWeightGrams).GreaterThan(0).LessThanOrEqualTo(x => x.GrossWeightGrams)
            .WithMessage("Net weight cannot exceed gross weight.");
        RuleFor(x => x.MetalRatePerGramAtListing).GreaterThan(0);
        RuleFor(x => x.MakingCharges).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MakingCharges).LessThanOrEqualTo(100).When(x => x.MakingChargesArePercentage)
            .WithMessage("Making charges expressed as a percentage cannot exceed 100%.");
        RuleFor(x => x.WastageCharges).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DiscountPercentage).InclusiveBetween(0, 100).When(x => x.DiscountPercentage.HasValue);
        RuleFor(x => x.InitialQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Images).Must(images => images.Count == 0 || images.Count(i => i.IsPrimary) <= 1)
            .WithMessage("Only one image can be marked as primary.");
        RuleForEach(x => x.Gemstones).ChildRules(gemstone =>
        {
            gemstone.RuleFor(g => g.GemstoneType).NotEmpty().MaximumLength(100);
            gemstone.RuleFor(g => g.WeightCarats).GreaterThan(0);
            gemstone.RuleFor(g => g.Quantity).GreaterThan(0);
            gemstone.RuleFor(g => g.Value).GreaterThanOrEqualTo(0);
        });
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var seller = await _unitOfWork.Sellers.Query()
            .FirstOrDefaultAsync(s => s.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new BusinessRuleException("No seller profile is associated with this account.");

        if (seller.VerificationStatus != SellerVerificationStatus.Approved)
        {
            throw new BusinessRuleException("Your seller account must be approved by an administrator before you can list products.");
        }

        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        if (await _unitOfWork.Products.AnyAsync(p => p.SellerId == seller.Id && p.Sku == request.Sku, cancellationToken))
        {
            throw new BusinessRuleException("You already have a product with this SKU.", nameof(request.Sku));
        }

        // --- Pricing breakdown, computed server-side; never trust a client-supplied total ---
        var gemstoneValue = request.Gemstones.Sum(g => g.Value * g.Quantity);
        var metalValue = request.NetWeightGrams * request.MetalRatePerGramAtListing;
        var makingChargesAmount = request.MakingChargesArePercentage
            ? Math.Round(metalValue * request.MakingCharges / 100m, 2)
            : request.MakingCharges;
        var basePrice = metalValue + makingChargesAmount + gemstoneValue + request.WastageCharges;

        var product = new Product
        {
            SellerId = seller.Id,
            CategoryId = category.Id,
            Name = request.Name.Trim(),
            Slug = $"{SlugGenerator.FromName(request.Name)}-{Guid.NewGuid().ToString("N")[..8]}",
            Sku = request.Sku.Trim(),
            Description = request.Description?.Trim(),
            ProductType = request.ProductType,
            Status = ProductStatus.Active,
            MetalType = request.MetalType,
            Purity = request.Purity,
            GrossWeightGrams = request.GrossWeightGrams,
            NetWeightGrams = request.NetWeightGrams,
            MetalRatePerGramAtListing = request.MetalRatePerGramAtListing,
            Size = request.Size,
            SizeUnit = request.SizeUnit,
            MetalValue = metalValue,
            MakingCharges = makingChargesAmount,
            MakingChargesArePercentage = request.MakingChargesArePercentage,
            GemstoneValue = gemstoneValue,
            WastageCharges = request.WastageCharges,
            BasePrice = basePrice,
            DiscountPercentage = request.DiscountPercentage,
            IsHallmarked = request.IsHallmarked,
            HallmarkUniqueId = request.HallmarkUniqueId,
            CertificationAuthority = request.CertificationAuthority,
        };

        foreach (var g in request.Gemstones)
        {
            product.Gemstones.Add(new ProductGemstone
            {
                GemstoneType = g.GemstoneType, WeightCarats = g.WeightCarats,
                ClarityGrade = g.ClarityGrade, ColorGrade = g.ColorGrade, CutGrade = g.CutGrade,
                Quantity = g.Quantity, Value = g.Value,
            });
        }

        foreach (var img in request.Images)
        {
            product.Images.Add(new ProductImage { Url = img.Url, AltText = img.AltText, DisplayOrder = img.DisplayOrder, IsPrimary = img.IsPrimary });
        }

        product.Inventory = new Inventory { ProductId = product.Id, QuantityAvailable = request.InitialQuantity, TrackInventory = true };

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        product.Seller = seller;
        product.Category = category;
        return ProductMapper.ToDto(product);
    }
}
