namespace DarwinLingua.Learning.Application.Models;

/// <summary>
/// Represents a presentation-safe snapshot of lightweight user-word state.
/// </summary>
public sealed record UserWordStateModel(
    Guid WordEntryPublicId,
    bool IsKnown,
    bool IsDifficult,
    DateTime? FirstViewedAtUtc,
    DateTime? LastViewedAtUtc,
    int ViewCount);
