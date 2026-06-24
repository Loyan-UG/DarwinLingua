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

public sealed class WritingTemplatePostgresRepositoryTests
{
    private const string DefaultDockerContainerName = "darwinlingua-postgres";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task WritingTemplateRepository_ShouldUsePostgresSearchAndProjectLocalizedHelpers()
    {
        string databaseName = $"darwin_writing_template_test_{Guid.NewGuid():N}"[..48];
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
                dbContext.WritingTemplates.AddRange(
                    CreateTemplate(nowUtc),
                    CreateTemplate(
                        nowUtc,
                        "a1-termin-verschieben",
                        "Einen Termin verschieben",
                        "Bitte um eine neue Uhrzeit.",
                        "Du kannst einen Termin nicht wahrnehmen.",
                        "appointment-reschedule",
                        "formal",
                        20));
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IWritingTemplateRepository repository = serviceProvider.GetRequiredService<IWritingTemplateRepository>();
            IReadOnlyList<WritingTemplateListItemModel> items = await repository.GetPublishedWritingTemplatesAsync(
                new WritingTemplateListFilterModel("A1", "email-to-school", "neutral", null, "VORSTELLUNG"),
                "fa",
                CancellationToken.None);

            WritingTemplateListItemModel item = Assert.Single(items);
            Assert.Equal("a1-kurze-vorstellung-nachricht", item.Slug);
            Assert.Equal("معرفی کوتاه", item.LearnerLanguageTitle);
            Assert.Equal("یک پیام ساده برای معرفی کردن خودت.", item.LearnerLanguageShortDescription);

            WritingTemplateDetailModel? detail = await repository.GetPublishedWritingTemplateBySlugAsync(
                "a1-kurze-vorstellung-nachricht",
                "fa",
                CancellationToken.None);

            Assert.NotNull(detail);
            Assert.Equal("به مدرس یا مسئول کلاس می‌نویسی و خودت را کوتاه معرفی می‌کنی.", detail.LearnerLanguageSituation);
            Assert.Equal("از جمله‌های کوتاه استفاده کن و فقط اطلاعات اصلی را بنویس.", detail.LearnerLanguageExplanation);
            Assert.Equal("سلام، نام من سارا است. من از ایران می‌آیم. در کلاس آلمانی یاد می‌گیرم.", detail.LearnerLanguageSampleFilledVersion);
            Assert.Equal("a1-eine-kurze-vorstellung-bauen", Assert.Single(detail.LinkedCourseLessonSlugs));

            IUnifiedLearningSearchRepository searchRepository = serviceProvider.GetRequiredService<IUnifiedLearningSearchRepository>();
            IReadOnlyList<UnifiedLearningSearchResultModel> results = await searchRepository.SearchAsync(
                new UnifiedLearningSearchFilterModel("VORSTELLUNG", "A1", "writing-template", null, null),
                CancellationToken.None);

            UnifiedLearningSearchResultModel result = Assert.Single(results);
            Assert.Equal("writing-template", result.ResultType);
            Assert.Equal("/learn/de/writing-templates/a1-kurze-vorstellung-nachricht", result.Url);
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

    private static WritingTemplate CreateTemplate(
        DateTime nowUtc,
        string slug = "a1-kurze-vorstellung-nachricht",
        string title = "Kurze Vorstellung",
        string shortDescription = "Eine einfache Nachricht, um dich vorzustellen.",
        string situation = "Du schreibst einer Kursleitung und stellst dich kurz vor.",
        string category = "email-to-school",
        string register = "neutral",
        int sortOrder = 10) =>
        new(
            Guid.NewGuid(),
            slug,
            title,
            shortDescription,
            CefrLevel.A1,
            category,
            situation,
            register,
            "Hallo, ich heisse {{name}}. Ich komme aus {{land}}. Ich lerne Deutsch im Kurs.",
            "Nutze kurze Saetze und nenne nur die wichtigsten Informationen.",
            JsonSerializer.Serialize(new[] { "name", "land" }, JsonOptions),
            "Hallo, ich heisse Sara. Ich komme aus Iran. Ich lerne Deutsch im Kurs.",
            "[]",
            "[]",
            "[]",
            JsonSerializer.Serialize(new[] { "a1-vorstellung-korrigieren" }, JsonOptions),
            JsonSerializer.Serialize(new[] { "a1-eine-kurze-vorstellung-bauen" }, JsonOptions),
            PublicationStatus.Active,
            sortOrder,
            nowUtc,
            JsonSerializer.Serialize(Translations("Short introduction", "معرفی کوتاه"), JsonOptions),
            JsonSerializer.Serialize(Translations("A simple message for introducing yourself.", "یک پیام ساده برای معرفی کردن خودت."), JsonOptions),
            JsonSerializer.Serialize(Translations("You write to a course teacher and introduce yourself briefly.", "به مدرس یا مسئول کلاس می‌نویسی و خودت را کوتاه معرفی می‌کنی."), JsonOptions),
            JsonSerializer.Serialize(Translations("Use short sentences and mention only the most important information.", "از جمله‌های کوتاه استفاده کن و فقط اطلاعات اصلی را بنویس."), JsonOptions),
            JsonSerializer.Serialize(Translations("Hello, my name is {{name}}. I come from {{country}}. I am learning German in the course.", "سلام، نام من {{name}} است. من از {{country}} می‌آیم. در کلاس آلمانی یاد می‌گیرم."), JsonOptions),
            JsonSerializer.Serialize(Translations("Hello, my name is Sara. I come from Iran. I am learning German in the course.", "سلام، نام من سارا است. من از ایران می‌آیم. در کلاس آلمانی یاد می‌گیرم."), JsonOptions));

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
