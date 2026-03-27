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
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.DependencyInjection;
using DarwinLingua.Learning.Infrastructure.DependencyInjection;
using DarwinLingua.Localization.Application.DependencyInjection;
using DarwinLingua.Localization.Infrastructure.DependencyInjection;
using DarwinLingua.Practice.Application.Abstractions;
using DarwinLingua.Practice.Application.DependencyInjection;
using DarwinLingua.Practice.Application.Models;
using DarwinLingua.Practice.Domain.Entities;
using DarwinLingua.Practice.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Practice.Infrastructure.Tests;

/// <summary>
/// Provides release-readiness performance validation against a realistic early-learning practice dataset.
/// </summary>
public sealed class PracticeReleaseReadinessPerformanceTests
{
    private const int StarterDatasetEntryCount = 180;
    private const int ReviewSeedCount = 90;
    private static readonly string[] TopicKeys =
    [
        "everyday-life",
        "housing",
        "shopping",
        "work-and-jobs",
        "appointments-and-health",
    ];

    /// <summary>
    /// Verifies that practice import, tracking, scheduling, and query flows stay within pragmatic local-only timing bounds.
    /// </summary>
    [Fact]
    public async Task EarlyLearningDataset_ShouldSupportPracticeFlowsWithinReleaseReadinessBounds()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-perf-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-perf-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreateStarterDatasetPackageJson("phase2-practice-performance"));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            IWordQueryService wordQueryService = serviceProvider.GetRequiredService<IWordQueryService>();
            IUserWordStateService userWordStateService = serviceProvider.GetRequiredService<IUserWordStateService>();
            IPracticeFlashcardAnswerService practiceFlashcardAnswerService =
                serviceProvider.GetRequiredService<IPracticeFlashcardAnswerService>();
            IPracticeOverviewService practiceOverviewService = serviceProvider.GetRequiredService<IPracticeOverviewService>();
            IPracticeReviewQueueService practiceReviewQueueService = serviceProvider.GetRequiredService<IPracticeReviewQueueService>();
            IPracticeReviewSessionService practiceReviewSessionService =
                serviceProvider.GetRequiredService<IPracticeReviewSessionService>();
            IPracticeRecentActivityService practiceRecentActivityService =
                serviceProvider.GetRequiredService<IPracticeRecentActivityService>();
            IPracticeLearningProgressSnapshotService practiceLearningProgressSnapshotService =
                serviceProvider.GetRequiredService<IPracticeLearningProgressSnapshotService>();

            Stopwatch importStopwatch = Stopwatch.StartNew();
            ImportContentPackageResult importResult = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);
            importStopwatch.Stop();

            Assert.True(
                importResult.IsSuccess,
                $"Import failed with status '{importResult.Status}'. Issues: {string.Join(" | ", importResult.Issues.Select(issue => issue.Message))}");
            Assert.Equal(StarterDatasetEntryCount, importResult.ImportedEntries);
            Assert.True(
                importStopwatch.Elapsed < TimeSpan.FromSeconds(20),
                $"Practice starter dataset import took {importStopwatch.Elapsed.TotalSeconds:F2}s, which exceeds the release-readiness bound.");

            Stopwatch seedingStopwatch = Stopwatch.StartNew();
            Dictionary<Guid, WordListItemModel> seededWords = [];

            foreach (string topicKey in TopicKeys)
            {
                IReadOnlyList<WordListItemModel> topicWords = await wordQueryService
                    .GetWordsByTopicAsync(topicKey, "en", CancellationToken.None);

                foreach (WordListItemModel word in topicWords)
                {
                    seededWords[word.PublicId] = word;
                }
            }

            Assert.True(seededWords.Count >= ReviewSeedCount, "Topic browse did not return enough rows to seed practice state.");

            DateTime baseAttemptAtUtc = DateTime.UtcNow.AddHours(-6);

            foreach ((WordListItemModel word, int index) in seededWords.Values.Take(ReviewSeedCount).Select((word, index) => (word, index)))
            {
                await userWordStateService.TrackWordViewedAsync(word.PublicId, CancellationToken.None);

                if (word.CefrLevel is "B2" or "C1" or "C2")
                {
                    await userWordStateService.MarkWordDifficultAsync(word.PublicId, CancellationToken.None);
                }

                PracticeAttemptOutcome outcome = word.CefrLevel switch
                {
                    "A1" => PracticeAttemptOutcome.Correct,
                    "A2" => PracticeAttemptOutcome.Easy,
                    "B1" => PracticeAttemptOutcome.Hard,
                    _ => PracticeAttemptOutcome.Incorrect,
                };

                await practiceFlashcardAnswerService.SubmitAsync(
                    new PracticeFlashcardAnswerRequestModel(
                        word.PublicId,
                        outcome,
                        AttemptedAtUtc: baseAttemptAtUtc.AddMinutes(index)),
                    CancellationToken.None);
            }

            seedingStopwatch.Stop();

            Stopwatch overviewStopwatch = Stopwatch.StartNew();
            PracticeOverviewModel overview = await practiceOverviewService.GetOverviewAsync("en", CancellationToken.None);
            overviewStopwatch.Stop();

            Stopwatch queueStopwatch = Stopwatch.StartNew();
            PracticeReviewQueueModel queue = await practiceReviewQueueService.GetQueueAsync("en", CancellationToken.None);
            queueStopwatch.Stop();

            Stopwatch sessionStopwatch = Stopwatch.StartNew();
            PracticeReviewSessionModel session = await practiceReviewSessionService.StartAsync("en", 20, CancellationToken.None);
            sessionStopwatch.Stop();

            Stopwatch activityStopwatch = Stopwatch.StartNew();
            PracticeRecentActivityModel activity = await practiceRecentActivityService.GetRecentActivityAsync("en", 25, CancellationToken.None);
            activityStopwatch.Stop();

            Stopwatch snapshotStopwatch = Stopwatch.StartNew();
            PracticeLearningProgressSnapshotModel snapshot = await practiceLearningProgressSnapshotService
                .GetSnapshotAsync(CancellationToken.None);
            snapshotStopwatch.Stop();

            Assert.True(
                seedingStopwatch.Elapsed < TimeSpan.FromSeconds(20),
                $"Practice state seeding took {seedingStopwatch.Elapsed.TotalSeconds:F2}s, which exceeds the release-readiness bound.");
            Assert.Equal(ReviewSeedCount, overview.TotalTrackedWords);
            Assert.InRange(queue.TotalCandidates, 1, ReviewSeedCount);
            Assert.Equal(20, session.Items.Count);
            Assert.Equal(25, activity.Items.Count);
            Assert.Equal(ReviewSeedCount, snapshot.TrackedWordCount);
            Assert.InRange(snapshot.DueNowCount, 1, ReviewSeedCount);
            Assert.True(snapshot.TotalAttemptCount >= ReviewSeedCount);

            Assert.True(
                overviewStopwatch.Elapsed < TimeSpan.FromSeconds(5),
                $"Practice overview query took {overviewStopwatch.Elapsed.TotalSeconds:F2}s, which exceeds the release-readiness bound.");
            Assert.True(
                queueStopwatch.Elapsed < TimeSpan.FromSeconds(5),
                $"Practice review queue query took {queueStopwatch.Elapsed.TotalSeconds:F2}s, which exceeds the release-readiness bound.");
            Assert.True(
                sessionStopwatch.Elapsed < TimeSpan.FromSeconds(5),
                $"Practice session snapshot query took {sessionStopwatch.Elapsed.TotalSeconds:F2}s, which exceeds the release-readiness bound.");
            Assert.True(
                activityStopwatch.Elapsed < TimeSpan.FromSeconds(5),
                $"Practice recent-activity query took {activityStopwatch.Elapsed.TotalSeconds:F2}s, which exceeds the release-readiness bound.");
            Assert.True(
                snapshotStopwatch.Elapsed < TimeSpan.FromSeconds(5),
                $"Practice progress snapshot query took {snapshotStopwatch.Elapsed.TotalSeconds:F2}s, which exceeds the release-readiness bound.");
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
            .AddLearningApplication()
            .AddLearningInfrastructure()
            .AddLocalizationApplication()
            .AddLocalizationInfrastructure()
            .AddPracticeApplication()
            .AddPracticeInfrastructure();

        return services.BuildServiceProvider();
    }

    private static string CreateStarterDatasetPackageJson(string packageId)
    {
        List<object> entries = [];

        for (int index = 1; index <= StarterDatasetEntryCount; index++)
        {
            string topicKey = TopicKeys[(index - 1) % TopicKeys.Length];
            string token = BuildAlphabeticToken(index);
            string cefrLevel = (index % 6) switch
            {
                0 => "A1",
                1 => "A2",
                2 => "B1",
                3 => "B2",
                4 => "C1",
                _ => "C2",
            };

            entries.Add(new
            {
                word = $"Praxiswort {token}",
                language = "de",
                cefrLevel,
                partOfSpeech = "Noun",
                article = "das",
                plural = $"Praxiswoerter {token}",
                topics = new[] { topicKey },
                meanings = new[]
                {
                    new
                    {
                        language = "en",
                        text = $"practice word {token}",
                    },
                },
                examples = new[]
                {
                    new
                    {
                        baseText = $"Das ist Praxiswort {token}.",
                        translations = new[]
                        {
                            new
                            {
                                language = "en",
                                text = $"This is practice word {token}.",
                            },
                        },
                    },
                },
            });
        }

        return JsonSerializer.Serialize(new
        {
            packageVersion = "1.0",
            packageId,
            packageName = "Phase 2 Practice Performance Package",
            source = "Hybrid",
            defaultMeaningLanguages = new[] { "en" },
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
