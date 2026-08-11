using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Infrastructure.Security;
using JewelryHub.Infrastructure.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JewelryHub.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Call from JewelryHub.API's Program.cs: builder.Services.AddInfrastructure(builder.Configuration);</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
