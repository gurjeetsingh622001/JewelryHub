using JewelryHub.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryHub.Persistence.Configurations.Payments;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("Payments");
        b.Property(x => x.Amount).HasPrecision(14, 2);
        b.Property(x => x.RefundedAmount).HasPrecision(14, 2);
        b.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        b.Property(x => x.GatewayProvider).HasMaxLength(50);
        b.Property(x => x.GatewayTransactionId).HasMaxLength(150);

        b.HasIndex(x => x.GatewayTransactionId);
        b.HasIndex(x => new { x.OrderId, x.Status });
    }
}
