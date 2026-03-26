using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Application.DependencyInjection;

/// <summary>
/// Registers content-operations application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the content-operations application module to the service collection.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddContentOpsApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IContentImportService, ContentImportService>();

        return services;
    }
}
