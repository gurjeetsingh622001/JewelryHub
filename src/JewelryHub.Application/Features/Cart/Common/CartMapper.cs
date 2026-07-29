using JewelryHub.Domain.Cart;

namespace JewelryHub.Application.Features.Cart.Common;

/// <summary>Caller must have loaded Items.Product.Images and Items.Product.Inventory first.</summary>
public static class CartMapper
{
    public static CartDto ToDto(Domain.Cart.Cart cart)
    {
        var items = cart.Items.Select(i =>
        {
            var currentPrice = i.Product.BasePrice;
            var primaryImage = i.Product.Images.OrderByDescending(img => img.IsPrimary).ThenBy(img => img.DisplayOrder).FirstOrDefault();
            var inStock = i.Product.Inventory is null || !i.Product.Inventory.TrackInventory
                || i.Product.Inventory.QuantityAvailable - i.Product.Inventory.QuantityReserved >= i.Quantity;

            return new CartItemDto(
                i.Id, i.ProductId, i.Product.Name, primaryImage?.Url, i.Product.Slug,
                i.UnitPriceSnapshot, currentPrice, i.UnitPriceSnapshot != currentPrice,
                i.Quantity, currentPrice * i.Quantity, inStock);
        }).ToList();

        return new CartDto(cart.Id, items, items.Sum(i => i.LineTotal), items.Sum(i => i.Quantity));
    }
}
