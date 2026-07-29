using JewelryHub.Application.Features.Cart.Commands.AddToCart;
using JewelryHub.Application.Features.Cart.Commands.ClearCart;
using JewelryHub.Application.Features.Cart.Commands.RemoveFromCart;
using JewelryHub.Application.Features.Cart.Commands.UpdateCartItemQuantity;
using JewelryHub.Application.Features.Cart.Common;
using JewelryHub.Application.Features.Cart.Queries.GetCart;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryHub.API.Controllers;

[ApiController]
[Route("api/v1/cart")]
[Authorize(Roles = "Customer")]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartDto>> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCartQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartDto>> AddItem(AddToCartCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("items/{productId:guid}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartDto>> UpdateItemQuantity(Guid productId, [FromBody] UpdateQuantityRequestBody body, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateCartItemQuantityCommand(productId, body.Quantity), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartDto>> RemoveItem(Guid productId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RemoveFromCartCommand(productId), cancellationToken);
        return Ok(result);
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        await _mediator.Send(new ClearCartCommand(), cancellationToken);
        return NoContent();
    }
}

public record UpdateQuantityRequestBody(int Quantity);
