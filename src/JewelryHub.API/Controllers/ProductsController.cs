using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Catalog.Products.Commands.AdjustInventory;
using JewelryHub.Application.Features.Catalog.Products.Commands.CreateProduct;
using JewelryHub.Application.Features.Catalog.Products.Commands.UpdateProduct;
using JewelryHub.Application.Features.Catalog.Products.Commands.UpdateProductStatus;
using JewelryHub.Application.Features.Catalog.Products.Common;
using JewelryHub.Application.Features.Catalog.Products.Queries.GetProductById;
using JewelryHub.Application.Features.Catalog.Products.Queries.GetProducts;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryHub.API.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ProductListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductListItemDto>>> GetProducts(
        [FromQuery] Guid? categoryId, [FromQuery] Guid? sellerId,
        [FromQuery] MetalType? metalType, [FromQuery] PurityType? purity,
        [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice,
        [FromQuery] string? search, [FromQuery] ProductSortOption sort = ProductSortOption.Newest,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsQuery(categoryId, sellerId, metalType, purity, minPrice, maxPrice, search, sort, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductDto>> Create(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Seller,Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductRequestBody body, CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(id, body.Name, body.Description, body.GrossWeightGrams, body.NetWeightGrams,
            body.MetalRatePerGramAtListing, body.Size, body.SizeUnit, body.MakingCharges, body.MakingChargesArePercentage,
            body.WastageCharges, body.DiscountPercentage);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateProductStatusRequestBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateProductStatusCommand(id, body.NewStatus, body.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/inventory/adjust")]
    [Authorize(Roles = "Seller,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AdjustInventory(Guid id, [FromBody] AdjustInventoryRequestBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AdjustInventoryCommand(id, body.QuantityDelta, body.Reason), cancellationToken);
        return NoContent();
    }
}

public record UpdateProductRequestBody(
    string Name, string? Description, decimal GrossWeightGrams, decimal NetWeightGrams,
    decimal MetalRatePerGramAtListing, string? Size, string? SizeUnit,
    decimal MakingCharges, bool MakingChargesArePercentage, decimal WastageCharges, decimal? DiscountPercentage);

public record UpdateProductStatusRequestBody(ProductStatus NewStatus, string? Reason);

public record AdjustInventoryRequestBody(int QuantityDelta, string? Reason);
