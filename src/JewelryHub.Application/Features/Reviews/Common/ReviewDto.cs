namespace JewelryHub.Application.Features.Reviews.Common;

public record ReviewDto(
    Guid Id,
    Guid ProductId,
    Guid SellerId,
    string CustomerFirstName,
    int ProductRating,
    int SellerRating,
    string? Title,
    string? Comment,
    string? SellerResponse,
    DateTime? SellerRespondedAtUtc,
    DateTime CreatedAtUtc);
