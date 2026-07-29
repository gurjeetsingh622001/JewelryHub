using JewelryHub.Domain.Cart;
using JewelryHub.Domain.Wishlist;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryHub.Persistence.Configurations.Cart;

public class CartConfiguration : IEntityTypeConfiguration<JewelryHub.Domain.Cart.Cart>
{
    public void Configure(EntityTypeBuilder<JewelryHub.Domain.Cart.Cart> b)
    {
        b.ToTable("Carts");
        b.HasIndex(x => x.CustomerId).IsUnique(); // one active cart per customer

        b.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Items).WithOne(i => i.Cart).HasForeignKey(i => i.CartId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> b)
    {
        b.ToTable("CartItems");
        b.Property(x => x.UnitPriceSnapshot).HasPrecision(14, 2);
        b.HasIndex(x => new { x.CartId, x.ProductId }).IsUnique(); // adding an already-present product increments quantity instead of duplicating

        b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> b)
    {
        b.ToTable("Wishlists");
        b.HasIndex(x => x.CustomerId).IsUnique();

        b.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Items).WithOne(i => i.Wishlist).HasForeignKey(i => i.WishlistId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> b)
    {
        b.ToTable("WishlistItems");
        b.HasIndex(x => new { x.WishlistId, x.ProductId }).IsUnique();

        b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
