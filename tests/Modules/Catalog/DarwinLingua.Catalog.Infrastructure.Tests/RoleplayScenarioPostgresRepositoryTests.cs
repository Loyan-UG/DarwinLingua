using System.Diagnostics;
using System.Text.Json;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DarwinLingua.Catalog.Infrastructure.Tests;

/// <summary>
/// Verifies Web/API RoleplayScenario repository behavior against PostgreSQL-specific search semantics.
/// </summary>
public sealed class RoleplayScenarioPostgresRepositoryTests
{
    private const string DefaultDockerContainerName = "darwinlingua-postgres";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task RoleplayScenarioRepositories_ShouldUsePostgresSearchAndProjectPlayableContent()
    {
        string databaseName = $"darwin_roleplay_test_{Guid.NewGuid():N}"[..48];
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
                Topic topic = CreateTopic("roleplay-postgres-topic", nowUtc);
                RoleplayScenario scenario = CreateRoleplayScenario(
                    "a2-termin-verschieben",
                    "Termin verschieben",
                    "Practice rescheduling an appointment.",
                    CefrLevel.A2,
                    "appointments",
                    "reschedule-appointment",
                    "phone",
                    "formal",
                    ["goethe-a2"],
                    ["speaking", "roleplay", "appointment-management"],
                    nowUtc);
                scenario.AddTopic(Guid.NewGuid(), topic.Id, true, nowUtc);

                RoleplayScenario otherScenario = CreateRoleplayScenario(
                    "b1-work-problem",
                    "Problem im Team erklären",
                    "Practice explaining a workplace problem.",
                    CefrLevel.B1,
                    "workplace",
                    "explain-problem",
                    "workplace",
                    "neutral",
                    ["berufssprache-b1"],
                    ["speaking", "workplace-communication"],
                    nowUtc,
                    sortOrder: 20);

                dbContext.Topics.Add(topic);
                dbContext.RoleplayScenarios.AddRange(scenario, otherScenario);
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IRoleplayScenarioRepository roleplayRepository = serviceProvider.GetRequiredService<IRoleplayScenarioRepository>();
            IReadOnlyList<RoleplayScenarioListItemModel> roleplays = await roleplayRepository.GetPublishedRoleplayScenariosAsync(
                new RoleplayScenarioListFilterModel(
                    "A2",
                    "appointments",
                    "roleplay-postgres-topic",
                    "goethe-a2",
                    "appointment-management",
                    "reschedule-appointment",
                    "phone",
                    "formal",
                    "TERMIN"),
                "fa",
                CancellationToken.None);

            RoleplayScenarioListItemModel item = Assert.Single(roleplays);
            Assert.Equal("a2-termin-verschieben", item.Slug);
            Assert.Equal(["roleplay-postgres-topic"], item.TopicKeys);
            Assert.Contains("goethe-a2", item.ExamProfiles);
            Assert.Contains("appointment-management", item.SkillFocus);

            RoleplayScenarioDetailModel? detail = await roleplayRepository.GetPublishedRoleplayScenarioBySlugAsync(
                "a2-termin-verschieben",
                "fa",
                CancellationToken.None);

            Assert.NotNull(detail);
            Assert.Equal("زبان‌آموز", Assert.Single(detail.Roles, role => role.RoleKey == "learner").LearnerLanguageDisplayName);
            Assert.Equal("سلام، چطور می‌توانم کمک کنم؟", Assert.Single(detail.Turns, turn => turn.SortOrder == 1).LearnerLanguageText);
            RoleplayScenarioAnswerChoiceGroupModel choices = Assert.Single(detail.AnswerChoices);
            Assert.Contains(choices.Choices, choice => choice.IsCorrect);
            Assert.Equal("خوب: درخواست روشن و مؤدبانه است.", Assert.Single(choices.Choices, choice => choice.IsCorrect).LearnerLanguageFeedback);
            Assert.Equal("از bitte برای لحن مؤدبانه استفاده کن.", Assert.Single(detail.StaticFeedback).LearnerLanguageText);
            RoleplayScenarioImageSlotModel imageSlot = Assert.Single(detail.ImageSlots);
            Assert.Null(imageSlot.AssetPath);
            Assert.Equal("اتاق پذیرش برای تماس وقت.", imageSlot.LearnerLanguageAltText);

            IUnifiedLearningSearchRepository searchRepository = serviceProvider.GetRequiredService<IUnifiedLearningSearchRepository>();
            IReadOnlyList<UnifiedLearningSearchResultModel> results = await searchRepository.SearchAsync(
                new UnifiedLearningSearchFilterModel("TERMIN", "A2", "roleplay", null, null),
                CancellationToken.None);

            UnifiedLearningSearchResultModel result = Assert.Single(results);
            Assert.Equal("roleplay", result.ResultType);
            Assert.Equal("Termin verschieben", result.Title);
            Assert.Equal("/roleplays/a2-termin-verschieben", result.Url);
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

    private static async Task CreateDatabaseAsync(string databaseName, CancellationToken cancellationToken)
    {
        await RunDockerPsqlAsync(
            $"""CREATE DATABASE "{databaseName}" OWNER darwinlingua_app;""",
            cancellationToken);
    }

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

    private static Topic CreateTopic(string key, DateTime nowUtc)
    {
        Topic topic = new(Guid.NewGuid(), key, 10, true, nowUtc);
        topic.AddOrUpdateLocalization(Guid.NewGuid(), LanguageCode.From("en"), key, nowUtc);
        return topic;
    }

    private static RoleplayScenario CreateRoleplayScenario(
        string slug,
        string title,
        string learnerGoal,
        CefrLevel cefrLevel,
        string category,
        string taskType,
        string interactionMode,
        string register,
        string[] examProfiles,
        string[] skillFocus,
        DateTime nowUtc,
        int sortOrder = 10)
    {
        return new RoleplayScenario(
            Guid.NewGuid(),
            slug,
            null,
            title,
            JsonSerializer.Serialize(Translations(title, "عنوان فارسی"), JsonOptions),
            "A deterministic roleplay scenario for PostgreSQL repository tests.",
            JsonSerializer.Serialize(Translations("A deterministic roleplay scenario for PostgreSQL repository tests.", "سناریوی نقش‌آفرینی قطعی برای تست‌های repository PostgreSQL."), JsonOptions),
            learnerGoal,
            JsonSerializer.Serialize(Translations(learnerGoal, "هدف فارسی"), JsonOptions),
            cefrLevel,
            category,
            taskType,
            interactionMode,
            register,
            8,
            JsonSerializer.Serialize(examProfiles, JsonOptions),
            JsonSerializer.Serialize(skillFocus, JsonOptions),
            JsonSerializer.Serialize(
                new[]
                {
                    new
                    {
                        roleKey = "learner",
                        displayName = "Learner",
                        translations = Translations("Learner", "زبان‌آموز")
                    },
                    new
                    {
                        roleKey = "staff",
                        displayName = "Staff",
                        translations = Translations("Staff", "کارمند")
                    }
                },
                JsonOptions),
            JsonSerializer.Serialize(
                new[]
                {
                    new
                    {
                        sortOrder = 1,
                        speakerRole = "staff",
                        baseText = "Guten Tag, wie kann ich helfen?",
                        translations = Translations("Hello, how can I help?", "سلام، چطور می‌توانم کمک کنم؟"),
                        function = "open",
                        toneNote = string.Empty,
                        expectedLearnerAction = string.Empty
                    },
                    new
                    {
                        sortOrder = 2,
                        speakerRole = "learner",
                        baseText = "Ich moechte den Termin verschieben.",
                        translations = Translations("I would like to reschedule the appointment.", "می‌خواهم وقت را جابه‌جا کنم."),
                        function = "request",
                        toneNote = "Polite request.",
                        expectedLearnerAction = "reschedule-appointment"
                    }
                },
                JsonOptions),
            JsonSerializer.Serialize(
                new[]
                {
                    new
                    {
                        turnSortOrder = 2,
                        choices = new[]
                        {
                            new
                            {
                                id = "a",
                                text = "Ich moechte den Termin verschieben.",
                                translations = Translations("I would like to reschedule the appointment.", "می‌خواهم وقت را جابه‌جا کنم."),
                                isCorrect = true,
                                feedback = "Good: the request is clear and polite.",
                                feedbackTranslations = Translations("Good: the request is clear and polite.", "خوب: درخواست روشن و مؤدبانه است."),
                                explanationKey = (string?)null
                            },
                            new
                            {
                                id = "b",
                                text = "Ich komme nicht.",
                                translations = Translations("I am not coming.", "من نمی‌آیم."),
                                isCorrect = false,
                                feedback = "This is too abrupt for this situation.",
                                feedbackTranslations = Translations("This is too abrupt for this situation.", "برای این موقعیت خیلی ناگهانی است."),
                                explanationKey = (string?)null
                            }
                        }
                    }
                },
                JsonOptions),
            JsonSerializer.Serialize(
                new[]
                {
                    new
                    {
                        turnSortOrder = 2,
                        feedbackType = "politeness",
                        text = "Use bitte for a polite tone.",
                        translations = Translations("Use bitte for a polite tone.", "از bitte برای لحن مؤدبانه استفاده کن.")
                    }
                },
                JsonOptions),
            JsonSerializer.Serialize(
                new[]
                {
                    new
                    {
                        slotKey = "scene",
                        placement = "header",
                        purpose = "Show a neutral appointment setting.",
                        altText = "Reception room for an appointment call.",
                        altTextTranslations = Translations("Reception room for an appointment call.", "اتاق پذیرش برای تماس وقت."),
                        imagePrompt = "Clean educational illustration of a German reception room, no logos, no text.",
                        assetPath = (string?)null,
                        isRequired = false
                    }
                },
                JsonOptions),
            PublicationStatus.Active,
            sortOrder,
            nowUtc);
    }

    private static object[] Translations(string english, string persian) =>
    [
        new { language = "en", text = english },
        new { language = "fa", text = persian },
    ];
}
