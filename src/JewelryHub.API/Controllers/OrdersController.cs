using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Orders.Commands.CancelOrder;
using JewelryHub.Application.Features.Orders.Commands.ConfirmPayment;
using JewelryHub.Application.Features.Orders.Commands.CreateOrder;
using JewelryHub.Application.Features.Orders.Commands.UpdateOrderItemStatus;
using JewelryHub.Application.Features.Orders.Common;
using JewelryHub.Application.Features.Orders.Queries.GetMyOrders;
using JewelryHub.Application.Features.Orders.Queries.GetOrderById;
using JewelryHub.Application.Features.Orders.Queries.GetSellerOrderItems;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryHub.API.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<OrderDto>> Checkout(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(PagedResult<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetMyOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetMyOrdersQuery(pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOrderRequestBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelOrderCommand(id, body.Reason), cancellationToken);
        return NoContent();
    }

    /// <summary>Stands in for a payment gateway webhook until a real gateway is integrated — see ConfirmPaymentCommand's remarks.</summary>
    [HttpPost("{id:guid}/payments/{paymentId:guid}/confirm")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ConfirmPayment(Guid id, Guid paymentId, [FromBody] ConfirmPaymentRequestBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ConfirmPaymentCommand(id, paymentId, body.GatewayTransactionId), cancellationToken);
        return NoContent();
    }

    [HttpGet("seller/queue")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(PagedResult<SellerOrderItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<SellerOrderItemDto>>> GetSellerQueue(
        [FromQuery] OrderStatus? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetSellerOrderItemsQuery(status, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("items/{orderItemId:guid}/status")]
    [Authorize(Roles = "Seller,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateItemStatus(Guid orderItemId, [FromBody] UpdateOrderItemStatusRequestBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateOrderItemStatusCommand(orderItemId, body.NewStatus, body.Carrier, body.TrackingNumber, body.TrackingUrl), cancellationToken);
        return NoContent();
    }
}

public record CancelOrderRequestBody(string? Reason);

public record ConfirmPaymentRequestBody(string GatewayTransactionId);

public record UpdateOrderItemStatusRequestBody(OrderStatus NewStatus, string? Carrier, string? TrackingNumber, string? TrackingUrl);
