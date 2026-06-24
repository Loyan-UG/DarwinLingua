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

public sealed class CountryGuidanceNotePostgresRepositoryTests
{
    private const string DefaultDockerContainerName = "darwinlingua-postgres";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task CountryGuidanceNoteRepository_ShouldFilterOrderAndProjectLocalizedHelpers()
    {
        string databaseName = $"darwin_cultural_note_test_{Guid.NewGuid():N}"[..48];
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
                dbContext.CountryGuidanceNotes.AddRange(
                    CreateNote(nowUtc),
                    CreateNote(
                        nowUtc,
                        "b1-termin-puenktlich-absagen",
                        "Einen Termin rechtzeitig absagen",
                        "Warum fruehes Absagen in Deutschland wichtig ist.",
                        CefrLevel.B1,
                        "appointments",
                        "Terminabsage im Alltag",
                        20));
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            ICountryGuidanceNoteRepository repository = serviceProvider.GetRequiredService<ICountryGuidanceNoteRepository>();
            IReadOnlyList<CountryGuidanceNoteListItemModel> items = await repository.GetPublishedCountryGuidanceNotesAsync(
                new CountryGuidanceNoteListFilterModel("B1", "democracy-and-state", "Mitbestimmung", "DEMOKRATIE"),
                "fa",
                CancellationToken.None);

            CountryGuidanceNoteListItemModel item = Assert.Single(items);
            Assert.Equal("b1-demokratie-im-alltag-verstehen", item.Slug);
            Assert.Equal("de", item.TargetLearningLanguageCode);
            Assert.Equal("DE", item.CountryContextCode);
            Assert.Equal("دموکراسی در زندگی روزمره", item.LearnerLanguageTitle);
            Assert.Equal("توضیحی روشن درباره مشارکت، قانون و مسئولیت در آلمان.", item.LearnerLanguageShortDescription);
            Assert.Equal("تصمیم‌گیری و مشارکت در زندگی روزمره", item.LearnerLanguageContext);

            CountryGuidanceNoteDetailModel? detail = await repository.GetPublishedCountryGuidanceNoteBySlugAsync(
                "b1-demokratie-im-alltag-verstehen",
                "fa",
                CancellationToken.None);

            Assert.NotNull(detail);
            Assert.Equal("de", detail.TargetLearningLanguageCode);
            Assert.Equal("DE", detail.CountryContextCode);
            Assert.Equal("دموکراسی فقط رأی دادن نیست؛ یعنی قانون‌ها، حقوق و مسئولیت‌ها زندگی مشترک را شکل می‌دهند.", Assert.Single(detail.LearnerLanguageSections));
            Assert.Equal("وقتی در مدرسه، محل کار یا ساختمان مسکونی نظری می‌دهی، بهتر است محترمانه دلیل بیاوری.", Assert.Single(detail.Examples).LearnerLanguageExplanation);
            Assert.Equal("نظر خود را با دلیل کوتاه و محترمانه بیان کن.", Assert.Single(detail.LearnerLanguageDoNotes));
            Assert.Equal("اختلاف نظر را حمله شخصی نکن.", Assert.Single(detail.LearnerLanguageDontNotes));
            Assert.Equal("b1-ueber-regeln-im-haus-sprechen", Assert.Single(detail.LinkedCourseLessonSlugs));

            IUnifiedLearningSearchRepository searchRepository = serviceProvider.GetRequiredService<IUnifiedLearningSearchRepository>();
            IReadOnlyList<UnifiedLearningSearchResultModel> searchResults = await searchRepository.SearchAsync(
                new UnifiedLearningSearchFilterModel("Demokratie", "B1", "country-guidance", "democracy-and-state", null),
                CancellationToken.None);

            UnifiedLearningSearchResultModel result = Assert.Single(searchResults);
            Assert.Equal("country-guidance", result.ResultType);
            Assert.Equal("/learn/de/country-guidance/de/b1-demokratie-im-alltag-verstehen", result.Url);
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

    private static CountryGuidanceNote CreateNote(
        DateTime nowUtc,
        string slug = "b1-demokratie-im-alltag-verstehen",
        string title = "Demokratie im Alltag verstehen",
        string shortDescription = "Eine klare Erklaerung zu Beteiligung, Regeln und Verantwortung in Deutschland.",
        CefrLevel cefrLevel = CefrLevel.B1,
        string category = "democracy-and-state",
        string context = "Mitbestimmung im Alltag",
        int sortOrder = 10) =>
        new(
            Guid.NewGuid(),
            slug,
            title,
            shortDescription,
            cefrLevel,
            category,
            context,
            JsonSerializer.Serialize(new[] { "Demokratie ist nicht nur Waehlen. Regeln, Rechte und Pflichten gestalten das Zusammenleben." }, JsonOptions),
            JsonSerializer.Serialize(
                new[]
                {
                    new
                    {
                        germanText = "Ich sehe das anders, weil ...",
                        explanation = "Eine sachliche Form, um eine andere Meinung mit Begruendung zu sagen.",
                    },
                },
                JsonOptions),
            JsonSerializer.Serialize(new[] { "Begruende deine Meinung kurz und respektvoll." }, JsonOptions),
            JsonSerializer.Serialize(new[] { "Mache aus anderer Meinung keinen persoenlichen Angriff." }, JsonOptions),
            null,
            "[]",
            "[]",
            "[]",
            "[]",
            JsonSerializer.Serialize(new[] { "b1-ueber-regeln-im-haus-sprechen" }, JsonOptions),
            PublicationStatus.Active,
            sortOrder,
            nowUtc,
            JsonSerializer.Serialize(Translations("Understanding democracy in everyday life", "دموکراسی در زندگی روزمره"), JsonOptions),
            JsonSerializer.Serialize(Translations("A clear explanation of participation, rules, and responsibility in Germany.", "توضیحی روشن درباره مشارکت، قانون و مسئولیت در آلمان."), JsonOptions),
            JsonSerializer.Serialize(Translations("Participation and decision-making in everyday life", "تصمیم‌گیری و مشارکت در زندگی روزمره"), JsonOptions),
            JsonSerializer.Serialize(ListTranslations(
                "Democracy is not only voting. Rules, rights, and duties shape shared life.",
                "دموکراسی فقط رأی دادن نیست؛ یعنی قانون‌ها، حقوق و مسئولیت‌ها زندگی مشترک را شکل می‌دهند."), JsonOptions),
            JsonSerializer.Serialize(
                new[]
                {
                    new
                    {
                        explanationTranslations = Translations(
                            "When you give an opinion at school, work, or in your building, it is better to give a respectful reason.",
                            "وقتی در مدرسه، محل کار یا ساختمان مسکونی نظری می‌دهی، بهتر است محترمانه دلیل بیاوری."),
                    },
                },
                JsonOptions),
            JsonSerializer.Serialize(ListTranslations("Give your opinion with a short, respectful reason.", "نظر خود را با دلیل کوتاه و محترمانه بیان کن."), JsonOptions),
            JsonSerializer.Serialize(ListTranslations("Do not turn disagreement into a personal attack.", "اختلاف نظر را حمله شخصی نکن."), JsonOptions));

    private static object[] Translations(string english, string persian) =>
    [
        new { language = "en", text = english },
        new { language = "fa", text = persian },
    ];

    private static object[] ListTranslations(string english, string persian) =>
    [
        new { language = "en", items = new[] { english } },
        new { language = "fa", items = new[] { persian } },
    ];

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
