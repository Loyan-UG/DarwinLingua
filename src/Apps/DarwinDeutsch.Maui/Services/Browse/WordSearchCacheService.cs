using System.Collections.Concurrent;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinDeutsch.Maui.Services.Browse;

/// <summary>
/// Adds a small in-memory cache and in-flight deduplication in front of search queries.
/// </summary>
internal sealed class WordSearchCacheService : IWordSearchCacheService
{
    private static readonly TimeSpan CacheEntryLifetime = TimeSpan.FromMinutes(5);
    private const int MaxCacheEntries = 64;

    private readonly IWordQueryService _wordQueryService;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Task<IReadOnlyList<WordListItemModel>>> _inFlightSearches = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="WordSearchCacheService"/> class.
    /// </summary>
    public WordSearchCacheService(IWordQueryService wordQueryService)
    {
        ArgumentNullException.ThrowIfNull(wordQueryService);
        _wordQueryService = wordQueryService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WordListItemModel>> SearchAsync(
        string query,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        ArgumentException.ThrowIfNullOrWhiteSpace(primaryMeaningLanguageCode);

        string cacheKey = BuildCacheKey(query, primaryMeaningLanguageCode);
        if (TryGetCachedValue(cacheKey, out IReadOnlyList<WordListItemModel>? cachedResults))
        {
            return cachedResults;
        }

        Task<IReadOnlyList<WordListItemModel>> searchTask = _inFlightSearches.GetOrAdd(
            cacheKey,
            _ => LoadResultsAsync(query, primaryMeaningLanguageCode, cacheKey, CancellationToken.None));

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await searchTask.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (searchTask.IsCompleted)
            {
                _inFlightSearches.TryRemove(cacheKey, out _);
            }
        }
    }

    /// <inheritdoc />
    public void ResetCache()
    {
        _cache.Clear();
        _inFlightSearches.Clear();
    }

    private bool TryGetCachedValue(string cacheKey, out IReadOnlyList<WordListItemModel> value)
    {
        if (_cache.TryGetValue(cacheKey, out CacheEntry? entry) && entry is not null)
        {
            if (DateTimeOffset.UtcNow - entry.CachedAtUtc <= CacheEntryLifetime)
            {
                value = entry.Value;
                return true;
            }

            _cache.TryRemove(cacheKey, out _);
        }

        value = Array.Empty<WordListItemModel>();
        return false;
    }

    private async Task<IReadOnlyList<WordListItemModel>> LoadResultsAsync(
        string query,
        string primaryMeaningLanguageCode,
        string cacheKey,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<WordListItemModel> results = await _wordQueryService
                .SearchWordsAsync(query, primaryMeaningLanguageCode, cancellationToken)
                .ConfigureAwait(false);

            _cache[cacheKey] = new CacheEntry(results, DateTimeOffset.UtcNow);
            TrimCacheIfNeeded();
            return results;
        }
        finally
        {
            _inFlightSearches.TryRemove(cacheKey, out _);
        }
    }

    private void TrimCacheIfNeeded()
    {
        if (_cache.Count <= MaxCacheEntries)
        {
            return;
        }

        foreach (string staleKey in _cache
                     .OrderBy(pair => pair.Value.CachedAtUtc)
                     .Take(Math.Max(1, _cache.Count - MaxCacheEntries))
                     .Select(pair => pair.Key)
                     .ToArray())
        {
            _cache.TryRemove(staleKey, out _);
        }
    }

    private static string BuildCacheKey(string query, string primaryMeaningLanguageCode)
    {
        return $"{query.Trim().ToLowerInvariant()}::{primaryMeaningLanguageCode.Trim().ToLowerInvariant()}";
    }

    private sealed record CacheEntry(IReadOnlyList<WordListItemModel> Value, DateTimeOffset CachedAtUtc);
}
