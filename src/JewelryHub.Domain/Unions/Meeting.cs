using JewelryHub.Domain.Common;
using JewelryHub.Domain.Enums;

namespace JewelryHub.Domain.Unions;

/// <summary>A union meeting. Union administrators (President/Secretary, enforced in Application layer) create meetings and publish agendas ahead of time.</summary>
public class Meeting : AuditableEntity, ISoftDelete
{
    public Guid UnionId { get; set; }
    public Union Union { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string? Location { get; set; }
    public string? VirtualMeetingLink { get; set; }

    public DateTime ScheduledAtUtc { get; set; }
    public int DurationMinutes { get; set; }

    public MeetingStatus Status { get; set; } = MeetingStatus.Scheduled;

    public Guid CreatedByMemberId { get; set; }
    public UnionMember CreatedByMember { get; set; } = default!;

    public ICollection<MeetingAgendaItem> AgendaItems { get; set; } = new List<MeetingAgendaItem>();
    public ICollection<MeetingAttendee> Attendees { get; set; } = new List<MeetingAttendee>();
    public ICollection<MeetingMinute> Minutes { get; set; } = new List<MeetingMinute>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }
}

/// <summary>A single published agenda line (e.g. "Discussion on gold price changes"), tracked to a status independent of the overall meeting.</summary>
public class MeetingAgendaItem : AuditableEntity
{
    public Guid MeetingId { get; set; }
    public Meeting Meeting { get; set; } = default!;

    public int DisplayOrder { get; set; }
    public string Topic { get; set; } = default!;
    public string? Description { get; set; }
    public AgendaItemStatus Status { get; set; } = AgendaItemStatus.Pending;

    public ICollection<MeetingMinute> RelatedMinutes { get; set; } = new List<MeetingMinute>();
}

/// <summary>RSVP / attendance record for a member against a specific meeting.</summary>
public class MeetingAttendee : BaseEntity
{
    public Guid MeetingId { get; set; }
    public Meeting Meeting { get; set; } = default!;

    public Guid UnionMemberId { get; set; }
    public UnionMember UnionMember { get; set; } = default!;

    public MeetingAttendanceStatus Status { get; set; } = MeetingAttendanceStatus.Invited;
}

/// <summary>
/// Recorded minutes/decision for an agenda item after the meeting.
/// Linked optionally to the agenda item it resolves, and can spawn a
/// tracked action item with an owner and a deadline.
/// </summary>
public class MeetingMinute : AuditableEntity
{
    public Guid MeetingId { get; set; }
    public Meeting Meeting { get; set; } = default!;

    public Guid? AgendaItemId { get; set; }
    public MeetingAgendaItem? AgendaItem { get; set; }

    public string DecisionSummary { get; set; } = default!;
    public string? DiscussionNotes { get; set; }

    public Guid RecordedByMemberId { get; set; }
    public UnionMember RecordedByMember { get; set; } = default!;

    public ICollection<ActionItem> ActionItems { get; set; } = new List<ActionItem>();
}

/// <summary>A follow-up task arising from a decision, assigned to a responsible member with a deadline.</summary>
public class ActionItem : AuditableEntity
{
    public Guid MeetingMinuteId { get; set; }
    public MeetingMinute MeetingMinute { get; set; } = default!;

    public string Description { get; set; } = default!;

    public Guid ResponsibleMemberId { get; set; }
    public UnionMember ResponsibleMember { get; set; } = default!;

    public DateTime? DueDateUtc { get; set; }
    public ActionItemStatus Status { get; set; } = ActionItemStatus.Open;
}
