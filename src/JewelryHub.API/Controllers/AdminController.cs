using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Admin.Commands.SetUserActiveStatus;
using JewelryHub.Application.Features.Admin.Common;
using JewelryHub.Application.Features.Admin.Queries.GetAllReviews;
using JewelryHub.Application.Features.Admin.Queries.GetDashboardOverview;
using JewelryHub.Application.Features.Admin.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryHub.API.Controllers;

/// <summary>
/// Platform-wide admin concerns that don't belong to any single existing
/// resource controller (dashboard overview, user management, review
/// moderation browsing). Approving a seller/union or moderating a specific
/// review deliberately stay on their own resource controllers
/// (SellersController, UnionsController, ReviewsController) — this
/// controller only holds what genuinely has no other home.
/// </summary>
[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator) => _mediator = mediator;

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardOverviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardOverviewDto>> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDashboardOverviewQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResult<UserSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserSummaryDto>>> GetUsers(
        [FromQuery] string? search, [FromQuery] string? role, [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetUsersQuery(search, role, isActive, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("users/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetUserStatus(Guid id, [FromBody] SetUserStatusRequestBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetUserActiveStatusCommand(id, body.IsActive), cancellationToken);
        return NoContent();
    }

    [HttpGet("reviews")]
    [ProducesResponseType(typeof(PagedResult<AdminReviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminReviewDto>>> GetReviews(
        [FromQuery] bool? isApproved, [FromQuery] bool? isFlagged,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllReviewsQuery(isApproved, isFlagged, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }
}

public record SetUserStatusRequestBody(bool IsActive);
