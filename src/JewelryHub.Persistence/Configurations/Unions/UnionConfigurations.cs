using JewelryHub.Domain.Unions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryHub.Persistence.Configurations.Unions;

public class UnionConfiguration : IEntityTypeConfiguration<Union>
{
    public void Configure(EntityTypeBuilder<Union> b)
    {
        b.ToTable("Unions");
        b.Property(x => x.Name).IsRequired().HasMaxLength(200);
        b.Property(x => x.City).IsRequired().HasMaxLength(100);
        b.Property(x => x.State).IsRequired().HasMaxLength(100);
        b.Property(x => x.AnnualMembershipFee).HasPrecision(10, 2);

        b.HasOne(x => x.CreatedBySeller).WithMany().HasForeignKey(x => x.CreatedBySellerId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Members).WithOne(m => m.Union).HasForeignKey(m => m.UnionId).OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.State, x.City });

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class UnionMemberConfiguration : IEntityTypeConfiguration<UnionMember>
{
    public void Configure(EntityTypeBuilder<UnionMember> b)
    {
        b.ToTable("UnionMembers");

        // A seller can only hold one membership record per union (role changes update this row, they don't add a new one).
        b.HasIndex(x => new { x.UnionId, x.SellerId }).IsUnique();
        b.HasIndex(x => new { x.UnionId, x.Role });

        b.HasOne(x => x.Seller).WithMany().HasForeignKey(x => x.SellerId).OnDelete(DeleteBehavior.Restrict);
    }
}
