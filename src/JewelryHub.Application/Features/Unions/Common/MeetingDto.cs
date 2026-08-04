using JewelryHub.Domain.Enums;

namespace JewelryHub.Application.Features.Unions.Common;

public record MeetingSummaryDto(
    Guid Id, Guid UnionId, string Title, DateTime ScheduledAtUtc, int DurationMinutes, MeetingStatus Status);

public record MeetingAgendaItemDto(
    Guid Id, int DisplayOrder, string Topic, string? Description, AgendaItemStatus Status);

public record MeetingAttendeeDto(
    Guid Id, Guid UnionMemberId, string SellerBusinessName, MeetingAttendanceStatus Status);

public record ActionItemDto(
    Guid Id, string Description, Guid ResponsibleMemberId, string ResponsibleBusinessName, DateTime? DueDateUtc, ActionItemStatus Status);

public record MeetingMinuteDto(
    Guid Id, Guid? AgendaItemId, string DecisionSummary, string? DiscussionNotes,
    Guid RecordedByMemberId, string RecordedByBusinessName, DateTime CreatedAtUtc,
    IReadOnlyList<ActionItemDto> ActionItems);

public record MeetingDto(
    Guid Id,
    Guid UnionId,
    string Title,
    string? Location,
    string? VirtualMeetingLink,
    DateTime ScheduledAtUtc,
    int DurationMinutes,
    MeetingStatus Status,
    Guid CreatedByMemberId,
    string CreatedByBusinessName,
    IReadOnlyList<MeetingAgendaItemDto> AgendaItems,
    IReadOnlyList<MeetingAttendeeDto> Attendees,
    IReadOnlyList<MeetingMinuteDto> Minutes);
