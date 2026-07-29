using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JewelryHub.Persistence;

/// <summary>
/// Used only by the `dotnet ef migrations add / database update` CLI.
/// The API project's DI container wires up the real connection string at
/// runtime (see AddPersistence in ServiceCollectionExtensions) — this
/// factory exists purely so migrations can be generated without spinning
/// up the whole host. Reads the connection string from an environment
/// variable so no real credentials sit in source control.
/// </summary>
public class JewelryHubDbContextFactory : IDesignTimeDbContextFactory<JewelryHubDbContext>
{
    public JewelryHubDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("JEWELRYHUB_CONNECTION_STRING")
            ?? "Server=localhost;Database=JewelryHubDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<JewelryHubDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sql =>
            sql.MigrationsAssembly(typeof(JewelryHubDbContext).Assembly.FullName));

        return new JewelryHubDbContext(optionsBuilder.Options);
    }
}
