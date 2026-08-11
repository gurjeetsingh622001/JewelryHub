namespace JewelryHub.Application.Features.Admin.Common;

public record DashboardOverviewDto(
    int TotalCustomers,
    int TotalSellers,
    int PendingSellers,
    int ApprovedSellers,
    int TotalUnions,
    int PendingUnions,
    int ApprovedUnions,
    int TotalOrders,
    int OrdersThisMonth,
    decimal RevenueThisMonth,
    int TotalReviews,
    int FlaggedReviews);

public record UserSummaryDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles,
    bool IsActive,
    bool IsLockedOut,
    DateTime CreatedAtUtc,
    DateTime? LastLoginAtUtc);

/// <summary>
/// A review with the extra context (product/seller name, moderation flags)
/// an admin needs to browse and decide what to moderate — deliberately
/// separate from the public-facing ReviewDto, which never exposes
/// IsApproved/IsFlagged since a customer has no use for them.
/// </summary>
public record AdminReviewDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid SellerId,
    string SellerBusinessName,
    string CustomerFirstName,
    int ProductRating,
    int SellerRating,
    string? Title,
    string? Comment,
    bool IsApproved,
    bool IsFlagged,
    DateTime CreatedAtUtc);
