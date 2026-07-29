using JewelryHub.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryHub.Persistence.Configurations.Customers;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.ToTable("Customers");
        b.HasIndex(x => x.UserId).IsUnique(); // enforces the 1:1 with User

        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Addresses).WithOne(a => a.Customer).HasForeignKey(a => a.CustomerId).OnDelete(DeleteBehavior.Cascade);

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> b)
    {
        b.ToTable("CustomerAddresses");
        b.Property(x => x.AddressLine1).IsRequired().HasMaxLength(250);
        b.Property(x => x.City).IsRequired().HasMaxLength(100);
        b.Property(x => x.State).IsRequired().HasMaxLength(100);
        b.Property(x => x.PostalCode).IsRequired().HasMaxLength(20);

        // Speeds up the CGST/SGST vs IGST lookup done at checkout time.
        b.HasIndex(x => x.State);
    }
}
