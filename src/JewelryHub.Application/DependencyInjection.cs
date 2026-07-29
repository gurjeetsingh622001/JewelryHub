using System.Reflection;
using FluentValidation;
using JewelryHub.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace JewelryHub.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Call from JewelryHub.API's Program.cs: builder.Services.AddApplication();
    /// Scans this assembly for MediatR handlers and FluentValidation
    /// validators — new features register themselves automatically, no
    /// manual entry needed here.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            // Order matters: exceptions from validation should surface as
            // validation errors, so Validation runs before anything else
            // that might swallow/rewrap the exception; Logging wraps the
            // whole pipeline so it captures the true elapsed time.
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);
        services.AddAutoMapper(assembly);
        services.AddScoped<Common.Interfaces.ITaxCalculator, Common.Services.GstTaxCalculator>();

        return services;
    }
}
