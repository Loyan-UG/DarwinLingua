using Microsoft.Extensions.DependencyInjection;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Infrastructure.Repositories;
using DarwinLingua.Catalog.Infrastructure.Seed;
using DarwinLingua.Infrastructure.Persistence.Abstractions;

namespace DarwinLingua.Catalog.Infrastructure.DependencyInjection;

/// <summary>
/// Registers catalog infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the catalog infrastructure module to the service collection.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ITopicRepository, TopicRepository>();
        services.AddScoped<IGrammarTopicRepository, GrammarTopicRepository>();
        services.AddScoped<IExpressionRepository, ExpressionRepository>();
        services.AddScoped<IExerciseRepository, ExerciseRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IWritingTemplateRepository, WritingTemplateRepository>();
        services.AddScoped<ICulturalNoteRepository, CulturalNoteRepository>();
        services.AddScoped<IExamPrepRepository, ExamPrepRepository>();
        services.AddScoped<IUnifiedLearningSearchRepository, UnifiedLearningSearchRepository>();
        services.AddScoped<IWordEntryRepository, WordEntryRepository>();
        services.AddScoped<IWordCollectionRepository, WordCollectionRepository>();
        services.AddScoped<IDialogueLessonRepository, DialogueLessonRepository>();
        services.AddScoped<ITalkTopicRepository, TalkTopicRepository>();
        services.AddScoped<IConversationStarterRepository, ConversationStarterRepository>();
        services.AddScoped<IEventPreparationRepository, EventPreparationRepository>();
        services.AddScoped<IConversationEventRepository, ConversationEventRepository>();
        services.AddScoped<IOrganizerProfileRepository, OrganizerProfileRepository>();
        services.AddSingleton<IDatabaseSeeder, CatalogReferenceDataSeeder>();
        services.AddSingleton<IDatabaseSeeder, CatalogWordCollectionSeeder>();

        return services;
    }
}
