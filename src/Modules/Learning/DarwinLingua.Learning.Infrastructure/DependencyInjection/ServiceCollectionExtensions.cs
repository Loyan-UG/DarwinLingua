using Microsoft.Extensions.DependencyInjection;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Infrastructure.Repositories;
using DarwinLingua.Learning.Infrastructure.Services;

namespace DarwinLingua.Learning.Infrastructure.DependencyInjection;

/// <summary>
/// Registers learning infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the learning infrastructure module to the service collection.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddLearningInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IUserLearningProfileRepository, UserLearningProfileRepository>();
        services.AddScoped<IUserFavoriteWordRepository, UserFavoriteWordRepository>();
        services.AddScoped<IUserFavoriteWordCatalogReader, UserFavoriteWordCatalogReader>();
        services.AddScoped<IUserWordStateRepository, UserWordStateRepository>();
        services.AddScoped<IUserWordStateCatalogReader, UserWordStateCatalogReader>();
        services.AddScoped<ILearningPreferenceLanguageValidator, LearningPreferenceLanguageValidator>();

        return services;
    }
}
