using Microsoft.Extensions.DependencyInjection;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Services;

namespace DarwinLingua.Learning.Application.DependencyInjection;

/// <summary>
/// Registers learning application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the learning application module to the service collection.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddLearningApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IUserLearningProfileService, UserLearningProfileService>();
        services.AddScoped<IUserFavoriteWordService, UserFavoriteWordService>();
        services.AddScoped<IUserWordStateService, UserWordStateService>();

        return services;
    }
}
