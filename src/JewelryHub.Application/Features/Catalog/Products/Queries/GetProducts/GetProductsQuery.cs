using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Catalog.Products.Common;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Catalog.Products.Queries.GetProducts;

public enum ProductSortOption
{
    Newest = 0,
    PriceLowToHigh = 1,
    PriceHighToLow = 2,
    RatingDesc = 3,
}

public record GetProductsQuery(
    Guid? CategoryId = null,
    Guid? SellerId = null,
    MetalType? MetalType = null,
    PurityType? Purity = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? SearchTerm = null,
    ProductSortOption Sort = ProductSortOption.Newest,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<ProductListItemDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductListItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<ProductListItemDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        // The storefront only ever shows Active/OutOfStock listings — this
        // query is intentionally never used to render a seller's Draft or
        // an admin's PendingReview queue (those use dedicated queries with
        // ownership checks instead of being a filter flag here).
        var query = _unitOfWork.Products.Query()
            .Include(p => p.Seller).Include(p => p.Category).Include(p => p.Images).Include(p => p.Inventory)
            .Where(p => p.Status == ProductStatus.Active || p.Status == ProductStatus.OutOfStock);

        if (request.CategoryId is not null) query = query.Where(p => p.CategoryId == request.CategoryId);
        if (request.SellerId is not null) query = query.Where(p => p.SellerId == request.SellerId);
        if (request.MetalType is not null) query = query.Where(p => p.MetalType == request.MetalType);
        if (request.Purity is not null) query = query.Where(p => p.Purity == request.Purity);
        if (request.MinPrice is not null) query = query.Where(p => p.BasePrice >= request.MinPrice);
        if (request.MaxPrice is not null) query = query.Where(p => p.BasePrice <= request.MaxPrice);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{term}%") || EF.Functions.Like(p.Sku, $"%{term}%"));
        }

        query = request.Sort switch
        {
            ProductSortOption.PriceLowToHigh => query.OrderBy(p => p.BasePrice),
            ProductSortOption.PriceHighToLow => query.OrderByDescending(p => p.BasePrice),
            ProductSortOption.RatingDesc => query.OrderByDescending(p => p.AverageRating).ThenByDescending(p => p.ReviewCount),
            _ => query.OrderByDescending(p => p.CreatedAtUtc),
        };

        var pagedProducts = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        var items = pagedProducts.Items.Select(ProductMapper.ToListItemDto).ToList();
        return new PagedResult<ProductListItemDto>(items, pagedProducts.TotalCount, pagedProducts.PageNumber, pagedProducts.PageSize);
    }
}
