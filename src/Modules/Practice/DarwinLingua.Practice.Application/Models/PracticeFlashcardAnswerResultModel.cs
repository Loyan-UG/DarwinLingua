using DarwinLingua.Practice.Domain.Entities;

namespace DarwinLingua.Practice.Application.Models;

/// <summary>
/// Represents the persisted flashcard-answer result and updated scheduling snapshot.
/// </summary>
public sealed record PracticeFlashcardAnswerResultModel(
    Guid WordEntryPublicId,
    PracticeAttemptOutcome Outcome,
    DateTime AttemptedAtUtc,
    DateTime? DueAtUtcBeforeAttempt,
    DateTime DueAtUtcAfterAttempt,
    int TotalAttemptCount,
    int ConsecutiveSuccessCount,
    int ConsecutiveFailureCount);
