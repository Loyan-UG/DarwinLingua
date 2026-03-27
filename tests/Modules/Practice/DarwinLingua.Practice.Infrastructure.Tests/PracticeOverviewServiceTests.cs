using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.DependencyInjection;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.DependencyInjection;
using DarwinLingua.Learning.Infrastructure.DependencyInjection;
using DarwinLingua.Localization.Application.DependencyInjection;
using DarwinLingua.Localization.Infrastructure.DependencyInjection;
using DarwinLingua.Practice.Application.Abstractions;
using DarwinLingua.Practice.Application.DependencyInjection;
using DarwinLingua.Practice.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Practice.Infrastructure.Tests;

/// <summary>
/// Verifies the early Phase 2 practice workflows against a temporary SQLite database.
/// </summary>
public sealed class PracticeOverviewServiceTests
{
    /// <summary>
    /// Verifies that imported content plus user-word-state interactions produce a meaningful practice overview.
    /// </summary>
    [Fact]
    public async Task GetOverviewAsync_ShouldSummarizeTrackedWordsAndReviewCandidates()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            File.Copy(GetSamplePackagePath(), packagePath, overwrite: true);

            serviceProvider = BuildServiceProvider(databasePath);
            SeededPracticeScenario scenario = await CreateSeededScenarioAsync(serviceProvider, packagePath);
            DateTime nowUtc = DateTime.UtcNow;

            await scenario.PracticeFlashcardAnswerService.SubmitAsync(
                new DarwinLingua.Practice.Application.Models.PracticeFlashcardAnswerRequestModel(
                    scenario.ApplicationWord.PublicId,
                    DarwinLingua.Practice.Domain.Entities.PracticeAttemptOutcome.Correct,
                    AttemptedAtUtc: nowUtc.AddHours(-12)),
                CancellationToken.None);

            await scenario.PracticeFlashcardAnswerService.SubmitAsync(
                new DarwinLingua.Practice.Application.Models.PracticeFlashcardAnswerRequestModel(
                    scenario.IndispensabilityWord.PublicId,
                    DarwinLingua.Practice.Domain.Entities.PracticeAttemptOutcome.Incorrect,
                    AttemptedAtUtc: nowUtc.AddHours(-2)),
                CancellationToken.None);

            DarwinLingua.Practice.Application.Models.PracticeOverviewModel overview = await scenario.PracticeOverviewService
                .GetOverviewAsync("en", CancellationToken.None);

            Assert.Equal(3, overview.TotalTrackedWords);
            Assert.Equal(1, overview.ReviewCandidateCount);
            Assert.Equal(1, overview.DifficultWordCount);
            Assert.Equal(1, overview.KnownWordCount);
            Assert.Equal(3, overview.RecentlyViewedCount);
            Assert.NotNull(overview.LastActivityAtUtc);
            DarwinLingua.Practice.Application.Models.PracticeWordPreviewModel reviewPreview = Assert.Single(overview.ReviewPreview);
            Assert.Equal("Unabdingbarkeit", reviewPreview.Lemma);
            Assert.Equal("indispensability", reviewPreview.PrimaryMeaning);
            Assert.False(reviewPreview.IsKnown);
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

    /// <summary>
    /// Verifies that the dedicated review queue follows the deterministic prioritization rule.
    /// </summary>
    [Fact]
    public async Task GetQueueAsync_ShouldReturnDeterministicReviewOrdering()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            File.Copy(GetSamplePackagePath(), packagePath, overwrite: true);

            serviceProvider = BuildServiceProvider(databasePath);
            SeededPracticeScenario scenario = await CreateSeededScenarioAsync(serviceProvider, packagePath);

            await scenario.UserWordStateService.TrackWordViewedAsync(scenario.TermWord.PublicId, CancellationToken.None);
            await Task.Delay(30);
            await scenario.UserWordStateService.TrackWordViewedAsync(scenario.ApplicationWord.PublicId, CancellationToken.None);
            await scenario.UserWordStateService.MarkWordDifficultAsync(scenario.ApplicationWord.PublicId, CancellationToken.None);
            await Task.Delay(30);
            await scenario.UserWordStateService.TrackWordViewedAsync(scenario.EnvironmentWord.PublicId, CancellationToken.None);
            await scenario.UserWordStateService.MarkWordDifficultAsync(scenario.EnvironmentWord.PublicId, CancellationToken.None);
            await Task.Delay(30);
            await scenario.UserWordStateService.TrackWordViewedAsync(scenario.IndispensabilityWord.PublicId, CancellationToken.None);

            DateTime nowUtc = DateTime.UtcNow;
            await scenario.PracticeFlashcardAnswerService.SubmitAsync(
                new DarwinLingua.Practice.Application.Models.PracticeFlashcardAnswerRequestModel(
                    scenario.ApplicationWord.PublicId,
                    DarwinLingua.Practice.Domain.Entities.PracticeAttemptOutcome.Correct,
                    AttemptedAtUtc: nowUtc.AddHours(-12)),
                CancellationToken.None);

            await scenario.PracticeFlashcardAnswerService.SubmitAsync(
                new DarwinLingua.Practice.Application.Models.PracticeFlashcardAnswerRequestModel(
                    scenario.EnvironmentWord.PublicId,
                    DarwinLingua.Practice.Domain.Entities.PracticeAttemptOutcome.Incorrect,
                    AttemptedAtUtc: nowUtc.AddHours(-1)),
                CancellationToken.None);

            DarwinLingua.Practice.Application.Models.PracticeReviewQueueModel queue = await scenario.PracticeReviewQueueService
                .GetQueueAsync("en", CancellationToken.None);

            Assert.Equal(3, queue.TotalCandidates);
            Assert.Equal(3, queue.Items.Count);
            Assert.Equal([1, 2, 3], queue.Items.Select(item => item.Position).ToArray());
            Assert.Equal("Umgebung", queue.Items[0].Lemma);
            Assert.True(queue.Items[0].IsDueNow);
            Assert.NotNull(queue.Items[0].DueAtUtc);
            Assert.Equal("Termin", queue.Items[1].Lemma);
            Assert.False(queue.Items[1].IsDueNow);
            Assert.Equal("Unabdingbarkeit", queue.Items[2].Lemma);
            Assert.DoesNotContain(queue.Items, item => item.Lemma == "Bewerbung");
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

    /// <summary>
    /// Verifies that submitting a flashcard answer persists attempt history and scheduling state.
    /// </summary>
    [Fact]
    public async Task SubmitAsync_ShouldPersistAttemptAndReviewState()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            File.Copy(GetSamplePackagePath(), packagePath, overwrite: true);

            serviceProvider = BuildServiceProvider(databasePath);
            SeededPracticeScenario scenario = await CreateSeededScenarioAsync(serviceProvider, packagePath);
            DateTime attemptedAtUtc = DateTime.UtcNow.AddMinutes(-15);

            DarwinLingua.Practice.Application.Models.PracticeFlashcardAnswerResultModel result =
                await scenario.PracticeFlashcardAnswerService.SubmitAsync(
                    new DarwinLingua.Practice.Application.Models.PracticeFlashcardAnswerRequestModel(
                        scenario.ApplicationWord.PublicId,
                        DarwinLingua.Practice.Domain.Entities.PracticeAttemptOutcome.Correct,
                        ResponseMilliseconds: 1250,
                        AttemptedAtUtc: attemptedAtUtc),
                    CancellationToken.None);

            Assert.Equal(scenario.ApplicationWord.PublicId, result.WordEntryPublicId);
            Assert.Equal(DarwinLingua.Practice.Domain.Entities.PracticeAttemptOutcome.Correct, result.Outcome);
            Assert.Null(result.DueAtUtcBeforeAttempt);
            Assert.Equal(attemptedAtUtc.AddDays(1), result.DueAtUtcAfterAttempt);
            Assert.Equal(1, result.TotalAttemptCount);
            Assert.Equal(1, result.ConsecutiveSuccessCount);
            Assert.Equal(0, result.ConsecutiveFailureCount);

            await using DarwinLinguaDbContext dbContext = await scenario.DbContextFactory.CreateDbContextAsync(CancellationToken.None);

            DarwinLingua.Practice.Domain.Entities.PracticeReviewState reviewState = Assert.Single(
                dbContext.PracticeReviewStates.Where(state => state.WordEntryPublicId == scenario.ApplicationWord.PublicId));
            DarwinLingua.Practice.Domain.Entities.PracticeAttempt attempt = Assert.Single(
                dbContext.PracticeAttempts.Where(row => row.WordEntryPublicId == scenario.ApplicationWord.PublicId));

            Assert.Equal(result.DueAtUtcAfterAttempt, reviewState.DueAtUtc);
            Assert.Equal(attemptedAtUtc, reviewState.LastAttemptedAtUtc);
            Assert.Equal(DarwinLingua.Practice.Domain.Entities.PracticeAttemptOutcome.Correct, reviewState.LastOutcome);
            Assert.Equal(1, reviewState.TotalAttemptCount);
            Assert.Equal(result.DueAtUtcAfterAttempt, attempt.DueAtUtcAfterAttempt);
            Assert.Equal(1250, attempt.ResponseMilliseconds);
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

    /// <summary>
    /// Verifies that repeated failures persist counters and shorten the next due window deterministically.
    /// </summary>
    [Fact]
    public async Task SubmitAsync_ShouldUpdateFailureCountersAndDueSchedule()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            File.Copy(GetSamplePackagePath(), packagePath, overwrite: true);

            serviceProvider = BuildServiceProvider(databasePath);
            SeededPracticeScenario scenario = await CreateSeededScenarioAsync(serviceProvider, packagePath);
            DateTime firstAttemptAtUtc = DateTime.UtcNow.AddHours(-3);
            DateTime secondAttemptAtUtc = firstAttemptAtUtc.AddMinutes(20);

            await scenario.PracticeFlashcardAnswerService.SubmitAsync(
                new DarwinLingua.Practice.Application.Models.PracticeFlashcardAnswerRequestModel(
                    scenario.IndispensabilityWord.PublicId,
                    DarwinLingua.Practice.Domain.Entities.PracticeAttemptOutcome.Incorrect,
                    AttemptedAtUtc: firstAttemptAtUtc),
                CancellationToken.None);

            DarwinLingua.Practice.Application.Models.PracticeFlashcardAnswerResultModel secondResult =
                await scenario.PracticeFlashcardAnswerService.SubmitAsync(
                    new DarwinLingua.Practice.Application.Models.PracticeFlashcardAnswerRequestModel(
                        scenario.IndispensabilityWord.PublicId,
                        DarwinLingua.Practice.Domain.Entities.PracticeAttemptOutcome.Incorrect,
                        AttemptedAtUtc: secondAttemptAtUtc),
                    CancellationToken.None);

            Assert.Equal(firstAttemptAtUtc.AddMinutes(10), secondResult.DueAtUtcBeforeAttempt);
            Assert.Equal(secondAttemptAtUtc.AddMinutes(30), secondResult.DueAtUtcAfterAttempt);
            Assert.Equal(2, secondResult.TotalAttemptCount);
            Assert.Equal(0, secondResult.ConsecutiveSuccessCount);
            Assert.Equal(2, secondResult.ConsecutiveFailureCount);

            await using DarwinLinguaDbContext dbContext = await scenario.DbContextFactory.CreateDbContextAsync(CancellationToken.None);
            DarwinLingua.Practice.Domain.Entities.PracticeReviewState reviewState = Assert.Single(
                dbContext.PracticeReviewStates.Where(state => state.WordEntryPublicId == scenario.IndispensabilityWord.PublicId));

            Assert.Equal(2, reviewState.TotalAttemptCount);
            Assert.Equal(2, reviewState.ConsecutiveFailureCount);
            Assert.Equal(secondAttemptAtUtc.AddMinutes(30), reviewState.DueAtUtc);
            Assert.Equal(2, dbContext.PracticeAttempts.Count(row => row.WordEntryPublicId == scenario.IndispensabilityWord.PublicId));
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

    private static string GetSamplePackagePath()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string samplePackagePath = Path.Combine(
            repositoryRoot,
            "tests/Modules/ContentOps/DarwinLingua.ContentOps.Infrastructure.Tests/Fixtures/phase1-sample-content-package.json");

        Assert.True(File.Exists(samplePackagePath), $"Sample package fixture was not found: {samplePackagePath}");
        return samplePackagePath;
    }

    private static string ResolveRepositoryRoot()
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string candidateSolutionPath = Path.Combine(currentDirectory.FullName, "DarwinLingua.slnx");

            if (File.Exists(candidateSolutionPath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException("Unable to resolve repository root from test execution directory.");
    }

    private static async Task<SeededPracticeScenario> CreateSeededScenarioAsync(ServiceProvider serviceProvider, string packagePath)
    {
        IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.InitializeAsync(CancellationToken.None);

        IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
        ImportContentPackageResult importResult = await contentImportService
            .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

        Assert.True(importResult.IsSuccess);

        IWordQueryService wordQueryService = serviceProvider.GetRequiredService<IWordQueryService>();
        IUserWordStateService userWordStateService = serviceProvider.GetRequiredService<IUserWordStateService>();

        WordListItemModel breadWord = Assert.Single(await wordQueryService.SearchWordsAsync("Brot", "en", CancellationToken.None));
        WordListItemModel applicationWord = Assert.Single(await wordQueryService.SearchWordsAsync("Bewerbung", "en", CancellationToken.None));
        WordListItemModel indispensabilityWord = Assert.Single(await wordQueryService.SearchWordsAsync("Unabdingbarkeit", "en", CancellationToken.None));
        WordListItemModel termWord = Assert.Single(await wordQueryService.SearchWordsAsync("Termin", "en", CancellationToken.None));
        WordListItemModel environmentWord = Assert.Single(await wordQueryService.SearchWordsAsync("Umgebung", "en", CancellationToken.None));

        await userWordStateService.TrackWordViewedAsync(breadWord.PublicId, CancellationToken.None);
        await userWordStateService.MarkWordKnownAsync(breadWord.PublicId, CancellationToken.None);

        await userWordStateService.TrackWordViewedAsync(applicationWord.PublicId, CancellationToken.None);
        await userWordStateService.MarkWordDifficultAsync(applicationWord.PublicId, CancellationToken.None);

        await userWordStateService.TrackWordViewedAsync(indispensabilityWord.PublicId, CancellationToken.None);

        return new SeededPracticeScenario(
            serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>(),
            serviceProvider.GetRequiredService<IPracticeFlashcardAnswerService>(),
            serviceProvider.GetRequiredService<IPracticeOverviewService>(),
            serviceProvider.GetRequiredService<IPracticeReviewQueueService>(),
            userWordStateService,
            breadWord,
            applicationWord,
            indispensabilityWord,
            termWord,
            environmentWord);
    }

    private sealed record SeededPracticeScenario(
        IDbContextFactory<DarwinLinguaDbContext> DbContextFactory,
        IPracticeFlashcardAnswerService PracticeFlashcardAnswerService,
        IPracticeOverviewService PracticeOverviewService,
        IPracticeReviewQueueService PracticeReviewQueueService,
        IUserWordStateService UserWordStateService,
        WordListItemModel BreadWord,
        WordListItemModel ApplicationWord,
        WordListItemModel IndispensabilityWord,
        WordListItemModel TermWord,
        WordListItemModel EnvironmentWord);
}
