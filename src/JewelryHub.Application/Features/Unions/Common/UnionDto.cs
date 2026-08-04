using JewelryHub.Domain.Enums;

namespace JewelryHub.Application.Features.Unions.Common;

public record UnionDto(
    Guid Id,
    string Name,
    string? Description,
    string? LogoUrl,
    string City,
    string State,
    Guid CreatedBySellerId,
    string CreatedBySellerBusinessName,
    bool IsApprovedByAdmin,
    DateTime? ApprovedAtUtc,
    decimal? AnnualMembershipFee,
    int ActiveMemberCount,
    DateTime CreatedAtUtc);

public record UnionMemberDto(
    Guid Id,
    Guid UnionId,
    Guid SellerId,
    string SellerBusinessName,
    UnionMemberRole Role,
    UnionMembershipStatus Status,
    DateTime JoinedAtUtc,
    DateTime? MembershipFeePaidThroughUtc);

public record UnionAnnouncementDto(
    Guid Id,
    Guid UnionId,
    string Title,
    string Body,
    bool IsPinned,
    Guid PublishedByMemberId,
    string PublishedByBusinessName,
    DateTime CreatedAtUtc);

public record UnionDocumentDto(
    Guid Id,
    Guid UnionId,
    string Title,
    string FileUrl,
    string? Category,
    Guid UploadedByMemberId,
    string UploadedByBusinessName,
    DateTime CreatedAtUtc);

public record UnionEventDto(
    Guid Id,
    Guid UnionId,
    string Title,
    string? Description,
    string? Location,
    DateTime StartsAtUtc,
    DateTime? EndsAtUtc,
    UnionEventStatus Status);
