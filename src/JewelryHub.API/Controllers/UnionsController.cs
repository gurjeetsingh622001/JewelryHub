using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Unions.Commands.ApproveUnion;
using JewelryHub.Application.Features.Unions.Commands.CreateAnnouncement;
using JewelryHub.Application.Features.Unions.Commands.CreateEvent;
using JewelryHub.Application.Features.Unions.Commands.CreateUnion;
using JewelryHub.Application.Features.Unions.Commands.DeleteAnnouncement;
using JewelryHub.Application.Features.Unions.Commands.RemoveMember;
using JewelryHub.Application.Features.Unions.Commands.RequestMembership;
using JewelryHub.Application.Features.Unions.Commands.ReviewMembership;
using JewelryHub.Application.Features.Unions.Commands.UpdateEventStatus;
using JewelryHub.Application.Features.Unions.Commands.UpdateMemberRole;
using JewelryHub.Application.Features.Unions.Commands.UploadDocument;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Application.Features.Unions.Queries.GetAnnouncements;
using JewelryHub.Application.Features.Unions.Queries.GetDocuments;
using JewelryHub.Application.Features.Unions.Queries.GetEvents;
using JewelryHub.Application.Features.Unions.Queries.GetMyMemberships;
using JewelryHub.Application.Features.Unions.Queries.GetPendingMemberships;
using JewelryHub.Application.Features.Unions.Queries.GetPendingUnions;
using JewelryHub.Application.Features.Unions.Queries.GetUnionById;
using JewelryHub.Application.Features.Unions.Queries.GetUnionMembers;
using JewelryHub.Application.Features.Unions.Queries.GetUnions;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryHub.API.Controllers;

[ApiController]
[Route("api/v1/unions")]
public class UnionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UnionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<UnionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UnionDto>>> GetUnions(
        [FromQuery] string? search, [FromQuery] string? city, [FromQuery] string? state,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetUnionsQuery(search, city, state, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(UnionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<UnionDto>> Create(CreateUnionCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Admin queue of newly-created unions awaiting approval.</summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<UnionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UnionDto>>> GetPending(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetPendingUnionsQuery(pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>Every union the authenticated seller belongs to or has a pending request with.</summary>
    [HttpGet("me/memberships")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(IReadOnlyList<UnionMemberDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UnionMemberDto>>> GetMyMemberships(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyMembershipsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UnionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UnionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUnionByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ApproveUnionCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/join")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(UnionMemberDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<UnionMemberDto>> Join(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RequestMembershipCommand(id), cancellationToken);
        return CreatedAtAction(nameof(GetMembers), new { id }, result);
    }

    [HttpGet("{id:guid}/members")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<UnionMemberDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UnionMemberDto>>> GetMembers(
        Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetUnionMembersQuery(id, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>Officer/Admin-only review queue of sellers asking to join this union.</summary>
    [HttpGet("{id:guid}/members/pending")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<UnionMemberDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UnionMemberDto>>> GetPendingMembers(
        Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetPendingMembershipsQuery(id, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("memberships/{membershipId:guid}/review")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReviewMembership(Guid membershipId, [FromBody] ReviewMembershipRequestBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReviewMembershipCommand(membershipId, body.Approve), cancellationToken);
        return NoContent();
    }

    [HttpPatch("memberships/{membershipId:guid}/role")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateMemberRole(Guid membershipId, [FromBody] UpdateMemberRoleRequestBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateMemberRoleCommand(membershipId, body.Role), cancellationToken);
        return NoContent();
    }

    [HttpDelete("memberships/{membershipId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveMember(Guid membershipId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveMemberCommand(membershipId), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/announcements")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<UnionAnnouncementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UnionAnnouncementDto>>> GetAnnouncements(
        Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAnnouncementsQuery(id, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/announcements")]
    [Authorize]
    [ProducesResponseType(typeof(UnionAnnouncementDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<UnionAnnouncementDto>> CreateAnnouncement(Guid id, [FromBody] CreateAnnouncementRequestBody body, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateAnnouncementCommand(id, body.Title, body.Body, body.IsPinned), cancellationToken);
        return CreatedAtAction(nameof(GetAnnouncements), new { id }, result);
    }

    [HttpDelete("announcements/{announcementId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAnnouncement(Guid announcementId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteAnnouncementCommand(announcementId), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/documents")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<UnionDocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UnionDocumentDto>>> GetDocuments(
        Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetDocumentsQuery(id, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/documents")]
    [Authorize]
    [ProducesResponseType(typeof(UnionDocumentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<UnionDocumentDto>> UploadDocument(Guid id, [FromBody] UploadDocumentRequestBody body, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UploadDocumentCommand(id, body.Title, body.FileUrl, body.Category), cancellationToken);
        return CreatedAtAction(nameof(GetDocuments), new { id }, result);
    }

    [HttpGet("{id:guid}/events")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<UnionEventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UnionEventDto>>> GetEvents(
        Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetEventsQuery(id, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/events")]
    [Authorize]
    [ProducesResponseType(typeof(UnionEventDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<UnionEventDto>> CreateEvent(Guid id, [FromBody] CreateEventRequestBody body, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateEventCommand(id, body.Title, body.Description, body.Location, body.StartsAtUtc, body.EndsAtUtc), cancellationToken);
        return CreatedAtAction(nameof(GetEvents), new { id }, result);
    }

    [HttpPatch("events/{eventId:guid}/status")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateEventStatus(Guid eventId, [FromBody] UpdateEventStatusRequestBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateEventStatusCommand(eventId, body.Status), cancellationToken);
        return NoContent();
    }
}

public record ReviewMembershipRequestBody(bool Approve);
public record UpdateMemberRoleRequestBody(UnionMemberRole Role);
public record CreateAnnouncementRequestBody(string Title, string Body, bool IsPinned);
public record UploadDocumentRequestBody(string Title, string FileUrl, string? Category);
public record CreateEventRequestBody(string Title, string? Description, string? Location, DateTime StartsAtUtc, DateTime? EndsAtUtc);
public record UpdateEventStatusRequestBody(UnionEventStatus Status);
