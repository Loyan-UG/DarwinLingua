using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Lexicon;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class WebsiteAdminQueryServiceLearningPortalReportTests
{
    [Fact]
    public async Task GetSystemReportAsync_ShouldIncludeLearningPortalCountsAndQualitySignals()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-admin-report-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);
            await SeedLearningPortalContentAsync(serviceProvider, CancellationToken.None);

            IWebsiteAdminQueryService service = serviceProvider.GetRequiredService<IWebsiteAdminQueryService>();
            AdminSystemReportResponse report = await service.GetSystemReportAsync(CancellationToken.None);

            Assert.Contains(report.LearningPortal.CountsByType, row => row.Key == "grammar-topic" && row.Count == 2);
            Assert.Contains(report.LearningPortal.CountsByCefr, row => row.Key == "A1" && row.Count >= 1);
            Assert.Contains(report.LearningPortal.GrammarByCategory, row => row.Key == "articles" && row.Count == 1);
            Assert.Contains(report.LearningPortal.ExercisesByType, row => row.Key == "multiple-choice" && row.Count == 1);
            Assert.Equal(1, report.LearningPortal.MissingTranslationCount);
            Assert.Equal(1, report.LearningPortal.UnresolvedLinkedContentReferenceCount);
            Assert.Equal(1, report.LearningPortal.GrammarTopicsMissingExercises);
            Assert.NotEmpty(report.LearningPortal.SampleIssues);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            TryDeleteFile(databasePath);
        }
    }

    private static ServiceProvider BuildServiceProvider(string databasePath)
    {
        ServiceCollection services = new();
        services
            .AddDarwinLinguaInfrastructure(options => options.DatabasePath = databasePath)
            .AddScoped<IWebsiteAdminQueryService, WebsiteAdminQueryService>();

        return services.BuildServiceProvider();
    }

    private static async Task SeedLearningPortalContentAsync(
        ServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        IDbContextFactory<DarwinLinguaDbContext> dbContextFactory = serviceProvider
            .GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        DateTime now = DateTime.UtcNow;
        GrammarTopic grammarTopic = new(
            Guid.NewGuid(),
            "a1-articles",
            "A1 articles",
            "Article basics",
            CefrLevel.A1,
            "articles",
            PublicationStatus.Active,
            10,
            now);
        grammarTopic.AddSection(Guid.NewGuid(), 10, "Articles", "Use der, die, das.", now);
        grammarTopic.AddLinkedExercise(Guid.NewGuid(), "missing-exercise", 10, now);
        GrammarTopic grammarTopicWithoutExercise = new(
            Guid.NewGuid(),
            "a1-nouns",
            "A1 nouns",
            "Noun basics",
            CefrLevel.A1,
            "nouns",
            PublicationStatus.Active,
            20,
            now);

        Exercise exercise = new(
            Guid.NewGuid(),
            "a1-article-choice",
            "Article choice",
            "Choose the article.",
            CefrLevel.A1,
            "multiple-choice",
            "grammar",
            "grammar-topic",
            "a1-articles",
            "{}",
            "{}",
            "Correct.",
            "Try again.",
            null,
            null,
            PublicationStatus.Active,
            10,
            now);

        dbContext.GrammarTopics.Add(grammarTopic);
        dbContext.GrammarTopics.Add(grammarTopicWithoutExercise);
        dbContext.Exercises.Add(exercise);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
    }
}
