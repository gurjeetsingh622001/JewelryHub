using JewelryHub.Domain.Sellers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryHub.Persistence.Configurations.Sellers;

public class SellerConfiguration : IEntityTypeConfiguration<Seller>
{
    public void Configure(EntityTypeBuilder<Seller> b)
    {
        b.ToTable("Sellers");
        b.HasIndex(x => x.UserId).IsUnique();

        b.Property(x => x.BusinessName).IsRequired().HasMaxLength(200);
        b.Property(x => x.GstNumber).IsRequired().HasMaxLength(15);
        b.HasIndex(x => x.GstNumber).IsUnique();
        b.Property(x => x.PanNumber).HasMaxLength(10);
        b.Property(x => x.BusinessRegistrationNumber).IsRequired().HasMaxLength(100);
        b.Property(x => x.State).IsRequired().HasMaxLength(100);

        b.Property(x => x.TotalRevenue).HasPrecision(18, 2);
        b.Property(x => x.AverageRating).HasPrecision(3, 2);

        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Documents).WithOne(d => d.Seller).HasForeignKey(d => d.SellerId).OnDelete(DeleteBehavior.Cascade);

        // Sellers pending approval or under review are the common admin queue filter.
        b.HasIndex(x => x.VerificationStatus);

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class SellerDocumentConfiguration : IEntityTypeConfiguration<SellerDocument>
{
    public void Configure(EntityTypeBuilder<SellerDocument> b)
    {
        b.ToTable("SellerDocuments");
        b.Property(x => x.DocumentType).IsRequired().HasMaxLength(100);
        b.Property(x => x.FileUrl).IsRequired().HasMaxLength(500);
    }
}
