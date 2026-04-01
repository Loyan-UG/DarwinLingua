using DarwinLingua.Catalog.Application.Models;

namespace DarwinDeutsch.Maui.Services.Browse;

/// <summary>
/// Provides a short-lived in-memory cache for localized topic lists.
/// </summary>
public interface ITopicCatalogCacheService
{
    /// <summary>
    /// Returns the localized topic list, using cache when available.
    /// </summary>
    Task<IReadOnlyList<TopicListItemModel>> GetTopicsAsync(string uiLanguageCode, CancellationToken cancellationToken);

    /// <summary>
    /// Clears all cached topic-list payloads.
    /// </summary>
    void ResetCache();
}
