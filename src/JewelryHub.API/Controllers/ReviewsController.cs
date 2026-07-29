using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Reviews.Commands.CreateReview;
using JewelryHub.Application.Features.Reviews.Commands.ModerateReview;
using JewelryHub.Application.Features.Reviews.Commands.RespondToReview;
using JewelryHub.Application.Features.Reviews.Common;
using JewelryHub.Application.Features.Reviews.Queries.GetProductReviews;
using JewelryHub.Application.Features.Reviews.Queries.GetSellerReviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryHub.API.Controllers;

[ApiController]
[Route("api/v1/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("product/{productId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ReviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetForProduct(Guid productId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetProductReviewsQuery(productId, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("seller/{sellerId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ReviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetForSeller(Guid sellerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetSellerReviewsQuery(sellerId, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReviewDto>> Create(CreateReviewCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetForProduct), new { productId = result.ProductId }, result);
    }

    [HttpPost("{id:guid}/response")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Respond(Guid id, [FromBody] RespondToReviewRequestBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RespondToReviewCommand(id, body.Response), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/moderate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Moderate(Guid id, [FromBody] ModerateReviewRequestBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ModerateReviewCommand(id, body.IsApproved), cancellationToken);
        return NoContent();
    }
}

public record RespondToReviewRequestBody(string Response);

public record ModerateReviewRequestBody(bool IsApproved);
