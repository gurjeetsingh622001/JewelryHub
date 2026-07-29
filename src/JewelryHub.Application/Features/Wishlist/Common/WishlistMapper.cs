namespace JewelryHub.Application.Features.Wishlist.Common;

/// <summary>Caller must have loaded Items.Product.Images and Items.Product.Inventory first.</summary>
public static class WishlistMapper
{
    public static WishlistDto ToDto(Domain.Wishlist.Wishlist wishlist)
    {
        var items = wishlist.Items.Select(i =>
        {
            var primaryImage = i.Product.Images.OrderByDescending(img => img.IsPrimary).ThenBy(img => img.DisplayOrder).FirstOrDefault();
            var inStock = i.Product.Inventory is null || !i.Product.Inventory.TrackInventory
                || i.Product.Inventory.QuantityAvailable > i.Product.Inventory.QuantityReserved;

            return new WishlistItemDto(i.Id, i.ProductId, i.Product.Name, i.Product.Slug, primaryImage?.Url, i.Product.BasePrice, inStock, i.AddedAtUtc);
        }).ToList();

        return new WishlistDto(wishlist.Id, items);
    }
}
