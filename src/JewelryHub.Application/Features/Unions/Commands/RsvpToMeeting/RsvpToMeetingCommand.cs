using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Unions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Commands.RsvpToMeeting;

/// <summary>
/// A member responds to their own meeting invite. Attendee rows are
/// normally pre-created for every active member when the meeting is
/// scheduled (see CreateMeetingCommand), but this creates one on demand
/// too, in case the caller joined the union after the meeting was created.
/// </summary>
public record RsvpToMeetingCommand(Guid MeetingId, MeetingAttendanceStatus Status) : IRequest;

public class RsvpToMeetingCommandValidator : AbstractValidator<RsvpToMeetingCommand>
{
    public RsvpToMeetingCommandValidator()
    {
        RuleFor(x => x.Status).NotEqual(MeetingAttendanceStatus.Invited)
            .WithMessage("RSVP status must be Confirmed, Attended, Absent, or Excused.");
    }
}

public class RsvpToMeetingCommandHandler : IRequestHandler<RsvpToMeetingCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RsvpToMeetingCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(RsvpToMeetingCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _unitOfWork.Meetings.GetByIdAsync(request.MeetingId, cancellationToken)
            ?? throw new NotFoundException(nameof(Meeting), request.MeetingId);

        var membership = await UnionAuthorization.GetActiveMembershipAsync(_unitOfWork, _currentUser, meeting.UnionId, cancellationToken);

        var attendee = await _unitOfWork.MeetingAttendees.QueryTracking()
            .FirstOrDefaultAsync(a => a.MeetingId == request.MeetingId && a.UnionMemberId == membership.Id, cancellationToken);

        if (attendee is null)
        {
            attendee = new MeetingAttendee { MeetingId = request.MeetingId, UnionMemberId = membership.Id };
            await _unitOfWork.MeetingAttendees.AddAsync(attendee, cancellationToken);
        }

        attendee.Status = request.Status;
        _unitOfWork.MeetingAttendees.Update(attendee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
