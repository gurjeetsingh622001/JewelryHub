using JewelryHub.Domain.Enums;

namespace JewelryHub.Application.Features.Sellers.Common;

public record SellerDto(
    Guid Id,
    Guid UserId,
    string BusinessName,
    string? BusinessDescription,
    string? LogoUrl,
    string GstNumber,
    string City,
    string State,
    SellerVerificationStatus VerificationStatus,
    string? RejectionReason,
    decimal TotalRevenue,
    int TotalOrdersFulfilled,
    decimal AverageRating,
    int ReviewCount,
    IReadOnlyList<SellerDocumentDto> Documents);

public record SellerDocumentDto(
    Guid Id,
    string DocumentType,
    string FileUrl,
    string? FileName,
    DocumentVerificationStatus Status,
    string? ReviewerNote,
    DateTime CreatedAtUtc);
