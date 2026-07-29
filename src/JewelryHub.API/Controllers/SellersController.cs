using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Sellers.Commands.ApproveSeller;
using JewelryHub.Application.Features.Sellers.Commands.RejectSeller;
using JewelryHub.Application.Features.Sellers.Commands.ReviewDocument;
using JewelryHub.Application.Features.Sellers.Commands.SubmitDocument;
using JewelryHub.Application.Features.Sellers.Common;
using JewelryHub.Application.Features.Sellers.Queries.GetPendingSellers;
using JewelryHub.Application.Features.Sellers.Queries.GetSellerProfile;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryHub.API.Controllers;

[ApiController]
[Route("api/v1/sellers")]
[Authorize]
public class SellersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SellersController(IMediator mediator) => _mediator = mediator;

    /// <summary>The authenticated seller's own profile, including their document review status.</summary>
    [HttpGet("me")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(SellerDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SellerDto>> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSellerProfileQuery(SellerId: null), cancellationToken);
        return Ok(result);
    }

    [HttpPost("me/documents")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(SellerDocumentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<SellerDocumentDto>> SubmitDocument(SubmitSellerDocumentCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMyProfile), null, result);
    }

    /// <summary>Admin queue of sellers awaiting KYC approval.</summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<SellerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<SellerDto>>> GetPending([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetPendingSellersQuery(pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SellerDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SellerDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSellerProfileQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("documents/{documentId:guid}/review")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReviewDocument(Guid documentId, [FromBody] ReviewDocumentRequestBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReviewSellerDocumentCommand(documentId, body.Decision, body.ReviewerNote), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ApproveSellerCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectSellerRequestBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RejectSellerCommand(id, body.Reason), cancellationToken);
        return NoContent();
    }
}

public record ReviewDocumentRequestBody(DocumentVerificationStatus Decision, string? ReviewerNote);

public record RejectSellerRequestBody(string Reason);
