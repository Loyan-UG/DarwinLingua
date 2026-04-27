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
/// Verifies flashcard-answer Practice application workflows over the transactional persistence boundary.
/// </summary>
public sealed class PracticeFlashcardAnswerServiceTests
{
    /// <summary>
    /// Verifies that submitting a correct flashcard answer persists both attempt history and updated review state
    /// with the Flashcard session type.
    /// </summary>
    [Fact]
    public async Task SubmitAsync_ShouldPersistFlashcardAttemptAndSchedulingState()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-app-flashcard-{Guid.NewGuid():N}.db");
        await using ServiceProvider serviceProvider = BuildServiceProvider(databasePath);

        IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.EnsureDatabaseSchemaAsync(CancellationToken.None);

        Guid wordPublicId = Guid.NewGuid();
        await SeedTrackedWordAsync(serviceProvider, wordPublicId, isActive: true);

        IPracticeFlashcardAnswerService service = serviceProvider.GetRequiredService<IPracticeFlashcardAnswerService>();
        DateTime attemptedAtUtc = DateTime.UtcNow.AddMinutes(-15);

        PracticeFlashcardAnswerResultModel result = await service.SubmitAsync(
            new PracticeFlashcardAnswerRequestModel(
                wordPublicId,
                PracticeAttemptOutcome.Correct,
                ResponseMilliseconds: 750,
                AttemptedAtUtc: attemptedAtUtc),
            CancellationToken.None);

        Assert.Equal(wordPublicId, result.WordEntryPublicId);
        Assert.Equal(PracticeAttemptOutcome.Correct, result.Outcome);
        Assert.Equal(attemptedAtUtc.AddDays(1), result.DueAtUtcAfterAttempt);
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

        Assert.Equal(PracticeSessionType.Flashcard, reviewState.LastSessionType);
        Assert.Equal(PracticeAttemptOutcome.Correct, reviewState.LastOutcome);
        Assert.Equal(result.DueAtUtcAfterAttempt, reviewState.DueAtUtc);
        Assert.Equal(PracticeSessionType.Flashcard, attempt.SessionType);
        Assert.Equal(750, attempt.ResponseMilliseconds);
    }

    /// <summary>
    /// Verifies that an incorrect flashcard answer is scheduled with the short failure interval.
    /// </summary>
    [Fact]
    public async Task SubmitAsync_IncorrectOutcome_ShouldScheduleShortFailureInterval()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-app-flashcard-incorrect-{Guid.NewGuid():N}.db");
        await using ServiceProvider serviceProvider = BuildServiceProvider(databasePath);

        IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.EnsureDatabaseSchemaAsync(CancellationToken.None);

        Guid wordPublicId = Guid.NewGuid();
        await SeedTrackedWordAsync(serviceProvider, wordPublicId, isActive: true);

        IPracticeFlashcardAnswerService service = serviceProvider.GetRequiredService<IPracticeFlashcardAnswerService>();
        DateTime attemptedAtUtc = DateTime.UtcNow.AddMinutes(-5);

        PracticeFlashcardAnswerResultModel result = await service.SubmitAsync(
            new PracticeFlashcardAnswerRequestModel(
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
    /// Verifies that an easy flashcard answer is scheduled with the longest initial easy interval.
    /// </summary>
    [Fact]
    public async Task SubmitAsync_EasyOutcome_ShouldScheduleLongestInitialInterval()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-app-flashcard-easy-{Guid.NewGuid():N}.db");
        await using ServiceProvider serviceProvider = BuildServiceProvider(databasePath);

        IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.EnsureDatabaseSchemaAsync(CancellationToken.None);

        Guid wordPublicId = Guid.NewGuid();
        await SeedTrackedWordAsync(serviceProvider, wordPublicId, isActive: true);

        IPracticeFlashcardAnswerService service = serviceProvider.GetRequiredService<IPracticeFlashcardAnswerService>();
        DateTime attemptedAtUtc = DateTime.UtcNow.AddMinutes(-2);

        PracticeFlashcardAnswerResultModel result = await service.SubmitAsync(
            new PracticeFlashcardAnswerRequestModel(
                wordPublicId,
                PracticeAttemptOutcome.Easy,
                ResponseMilliseconds: null,
                AttemptedAtUtc: attemptedAtUtc),
            CancellationToken.None);

        Assert.Equal(PracticeAttemptOutcome.Easy, result.Outcome);
        Assert.Equal(attemptedAtUtc.AddDays(3), result.DueAtUtcAfterAttempt);
        Assert.Equal(1, result.ConsecutiveSuccessCount);
    }

    /// <summary>
    /// Verifies that flashcard answers are rejected when the target lexical entry is not an active tracked word.
    /// </summary>
    [Fact]
    public async Task SubmitAsync_ShouldRejectUntrackedOrInactiveWord()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-app-flashcard-reject-{Guid.NewGuid():N}.db");
        await using ServiceProvider serviceProvider = BuildServiceProvider(databasePath);

        IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.EnsureDatabaseSchemaAsync(CancellationToken.None);

        Guid wordPublicId = Guid.NewGuid();
        await SeedTrackedWordAsync(serviceProvider, wordPublicId, isActive: false);

        IPracticeFlashcardAnswerService service = serviceProvider.GetRequiredService<IPracticeFlashcardAnswerService>();

        await Assert.ThrowsAsync<DomainRuleException>(() => service.SubmitAsync(
            new PracticeFlashcardAnswerRequestModel(
                wordPublicId,
                PracticeAttemptOutcome.Correct,
                ResponseMilliseconds: 500,
                AttemptedAtUtc: DateTime.UtcNow.AddMinutes(-3)),
            CancellationToken.None));
    }

    /// <summary>
    /// Verifies that a flashcard answer with an empty word public identifier is rejected.
    /// </summary>
    [Fact]
    public async Task SubmitAsync_ShouldRejectEmptyWordPublicId()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-practice-app-flashcard-empty-guid-{Guid.NewGuid():N}.db");
        await using ServiceProvider serviceProvider = BuildServiceProvider(databasePath);

        IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.EnsureDatabaseSchemaAsync(CancellationToken.None);

        IPracticeFlashcardAnswerService service = serviceProvider.GetRequiredService<IPracticeFlashcardAnswerService>();

        await Assert.ThrowsAsync<ArgumentException>(() => service.SubmitAsync(
            new PracticeFlashcardAnswerRequestModel(
                Guid.Empty,
                PracticeAttemptOutcome.Correct,
                ResponseMilliseconds: 500,
                AttemptedAtUtc: DateTime.UtcNow.AddMinutes(-3)),
            CancellationToken.None));
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
            "Fenster",
            LanguageCode.From("de"),
            CefrLevel.A1,
            PartOfSpeech.Noun,
            isActive ? PublicationStatus.Active : PublicationStatus.Draft,
            ContentSourceType.ExternalCurated,
            DateTime.UtcNow.AddDays(-3),
            article: "das");

        UserWordState userWordState = new(Guid.NewGuid(), "local-installation-user", wordPublicId, DateTime.UtcNow.AddDays(-2));
        userWordState.TrackViewed(DateTime.UtcNow.AddHours(-6));

        dbContext.WordEntries.Add(wordEntry);
        dbContext.UserWordStates.Add(userWordState);
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }
}
