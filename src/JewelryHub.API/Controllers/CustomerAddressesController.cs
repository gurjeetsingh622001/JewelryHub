using JewelryHub.Application.Features.Customers.Addresses.Commands.CreateAddress;
using JewelryHub.Application.Features.Customers.Addresses.Commands.DeleteAddress;
using JewelryHub.Application.Features.Customers.Addresses.Commands.UpdateAddress;
using JewelryHub.Application.Features.Customers.Addresses.Common;
using JewelryHub.Application.Features.Customers.Addresses.Queries.GetMyAddresses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryHub.API.Controllers;

/// <summary>
/// Saved shipping/billing addresses for the authenticated Customer — the
/// piece Checkout needs but that had no endpoint at all: CreateOrderCommand
/// requires a ShippingAddressId/BillingAddressId to already exist, and
/// this is the only way one gets created.
/// </summary>
[ApiController]
[Route("api/v1/customer-addresses")]
[Authorize(Roles = "Customer")]
public class CustomerAddressesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomerAddressesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerAddressDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CustomerAddressDto>>> GetMyAddresses(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyAddressesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerAddressDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<CustomerAddressDto>> Create(CreateAddressCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMyAddresses), null, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerAddressDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CustomerAddressDto>> Update(Guid id, [FromBody] UpdateAddressRequestBody body, CancellationToken cancellationToken)
    {
        var command = new UpdateAddressCommand(id, body.Label, body.AddressLine1, body.AddressLine2, body.City, body.State,
            body.PostalCode, body.ContactPhone, body.IsDefault);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteAddressCommand(id), cancellationToken);
        return NoContent();
    }
}

/// <summary>Route id is the source of truth for which address to update, so the body omits it (avoids a mismatched-id footgun).</summary>
public record UpdateAddressRequestBody(
    string Label, string AddressLine1, string? AddressLine2, string City, string State,
    string PostalCode, string? ContactPhone, bool IsDefault);
