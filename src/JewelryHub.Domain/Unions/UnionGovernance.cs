using JewelryHub.Domain.Common;
using JewelryHub.Domain.Enums;

namespace JewelryHub.Domain.Unions;

public class UnionAnnouncement : AuditableEntity, ISoftDelete
{
    public Guid UnionId { get; set; }
    public Union Union { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;
    public bool IsPinned { get; set; }

    public Guid PublishedByMemberId { get; set; }
    public UnionMember PublishedByMember { get; set; } = default!;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }
}

public class UnionDocument : AuditableEntity, ISoftDelete
{
    public Guid UnionId { get; set; }
    public Union Union { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string FileUrl { get; set; } = default!;
    public string? Category { get; set; } // "Bylaws", "Financial Report", "Meeting Record"

    public Guid UploadedByMemberId { get; set; }
    public UnionMember UploadedByMember { get; set; } = default!;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }
}

/// <summary>A union-organized event (trade fair, festival meet-up, training session) distinct from internal governance Meetings.</summary>
public class UnionEvent : AuditableEntity, ISoftDelete
{
    public Guid UnionId { get; set; }
    public Union Union { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public DateTime? EndsAtUtc { get; set; }
    public UnionEventStatus Status { get; set; } = UnionEventStatus.Upcoming;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }
}

/// <summary>A governance poll (e.g. voting on a proposed membership fee change).</summary>
public class UnionPoll : AuditableEntity, ISoftDelete
{
    public Guid UnionId { get; set; }
    public Union Union { get; set; } = default!;

    public string Question { get; set; } = default!;
    public bool AllowMultipleSelections { get; set; }
    public PollStatus Status { get; set; } = PollStatus.Draft;

    public Guid CreatedByMemberId { get; set; }
    public UnionMember CreatedByMember { get; set; } = default!;

    public DateTime? ClosesAtUtc { get; set; }

    public ICollection<PollOption> Options { get; set; } = new List<PollOption>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }
}

public class PollOption : BaseEntity
{
    public Guid PollId { get; set; }
    public UnionPoll Poll { get; set; } = default!;

    public string Text { get; set; } = default!;
    public int DisplayOrder { get; set; }

    public ICollection<PollVote> Votes { get; set; } = new List<PollVote>();
}

/// <summary>One member's vote for one option. Unique (PollOptionId, UnionMemberId) unless the poll allows multiple selections — enforced via a unique index + AllowMultipleSelections check in Application logic.</summary>
public class PollVote : BaseEntity
{
    public Guid PollOptionId { get; set; }
    public PollOption PollOption { get; set; } = default!;

    public Guid UnionMemberId { get; set; }
    public UnionMember UnionMember { get; set; } = default!;

    public DateTime VotedAtUtc { get; set; } = DateTime.UtcNow;
}
