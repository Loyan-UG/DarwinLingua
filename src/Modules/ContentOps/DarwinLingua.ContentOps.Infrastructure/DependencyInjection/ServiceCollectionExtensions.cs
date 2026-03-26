using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Infrastructure.Repositories;
using DarwinLingua.ContentOps.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.DependencyInjection;

/// <summary>
/// Registers content-operations infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the content-operations infrastructure module to the service collection.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddContentOpsInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IContentImportFileReader, ContentImportFileReader>();
        services.AddScoped<IContentImportParser, ContentImportParser>();
        services.AddScoped<IContentImportRepository, ContentImportRepository>();

        return services;
    }
}
