using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Unions.Commands.CreateMeeting;
using JewelryHub.Application.Features.Unions.Commands.RecordMinute;
using JewelryHub.Application.Features.Unions.Commands.RsvpToMeeting;
using JewelryHub.Application.Features.Unions.Commands.UpdateMeetingStatus;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Application.Features.Unions.Queries.GetMeetingById;
using JewelryHub.Application.Features.Unions.Queries.GetMeetings;
using JewelryHub.Application.Features.Unions.Queries.GetMyActionItems;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryHub.API.Controllers;

[ApiController]
[Route("api/v1/union-meetings")]
[Authorize]
public class UnionMeetingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UnionMeetingsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MeetingSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<MeetingSummaryDto>>> GetMeetings(
        [FromQuery] Guid unionId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetMeetingsQuery(unionId, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(MeetingDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<MeetingDto>> Create(CreateMeetingCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MeetingDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MeetingDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMeetingByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateMeetingStatusRequestBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateMeetingStatusCommand(id, body.Status), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/rsvp")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Rsvp(Guid id, [FromBody] RsvpRequestBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RsvpToMeetingCommand(id, body.Status), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/minutes")]
    [ProducesResponseType(typeof(MeetingMinuteDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<MeetingMinuteDto>> RecordMinute(Guid id, [FromBody] RecordMinuteRequestBody body, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RecordMinuteCommand(id, body.AgendaItemId, body.DecisionSummary, body.DiscussionNotes, body.ActionItems), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    /// <summary>The authenticated seller's open action items across every union they're a member of.</summary>
    [HttpGet("my-action-items")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(IReadOnlyList<ActionItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ActionItemDto>>> GetMyActionItems(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyActionItemsQuery(), cancellationToken);
        return Ok(result);
    }
}

public record UpdateMeetingStatusRequestBody(MeetingStatus Status);
public record RsvpRequestBody(MeetingAttendanceStatus Status);
public record RecordMinuteRequestBody(Guid? AgendaItemId, string DecisionSummary, string? DiscussionNotes, List<ActionItemInput> ActionItems);
