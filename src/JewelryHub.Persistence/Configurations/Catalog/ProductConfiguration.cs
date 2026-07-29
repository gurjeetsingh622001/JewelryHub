using JewelryHub.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryHub.Persistence.Configurations.Catalog;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("Products");

        b.Property(x => x.Name).IsRequired().HasMaxLength(250);
        b.Property(x => x.Slug).IsRequired().HasMaxLength(280);
        b.Property(x => x.Sku).IsRequired().HasMaxLength(80);
        // SKU only needs to be unique per seller, not globally.
        b.HasIndex(x => new { x.SellerId, x.Sku }).IsUnique();
        b.HasIndex(x => x.Slug).IsUnique();

        // Every monetary/weight column gets explicit precision — leaving
        // it to EF Core's default risks silent truncation of paise/mg.
        b.Property(x => x.GrossWeightGrams).HasPrecision(10, 3);
        b.Property(x => x.NetWeightGrams).HasPrecision(10, 3);
        b.Property(x => x.MetalRatePerGramAtListing).HasPrecision(12, 2);
        b.Property(x => x.MetalValue).HasPrecision(14, 2);
        b.Property(x => x.MakingCharges).HasPrecision(14, 2);
        b.Property(x => x.GemstoneValue).HasPrecision(14, 2);
        b.Property(x => x.WastageCharges).HasPrecision(14, 2);
        b.Property(x => x.BasePrice).HasPrecision(14, 2);
        b.Property(x => x.DiscountPercentage).HasPrecision(5, 2);
        b.Property(x => x.AverageRating).HasPrecision(3, 2);

        // The most common storefront filters/search patterns.
        b.HasIndex(x => new { x.CategoryId, x.Status });
        b.HasIndex(x => new { x.MetalType, x.Purity });
        b.HasIndex(x => x.SellerId);

        b.HasOne(x => x.Seller).WithMany().HasForeignKey(x => x.SellerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Category).WithMany(c => c.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Images).WithOne(i => i.Product).HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Certificates).WithOne(c => c.Product).HasForeignKey(c => c.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Gemstones).WithOne(g => g.Product).HasForeignKey(g => g.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Inventory).WithOne(i => i.Product).HasForeignKey<Inventory>(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class ProductGemstoneConfiguration : IEntityTypeConfiguration<ProductGemstone>
{
    public void Configure(EntityTypeBuilder<ProductGemstone> b)
    {
        b.ToTable("ProductGemstones");
        b.Property(x => x.GemstoneType).IsRequired().HasMaxLength(100);
        b.Property(x => x.WeightCarats).HasPrecision(8, 3);
        b.Property(x => x.Value).HasPrecision(14, 2);
    }
}

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> b)
    {
        b.ToTable("ProductImages");
        b.Property(x => x.Url).IsRequired().HasMaxLength(500);
    }
}

public class ProductCertificateConfiguration : IEntityTypeConfiguration<ProductCertificate>
{
    public void Configure(EntityTypeBuilder<ProductCertificate> b)
    {
        b.ToTable("ProductCertificates");
        b.Property(x => x.CertificateType).IsRequired().HasMaxLength(100);
        b.Property(x => x.IssuingAuthority).IsRequired().HasMaxLength(150);
        b.Property(x => x.FileUrl).IsRequired().HasMaxLength(500);
    }
}

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> b)
    {
        b.ToTable("Inventory");
        b.HasIndex(x => x.ProductId).IsUnique();
    }
}
