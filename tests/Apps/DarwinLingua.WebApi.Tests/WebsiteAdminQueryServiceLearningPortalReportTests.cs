using System.Text.Json;
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
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.WebApi.Tests;

public sealed class WebsiteAdminQueryServiceLearningPortalReportTests
{
    [Fact]
    public async Task GetSystemReportAsync_ShouldIncludeLearningPortalCountsAndQualitySignals()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_admin_report");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);
            await SeedLearningPortalContentAsync(serviceProvider, CancellationToken.None);

            IWebsiteAdminQueryService service = serviceProvider.GetRequiredService<IWebsiteAdminQueryService>();
            AdminSystemReportResponse report = await service.GetSystemReportAsync(CancellationToken.None);

            Assert.Contains(report.LearningPortal.CountsByType, row => row.Key == "grammar-topic" && row.Count == 2);
            Assert.Contains(report.LearningPortal.CountsByType, row => row.Key == "roleplay" && row.Count == 1);
            Assert.Contains(report.LearningPortal.CountsByCefr, row => row.Key == "A1" && row.Count >= 1);
            Assert.Contains(report.LearningPortal.CountsByCefr, row => row.Key == "B1" && row.Count >= 1);
            Assert.Contains(report.LearningPortal.GrammarByCategory, row => row.Key == "articles" && row.Count == 1);
            Assert.Contains(report.LearningPortal.ExpressionsByType, row => row.Key == "fixed-expression" && row.Count == 1);
            Assert.Contains(report.LearningPortal.ExpressionsByRegister, row => row.Key == "neutral" && row.Count == 1);
            Assert.Contains(report.LearningPortal.ExpressionsByMeaningTransparency, row => row.Key == "pragmatic-formula" && row.Count == 1);
            Assert.Contains(report.LearningPortal.ExpressionsBySafetyRating, row => row.Key == "general" && row.Count == 1);
            Assert.Contains(report.LearningPortal.ExercisesByType, row => row.Key == "multiple-choice" && row.Count == 1);
            Assert.Equal(1, report.LearningPortal.MissingTranslationCount);
            Assert.Equal(1, report.LearningPortal.UnresolvedLinkedContentReferenceCount);
            Assert.Equal(1, report.LearningPortal.GrammarTopicsMissingExercises);
            Assert.Equal(0, report.LearningPortal.ExpressionEntriesMissingEligibilityMetadata);
            Assert.Equal(0, report.LearningPortal.ExpressionEntriesWithOrdinaryLiteralLeakage);
            Assert.Equal(0, report.LearningPortal.ExpressionEntriesMissingTeachingReason);
            Assert.Equal(1, report.LearningPortal.ExpressionEntriesWithFewerThanTwoExamples);
            Assert.NotEmpty(report.LearningPortal.SampleIssues);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

        }
    }

    [Fact]
    public async Task GetSystemReportAsync_ShouldTreatMissingLearningPortalTablesAsEmpty()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_admin_report");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(database.ConnectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);
            await SeedLearningPortalContentAsync(serviceProvider, CancellationToken.None);
            await DropTableAsync(serviceProvider, "Exercises", CancellationToken.None);

            IWebsiteAdminQueryService service = serviceProvider.GetRequiredService<IWebsiteAdminQueryService>();
            AdminSystemReportResponse report = await service.GetSystemReportAsync(CancellationToken.None);

            Assert.Contains(report.LearningPortal.CountsByType, row => row.Key == "expression" && row.Count == 1);
            Assert.Contains(report.LearningPortal.CountsByType, row => row.Key == "exercise" && row.Count == 0);
            Assert.Empty(report.LearningPortal.ExercisesByType);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

        }
    }

    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        ServiceCollection services = new();
        services
            .AddDarwinLinguaInfrastructureForPostgres(connectionString)
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

        ExpressionEntry expression = new(
            Guid.NewGuid(),
            "alles-klar",
            "Alles klar.",
            "Everything clear.",
            "Understood or okay.",
            "Use it to confirm a simple plan.",
            CefrLevel.A1,
            "fixed-expression",
            "neutral",
            "daily-life",
            "de",
            false,
            PublicationStatus.Active,
            10,
            now,
            "pragmatic-formula",
            "It is a conventional confirmation formula.",
            "general",
            0,
            false);
        expression.AddMeaning(Guid.NewGuid(), LanguageCode.From("en"), "Understood or okay.", "Everything clear.", "Use it to confirm a simple plan.", now);

        RoleplayScenario roleplayScenario = new(
            Guid.NewGuid(),
            "b1-polito-complaint-roleplay",
            null,
            "Polite complaint roleplay",
            "[]",
            "A deterministic roleplay for making a polite complaint.",
            "[]",
            "Explain a problem and choose a polite next step.",
            "[]",
            CefrLevel.B1,
            "customer-service",
            "complain-politely",
            "service-counter",
            "formal",
            12,
            JsonSerializer.Serialize(new[] { "goethe-b1" }, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            JsonSerializer.Serialize(new[] { "speaking", "roleplay", "complaint-handling" }, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            PublicationStatus.Active,
            30,
            now);

        dbContext.GrammarTopics.Add(grammarTopic);
        dbContext.GrammarTopics.Add(grammarTopicWithoutExercise);
        dbContext.Exercises.Add(exercise);
        dbContext.ExpressionEntries.Add(expression);
        dbContext.RoleplayScenarios.Add(roleplayScenario);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task DropTableAsync(
        ServiceProvider serviceProvider,
        string tableName,
        CancellationToken cancellationToken)
    {
        IDbContextFactory<DarwinLinguaDbContext> dbContextFactory = serviceProvider
            .GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        string sql = tableName switch
        {
            "Exercises" => "DROP TABLE \"Exercises\"",
            _ => throw new ArgumentOutOfRangeException(nameof(tableName), tableName, "Unsupported test table."),
        };

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken)
            .ConfigureAwait(false);
    }

}
