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
/// Verifies Web/API Exam Prep repository behavior against PostgreSQL-specific search semantics.
/// </summary>
public sealed class ExamPrepPostgresRepositoryTests
{
    private const string DefaultDockerContainerName = "darwinlingua-postgres";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task ExamPrepRepositories_ShouldUsePostgresSearchAndProjectLocalizedHelpers()
    {
        string databaseName = $"darwin_exam_prep_test_{Guid.NewGuid():N}"[..48];
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
                ExamProfile profile = CreateProfile(nowUtc);
                ExamPrepUnit speakingUnit = CreateUnit(nowUtc);
                ExamPrepUnit writingUnit = CreateUnit(
                    nowUtc,
                    "a2-goethe-email-basics",
                    "Eine kurze E-Mail planen",
                    "Plane eine kurze A2-E-Mail.",
                    CefrLevel.A2,
                    "writing",
                    "email",
                    "writing",
                    sortOrder: 20);

                dbContext.ExamProfiles.Add(profile);
                dbContext.ExamPrepUnits.AddRange(speakingUnit, writingUnit);
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IExamPrepRepository repository = serviceProvider.GetRequiredService<IExamPrepRepository>();
            IReadOnlyList<ExamProfileModel> profiles = await repository.GetPublishedExamProfilesAsync("fa", CancellationToken.None);

            ExamProfileModel profileModel = Assert.Single(profiles);
            Assert.Equal("goethe-a2", profileModel.Key);
            Assert.Equal("گوته A2", profileModel.LearnerLanguageDisplayName);

            IReadOnlyList<ExamPrepUnitListItemModel> units = await repository.GetPublishedExamPrepUnitsAsync(
                new ExamPrepListFilterModel("goethe-a2", "A2", "exam-preparation", "roleplay", "speaking", "ROLLENSPIEL"),
                "fa",
                CancellationToken.None);

            ExamPrepUnitListItemModel item = Assert.Single(units);
            Assert.Equal("a2-goethe-speaking-roleplay", item.Slug);
            Assert.Equal("راهبرد نقش‌آفرینی شفاهی", item.LearnerLanguageTitle);
            Assert.Equal("برای یک نقش‌آفرینی کوتاه A2 آماده شو.", item.LearnerLanguageShortDescription);

            ExamPrepUnitDetailModel? detail = await repository.GetPublishedExamPrepUnitBySlugAsync(
                "a2-goethe-speaking-roleplay",
                "fa",
                CancellationToken.None);

            Assert.NotNull(detail);
            Assert.Equal("از پرسش‌ها و پاسخ‌های کوتاه و روشن استفاده کن.", detail.LearnerLanguageExplanation);
            Assert.Equal("وقتی لازم است، درخواست توضیح بیشتر کن.", Assert.Single(detail.LearnerLanguageStrategyNotes));
            Assert.Equal("مستقیم به خواسته پاسخ بده.", Assert.Single(detail.LearnerLanguageChecklist));

            IUnifiedLearningSearchRepository searchRepository = serviceProvider.GetRequiredService<IUnifiedLearningSearchRepository>();
            IReadOnlyList<UnifiedLearningSearchResultModel> results = await searchRepository.SearchAsync(
                new UnifiedLearningSearchFilterModel("ROLLENSPIEL", "A2", "exam-prep", null, null),
                CancellationToken.None);

            UnifiedLearningSearchResultModel result = Assert.Single(results);
            Assert.Equal("exam-prep", result.ResultType);
            Assert.Equal("/exam-prep/a2-goethe-speaking-roleplay", result.Url);
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

    private static ExamProfile CreateProfile(DateTime nowUtc) =>
        new(
            Guid.NewGuid(),
            "goethe-a2",
            "Goethe A2",
            "A2",
            "Vorbereitung auf die Goethe-A2-Pruefung.",
            PublicationStatus.Active,
            10,
            nowUtc,
            JsonSerializer.Serialize(Translations("Goethe A2", "گوته A2"), JsonOptions),
            JsonSerializer.Serialize(Translations("Goethe A2 exam preparation", "آمادگی آزمون گوته A2"), JsonOptions));

    private static ExamPrepUnit CreateUnit(
        DateTime nowUtc,
        string slug = "a2-goethe-speaking-roleplay",
        string title = "Strategie fuer das Sprechrollenspiel",
        string shortDescription = "Bereite ein kurzes A2-Rollenspiel vor.",
        CefrLevel cefrLevel = CefrLevel.A2,
        string examSection = "speaking",
        string taskType = "roleplay",
        string skillFocus = "exam-preparation",
        int sortOrder = 10) =>
        new(
            Guid.NewGuid(),
            slug,
            "goethe-a2",
            title,
            shortDescription,
            cefrLevel,
            examSection,
            taskType,
            skillFocus,
            "Nutze kurze, klare Fragen und Antworten.",
            JsonSerializer.Serialize(new[] { "Ask for clarification when needed." }, JsonOptions),
            JsonSerializer.Serialize(new[] { "Answer the prompt directly." }, JsonOptions),
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            PublicationStatus.Active,
            sortOrder,
            nowUtc,
            JsonSerializer.Serialize(Translations("Speaking roleplay strategy", "راهبرد نقش‌آفرینی شفاهی"), JsonOptions),
            JsonSerializer.Serialize(Translations("Prepare a short A2 roleplay.", "برای یک نقش‌آفرینی کوتاه A2 آماده شو."), JsonOptions),
            JsonSerializer.Serialize(Translations("Use short, clear questions and answers.", "از پرسش‌ها و پاسخ‌های کوتاه و روشن استفاده کن."), JsonOptions),
            JsonSerializer.Serialize(TextListTranslations(["Ask for clarification when needed."], ["وقتی لازم است، درخواست توضیح بیشتر کن."]), JsonOptions),
            JsonSerializer.Serialize(TextListTranslations(["Answer the prompt directly."], ["مستقیم به خواسته پاسخ بده."]), JsonOptions));

    private static object[] Translations(string english, string persian) =>
    [
        new { language = "en", text = english },
        new { language = "fa", text = persian },
    ];

    private static object[] TextListTranslations(string[] english, string[] persian) =>
    [
        new { language = "en", texts = english },
        new { language = "fa", texts = persian },
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
