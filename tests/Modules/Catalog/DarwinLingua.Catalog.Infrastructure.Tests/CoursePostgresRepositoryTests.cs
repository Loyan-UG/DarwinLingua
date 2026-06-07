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

public sealed class CoursePostgresRepositoryTests
{
    private const string DefaultDockerContainerName = "darwinlingua-postgres";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task CourseRepository_ShouldUsePostgresSearchAndProjectLocalizedHelpers()
    {
        string databaseName = $"darwin_course_test_{Guid.NewGuid():N}"[..46];
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
                CoursePath path = new(
                    Guid.NewGuid(),
                    "a1-deutsch-start",
                    "A1 Deutsch Start",
                    "Ein kurzer Lernpfad fuer die ersten Schritte.",
                    CefrLevel.A1,
                    null,
                    PublicationStatus.Active,
                    10,
                    nowUtc,
                    JsonSerializer.Serialize(Translations("A1 German start", "شروع آلمانی A1"), JsonOptions),
                    JsonSerializer.Serialize(Translations("A short learning path for first steps.", "یک مسیر کوتاه برای قدم‌های اول."), JsonOptions));

                CourseModule module = new(
                    Guid.NewGuid(),
                    path.Slug,
                    "a1-erste-schritte",
                    "Erste Schritte",
                    "Begruessungen, Namen und einfache Saetze.",
                    1,
                    CefrLevel.A1,
                    PublicationStatus.Active,
                    10,
                    nowUtc,
                    JsonSerializer.Serialize(Translations("First steps", "قدم‌های اول"), JsonOptions),
                    JsonSerializer.Serialize(Translations("Greetings, names, and simple sentences.", "سلام‌کردن، نام‌ها و جمله‌های ساده."), JsonOptions));
                module.AttachToCoursePath(path.Id);

                CourseLesson lesson = new(
                    Guid.NewGuid(),
                    path.Slug,
                    module.Slug,
                    "a1-begruessung-und-name",
                    1,
                    "Begruessung und Name",
                    "Du lernst, jemanden zu begruessen und deinen Namen zu sagen.",
                    "In dieser Lektion kombinierst du kurze deutsche Saetze mit bekannten Dialogen und Uebungen.",
                    CefrLevel.A1,
                    20,
                    JsonSerializer.Serialize(new[] { "Jemanden begruessen", "Den eigenen Namen sagen" }, JsonOptions),
                    "[]",
                    null,
                    JsonSerializer.Serialize(new[] { "a1-word-order" }, JsonOptions),
                    JsonSerializer.Serialize(new[] { "hallo" }, JsonOptions),
                    JsonSerializer.Serialize(new[] { "guten-morgen" }, JsonOptions),
                    JsonSerializer.Serialize(new[] { "a1-introductions" }, JsonOptions),
                    JsonSerializer.Serialize(new[] { "a1-greetings" }, JsonOptions),
                    JsonSerializer.Serialize(new[] { "a1-core-practice" }, JsonOptions),
                    "[]",
                    "Wiederhole die Begruessung.",
                    "Schreibe zwei kurze Saetze.",
                    PublicationStatus.Active,
                    10,
                    nowUtc,
                    JsonSerializer.Serialize(Translations("Greeting and name", "سلام و نام"), JsonOptions),
                    JsonSerializer.Serialize(Translations("You learn to greet someone and say your name.", "یاد می‌گیری به کسی سلام کنی و نامت را بگویی."), JsonOptions),
                    JsonSerializer.Serialize(Translations("In this lesson you combine short German sentences with known dialogues and exercises.", "در این درس جمله‌های کوتاه آلمانی را با دیالوگ‌ها و تمرین‌های شناخته‌شده ترکیب می‌کنی."), JsonOptions),
                    JsonSerializer.Serialize(TextListTranslations(["Greet someone", "Say your own name"], ["به کسی سلام کنی", "نام خودت را بگویی"]), JsonOptions),
                    JsonSerializer.Serialize(Translations("Repeat the greeting.", "سلام‌کردن را تکرار کن."), JsonOptions),
                    JsonSerializer.Serialize(Translations("Write two short sentences.", "دو جمله کوتاه بنویس."), JsonOptions));
                lesson.AttachToCourseModule(module.Id);

                dbContext.CoursePaths.Add(path);
                dbContext.CourseModules.Add(module);
                dbContext.CourseLessons.Add(lesson);
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            ICourseRepository repository = serviceProvider.GetRequiredService<ICourseRepository>();
            IReadOnlyList<CoursePathListItemModel> courses = await repository.GetPublishedCoursePathsAsync(
                new CoursePathListFilterModel("A1", "deutsch"),
                "fa",
                CancellationToken.None);

            CoursePathListItemModel item = Assert.Single(courses);
            Assert.Equal("شروع آلمانی A1", item.LearnerLanguageTitle);

            CoursePathDetailModel? detail = await repository.GetPublishedCoursePathBySlugAsync("a1-deutsch-start", "fa", CancellationToken.None);
            Assert.NotNull(detail);
            CourseModuleModel moduleDetail = Assert.Single(detail.Modules);
            Assert.Equal("قدم‌های اول", moduleDetail.LearnerLanguageTitle);
            Assert.Equal("سلام و نام", Assert.Single(moduleDetail.Lessons).LearnerLanguageTitle);

            CourseLessonDetailModel? lessonDetail = await repository.GetPublishedCourseLessonBySlugAsync("a1-begruessung-und-name", "fa", CancellationToken.None);
            Assert.NotNull(lessonDetail);
            Assert.Equal("سلام و نام", lessonDetail.LearnerLanguageTitle);
            Assert.Equal("به کسی سلام کنی", lessonDetail.LearnerLanguageLearningGoals[0]);
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

        string stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        string stderr = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"PostgreSQL helper failed with exit code {process.ExitCode}: {stdout}{stderr}");
        }
    }
}
