using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Unions.Commands.ClosePoll;
using JewelryHub.Application.Features.Unions.Commands.CreatePoll;
using JewelryHub.Application.Features.Unions.Commands.OpenPoll;
using JewelryHub.Application.Features.Unions.Commands.Vote;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Application.Features.Unions.Queries.GetPollById;
using JewelryHub.Application.Features.Unions.Queries.GetPolls;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryHub.API.Controllers;

[ApiController]
[Route("api/v1/union-polls")]
[Authorize]
public class UnionPollsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UnionPollsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PollSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PollSummaryDto>>> GetPolls(
        [FromQuery] Guid unionId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetPollsQuery(unionId, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PollDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PollDto>> Create(CreatePollCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PollDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PollDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPollByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/open")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Open(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new OpenPollCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ClosePollCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/vote")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Vote(Guid id, [FromBody] VoteRequestBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new VoteCommand(id, body.OptionIds), cancellationToken);
        return NoContent();
    }
}

public record VoteRequestBody(List<Guid> OptionIds);
