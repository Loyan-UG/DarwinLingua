using DarwinDeutsch.Maui.Services.Browse.Models;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Models;
using System.Collections.Concurrent;

namespace DarwinDeutsch.Maui.Services.Browse;

/// <summary>
/// Caches curated word collections for the current meaning-language preference.
/// </summary>
internal sealed class WordCollectionBrowseStateService : IWordCollectionBrowseStateService
{
    private readonly ConcurrentDictionary<string, IReadOnlyList<WordCollectionListItemModel>> _collectionListCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, WordCollectionDetailModel?> _collectionDetailCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _detailGates = new(StringComparer.OrdinalIgnoreCase);
    private readonly IWordCollectionQueryService _wordCollectionQueryService;
    private readonly IActiveLearningProfileCacheService _activeLearningProfileCacheService;

    public WordCollectionBrowseStateService(
        IWordCollectionQueryService wordCollectionQueryService,
        IActiveLearningProfileCacheService activeLearningProfileCacheService)
    {
        ArgumentNullException.ThrowIfNull(wordCollectionQueryService);
        ArgumentNullException.ThrowIfNull(activeLearningProfileCacheService);

        _wordCollectionQueryService = wordCollectionQueryService;
        _activeLearningProfileCacheService = activeLearningProfileCacheService;
    }

    public async Task<IReadOnlyList<WordCollectionListItemModel>> GetCollectionsAsync(CancellationToken cancellationToken)
    {
        UserLearningProfileModel profile = await _activeLearningProfileCacheService
            .GetCurrentProfileAsync(cancellationToken)
            .ConfigureAwait(false);

        string cacheKey = BuildCollectionsCacheKey(profile.PreferredMeaningLanguage1);
        if (_collectionListCache.TryGetValue(cacheKey, out IReadOnlyList<WordCollectionListItemModel>? cachedCollections))
        {
            return cachedCollections;
        }

        IReadOnlyList<WordCollectionListItemModel> collections = await _wordCollectionQueryService
            .GetPublishedCollectionsAsync(profile.PreferredMeaningLanguage1, cancellationToken)
            .ConfigureAwait(false);

        _collectionListCache[cacheKey] = collections;
        return collections;
    }

    public async Task<WordCollectionDetailModel?> GetCollectionAsync(string slug, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        UserLearningProfileModel profile = await _activeLearningProfileCacheService
            .GetCurrentProfileAsync(cancellationToken)
            .ConfigureAwait(false);

        string cacheKey = BuildCollectionDetailCacheKey(slug, profile.PreferredMeaningLanguage1);
        if (_collectionDetailCache.TryGetValue(cacheKey, out WordCollectionDetailModel? cachedCollection))
        {
            return cachedCollection;
        }

        SemaphoreSlim gate = _detailGates.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_collectionDetailCache.TryGetValue(cacheKey, out cachedCollection))
            {
                return cachedCollection;
            }

            WordCollectionDetailModel? collection = await _wordCollectionQueryService
                .GetPublishedCollectionBySlugAsync(slug, profile.PreferredMeaningLanguage1, cancellationToken)
                .ConfigureAwait(false);

            _collectionDetailCache[cacheKey] = collection;
            return collection;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<Guid?> GetStartingWordPublicIdAsync(string slug, CancellationToken cancellationToken)
    {
        WordCollectionDetailModel? collection = await GetCollectionAsync(slug, cancellationToken).ConfigureAwait(false);
        return collection?.Words.FirstOrDefault()?.PublicId;
    }

    public async Task<CefrBrowseNavigationState> GetNavigationStateAsync(
        string slug,
        Guid currentWordPublicId,
        CancellationToken cancellationToken)
    {
        WordCollectionDetailModel? collection = await GetCollectionAsync(slug, cancellationToken).ConfigureAwait(false);
        if (collection is null || collection.Words.Count == 0)
        {
            return new CefrBrowseNavigationState(null, null, 0, 0);
        }

        int currentIndex = collection.Words
            .Select((word, index) => new { word.PublicId, Index = index })
            .Where(item => item.PublicId == currentWordPublicId)
            .Select(item => item.Index)
            .DefaultIfEmpty(-1)
            .First();

        if (currentIndex < 0)
        {
            return new CefrBrowseNavigationState(null, null, 0, collection.Words.Count);
        }

        Guid? previousWordPublicId = currentIndex > 0
            ? collection.Words[currentIndex - 1].PublicId
            : null;
        Guid? nextWordPublicId = currentIndex < collection.Words.Count - 1
            ? collection.Words[currentIndex + 1].PublicId
            : null;

        return new CefrBrowseNavigationState(previousWordPublicId, nextWordPublicId, currentIndex + 1, collection.Words.Count);
    }

    public async Task PrefetchCollectionAsync(string slug, CancellationToken cancellationToken)
    {
        _ = await GetCollectionAsync(slug, cancellationToken).ConfigureAwait(false);
    }

    public void ResetCache()
    {
        _collectionListCache.Clear();
        _collectionDetailCache.Clear();
    }

    private static string BuildCollectionsCacheKey(string meaningLanguageCode) =>
        $"collections::{meaningLanguageCode.Trim().ToLowerInvariant()}";

    private static string BuildCollectionDetailCacheKey(string slug, string meaningLanguageCode) =>
        $"{slug.Trim().ToLowerInvariant()}::{meaningLanguageCode.Trim().ToLowerInvariant()}";
}
