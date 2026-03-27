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
using DarwinLingua.Practice.Application.Models;
using DarwinLingua.Practice.Domain.Entities;
using DarwinLingua.Practice.Infrastructure.DependencyInjection;
using DarwinLingua.SharedKernel.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Practice.Infrastructure.Tests;

/// <summary>
/// Verifies Practice read models and persistence behavior against the shared SQLite database.
/// </summary>
public sealed class PracticeQueryInfrastructureTests
{
    /// <summary>
    /// Verifies that Practice read models tolerate missing requested meanings while keeping the word visible.
    /// </summary>
    [Fact]
    public async Task PracticeQueries_ShouldReturnRowsWhenRequestedMeaningLanguageIsMissing()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-query-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-query-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            File.Copy(GetSamplePackagePath(), packagePath, overwrite: true);

            serviceProvider = BuildServiceProvider(databasePath);
            SeededPracticeScenario scenario = await CreateSeededScenarioAsync(serviceProvider, packagePath);
            DateTime attemptedAtUtc = DateTime.UtcNow.AddMinutes(-45);

            await scenario.PracticeFlashcardAnswerService.SubmitAsync(
                new PracticeFlashcardAnswerRequestModel(
                    scenario.ApplicationWord.PublicId,
                    PracticeAttemptOutcome.Correct,
                    AttemptedAtUtc: attemptedAtUtc),
                CancellationToken.None);

            PracticeOverviewModel overview = await scenario.PracticeOverviewService
                .GetOverviewAsync("fr", CancellationToken.None);
            PracticeReviewQueueModel queue = await scenario.PracticeReviewQueueService
                .GetQueueAsync("fr", CancellationToken.None);
            PracticeRecentActivityModel activity = await scenario.PracticeRecentActivityService
                .GetRecentActivityAsync("fr", 5, CancellationToken.None);

            Assert.NotEmpty(overview.ReviewPreview);
            Assert.NotEmpty(queue.Items);
            PracticeRecentActivityItemModel activityItem = Assert.Single(activity.Items);

            Assert.All(overview.ReviewPreview, item => Assert.Null(item.PrimaryMeaning));
            Assert.All(queue.Items, item => Assert.Null(item.PrimaryMeaning));
            Assert.Equal("Bewerbung", activityItem.Lemma);
            Assert.Null(activityItem.PrimaryMeaning);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            TryDeleteFile(packagePath);
            TryDeleteFile(databasePath);
        }
    }

    /// <summary>
    /// Verifies that inactive words stop contributing to Practice read models while their history stays persisted.
    /// </summary>
    [Fact]
    public async Task PracticeQueries_ShouldExcludeInactiveWordsButKeepAttemptHistoryPersisted()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-query-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-query-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            File.Copy(GetSamplePackagePath(), packagePath, overwrite: true);

            serviceProvider = BuildServiceProvider(databasePath);
            SeededPracticeScenario scenario = await CreateSeededScenarioAsync(serviceProvider, packagePath);
            DateTime attemptedAtUtc = DateTime.UtcNow.AddMinutes(-15);

            await scenario.PracticeQuizAnswerService.SubmitAsync(
                new PracticeQuizAnswerRequestModel(
                    scenario.ApplicationWord.PublicId,
                    PracticeAttemptOutcome.Incorrect,
                    AttemptedAtUtc: attemptedAtUtc),
                CancellationToken.None);

            await using (DarwinLinguaDbContext dbContext = await scenario.DbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                DarwinLingua.Catalog.Domain.Entities.WordEntry wordEntry = await dbContext.WordEntries
                    .SingleAsync(row => row.PublicId == scenario.ApplicationWord.PublicId, CancellationToken.None);
                dbContext.Entry(wordEntry).Property(row => row.PublicationStatus).CurrentValue = PublicationStatus.Archived;
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            PracticeOverviewModel overview = await scenario.PracticeOverviewService
                .GetOverviewAsync("en", CancellationToken.None);
            PracticeReviewQueueModel queue = await scenario.PracticeReviewQueueService
                .GetQueueAsync("en", CancellationToken.None);
            PracticeRecentActivityModel activity = await scenario.PracticeRecentActivityService
                .GetRecentActivityAsync("en", 10, CancellationToken.None);
            PracticeLearningProgressSnapshotModel snapshot = await scenario.PracticeLearningProgressSnapshotService
                .GetSnapshotAsync(CancellationToken.None);

            Assert.DoesNotContain(overview.ReviewPreview, item => item.WordEntryPublicId == scenario.ApplicationWord.PublicId);
            Assert.DoesNotContain(queue.Items, item => item.WordEntryPublicId == scenario.ApplicationWord.PublicId);
            Assert.DoesNotContain(activity.Items, item => item.WordEntryPublicId == scenario.ApplicationWord.PublicId);
            Assert.Equal(2, overview.TotalTrackedWords);
            Assert.Equal(1, activity.TotalAttempts);
            Assert.Equal(1, snapshot.TotalAttemptCount);

            await using DarwinLinguaDbContext verificationDbContext =
                await scenario.DbContextFactory.CreateDbContextAsync(CancellationToken.None);
            Assert.Equal(
                1,
                await verificationDbContext.PracticeAttempts.CountAsync(
                    row => row.WordEntryPublicId == scenario.ApplicationWord.PublicId,
                    CancellationToken.None));
            Assert.Equal(
                1,
                await verificationDbContext.PracticeReviewStates.CountAsync(
                    row => row.WordEntryPublicId == scenario.ApplicationWord.PublicId,
                    CancellationToken.None));
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            TryDeleteFile(packagePath);
            TryDeleteFile(databasePath);
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

        await userWordStateService.TrackWordViewedAsync(breadWord.PublicId, CancellationToken.None);
        await userWordStateService.MarkWordKnownAsync(breadWord.PublicId, CancellationToken.None);

        await userWordStateService.TrackWordViewedAsync(applicationWord.PublicId, CancellationToken.None);
        await userWordStateService.MarkWordDifficultAsync(applicationWord.PublicId, CancellationToken.None);

        await userWordStateService.TrackWordViewedAsync(indispensabilityWord.PublicId, CancellationToken.None);

        return new SeededPracticeScenario(
            serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>(),
            serviceProvider.GetRequiredService<IPracticeFlashcardAnswerService>(),
            serviceProvider.GetRequiredService<IPracticeLearningProgressSnapshotService>(),
            serviceProvider.GetRequiredService<IPracticeOverviewService>(),
            serviceProvider.GetRequiredService<IPracticeQuizAnswerService>(),
            serviceProvider.GetRequiredService<IPracticeRecentActivityService>(),
            serviceProvider.GetRequiredService<IPracticeReviewQueueService>(),
            applicationWord);
    }

    private static void TryDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private sealed record SeededPracticeScenario(
        IDbContextFactory<DarwinLinguaDbContext> DbContextFactory,
        IPracticeFlashcardAnswerService PracticeFlashcardAnswerService,
        IPracticeLearningProgressSnapshotService PracticeLearningProgressSnapshotService,
        IPracticeOverviewService PracticeOverviewService,
        IPracticeQuizAnswerService PracticeQuizAnswerService,
        IPracticeRecentActivityService PracticeRecentActivityService,
        IPracticeReviewQueueService PracticeReviewQueueService,
        WordListItemModel ApplicationWord);
}
