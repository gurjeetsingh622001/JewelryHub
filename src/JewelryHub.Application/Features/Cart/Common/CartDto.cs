namespace JewelryHub.Application.Features.Cart.Common;

public record CartDto(Guid Id, IReadOnlyList<CartItemDto> Items, decimal Subtotal, int TotalItemCount);

public record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string? ProductImageUrl,
    string ProductSlug,
    decimal UnitPriceSnapshot,
    decimal CurrentUnitPrice,
    bool PriceHasChanged,
    int Quantity,
    decimal LineTotal,
    bool IsInStock);
