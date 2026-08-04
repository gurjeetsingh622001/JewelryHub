using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Unions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Commands.CreateMeeting;

/// <summary>Every currently-active member is auto-invited (a MeetingAttendee row, Status=Invited) — matches how a real union secretary would circulate a meeting notice.</summary>
public record CreateMeetingCommand(
    Guid UnionId, string Title, string? Location, string? VirtualMeetingLink,
    DateTime ScheduledAtUtc, int DurationMinutes, List<string> AgendaTopics)
    : IRequest<MeetingDto>;

public class CreateMeetingCommandValidator : AbstractValidator<CreateMeetingCommand>
{
    public CreateMeetingCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleForEach(x => x.AgendaTopics).NotEmpty().MaximumLength(250);
    }
}

public class CreateMeetingCommandHandler : IRequestHandler<CreateMeetingCommand, MeetingDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateMeetingCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<MeetingDto> Handle(CreateMeetingCommand request, CancellationToken cancellationToken)
    {
        var organizer = await UnionAuthorization.GetActiveOfficerMembershipAsync(_unitOfWork, _currentUser, request.UnionId, cancellationToken);

        var meeting = new Meeting
        {
            UnionId = request.UnionId,
            Title = request.Title.Trim(),
            Location = request.Location,
            VirtualMeetingLink = request.VirtualMeetingLink,
            ScheduledAtUtc = request.ScheduledAtUtc,
            DurationMinutes = request.DurationMinutes,
            CreatedByMemberId = organizer.Id,
        };
        await _unitOfWork.Meetings.AddAsync(meeting, cancellationToken);

        for (var i = 0; i < request.AgendaTopics.Count; i++)
        {
            await _unitOfWork.MeetingAgendaItems.AddAsync(new MeetingAgendaItem
            {
                MeetingId = meeting.Id,
                DisplayOrder = i,
                Topic = request.AgendaTopics[i].Trim(),
            }, cancellationToken);
        }

        var activeMembers = await _unitOfWork.UnionMembers.Query()
            .Where(m => m.UnionId == request.UnionId && m.Status == UnionMembershipStatus.Active)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        foreach (var memberId in activeMembers)
        {
            await _unitOfWork.MeetingAttendees.AddAsync(new MeetingAttendee
            {
                MeetingId = meeting.Id,
                UnionMemberId = memberId,
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var withDetails = await _unitOfWork.Meetings.Query().WithFullDetails()
            .FirstOrDefaultAsync(m => m.Id == meeting.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Meeting), meeting.Id);
        return MeetingMapper.ToDto(withDetails);
    }
}
