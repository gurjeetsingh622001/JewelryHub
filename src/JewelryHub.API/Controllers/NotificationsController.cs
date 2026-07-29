using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Notifications.Commands.MarkAllAsRead;
using JewelryHub.Application.Features.Notifications.Commands.MarkAsRead;
using JewelryHub.Application.Features.Notifications.Common;
using JewelryHub.Application.Features.Notifications.Queries.GetMyNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryHub.API.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<NotificationDto>>> Get(
        [FromQuery] bool unreadOnly = false, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetMyNotificationsQuery(unreadOnly, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkNotificationAsReadCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkAllNotificationsAsReadCommand(), cancellationToken);
        return NoContent();
    }
}
