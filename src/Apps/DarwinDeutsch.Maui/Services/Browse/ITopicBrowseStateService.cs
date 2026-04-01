using DarwinLingua.Catalog.Application.Models;

namespace DarwinDeutsch.Maui.Services.Browse;

/// <summary>
/// Provides cached topic-browse pages and lightweight starting-word lookups.
/// </summary>
public interface ITopicBrowseStateService
{
    /// <summary>
    /// Loads one visual browse page for the specified topic using the current meaning-language preference.
    /// </summary>
    Task<IReadOnlyList<WordListItemModel>> GetWordsPageAsync(
        string topicKey,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Resolves the word that should open first for the specified topic.
    /// </summary>
    Task<Guid?> GetStartingWordPublicIdAsync(string topicKey, CancellationToken cancellationToken);

    /// <summary>
    /// Prefetches the initial topic slice to accelerate first navigation.
    /// </summary>
    Task PrefetchInitialSliceAsync(string topicKey, CancellationToken cancellationToken);

    /// <summary>
    /// Clears any cached topic browse data after local content changes.
    /// </summary>
    void ResetCache();
}
