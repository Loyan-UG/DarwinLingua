using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Learning.Domain.Entities;
using DarwinLingua.Practice.Application.Abstractions;
using DarwinLingua.Practice.Application.DependencyInjection;
using DarwinLingua.Practice.Application.Models;
using DarwinLingua.Practice.Domain.Entities;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Practice.Application.Tests;

/// <summary>
/// Verifies quiz-answer Practice application workflows over the transactional persistence boundary.
/// </summary>
public sealed class PracticeQuizAnswerServiceTests
{
    /// <summary>
    /// Verifies that submitting a quiz answer persists both attempt history and updated review state.
    /// </summary>
    [Fact]
    public async Task SubmitAsync_ShouldPersistQuizAttemptAndSchedulingState()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-app-quiz-{Guid.NewGuid():N}.db");
        await using ServiceProvider serviceProvider = BuildServiceProvider(databasePath);

        IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.EnsureDatabaseSchemaAsync(CancellationToken.None);

        Guid wordPublicId = Guid.NewGuid();
        await SeedTrackedWordAsync(serviceProvider, wordPublicId, isActive: true);

        IPracticeQuizAnswerService service = serviceProvider.GetRequiredService<IPracticeQuizAnswerService>();
        DateTime attemptedAtUtc = DateTime.UtcNow.AddMinutes(-20);

        PracticeQuizAnswerResultModel result = await service.SubmitAsync(
            new PracticeQuizAnswerRequestModel(
                wordPublicId,
                PracticeAttemptOutcome.Hard,
                ResponseMilliseconds: 980,
                AttemptedAtUtc: attemptedAtUtc),
            CancellationToken.None);

        Assert.Equal(wordPublicId, result.WordEntryPublicId);
        Assert.Equal(PracticeAttemptOutcome.Hard, result.Outcome);
        Assert.Equal(attemptedAtUtc.AddHours(8), result.DueAtUtcAfterAttempt);
        Assert.Equal(1, result.TotalAttemptCount);
        Assert.Equal(1, result.ConsecutiveSuccessCount);
        Assert.Equal(0, result.ConsecutiveFailureCount);

        IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
            serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None);

        PracticeReviewState reviewState = Assert.Single(
            dbContext.PracticeReviewStates.Where(state => state.WordEntryPublicId == wordPublicId));
        PracticeAttempt attempt = Assert.Single(
            dbContext.PracticeAttempts.Where(row => row.WordEntryPublicId == wordPublicId));

        Assert.Equal(PracticeSessionType.Quiz, reviewState.LastSessionType);
        Assert.Equal(PracticeAttemptOutcome.Hard, reviewState.LastOutcome);
        Assert.Equal(result.DueAtUtcAfterAttempt, reviewState.DueAtUtc);
        Assert.Equal(PracticeSessionType.Quiz, attempt.SessionType);
        Assert.Equal(980, attempt.ResponseMilliseconds);
    }

    /// <summary>
    /// Verifies that quiz answers are rejected when the target lexical entry is not an active tracked word.
    /// </summary>
    [Fact]
    public async Task SubmitAsync_ShouldRejectUntrackedOrInactiveWord()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-app-quiz-reject-{Guid.NewGuid():N}.db");
        await using ServiceProvider serviceProvider = BuildServiceProvider(databasePath);

        IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.EnsureDatabaseSchemaAsync(CancellationToken.None);

        Guid wordPublicId = Guid.NewGuid();
        await SeedTrackedWordAsync(serviceProvider, wordPublicId, isActive: false);

        IPracticeQuizAnswerService service = serviceProvider.GetRequiredService<IPracticeQuizAnswerService>();

        await Assert.ThrowsAsync<DomainRuleException>(() => service.SubmitAsync(
            new PracticeQuizAnswerRequestModel(
                wordPublicId,
                PracticeAttemptOutcome.Incorrect,
                ResponseMilliseconds: 1100,
                AttemptedAtUtc: DateTime.UtcNow.AddMinutes(-5)),
            CancellationToken.None));
    }

    /// <summary>
    /// Verifies that an incorrect quiz answer is scheduled with the short first-failure interval.
    /// </summary>
    [Fact]
    public async Task SubmitAsync_IncorrectOutcome_ShouldScheduleShortFailureInterval()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-app-quiz-incorrect-{Guid.NewGuid():N}.db");
        await using ServiceProvider serviceProvider = BuildServiceProvider(databasePath);

        IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.EnsureDatabaseSchemaAsync(CancellationToken.None);

        Guid wordPublicId = Guid.NewGuid();
        await SeedTrackedWordAsync(serviceProvider, wordPublicId, isActive: true);

        IPracticeQuizAnswerService service = serviceProvider.GetRequiredService<IPracticeQuizAnswerService>();
        DateTime attemptedAtUtc = DateTime.UtcNow.AddMinutes(-5);

        PracticeQuizAnswerResultModel result = await service.SubmitAsync(
            new PracticeQuizAnswerRequestModel(
                wordPublicId,
                PracticeAttemptOutcome.Incorrect,
                ResponseMilliseconds: null,
                AttemptedAtUtc: attemptedAtUtc),
            CancellationToken.None);

        Assert.Equal(PracticeAttemptOutcome.Incorrect, result.Outcome);
        Assert.Equal(attemptedAtUtc.AddMinutes(10), result.DueAtUtcAfterAttempt);
        Assert.Equal(0, result.ConsecutiveSuccessCount);
        Assert.Equal(1, result.ConsecutiveFailureCount);
    }

    /// <summary>
    /// Verifies that a correct quiz answer is scheduled with the initial one-day interval.
    /// </summary>
    [Fact]
    public async Task SubmitAsync_CorrectOutcome_ShouldScheduleOneDayInterval()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-app-quiz-correct-{Guid.NewGuid():N}.db");
        await using ServiceProvider serviceProvider = BuildServiceProvider(databasePath);

        IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.EnsureDatabaseSchemaAsync(CancellationToken.None);

        Guid wordPublicId = Guid.NewGuid();
        await SeedTrackedWordAsync(serviceProvider, wordPublicId, isActive: true);

        IPracticeQuizAnswerService service = serviceProvider.GetRequiredService<IPracticeQuizAnswerService>();
        DateTime attemptedAtUtc = DateTime.UtcNow.AddMinutes(-10);

        PracticeQuizAnswerResultModel result = await service.SubmitAsync(
            new PracticeQuizAnswerRequestModel(
                wordPublicId,
                PracticeAttemptOutcome.Correct,
                ResponseMilliseconds: 500,
                AttemptedAtUtc: attemptedAtUtc),
            CancellationToken.None);

        Assert.Equal(PracticeAttemptOutcome.Correct, result.Outcome);
        Assert.Equal(attemptedAtUtc.AddDays(1), result.DueAtUtcAfterAttempt);
        Assert.Equal(1, result.ConsecutiveSuccessCount);
        Assert.Equal(0, result.ConsecutiveFailureCount);
    }

    private static ServiceProvider BuildServiceProvider(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        ServiceCollection services = new();
        services.AddDarwinLinguaInfrastructure(options => options.DatabasePath = databasePath);
        services.AddPracticeApplication();

        return services.BuildServiceProvider();
    }

    private static async Task SeedTrackedWordAsync(ServiceProvider serviceProvider, Guid wordPublicId, bool isActive)
    {
        IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
            serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None);

        WordEntry wordEntry = new(
            Guid.NewGuid(),
            wordPublicId,
            "Haus",
            LanguageCode.From("de"),
            CefrLevel.A1,
            PartOfSpeech.Noun,
            isActive ? PublicationStatus.Active : PublicationStatus.Draft,
            ContentSourceType.ExternalCurated,
            DateTime.UtcNow.AddDays(-2),
            article: "das");

        UserWordState userWordState = new(Guid.NewGuid(), "local-installation-user", wordPublicId, DateTime.UtcNow.AddDays(-1));
        userWordState.TrackViewed(DateTime.UtcNow.AddHours(-12));

        dbContext.WordEntries.Add(wordEntry);
        dbContext.UserWordStates.Add(userWordState);
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }
}
