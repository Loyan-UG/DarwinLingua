using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Practice.Application.Abstractions;
using DarwinLingua.Practice.Application.Models;
using DarwinLingua.Practice.Domain.Entities;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Practice.Application.Services;

/// <summary>
/// Persists flashcard answers and updates the learner's review scheduling state.
/// </summary>
internal sealed class PracticeFlashcardAnswerService : IPracticeFlashcardAnswerService
{
    private const string LocalUserId = "local-installation-user";

    private readonly ITransactionalExecutionService _transactionalExecutionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PracticeFlashcardAnswerService"/> class.
    /// </summary>
    public PracticeFlashcardAnswerService(ITransactionalExecutionService transactionalExecutionService)
    {
        ArgumentNullException.ThrowIfNull(transactionalExecutionService);

        _transactionalExecutionService = transactionalExecutionService;
    }

    /// <inheritdoc />
    public Task<PracticeFlashcardAnswerResultModel> SubmitAsync(
        PracticeFlashcardAnswerRequestModel request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.WordEntryPublicId == Guid.Empty)
        {
            throw new ArgumentException("Word public identifier cannot be empty.", nameof(request));
        }

        if (!Enum.IsDefined(request.Outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(request), request.Outcome, "Unsupported practice attempt outcome.");
        }

        DateTime attemptedAtUtc = request.AttemptedAtUtc ?? DateTime.UtcNow;
        attemptedAtUtc = attemptedAtUtc.Kind == DateTimeKind.Utc ? attemptedAtUtc : attemptedAtUtc.ToUniversalTime();

        return _transactionalExecutionService.ExecuteAsync(
            async (dbContext, operationCancellationToken) =>
                await SubmitAsync(dbContext, request, attemptedAtUtc, operationCancellationToken).ConfigureAwait(false),
            cancellationToken);
    }

    private static async Task<PracticeFlashcardAnswerResultModel> SubmitAsync(
        DarwinLinguaDbContext dbContext,
        PracticeFlashcardAnswerRequestModel request,
        DateTime attemptedAtUtc,
        CancellationToken cancellationToken)
    {
        if (!await IsTrackedActiveWordAsync(dbContext, request.WordEntryPublicId, cancellationToken).ConfigureAwait(false))
        {
            throw new DomainRuleException("Flashcard answers can only be recorded for active tracked lexical entries.");
        }

        PracticeReviewState? reviewState = await dbContext.PracticeReviewStates
            .SingleOrDefaultAsync(
                state => state.UserId == LocalUserId && state.WordEntryPublicId == request.WordEntryPublicId,
                cancellationToken)
            .ConfigureAwait(false);

        reviewState ??= CreateReviewState(request.WordEntryPublicId, attemptedAtUtc);

        int consecutiveSuccessCountBeforeAttempt = reviewState.ConsecutiveSuccessCount;
        int consecutiveFailureCountBeforeAttempt = reviewState.ConsecutiveFailureCount;
        DateTime? dueAtUtcBeforeAttempt = reviewState.DueAtUtc;
        DateTime dueAtUtcAfterAttempt = PracticeSchedulingPolicy.GetNextDueAtUtc(
            request.Outcome,
            consecutiveSuccessCountBeforeAttempt,
            consecutiveFailureCountBeforeAttempt,
            attemptedAtUtc);

        PracticeAttempt attempt = new(
            Guid.NewGuid(),
            LocalUserId,
            request.WordEntryPublicId,
            PracticeSessionType.Flashcard,
            request.Outcome,
            attemptedAtUtc,
            dueAtUtcBeforeAttempt,
            dueAtUtcAfterAttempt,
            request.ResponseMilliseconds);

        reviewState.RecordAttempt(
            PracticeSessionType.Flashcard,
            request.Outcome,
            attemptedAtUtc,
            dueAtUtcAfterAttempt);

        if (dbContext.Entry(reviewState).State == EntityState.Detached)
        {
            dbContext.PracticeReviewStates.Add(reviewState);
        }

        dbContext.PracticeAttempts.Add(attempt);

        return new PracticeFlashcardAnswerResultModel(
            reviewState.WordEntryPublicId,
            request.Outcome,
            attemptedAtUtc,
            dueAtUtcBeforeAttempt,
            dueAtUtcAfterAttempt,
            reviewState.TotalAttemptCount,
            reviewState.ConsecutiveSuccessCount,
            reviewState.ConsecutiveFailureCount);
    }

    private static PracticeReviewState CreateReviewState(Guid wordEntryPublicId, DateTime attemptedAtUtc)
    {
        return new PracticeReviewState(
            Guid.NewGuid(),
            LocalUserId,
            wordEntryPublicId,
            attemptedAtUtc);
    }

    private static Task<bool> IsTrackedActiveWordAsync(
        DarwinLinguaDbContext dbContext,
        Guid wordEntryPublicId,
        CancellationToken cancellationToken)
    {
        return (
            from userWordState in dbContext.UserWordStates
            join wordEntry in dbContext.WordEntries
                on userWordState.WordEntryPublicId equals wordEntry.PublicId
            where userWordState.UserId == LocalUserId &&
                userWordState.WordEntryPublicId == wordEntryPublicId &&
                wordEntry.PublicationStatus == PublicationStatus.Active
            select userWordState.WordEntryPublicId)
            .AnyAsync(cancellationToken);
    }
}
