using JewelryHub.Domain.Enums;

namespace JewelryHub.Application.Features.Unions.Common;

public record PollOptionResultDto(Guid Id, string Text, int DisplayOrder, int VoteCount);

public record PollSummaryDto(Guid Id, Guid UnionId, string Question, PollStatus Status, DateTime? ClosesAtUtc);

public record PollDto(
    Guid Id,
    Guid UnionId,
    string Question,
    bool AllowMultipleSelections,
    PollStatus Status,
    Guid CreatedByMemberId,
    string CreatedByBusinessName,
    DateTime? ClosesAtUtc,
    DateTime CreatedAtUtc,
    IReadOnlyList<PollOptionResultDto> Options,
    int TotalVotes);
