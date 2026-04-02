using DarwinLingua.Catalog.Application.Models;

namespace DarwinDeutsch.Maui.Services.Browse;

/// <summary>
/// Provides a lightweight L1 cache in front of word search queries.
/// </summary>
public interface IWordSearchCacheService
{
    /// <summary>
    /// Gets cached or freshly loaded search results for the supplied query and meaning language.
    /// </summary>
    Task<IReadOnlyList<WordListItemModel>> SearchAsync(
        string query,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken);

    /// <summary>
    /// Clears cached search results after local content changes.
    /// </summary>
    void ResetCache();
}
