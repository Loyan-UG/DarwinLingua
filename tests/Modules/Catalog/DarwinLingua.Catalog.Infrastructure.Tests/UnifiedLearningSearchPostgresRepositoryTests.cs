using System.Diagnostics;
using System.Text.Json;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DarwinLingua.Catalog.Infrastructure.Tests;

public sealed class UnifiedLearningSearchPostgresRepositoryTests
{
    private const string DefaultDockerContainerName = "darwinlingua-postgres";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task SearchAsync_ShouldRankAndFilterSeededLearningContentAcrossTypes()
    {
        string databaseName = $"darwin_unified_search_test_{Guid.NewGuid():N}"[..48];
        string connectionString = BuildAppConnectionString(databaseName);
        ServiceProvider? serviceProvider = null;

        await CreateDatabaseAsync(databaseName, CancellationToken.None);

        try
        {
            serviceProvider = BuildServiceProvider(connectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                DateTime nowUtc = DateTime.UtcNow;
                SeedCourse(dbContext, nowUtc);

                dbContext.GrammarTopics.Add(new GrammarTopic(
                    Guid.NewGuid(),
                    "b1-integration-nebensaetze",
                    "Nebensaetze in Integrationstexten",
                    "Nebensaetze helfen, Gruende und Folgen im Alltag der Integration genauer zu beschreiben.",
                    CefrLevel.B1,
                    "sentence-structure",
                    PublicationStatus.Active,
                    40,
                    nowUtc));

                dbContext.WritingTemplates.Add(new WritingTemplate(
                    Guid.NewGuid(),
                    "b1-integration-kursleitung-fragen",
                    "Integration freundlich erfragen",
                    "Eine formelle Nachricht mit einer klaren Frage zum Integrationskurs.",
                    CefrLevel.B1,
                    "email-to-school",
                    "Du fragst die Kursleitung nach einem Termin oder einer Information.",
                    "formal",
                    "Sehr geehrte Damen und Herren, ich habe eine Frage zu {{thema}}.",
                    "Formuliere die Frage klar und freundlich.",
                    JsonSerializer.Serialize(new[] { "thema" }, JsonOptions),
                    "Sehr geehrte Damen und Herren, ich habe eine Frage zu meinem Integrationstermin.",
                    "[]",
                    "[]",
                    "[]",
                    "[]",
                    "[]",
                    PublicationStatus.Active,
                    20,
                    nowUtc));

                dbContext.CountryGuidanceNotes.Add(new CountryGuidanceNote(
                    Guid.NewGuid(),
                    "b1-integration-im-alltag",
                    "Integration",
                    "Integration bedeutet, Rechte zu kennen, Pflichten ernst zu nehmen und am Alltag teilzunehmen.",
                    CefrLevel.B1,
                    "society-and-family",
                    "Integration ist mehr als ein Test. Sie zeigt sich im Alltag: in Sprache, Arbeit, Schule, Nachbarschaft, Respekt und Mitwirkung.",
                    JsonSerializer.Serialize(new[] { "Integration verbindet eigenes Leben mit den Regeln und Moeglichkeiten der Gesellschaft." }, JsonOptions),
                    "[]",
                    "[]",
                    "[]",
                    null,
                    "[]",
                    "[]",
                    "[]",
                    "[]",
                    JsonSerializer.Serialize(new[] { "b1-integration-im-alltag-verstehen" }, JsonOptions),
                    PublicationStatus.Active,
                    10,
                    nowUtc));

                await dbContext.SaveChangesAsync(CancellationToken.None);

                await InsertWrongLanguageGrammarTopicAsync(dbContext, nowUtc);
            }

            IUnifiedLearningSearchRepository repository = serviceProvider.GetRequiredService<IUnifiedLearningSearchRepository>();
            IReadOnlyList<UnifiedLearningSearchResultModel> allResults = await repository.SearchAsync(
                new UnifiedLearningSearchFilterModel("Integration", "B1", null, null, null),
                CancellationToken.None);

            Assert.Equal("country-guidance", allResults[0].ResultType);
            Assert.Equal("/learn/de/country-guidance/de/b1-integration-im-alltag", allResults[0].Url);
            Assert.Equal(4, allResults.Count);
            Assert.DoesNotContain(allResults, result => result.Url.StartsWith("/learn/en/", StringComparison.Ordinal));
            Assert.DoesNotContain(allResults, result => string.Equals(result.Title, "Integration in English", StringComparison.Ordinal));
            Assert.Contains(allResults, result => result.ResultType == "grammar" && result.Url == "/learn/de/grammar/b1-integration-nebensaetze");
            Assert.Contains(allResults, result => result.ResultType == "course-lesson" && result.Url == "/learn/de/courses/b1-integration/b1-integration-im-alltag-verstehen");
            Assert.Contains(allResults, result => result.ResultType == "writing-template" && result.Url == "/learn/de/writing-templates/b1-integration-kursleitung-fragen");

            IReadOnlyList<UnifiedLearningSearchResultModel> courseResults = await repository.SearchAsync(
                new UnifiedLearningSearchFilterModel("Integration", "B1", "course-lesson", "b1-integration", null),
                CancellationToken.None);

            UnifiedLearningSearchResultModel courseResult = Assert.Single(courseResults);
            Assert.Equal("Integration im Alltag verstehen", courseResult.Title);
            Assert.Contains("title", courseResult.MatchedFields);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            await DropDatabaseAsync(databaseName, CancellationToken.None);
        }
    }

    [Fact]
    public async Task SearchAsync_ShouldStayBoundedOnSeededBulkCorpus()
    {
        string databaseName = $"darwin_unified_search_perf_{Guid.NewGuid():N}"[..48];
        string connectionString = BuildAppConnectionString(databaseName);
        ServiceProvider? serviceProvider = null;

        await CreateDatabaseAsync(databaseName, CancellationToken.None);

        try
        {
            serviceProvider = BuildServiceProvider(connectionString);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                SeedBulkSearchCorpus(dbContext, DateTime.UtcNow);
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IUnifiedLearningSearchRepository repository = serviceProvider.GetRequiredService<IUnifiedLearningSearchRepository>();

            Stopwatch stopwatch = Stopwatch.StartNew();
            IReadOnlyList<UnifiedLearningSearchResultModel> results = await repository.SearchAsync(
                new UnifiedLearningSearchFilterModel("Integration", null, null, null, null),
                CancellationToken.None);
            stopwatch.Stop();

            Assert.Equal(80, results.Count);
            Assert.All(results, result => Assert.True(result.RelevanceScore > 0));
            Assert.InRange(stopwatch.Elapsed, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            await DropDatabaseAsync(databaseName, CancellationToken.None);
        }
    }

    private static void SeedCourse(DarwinLinguaDbContext dbContext, DateTime nowUtc)
    {
        CoursePath path = new(
            Guid.NewGuid(),
            "b1-integration",
            "B1 Integration",
            "Ein Lernpfad fuer Alltag, Gesellschaft und Beteiligung.",
            CefrLevel.B1,
            null,
            PublicationStatus.Active,
            10,
            nowUtc);

        CourseModule module = new(
            Guid.NewGuid(),
            path.Slug,
            "b1-gesellschaft-und-alltag",
            "Gesellschaft und Alltag",
            "Integration im Alltag verstehen und sprachlich beschreiben.",
            1,
            CefrLevel.B1,
            PublicationStatus.Active,
            10,
            nowUtc);
        module.AttachToCoursePath(path.Id);

        CourseLesson lesson = new(
            Guid.NewGuid(),
            path.Slug,
            module.Slug,
            "b1-integration-im-alltag-verstehen",
            1,
            "Integration im Alltag verstehen",
            "Du lernst, Integration im Alltag mit klaren Beispielen zu beschreiben.",
            "Integration zeigt sich in Sprache, Regeln, Respekt, Arbeit, Schule und Nachbarschaft.",
            CefrLevel.B1,
            25,
            JsonSerializer.Serialize(new[] { "Integration im Alltag beschreiben" }, JsonOptions),
            "[]",
            null,
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            "Fasse zwei Beispiele zusammen.",
            "Schreibe drei Saetze zu Integration im Alltag.",
            PublicationStatus.Active,
            30,
            nowUtc);
        lesson.AttachToCourseModule(module.Id);

        dbContext.CoursePaths.Add(path);
        dbContext.CourseModules.Add(module);
        dbContext.CourseLessons.Add(lesson);
    }

    private static async Task InsertWrongLanguageGrammarTopicAsync(DarwinLinguaDbContext dbContext, DateTime nowUtc)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO "GrammarTopics" (
                "Id",
                "TargetLearningLanguageCode",
                "Slug",
                "Title",
                "ShortDescription",
                "ContentRevision",
                "TitleLocalizedJson",
                "ShortDescriptionLocalizedJson",
                "ImageSlotsJson",
                "CefrLevel",
                "GrammarCategory",
                "PublicationStatus",
                "SortOrder",
                "CreatedAtUtc",
                "UpdatedAtUtc")
            VALUES (
                {Guid.NewGuid()},
                {"en"},
                {"b1-integration-english-leakage"},
                {"Integration in English"},
                {"This row simulates wrong-language content that must not appear in German search."},
                {0},
                {"[]"},
                {"[]"},
                {"[]"},
                {"B1"},
                {"sentence-structure"},
                {"Active"},
                {1},
                {nowUtc},
                {nowUtc});
            """).ConfigureAwait(false);
    }

    private static void SeedBulkSearchCorpus(DarwinLinguaDbContext dbContext, DateTime nowUtc)
    {
        CoursePath path = new(
            Guid.NewGuid(),
            "b1-integration-performance",
            "B1 Integration Performance",
            "Ein Suchpfad fuer Performance-Schutz.",
            CefrLevel.B1,
            null,
            PublicationStatus.Active,
            10,
            nowUtc);

        CourseModule module = new(
            Guid.NewGuid(),
            path.Slug,
            "b1-integration-performance-module",
            "Integration Performance",
            "Viele kurze Suchlektionen fuer den Performance-Guard.",
            1,
            CefrLevel.B1,
            PublicationStatus.Active,
            10,
            nowUtc);
        module.AttachToCoursePath(path.Id);

        dbContext.CoursePaths.Add(path);
        dbContext.CourseModules.Add(module);

        for (int index = 1; index <= 30; index++)
        {
            CourseLesson lesson = new(
                Guid.NewGuid(),
                path.Slug,
                module.Slug,
                $"b1-integration-performance-lesson-{index:D2}",
                index,
                $"Integration Alltag {index:D2}",
                "Eine kurze Lektion ueber Integration, Alltag und Beteiligung.",
                "Integration wird in Alltag, Arbeit, Schule und Nachbarschaft praktisch sichtbar.",
                CefrLevel.B1,
                12,
                JsonSerializer.Serialize(new[] { "Integration im Alltag beschreiben" }, JsonOptions),
                "[]",
                null,
                "[]",
                "[]",
                "[]",
                "[]",
                "[]",
                "[]",
                "[]",
                "Fasse einen Integrationsaspekt zusammen.",
                "Schreibe zwei Saetze zur Integration.",
                PublicationStatus.Active,
                index,
                nowUtc);
            lesson.AttachToCourseModule(module.Id);
            dbContext.CourseLessons.Add(lesson);

            dbContext.GrammarTopics.Add(new GrammarTopic(
                Guid.NewGuid(),
                $"b1-integration-performance-grammar-{index:D2}",
                $"Integration Satzbau {index:D2}",
                "Kurze Grammatik fuer klare Integrationstexte.",
                CefrLevel.B1,
                "sentence-structure",
                PublicationStatus.Active,
                index,
                nowUtc));

            dbContext.WritingTemplates.Add(new WritingTemplate(
                Guid.NewGuid(),
                $"b1-integration-performance-template-{index:D2}",
                $"Integration Nachricht {index:D2}",
                "Eine kurze Nachricht zu Integration und Alltag.",
                CefrLevel.B1,
                "email-to-school",
                "Du schreibst eine kurze Frage zu Integration im Kurs.",
                "formal",
                "Sehr geehrte Damen und Herren, ich habe eine Frage zu {{thema}}.",
                "Nenne das Thema klar und bleibe hoeflich.",
                JsonSerializer.Serialize(new[] { "thema" }, JsonOptions),
                "Sehr geehrte Damen und Herren, ich habe eine Frage zur Integration im Alltag.",
                "[]",
                "[]",
                "[]",
                "[]",
                "[]",
                PublicationStatus.Active,
                index,
                nowUtc));

            dbContext.CountryGuidanceNotes.Add(new CountryGuidanceNote(
                Guid.NewGuid(),
                $"b1-integration-performance-note-{index:D2}",
                $"Integration verstehen {index:D2}",
                "Ein kurzer Hinweis zu Integration, Rechten, Pflichten und Alltag.",
                CefrLevel.B1,
                "society-and-family",
                "Integration bedeutet, Regeln zu kennen, Sprache zu nutzen und im Alltag verantwortlich mitzuwirken.",
                JsonSerializer.Serialize(new[] { "Integration verbindet persoenliche Ziele mit gesellschaftlicher Teilhabe." }, JsonOptions),
                "[]",
                "[]",
                "[]",
                null,
                "[]",
                "[]",
                "[]",
                "[]",
                "[]",
                PublicationStatus.Active,
                index,
                nowUtc));
        }
    }

    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        ServiceCollection services = new();
        services
            .AddDarwinLinguaInfrastructureForPostgres(connectionString)
            .AddCatalogInfrastructure();

        return services.BuildServiceProvider();
    }

    private static string BuildAppConnectionString(string databaseName)
    {
        string? configuredTemplate = Environment.GetEnvironmentVariable("DARWINLINGUA_TEST_POSTGRES_APP_CONNECTION_TEMPLATE");
        if (!string.IsNullOrWhiteSpace(configuredTemplate))
        {
            return string.Format(configuredTemplate, databaseName);
        }

        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = "localhost",
            Port = 5432,
            Database = databaseName,
            Username = "darwinlingua_app",
            Password = "@pP@sS!13;X"
        };

        return builder.ConnectionString;
    }

    private static async Task CreateDatabaseAsync(string databaseName, CancellationToken cancellationToken) =>
        await RunDockerPsqlAsync($"""CREATE DATABASE "{databaseName}" OWNER darwinlingua_app;""", cancellationToken);

    private static async Task DropDatabaseAsync(string databaseName, CancellationToken cancellationToken)
    {
        await RunDockerPsqlAsync(
            $"""
            SELECT pg_terminate_backend(pid)
            FROM pg_stat_activity
            WHERE datname = '{databaseName}'
              AND pid <> pg_backend_pid();
            """,
            cancellationToken);
        await RunDockerPsqlAsync($"""DROP DATABASE IF EXISTS "{databaseName}";""", cancellationToken);
    }

    private static async Task RunDockerPsqlAsync(string sql, CancellationToken cancellationToken)
    {
        string containerName = Environment.GetEnvironmentVariable("DARWINLINGUA_TEST_POSTGRES_CONTAINER") ?? DefaultDockerContainerName;
        ProcessStartInfo startInfo = new()
        {
            FileName = "docker",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        startInfo.ArgumentList.Add("exec");
        startInfo.ArgumentList.Add(containerName);
        startInfo.ArgumentList.Add("psql");
        startInfo.ArgumentList.Add("-U");
        startInfo.ArgumentList.Add("postgres");
        startInfo.ArgumentList.Add("-d");
        startInfo.ArgumentList.Add("postgres");
        startInfo.ArgumentList.Add("-v");
        startInfo.ArgumentList.Add("ON_ERROR_STOP=1");
        startInfo.ArgumentList.Add("-c");
        startInfo.ArgumentList.Add(sql);

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start Docker PostgreSQL helper process.");
        string standardOutput = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        string standardError = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"PostgreSQL test database command failed with exit code {process.ExitCode}.{Environment.NewLine}{standardOutput}{Environment.NewLine}{standardError}");
        }
    }
}
