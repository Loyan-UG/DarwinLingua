using DarwinLingua.Catalog.Application.Models;

namespace DarwinDeutsch.Maui.Services.Browse;

/// <summary>
/// Provides a short-lived in-memory cache for learner-facing word detail screens.
/// </summary>
public interface IWordDetailCacheService
{
    /// <summary>
    /// Loads one word-detail model from cache when possible, otherwise from the catalog query service.
    /// </summary>
    Task<WordDetailModel?> GetWordDetailsAsync(
        Guid publicId,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode,
        CancellationToken cancellationToken);

    /// <summary>
    /// Warms the in-memory cache for one word-detail payload without blocking the caller.
    /// </summary>
    Task PrefetchWordDetailsAsync(
        Guid publicId,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode,
        CancellationToken cancellationToken);

    /// <summary>
    /// Clears all cached word-detail payloads after content changes.
    /// </summary>
    void ResetCache();
}
