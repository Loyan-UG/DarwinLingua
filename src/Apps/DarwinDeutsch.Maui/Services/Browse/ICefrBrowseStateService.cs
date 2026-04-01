using DarwinDeutsch.Maui.Services.Browse.Models;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinDeutsch.Maui.Services.Browse;

/// <summary>
/// Provides cached CEFR-browse state and remembers the learner's last viewed word per level.
/// </summary>
public interface ICefrBrowseStateService
{
    /// <summary>
    /// Loads the browseable words for the specified CEFR level using the current meaning-language preference.
    /// </summary>
    Task<IReadOnlyList<WordListItemModel>> GetWordsAsync(string cefrLevel, CancellationToken cancellationToken);

    /// <summary>
    /// Loads one visual browse page for the specified CEFR level using the current meaning-language preference.
    /// </summary>
    Task<IReadOnlyList<WordListItemModel>> GetWordsPageAsync(
        string cefrLevel,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Resolves the word that should open first for the specified CEFR level.
    /// </summary>
    Task<Guid?> GetStartingWordPublicIdAsync(string cefrLevel, CancellationToken cancellationToken);

    /// <summary>
    /// Resolves the previous/next navigation state for the current word inside the specified CEFR level.
    /// </summary>
    Task<CefrBrowseNavigationState> GetNavigationStateAsync(
        string cefrLevel,
        Guid currentWordPublicId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Clears any cached CEFR browse data after local content changes.
    /// </summary>
    void ResetCache();

    /// <summary>
    /// Persists the most recently viewed word for the specified CEFR level.
    /// </summary>
    void RememberLastViewedWord(string cefrLevel, Guid wordPublicId);
}
