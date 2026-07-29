using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Persistence.Interceptors;
using JewelryHub.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JewelryHub.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Call from JewelryHub.API's Program.cs:
    ///   builder.Services.AddPersistence(builder.Configuration);
    /// Registers the DbContext, the audit interceptor, and the
    /// Repository/Unit-of-Work implementations — kept in one place so
    /// Program.cs stays a thin composition root.
    /// </summary>
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        // ICurrentUserService is implemented in Infrastructure (reads the
        // JWT "sub" claim from HttpContext) and registered there; this
        // registration only requires that *some* implementation exists.
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        services.AddDbContext<JewelryHubDbContext>((provider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(JewelryHubDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(maxRetryCount: 3); // transient Azure SQL / network blips
            });

            options.AddInterceptors(provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}
