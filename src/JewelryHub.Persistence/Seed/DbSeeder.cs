using JewelryHub.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Persistence.Seed;

/// <summary>
/// Seeds the handful of rows the application cannot function without
/// (system roles). Called once at startup — idempotent, so it's safe to
/// run on every deploy rather than only "the first time".
/// </summary>
public static class DbSeeder
{
    private static readonly (string Name, string Description)[] SystemRoles =
    {
        ("Admin", "Full platform administration access."),
        ("Seller", "Jewelry business account — manages products, orders, and union membership."),
        ("Customer", "Buyer account — browses, purchases, and reviews jewelry."),
    };

    public static async Task SeedAsync(JewelryHubDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        foreach (var (name, description) in SystemRoles)
        {
            var normalized = name.ToUpperInvariant();
            var exists = await context.Roles.AnyAsync(r => r.NormalizedName == normalized, cancellationToken);
            if (exists) continue;

            context.Roles.Add(new Role
            {
                Name = name,
                NormalizedName = normalized,
                Description = description,
                IsSystemRole = true,
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
