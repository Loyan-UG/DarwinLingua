using DarwinLingua.Practice.Domain.Entities;

namespace DarwinLingua.Practice.Application.Models;

/// <summary>
/// Represents one flashcard answer submitted by the learner.
/// </summary>
public sealed record PracticeFlashcardAnswerRequestModel(
    Guid WordEntryPublicId,
    PracticeAttemptOutcome Outcome,
    int? ResponseMilliseconds = null,
    DateTime? AttemptedAtUtc = null);
