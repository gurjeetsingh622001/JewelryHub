namespace JewelryHub.Application.Features.Wishlist.Common;

public record WishlistDto(Guid Id, IReadOnlyList<WishlistItemDto> Items);

public record WishlistItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSlug,
    string? ProductImageUrl,
    decimal CurrentPrice,
    bool IsInStock,
    DateTime AddedAtUtc);
