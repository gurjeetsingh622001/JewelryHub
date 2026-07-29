using JewelryHub.Application.Features.Wishlist.Commands.AddToWishlist;
using JewelryHub.Application.Features.Wishlist.Commands.RemoveFromWishlist;
using JewelryHub.Application.Features.Wishlist.Common;
using JewelryHub.Application.Features.Wishlist.Queries.GetWishlist;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryHub.API.Controllers;

[ApiController]
[Route("api/v1/wishlist")]
[Authorize(Roles = "Customer")]
public class WishlistController : ControllerBase
{
    private readonly IMediator _mediator;

    public WishlistController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(WishlistDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WishlistDto>> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWishlistQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("items/{productId:guid}")]
    [ProducesResponseType(typeof(WishlistDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WishlistDto>> AddItem(Guid productId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AddToWishlistCommand(productId), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveItem(Guid productId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveFromWishlistCommand(productId), cancellationToken);
        return NoContent();
    }
}
