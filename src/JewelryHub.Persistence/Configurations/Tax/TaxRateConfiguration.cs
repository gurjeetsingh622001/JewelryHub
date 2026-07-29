using JewelryHub.Domain.Tax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryHub.Persistence.Configurations.Tax;

public class TaxRateConfiguration : IEntityTypeConfiguration<TaxRate>
{
    public void Configure(EntityTypeBuilder<TaxRate> b)
    {
        b.ToTable("TaxRates");
        b.Property(x => x.Name).IsRequired().HasMaxLength(150);
        b.Property(x => x.RatePercentage).HasPrecision(5, 2);

        b.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

        // The checkout-time lookup: "which active rates apply right now, for this scope".
        b.HasIndex(x => new { x.IsActive, x.Applicability, x.EffectiveFromUtc, x.EffectiveToUtc });

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
