namespace DarwinLingua.Practice.Application.Models;

/// <summary>
/// Represents one ordered item in the learner's current review queue.
/// </summary>
public sealed record PracticeReviewQueueItemModel(
    int Position,
    Guid WordEntryPublicId,
    string Lemma,
    string CefrLevel,
    string? PrimaryMeaning,
    DateTime? DueAtUtc,
    bool IsDueNow,
    bool IsDifficult,
    bool IsKnown,
    int ViewCount,
    DateTime LastViewedAtUtc);
