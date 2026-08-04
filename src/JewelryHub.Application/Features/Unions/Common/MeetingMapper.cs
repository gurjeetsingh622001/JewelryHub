using JewelryHub.Domain.Unions;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Common;

public static class MeetingMapper
{
    /// <summary>
    /// Every handler that returns a full MeetingDto (create, get-by-id,
    /// record-minute, rsvp) needs the same deep include chain — centralized
    /// here so it only has to be gotten right once.
    /// </summary>
    public static IQueryable<Meeting> WithFullDetails(this IQueryable<Meeting> query) => query
        .Include(m => m.CreatedByMember).ThenInclude(cm => cm.Seller)
        .Include(m => m.AgendaItems)
        .Include(m => m.Attendees).ThenInclude(a => a.UnionMember).ThenInclude(um => um.Seller)
        .Include(m => m.Minutes).ThenInclude(mm => mm.RecordedByMember).ThenInclude(rm => rm.Seller)
        .Include(m => m.Minutes).ThenInclude(mm => mm.ActionItems).ThenInclude(ai => ai.ResponsibleMember).ThenInclude(rm => rm.Seller);

    public static MeetingSummaryDto ToSummaryDto(Meeting m) => new(m.Id, m.UnionId, m.Title, m.ScheduledAtUtc, m.DurationMinutes, m.Status);

    public static MeetingDto ToDto(Meeting m) => new(
        m.Id, m.UnionId, m.Title, m.Location, m.VirtualMeetingLink, m.ScheduledAtUtc, m.DurationMinutes, m.Status,
        m.CreatedByMemberId, m.CreatedByMember.Seller.BusinessName,
        m.AgendaItems.OrderBy(a => a.DisplayOrder)
            .Select(a => new MeetingAgendaItemDto(a.Id, a.DisplayOrder, a.Topic, a.Description, a.Status)).ToList(),
        m.Attendees
            .Select(a => new MeetingAttendeeDto(a.Id, a.UnionMemberId, a.UnionMember.Seller.BusinessName, a.Status)).ToList(),
        m.Minutes.OrderBy(mm => mm.CreatedAtUtc)
            .Select(mm => new MeetingMinuteDto(
                mm.Id, mm.AgendaItemId, mm.DecisionSummary, mm.DiscussionNotes,
                mm.RecordedByMemberId, mm.RecordedByMember.Seller.BusinessName, mm.CreatedAtUtc,
                mm.ActionItems.Select(ai => new ActionItemDto(
                    ai.Id, ai.Description, ai.ResponsibleMemberId, ai.ResponsibleMember.Seller.BusinessName, ai.DueDateUtc, ai.Status)).ToList()))
            .ToList());
}
