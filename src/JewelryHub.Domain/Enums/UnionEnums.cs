namespace JewelryHub.Domain.Enums;

/// <summary>Governance role a seller holds within a specific union (a seller can hold different roles in different unions).</summary>
public enum UnionMemberRole
{
    Member = 0,
    Treasurer = 1,
    Secretary = 2,
    VicePresident = 3,
    President = 4
}

public enum UnionMembershipStatus
{
    PendingApproval = 0,
    Active = 1,
    Inactive = 2,
    Removed = 3
}

public enum MeetingStatus
{
    Scheduled = 0,
    Ongoing = 1,
    Completed = 2,
    Cancelled = 3
}

/// <summary>Workflow status of a single agenda line item, tracked independently of overall MeetingStatus.</summary>
public enum AgendaItemStatus
{
    Pending = 0,
    InDiscussion = 1,
    Completed = 2,
    Postponed = 3
}

public enum MeetingAttendanceStatus
{
    Invited = 0,
    Confirmed = 1,
    Attended = 2,
    Absent = 3,
    Excused = 4
}

public enum PollStatus
{
    Draft = 0,
    Active = 1,
    Closed = 2
}

public enum UnionEventStatus
{
    Upcoming = 0,
    Ongoing = 1,
    Completed = 2,
    Cancelled = 3
}

public enum ActionItemStatus
{
    Open = 0,
    InProgress = 1,
    Completed = 2,
    Overdue = 3
}
