using JewelryHub.Application.Common.Interfaces;
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

    // Local-dev-only bootstrap credentials — see SeedDevAdminAsync. There is
    // no self-service "register as Admin" endpoint by design, so a fresh
    // database otherwise has zero way to reach any Admin-gated endpoint.
    public const string DevAdminEmail = "admin@jewelryhub.local";
    public const string DevAdminPassword = "Admin@12345";

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

    /// <summary>
    /// Creates a single Admin-role user (DevAdminEmail/DevAdminPassword) if
    /// one doesn't already exist. Call only in Development — this is a
    /// convenience bootstrap for local testing, not something a real
    /// deployment should ever run with a well-known password.
    /// </summary>
    public static async Task SeedDevAdminAsync(JewelryHubDbContext context, IPasswordHasher passwordHasher, CancellationToken cancellationToken = default)
    {
        var alreadyExists = await context.Users.AnyAsync(u => u.Email == DevAdminEmail, cancellationToken);
        if (alreadyExists) return;

        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "ADMIN", cancellationToken)
            ?? throw new InvalidOperationException("The 'Admin' role is not seeded. Run SeedAsync before SeedDevAdminAsync.");

        var user = new User
        {
            Email = DevAdminEmail,
            PasswordHash = passwordHasher.Hash(DevAdminPassword),
            SecurityStamp = Guid.NewGuid().ToString("N"),
            FirstName = "Dev",
            LastName = "Admin",
            EmailConfirmed = true,
        };
        user.UserRoles.Add(new UserRole { RoleId = adminRole.Id, User = user });

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
    }
}
