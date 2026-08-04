using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Unions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Commands.RecordMinute;

public record ActionItemInput(string Description, Guid ResponsibleMemberId, DateTime? DueDateUtc);

public record RecordMinuteCommand(
    Guid MeetingId, Guid? AgendaItemId, string DecisionSummary, string? DiscussionNotes, List<ActionItemInput> ActionItems)
    : IRequest<MeetingMinuteDto>;

public class RecordMinuteCommandValidator : AbstractValidator<RecordMinuteCommand>
{
    public RecordMinuteCommandValidator()
    {
        RuleFor(x => x.DecisionSummary).NotEmpty().MaximumLength(2000);
        RuleForEach(x => x.ActionItems).ChildRules(item =>
        {
            item.RuleFor(a => a.Description).NotEmpty().MaximumLength(1000);
        });
    }
}

public class RecordMinuteCommandHandler : IRequestHandler<RecordMinuteCommand, MeetingMinuteDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RecordMinuteCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<MeetingMinuteDto> Handle(RecordMinuteCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _unitOfWork.Meetings.GetByIdAsync(request.MeetingId, cancellationToken)
            ?? throw new NotFoundException(nameof(Meeting), request.MeetingId);

        var recorder = await UnionAuthorization.GetActiveOfficerMembershipAsync(_unitOfWork, _currentUser, meeting.UnionId, cancellationToken);

        if (request.AgendaItemId is not null)
        {
            var agendaItemBelongsToMeeting = await _unitOfWork.MeetingAgendaItems.Query()
                .AnyAsync(a => a.Id == request.AgendaItemId && a.MeetingId == request.MeetingId, cancellationToken);
            if (!agendaItemBelongsToMeeting)
            {
                throw new BusinessRuleException("The specified agenda item does not belong to this meeting.");
            }
        }

        var responsibleIds = request.ActionItems.Select(a => a.ResponsibleMemberId).Distinct().ToList();
        var responsibleMembers = await _unitOfWork.UnionMembers.Query().Include(m => m.Seller)
            .Where(m => responsibleIds.Contains(m.Id) && m.UnionId == meeting.UnionId)
            .ToDictionaryAsync(m => m.Id, cancellationToken);

        if (responsibleMembers.Count != responsibleIds.Count)
        {
            throw new BusinessRuleException("One or more responsible members are not part of this union.");
        }

        var minute = new MeetingMinute
        {
            MeetingId = request.MeetingId,
            AgendaItemId = request.AgendaItemId,
            DecisionSummary = request.DecisionSummary.Trim(),
            DiscussionNotes = request.DiscussionNotes,
            RecordedByMemberId = recorder.Id,
        };
        await _unitOfWork.MeetingMinutes.AddAsync(minute, cancellationToken);

        var actionItems = new List<ActionItem>();
        foreach (var input in request.ActionItems)
        {
            var actionItem = new ActionItem
            {
                MeetingMinuteId = minute.Id,
                Description = input.Description.Trim(),
                ResponsibleMemberId = input.ResponsibleMemberId,
                DueDateUtc = input.DueDateUtc,
            };
            actionItems.Add(actionItem);
            await _unitOfWork.ActionItems.AddAsync(actionItem, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new MeetingMinuteDto(
            minute.Id, minute.AgendaItemId, minute.DecisionSummary, minute.DiscussionNotes,
            minute.RecordedByMemberId, recorder.Seller.BusinessName, minute.CreatedAtUtc,
            actionItems.Select(ai => new ActionItemDto(
                ai.Id, ai.Description, ai.ResponsibleMemberId, responsibleMembers[ai.ResponsibleMemberId].Seller.BusinessName,
                ai.DueDateUtc, ai.Status)).ToList());
    }
}
