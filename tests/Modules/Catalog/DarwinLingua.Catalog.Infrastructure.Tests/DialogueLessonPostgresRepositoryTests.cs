using System.Diagnostics;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
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

public sealed class DialogueLessonPostgresRepositoryTests
{
    private const string DefaultDockerContainerName = "darwinlingua-postgres";

    [Fact]
    public async Task GetPublishedDialogueBySlugAsync_ShouldProjectPrimaryAndSecondaryMeanings()
    {
        string databaseName = $"darwin_dialogue_test_{Guid.NewGuid():N}"[..46];
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
                dbContext.DialogueLessons.Add(CreateDialogueLesson());
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IDialogueLessonRepository repository = serviceProvider.GetRequiredService<IDialogueLessonRepository>();

            DialogueLessonDetailModel? detail = await repository.GetPublishedDialogueBySlugAsync(
                "a1-dual-meaning-dialogue",
                "fa",
                "en",
                CancellationToken.None);

            Assert.NotNull(detail);
            Assert.Equal("راهنما: اول سلام کن، بعد نامت را بگو.", Assert.Single(detail.SpeakingPrompts).PrimaryMeaning);
            Assert.Equal("Guide: greet first, then say your name.", Assert.Single(detail.SpeakingPrompts).SecondaryMeaning);
            Assert.Equal("من یک قرار لازم دارم.", Assert.Single(detail.DialogueTurns).PrimaryMeaning);
            Assert.Equal("I need an appointment.", Assert.Single(detail.DialogueTurns).SecondaryMeaning);
            Assert.Equal("برای درخواست مودبانه استفاده می‌شود.", Assert.Single(detail.UsefulPhrases).PrimaryMeaning);
            Assert.Equal("Used for a polite request.", Assert.Single(detail.UsefulPhrases).SecondaryMeaning);

            DialogueQuestionModel question = Assert.Single(detail.Questions);
            Assert.Equal("شخص چه چیزی لازم دارد؟", question.PrimaryMeaning);
            Assert.Equal("What does the person need?", question.SecondaryMeaning);

            DialogueAnswerModel answer = Assert.Single(question.Answers);
            Assert.Equal("یک قرار.", answer.PrimaryMeaning);
            Assert.Equal("An appointment.", answer.SecondaryMeaning);
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
    public async Task GetPublishedDialogueBySlugAsync_ShouldOmitSecondaryMeanings_WhenSecondaryLanguageIsUnavailable()
    {
        string databaseName = $"darwin_dialogue_test_{Guid.NewGuid():N}"[..46];
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
                dbContext.DialogueLessons.Add(CreateDialogueLesson());
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IDialogueLessonRepository repository = serviceProvider.GetRequiredService<IDialogueLessonRepository>();

            DialogueLessonDetailModel? detail = await repository.GetPublishedDialogueBySlugAsync(
                "a1-dual-meaning-dialogue",
                "fa",
                null,
                CancellationToken.None);

            Assert.NotNull(detail);
            Assert.Equal("راهنما: اول سلام کن، بعد نامت را بگو.", Assert.Single(detail.SpeakingPrompts).PrimaryMeaning);
            Assert.Null(Assert.Single(detail.SpeakingPrompts).SecondaryMeaning);
            Assert.Equal("من یک قرار لازم دارم.", Assert.Single(detail.DialogueTurns).PrimaryMeaning);
            Assert.Null(Assert.Single(detail.DialogueTurns).SecondaryMeaning);
            Assert.Equal("برای درخواست مودبانه استفاده می‌شود.", Assert.Single(detail.UsefulPhrases).PrimaryMeaning);
            Assert.Null(Assert.Single(detail.UsefulPhrases).SecondaryMeaning);

            DialogueQuestionModel question = Assert.Single(detail.Questions);
            Assert.Equal("شخص چه چیزی لازم دارد؟", question.PrimaryMeaning);
            Assert.Null(question.SecondaryMeaning);

            DialogueAnswerModel answer = Assert.Single(question.Answers);
            Assert.Equal("یک قرار.", answer.PrimaryMeaning);
            Assert.Null(answer.SecondaryMeaning);
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

    private static DialogueLesson CreateDialogueLesson()
    {
        DateTime nowUtc = DateTime.UtcNow;
        DialogueLesson lesson = new(
            Guid.NewGuid(),
            "a1-dual-meaning-dialogue",
            "Einen Termin erfragen",
            "Ein kurzer Dialog zum Testen der Bedeutungsprojektion.",
            "Lernende fragen nach einem Termin.",
            CefrLevel.A1,
            "alltag",
            PublicationStatus.Active,
            10,
            nowUtc);

        lesson.UpdateExamMetadata(
            "practice-dialogue",
            "face-to-face",
            "formal",
            5,
            "Kurzer Testdialog.",
            "Nicht pruefungsgebunden.",
            nowUtc);
        lesson.AddSkillFocus(Guid.NewGuid(), "speaking", 10, nowUtc);
        lesson.AddSpeakingFunction(Guid.NewGuid(), "requesting", 10, nowUtc);

        DialogueSpeakingPrompt prompt = lesson.AddSpeakingPrompt(
            Guid.NewGuid(),
            10,
            "instruction",
            "Begruessen Sie die Person und nennen Sie Ihren Namen.",
            nowUtc);
        prompt.AddTranslation(Guid.NewGuid(), LanguageCode.From("fa"), "راهنما: اول سلام کن، بعد نامت را بگو.", nowUtc);
        prompt.AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), "Guide: greet first, then say your name.", nowUtc);

        DialogueTurn turn = lesson.AddDialogueTurn(
            Guid.NewGuid(),
            10,
            "learner",
            "Guten Tag, ich brauche einen Termin.",
            nowUtc);
        turn.AddTranslation(Guid.NewGuid(), LanguageCode.From("fa"), "من یک قرار لازم دارم.", nowUtc);
        turn.AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), "I need an appointment.", nowUtc);

        DialoguePhrase phrase = lesson.AddUsefulPhrase(
            Guid.NewGuid(),
            10,
            "Ich brauche einen Termin.",
            "Hoefliche Bitte in einer Alltagssituation.",
            nowUtc);
        phrase.AddTranslation(Guid.NewGuid(), LanguageCode.From("fa"), "برای درخواست مودبانه استفاده می‌شود.", nowUtc);
        phrase.AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), "Used for a polite request.", nowUtc);

        DialogueQuestion question = lesson.AddQuestion(
            Guid.NewGuid(),
            10,
            "Was braucht die Person?",
            nowUtc);
        question.AddTranslation(Guid.NewGuid(), LanguageCode.From("fa"), "شخص چه چیزی لازم دارد؟", nowUtc);
        question.AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), "What does the person need?", nowUtc);

        DialogueAnswer answer = question.AddAnswer(
            Guid.NewGuid(),
            10,
            "Einen Termin.",
            true,
            "Richtig.",
            nowUtc);
        answer.AddTranslation(Guid.NewGuid(), LanguageCode.From("fa"), "یک قرار.", nowUtc);
        answer.AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), "An appointment.", nowUtc);

        return lesson;
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
