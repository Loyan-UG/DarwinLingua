using DarwinLingua.Practice.Application.Abstractions;
using DarwinLingua.Practice.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Practice.Application.DependencyInjection;

/// <summary>
/// Registers practice application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the practice application module to the service collection.
    /// </summary>
    public static IServiceCollection AddPracticeApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IPracticeFlashcardAnswerService, PracticeFlashcardAnswerService>();
        services.AddScoped<IPracticeOverviewService, PracticeOverviewService>();
        services.AddScoped<IPracticeReviewQueueService, PracticeReviewQueueService>();

        return services;
    }
}
