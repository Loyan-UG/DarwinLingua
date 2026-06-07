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

/// <summary>
/// Verifies Web/API Exercise repository behavior against PostgreSQL-specific search semantics.
/// </summary>
public sealed class ExercisePostgresRepositoryTests
{
    private const string DefaultDockerContainerName = "darwinlingua-postgres";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task ExerciseRepositories_ShouldUsePostgresSearchAndProjectLocalizedHelpers()
    {
        string databaseName = $"darwin_exercise_test_{Guid.NewGuid():N}"[..48];
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
                Exercise exercise = CreateExercise(nowUtc);
                Exercise otherExercise = CreateExercise(nowUtc, "a2-dativ-choice", "Dativ wählen", "Wähle den Dativ.", CefrLevel.A2, "case-selection", "grammar", "grammar-topic", "a2-dative", 20);
                ExerciseSet set = new(
                    Guid.NewGuid(),
                    "a1-core-exercises",
                    "A1 Basisübungen",
                    "Kurze Übungen zu Artikeln.",
                    CefrLevel.A1,
                    "grammar-topic",
                    "a1-articles",
                    PublicationStatus.Active,
                    10,
                    nowUtc,
                    JsonSerializer.Serialize(Translations("A1 basic exercises", "تمرین‌های پایه A1"), JsonOptions),
                    JsonSerializer.Serialize(Translations("Short exercises for articles.", "تمرین‌های کوتاه برای حرف تعریف."), JsonOptions));
                set.AddExercise(Guid.NewGuid(), exercise.Slug, 10, nowUtc);

                dbContext.Exercises.AddRange(exercise, otherExercise);
                dbContext.ExerciseSets.Add(set);
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IExerciseRepository repository = serviceProvider.GetRequiredService<IExerciseRepository>();
            IReadOnlyList<ExerciseListItemModel> exercises = await repository.GetPublishedExercisesAsync(
                new ExerciseListFilterModel("A1", "article-selection", "grammar", "grammar-topic", "a1-articles", "KAFFEE"),
                "fa",
                CancellationToken.None);

            ExerciseListItemModel item = Assert.Single(exercises);
            Assert.Equal("a1-article-choice", item.Slug);
            Assert.Equal("انتخاب حرف تعریف", item.LearnerLanguageTitle);
            Assert.Equal("حرف تعریف درست را انتخاب کن.", item.LearnerLanguageInstruction);

            ExerciseDetailModel? detail = await repository.GetPublishedExerciseBySlugAsync("a1-article-choice", "fa", CancellationToken.None);
            Assert.NotNull(detail);
            Assert.Equal("Kaffee ist maskulin.", detail.Hint);
            Assert.Equal("Kaffee مذکر است.", detail.LearnerLanguageHint);

            ExerciseSetDetailModel? setDetail = await repository.GetPublishedExerciseSetBySlugAsync("a1-core-exercises", "fa", CancellationToken.None);
            Assert.NotNull(setDetail);
            Assert.Equal("تمرین‌های پایه A1", setDetail.LearnerLanguageTitle);
            Assert.Equal("a1-article-choice", Assert.Single(setDetail.Exercises).Slug);

            IUnifiedLearningSearchRepository searchRepository = serviceProvider.GetRequiredService<IUnifiedLearningSearchRepository>();
            IReadOnlyList<UnifiedLearningSearchResultModel> results = await searchRepository.SearchAsync(
                new UnifiedLearningSearchFilterModel("KAFFEE", "A1", "exercise", null, null),
                CancellationToken.None);

            UnifiedLearningSearchResultModel result = Assert.Single(results);
            Assert.Equal("exercise", result.ResultType);
            Assert.Equal("/exercises/a1-article-choice", result.Url);
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

    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        ServiceCollection services = new();
        services
            .AddDarwinLinguaInfrastructureForPostgres(connectionString)
            .AddCatalogInfrastructure();

        return services.BuildServiceProvider();
    }

    private static Exercise CreateExercise(
        DateTime nowUtc,
        string slug = "a1-article-choice",
        string title = "Artikel wählen",
        string instruction = "Wähle den richtigen Artikel für Kaffee.",
        CefrLevel cefrLevel = CefrLevel.A1,
        string exerciseType = "article-selection",
        string targetSkill = "grammar",
        string ownerType = "grammar-topic",
        string? ownerSlug = "a1-articles",
        int sortOrder = 10) =>
        new(
            Guid.NewGuid(),
            slug,
            title,
            instruction,
            cefrLevel,
            exerciseType,
            targetSkill,
            ownerType,
            ownerSlug,
            """{ "stem": "___ Kaffee", "options": [{ "id": "der", "text": "der" }, { "id": "die", "text": "die" }] }""",
            """{ "correctOptionIds": ["der"] }""",
            "Es heißt: der Kaffee.",
            "Kaffee ist maskulin: der Kaffee.",
            "Kaffee ist maskulin.",
            null,
            PublicationStatus.Active,
            sortOrder,
            nowUtc,
            JsonSerializer.Serialize(Translations("Choose the article", "انتخاب حرف تعریف"), JsonOptions),
            JsonSerializer.Serialize(Translations("Choose the correct article.", "حرف تعریف درست را انتخاب کن."), JsonOptions),
            JsonSerializer.Serialize(Translations("The correct form is: der Kaffee.", "شکل درست این است: der Kaffee."), JsonOptions),
            JsonSerializer.Serialize(Translations("Kaffee is masculine: der Kaffee.", "Kaffee مذکر است: der Kaffee."), JsonOptions),
            JsonSerializer.Serialize(Translations("Kaffee is masculine.", "Kaffee مذکر است."), JsonOptions),
            "[]");

    private static object[] Translations(string english, string persian) =>
    [
        new { language = "en", text = english },
        new { language = "fa", text = persian },
    ];

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
