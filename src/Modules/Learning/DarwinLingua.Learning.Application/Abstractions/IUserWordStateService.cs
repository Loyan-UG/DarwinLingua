using DarwinLingua.Learning.Application.Models;

namespace DarwinLingua.Learning.Application.Abstractions;

/// <summary>
/// Coordinates the Phase 1 lightweight user-word-state workflows.
/// </summary>
public interface IUserWordStateService
{
    /// <summary>
    /// Loads the current user-word state for the specified lexical entry.
    /// </summary>
    Task<UserWordStateModel?> GetWordStateAsync(Guid wordEntryPublicId, CancellationToken cancellationToken);

    /// <summary>
    /// Records that the user viewed the specified lexical entry.
    /// </summary>
    Task<UserWordStateModel> TrackWordViewedAsync(Guid wordEntryPublicId, CancellationToken cancellationToken);

    /// <summary>
    /// Marks the specified lexical entry as known.
    /// </summary>
    Task<UserWordStateModel> MarkWordKnownAsync(Guid wordEntryPublicId, CancellationToken cancellationToken);

    /// <summary>
    /// Marks the specified lexical entry as difficult.
    /// </summary>
    Task<UserWordStateModel> MarkWordDifficultAsync(Guid wordEntryPublicId, CancellationToken cancellationToken);

    /// <summary>
    /// Clears the known marker from the specified lexical entry.
    /// </summary>
    Task<UserWordStateModel> ClearWordKnownStateAsync(Guid wordEntryPublicId, CancellationToken cancellationToken);

    /// <summary>
    /// Clears the difficult marker from the specified lexical entry.
    /// </summary>
    Task<UserWordStateModel> ClearWordDifficultStateAsync(Guid wordEntryPublicId, CancellationToken cancellationToken);
}
