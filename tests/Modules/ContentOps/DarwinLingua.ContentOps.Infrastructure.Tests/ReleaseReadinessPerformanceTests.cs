using System.Diagnostics;
using System.Text.Json;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.DependencyInjection;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Localization.Application.DependencyInjection;
using DarwinLingua.Localization.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

/// <summary>
/// Provides release-readiness performance validation against a realistic starter dataset size.
/// </summary>
public sealed class ReleaseReadinessPerformanceTests
{
    private const int StarterDatasetEntryCount = 150;
    private const int SearchResultLimit = 50;
    private static readonly string[] TopicKeys =
    [
        "everyday-life",
        "housing",
        "shopping",
        "work-and-jobs",
        "appointments-and-health",
    ];

    /// <summary>
    /// Verifies that import and browse/search queries stay within pragmatic local-only timing bounds.
    /// </summary>
    [Fact]
    public async Task StarterDataset_ShouldImportAndQueryWithinReleaseReadinessBounds()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-perf-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-perf-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreateStarterDatasetPackageJson("phase1-starter-performance"));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            IWordQueryService wordQueryService = serviceProvider.GetRequiredService<IWordQueryService>();

            Stopwatch importStopwatch = Stopwatch.StartNew();
            ImportContentPackageResult importResult = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);
            importStopwatch.Stop();

            Assert.True(
                importResult.IsSuccess,
                $"Import failed with status '{importResult.Status}'. Issues: {string.Join(" | ", importResult.Issues.Select(issue => issue.Message))}");
            Assert.Equal("Completed", importResult.Status);
            Assert.Equal(StarterDatasetEntryCount, importResult.TotalEntries);
            Assert.Equal(StarterDatasetEntryCount, importResult.ImportedEntries);
            Assert.True(
                importStopwatch.Elapsed < TimeSpan.FromSeconds(20),
                $"Starter dataset import took {importStopwatch.Elapsed.TotalSeconds:F2}s, which exceeds the release-readiness bound.");

            Stopwatch topicBrowseStopwatch = Stopwatch.StartNew();
            IReadOnlyList<WordListItemModel> shoppingWords = await wordQueryService
                .GetWordsByTopicAsync("shopping", "en", CancellationToken.None);
            topicBrowseStopwatch.Stop();

            Stopwatch cefrBrowseStopwatch = Stopwatch.StartNew();
            IReadOnlyList<WordListItemModel> a1Words = await wordQueryService
                .GetWordsByCefrAsync("A1", "en", CancellationToken.None);
            cefrBrowseStopwatch.Stop();

            Stopwatch searchStopwatch = Stopwatch.StartNew();
            IReadOnlyList<WordListItemModel> searchedWords = await wordQueryService
                .SearchWordsAsync("Starterwort", "en", CancellationToken.None);
            searchStopwatch.Stop();

            Assert.Equal(StarterDatasetEntryCount / TopicKeys.Length, shoppingWords.Count);
            Assert.NotEmpty(a1Words);
            Assert.Equal(SearchResultLimit, searchedWords.Count);

            Assert.True(
                topicBrowseStopwatch.Elapsed < TimeSpan.FromSeconds(5),
                $"Topic browse query took {topicBrowseStopwatch.Elapsed.TotalSeconds:F2}s, which exceeds the release-readiness bound.");
            Assert.True(
                cefrBrowseStopwatch.Elapsed < TimeSpan.FromSeconds(5),
                $"CEFR browse query took {cefrBrowseStopwatch.Elapsed.TotalSeconds:F2}s, which exceeds the release-readiness bound.");
            Assert.True(
                searchStopwatch.Elapsed < TimeSpan.FromSeconds(5),
                $"Search query took {searchStopwatch.Elapsed.TotalSeconds:F2}s, which exceeds the release-readiness bound.");
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    private static ServiceProvider BuildServiceProvider(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        ServiceCollection services = new();
        services
            .AddDarwinLinguaInfrastructure(options => options.DatabasePath = databasePath)
            .AddCatalogApplication()
            .AddCatalogInfrastructure()
            .AddContentOpsApplication()
            .AddContentOpsInfrastructure()
            .AddLocalizationApplication()
            .AddLocalizationInfrastructure();

        return services.BuildServiceProvider();
    }

    private static string CreateStarterDatasetPackageJson(string packageId)
    {
        List<object> entries = [];
        string[] requiredMeaningLanguages = ["ar", "ckb", "en", "fa", "kmr", "pl", "ro", "ru", "sq", "tr"];
        string[] requiredLocalizationLanguages = ["de", .. requiredMeaningLanguages];

        for (int index = 1; index <= StarterDatasetEntryCount; index++)
        {
            string topicKey = TopicKeys[(index - 1) % TopicKeys.Length];
            string token = BuildAlphabeticToken(index);
            string cefrLevel = (index % 3) switch
            {
                0 => "A1",
                1 => "A2",
                _ => "B1",
            };

            entries.Add(new
            {
                word = $"Starterwort {token}",
                language = "de",
                cefrLevel,
                partOfSpeech = "Noun",
                article = "das",
                plural = $"Starterwoerter {token}",
                topics = new[] { topicKey },
                meanings = requiredMeaningLanguages.Select(language => new
                {
                    language,
                    text = $"starter word {token}",
                }),
                examples = new[]
                {
                    new
                    {
                        baseText = $"Das ist Starterwort {token}.",
                        translations = requiredMeaningLanguages.Select(language => new
                        {
                            language,
                            text = $"This is starter word {token}.",
                        }),
                    },
                },
            });
        }

        return JsonSerializer.Serialize(new
        {
            packageVersion = "1.0",
            packageId,
            packageName = "Phase 1 Starter Performance Package",
            source = "Hybrid",
            defaultMeaningLanguages = requiredMeaningLanguages,
            labels = new[]
            {
                new
                {
                    kind = "usage",
                    key = "general",
                    displayName = "General",
                    localizations = requiredLocalizationLanguages.Select(language => new
                    {
                        language,
                        name = "General",
                        description = "General label",
                    }),
                    sortOrder = 10,
                },
            },
            entries,
        });
    }

    private static string BuildAlphabeticToken(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(index);

        int value = index;
        System.Text.StringBuilder builder = new();

        while (value > 0)
        {
            value--;
            builder.Insert(0, (char)('a' + (value % 26)));
            value /= 26;
        }

        return builder.ToString();
    }
}
