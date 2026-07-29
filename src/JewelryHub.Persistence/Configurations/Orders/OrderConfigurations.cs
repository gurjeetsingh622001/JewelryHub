using JewelryHub.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryHub.Persistence.Configurations.Orders;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("Orders");
        b.Property(x => x.OrderNumber).IsRequired().HasMaxLength(30);
        b.HasIndex(x => x.OrderNumber).IsUnique();

        b.Property(x => x.Subtotal).HasPrecision(14, 2);
        b.Property(x => x.TotalTax).HasPrecision(14, 2);
        b.Property(x => x.ShippingCharges).HasPrecision(14, 2);
        b.Property(x => x.DiscountAmount).HasPrecision(14, 2);
        b.Property(x => x.GrandTotal).HasPrecision(14, 2);

        b.HasOne(x => x.Customer).WithMany(c => c.Orders).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);

        // Addresses are snapshotted by FK, not copied inline — acceptable
        // here because CustomerAddress rows are never hard-deleted, only
        // soft-deleted, so historical orders can always resolve them.
        b.HasOne(x => x.ShippingAddress).WithMany().HasForeignKey(x => x.ShippingAddressId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.BillingAddress).WithMany().HasForeignKey(x => x.BillingAddressId).OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Payments).WithOne(p => p.Order).HasForeignKey(p => p.OrderId).OnDelete(DeleteBehavior.Cascade);

        // Customer order history and admin/seller reporting both filter by these.
        b.HasIndex(x => new { x.CustomerId, x.Status });
        b.HasIndex(x => x.CreatedAtUtc);

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.ToTable("OrderItems");
        b.Property(x => x.ProductNameSnapshot).IsRequired().HasMaxLength(250);
        b.Property(x => x.MetalRateSnapshot).HasPrecision(12, 2);
        b.Property(x => x.UnitPriceSnapshot).HasPrecision(14, 2);
        b.Property(x => x.LineTotal).HasPrecision(14, 2);

        b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Seller).WithMany().HasForeignKey(x => x.SellerId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Taxes).WithOne(t => t.OrderItem).HasForeignKey(t => t.OrderItemId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Shipment).WithOne(s => s.OrderItem).HasForeignKey<Shipment>(s => s.OrderItemId).OnDelete(DeleteBehavior.Cascade);

        // Sellers filter "my orders to fulfill" constantly — this is the seller dashboard's primary query.
        b.HasIndex(x => new { x.SellerId, x.ItemStatus });
    }
}

public class OrderTaxConfiguration : IEntityTypeConfiguration<OrderTax>
{
    public void Configure(EntityTypeBuilder<OrderTax> b)
    {
        b.ToTable("OrderTaxes");
        b.Property(x => x.RatePercentageApplied).HasPrecision(5, 2);
        b.Property(x => x.TaxableAmount).HasPrecision(14, 2);
        b.Property(x => x.TaxAmount).HasPrecision(14, 2);

        // Restrict (not cascade): a TaxRate must not be deletable while
        // historical orders still reference it — supersede it instead.
        b.HasOne(x => x.TaxRate).WithMany().HasForeignKey(x => x.TaxRateId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> b)
    {
        b.ToTable("Shipments");
        b.Property(x => x.Carrier).HasMaxLength(100);
        b.Property(x => x.TrackingNumber).HasMaxLength(100);
        b.HasIndex(x => x.OrderItemId).IsUnique();
    }
}
