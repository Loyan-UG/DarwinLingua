using DarwinLingua.Practice.Application.Abstractions;
using DarwinLingua.Practice.Application.DependencyInjection;
using DarwinLingua.Practice.Application.Models;
using DarwinLingua.Practice.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Practice.Application.Tests;

/// <summary>
/// Verifies review-oriented Practice application workflows without a persistence dependency.
/// </summary>
public sealed class PracticeReviewWorkflowServiceTests
{
    /// <summary>
    /// Verifies that the review queue workflow normalizes the meaning language and returns the reader payload.
    /// </summary>
    [Fact]
    public async Task GetQueueAsync_ShouldNormalizeLanguageAndReturnReaderPayload()
    {
        FakePracticeOverviewReader reader = new();
        await using ServiceProvider serviceProvider = BuildServiceProvider(reader);

        IPracticeReviewQueueService service = serviceProvider.GetRequiredService<IPracticeReviewQueueService>();
        PracticeReviewQueueModel queue = await service.GetQueueAsync("EN", CancellationToken.None);

        Assert.Equal("en", reader.LastMeaningLanguageCode);
        Assert.Same(reader.ReviewQueue, queue);
    }

    /// <summary>
    /// Verifies that the review-session workflow rejects non-positive desired counts.
    /// </summary>
    [Fact]
    public async Task StartAsync_ShouldRejectNonPositiveDesiredItemCount()
    {
        FakePracticeOverviewReader reader = new();
        await using ServiceProvider serviceProvider = BuildServiceProvider(reader);

        IPracticeReviewSessionService service = serviceProvider.GetRequiredService<IPracticeReviewSessionService>();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.StartAsync(
            "en",
            0,
            CancellationToken.None));
    }

    /// <summary>
    /// Verifies that the review-session workflow forwards the desired count and normalized language to the reader.
    /// </summary>
    [Fact]
    public async Task StartAsync_ShouldNormalizeLanguageAndForwardDesiredCount()
    {
        FakePracticeOverviewReader reader = new();
        await using ServiceProvider serviceProvider = BuildServiceProvider(reader);

        IPracticeReviewSessionService service = serviceProvider.GetRequiredService<IPracticeReviewSessionService>();
        PracticeReviewSessionModel session = await service.StartAsync("EN", 3, CancellationToken.None);

        Assert.Equal("en", reader.LastMeaningLanguageCode);
        Assert.Equal(3, reader.LastRequestedItemCount);
        Assert.Same(reader.ReviewSession, session);
    }

    /// <summary>
    /// Verifies that the overview workflow normalizes the meaning language and returns the reader payload.
    /// </summary>
    [Fact]
    public async Task GetOverviewAsync_ShouldNormalizeLanguageAndReturnReaderPayload()
    {
        FakePracticeOverviewReader reader = new();
        await using ServiceProvider serviceProvider = BuildServiceProvider(reader);

        IPracticeOverviewService service = serviceProvider.GetRequiredService<IPracticeOverviewService>();
        PracticeOverviewModel overview = await service.GetOverviewAsync("EN", CancellationToken.None);

        Assert.Equal("en", reader.LastMeaningLanguageCode);
        Assert.Same(reader.Overview, overview);
    }

    /// <summary>
    /// Verifies that the recent-activity workflow rejects non-positive max item counts.
    /// </summary>
    [Fact]
    public async Task GetRecentActivityAsync_ShouldRejectNonPositiveMaxItemCount()
    {
        FakePracticeOverviewReader reader = new();
        await using ServiceProvider serviceProvider = BuildServiceProvider(reader);

        IPracticeRecentActivityService service = serviceProvider.GetRequiredService<IPracticeRecentActivityService>();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.GetRecentActivityAsync(
            "en",
            0,
            CancellationToken.None));
    }

    /// <summary>
    /// Verifies that the recent-activity workflow normalizes the language and forwards the max item count.
    /// </summary>
    [Fact]
    public async Task GetRecentActivityAsync_ShouldNormalizeLanguageAndReturnReaderPayload()
    {
        FakePracticeOverviewReader reader = new();
        await using ServiceProvider serviceProvider = BuildServiceProvider(reader);

        IPracticeRecentActivityService service = serviceProvider.GetRequiredService<IPracticeRecentActivityService>();
        PracticeRecentActivityModel activity = await service.GetRecentActivityAsync("EN", 5, CancellationToken.None);

        Assert.Equal("en", reader.LastMeaningLanguageCode);
        Assert.Same(reader.RecentActivity, activity);
    }

    /// <summary>
    /// Verifies that the snapshot workflow delegates to the reader and returns its result.
    /// </summary>
    [Fact]
    public async Task GetSnapshotAsync_ShouldForwardToReaderAndReturnPayload()
    {
        FakePracticeOverviewReader reader = new();
        await using ServiceProvider serviceProvider = BuildServiceProvider(reader);

        IPracticeLearningProgressSnapshotService service =
            serviceProvider.GetRequiredService<IPracticeLearningProgressSnapshotService>();
        PracticeLearningProgressSnapshotModel snapshot = await service.GetSnapshotAsync(CancellationToken.None);

        Assert.Same(reader.Snapshot, snapshot);
    }

    private static ServiceProvider BuildServiceProvider(FakePracticeOverviewReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        ServiceCollection services = new();
        services.AddPracticeApplication();
        services.AddSingleton<IPracticeOverviewReader>(reader);

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Supplies stable read-model payloads for Practice application tests.
    /// </summary>
    private sealed class FakePracticeOverviewReader : IPracticeOverviewReader
    {
        public FakePracticeOverviewReader()
        {
            Overview = new PracticeOverviewModel(
                2,
                1,
                0,
                1,
                1,
                DateTime.UtcNow,
                []);
            ReviewQueue = new PracticeReviewQueueModel(
                1,
                [new PracticeReviewQueueItemModel(
                    1,
                    Guid.NewGuid(),
                    "Haus",
                    "A1",
                    "house",
                    DateTime.UtcNow,
                    true,
                    false,
                    false,
                    2,
                    DateTime.UtcNow.AddDays(-1))]);
            ReviewSession = new PracticeReviewSessionModel(
                DateTime.UtcNow,
                3,
                1,
                [new PracticeReviewSessionItemModel(
                    1,
                    Guid.NewGuid(),
                    "Haus",
                    "A1",
                    "house",
                    DateTime.UtcNow,
                    true,
                    false,
                    false,
                    2,
                    DateTime.UtcNow.AddDays(-1),
                    PracticeAttemptOutcome.Correct,
                    3)]);
            RecentActivity = new PracticeRecentActivityModel(
                1,
                [new PracticeRecentActivityItemModel(
                    Guid.NewGuid(),
                    "Haus",
                    "A1",
                    "house",
                    PracticeSessionType.Flashcard,
                    PracticeAttemptOutcome.Correct,
                    DateTime.UtcNow,
                    DateTime.UtcNow.AddDays(1),
                    650)]);
            Snapshot = new PracticeLearningProgressSnapshotModel(
                3,
                2,
                2,
                1,
                1,
                1,
                4,
                3,
                1,
                0.75d,
                DateTime.UtcNow);
        }

        public string? LastMeaningLanguageCode { get; private set; }

        public int? LastRequestedItemCount { get; private set; }

        public PracticeOverviewModel Overview { get; }

        public PracticeReviewQueueModel ReviewQueue { get; }

        public PracticeReviewSessionModel ReviewSession { get; }

        public PracticeRecentActivityModel RecentActivity { get; }

        public PracticeLearningProgressSnapshotModel Snapshot { get; }

        public Task<PracticeOverviewModel> GetOverviewAsync(LanguageCode meaningLanguageCode, CancellationToken cancellationToken)
        {
            LastMeaningLanguageCode = meaningLanguageCode.Value;
            return Task.FromResult(Overview);
        }

        public Task<PracticeReviewQueueModel> GetReviewQueueAsync(LanguageCode meaningLanguageCode, CancellationToken cancellationToken)
        {
            LastMeaningLanguageCode = meaningLanguageCode.Value;
            return Task.FromResult(ReviewQueue);
        }

        public Task<PracticeReviewSessionModel> GetReviewSessionAsync(
            LanguageCode meaningLanguageCode,
            int desiredItemCount,
            CancellationToken cancellationToken)
        {
            LastMeaningLanguageCode = meaningLanguageCode.Value;
            LastRequestedItemCount = desiredItemCount;
            return Task.FromResult(ReviewSession);
        }

        public Task<PracticeRecentActivityModel> GetRecentActivityAsync(
            LanguageCode meaningLanguageCode,
            int maxItemCount,
            CancellationToken cancellationToken)
        {
            LastMeaningLanguageCode = meaningLanguageCode.Value;
            return Task.FromResult(RecentActivity);
        }

        public Task<PracticeLearningProgressSnapshotModel> GetLearningProgressSnapshotAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Snapshot);
        }
    }
}
