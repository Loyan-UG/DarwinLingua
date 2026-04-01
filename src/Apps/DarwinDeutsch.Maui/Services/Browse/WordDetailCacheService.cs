using System.Collections.Concurrent;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinDeutsch.Maui.Services.Browse;

/// <summary>
/// Adds a small L1 cache in front of word-detail queries to reduce repeated SQLite work.
/// </summary>
internal sealed class WordDetailCacheService : IWordDetailCacheService
{
    private static readonly TimeSpan CacheEntryLifetime = TimeSpan.FromMinutes(10);
    private const int MaxCacheEntries = 96;

    private readonly IWordDetailQueryService _wordDetailQueryService;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="WordDetailCacheService"/> class.
    /// </summary>
    public WordDetailCacheService(IWordDetailQueryService wordDetailQueryService)
    {
        ArgumentNullException.ThrowIfNull(wordDetailQueryService);
        _wordDetailQueryService = wordDetailQueryService;
    }

    /// <inheritdoc />
    public async Task<WordDetailModel?> GetWordDetailsAsync(
        Guid publicId,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode,
        CancellationToken cancellationToken)
    {
        string cacheKey = BuildCacheKey(publicId, primaryMeaningLanguageCode, secondaryMeaningLanguageCode, uiLanguageCode);
        if (TryGetCachedValue(cacheKey, out WordDetailModel? cachedValue))
        {
            return cachedValue;
        }

        SemaphoreSlim gate = _gates.GetOrAdd(cacheKey, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (TryGetCachedValue(cacheKey, out cachedValue))
            {
                return cachedValue;
            }

            WordDetailModel? loadedValue = await _wordDetailQueryService
                .GetWordDetailsAsync(
                    publicId,
                    primaryMeaningLanguageCode,
                    secondaryMeaningLanguageCode,
                    uiLanguageCode,
                    cancellationToken)
                .ConfigureAwait(false);

            _cache[cacheKey] = new CacheEntry(loadedValue, DateTimeOffset.UtcNow);
            TrimCacheIfNeeded();
            return loadedValue;
        }
        finally
        {
            gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task PrefetchWordDetailsAsync(
        Guid publicId,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode,
        CancellationToken cancellationToken)
    {
        string cacheKey = BuildCacheKey(publicId, primaryMeaningLanguageCode, secondaryMeaningLanguageCode, uiLanguageCode);
        if (TryGetCachedValue(cacheKey, out _))
        {
            return;
        }

        await GetWordDetailsAsync(publicId, primaryMeaningLanguageCode, secondaryMeaningLanguageCode, uiLanguageCode, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void ResetCache()
    {
        _cache.Clear();
    }

    private bool TryGetCachedValue(string cacheKey, out WordDetailModel? value)
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

        value = null;
        return false;
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
                     .Select(pair => pair.Key!)
                     .ToArray())
        {
            _cache.TryRemove(staleKey, out _);
        }
    }

    private static string BuildCacheKey(Guid publicId, string primaryMeaningLanguageCode, string? secondaryMeaningLanguageCode, string uiLanguageCode)
    {
        return string.Join(
            '|',
            publicId.ToString("D"),
            primaryMeaningLanguageCode.Trim().ToLowerInvariant(),
            (secondaryMeaningLanguageCode ?? string.Empty).Trim().ToLowerInvariant(),
            uiLanguageCode.Trim().ToLowerInvariant());
    }

    private sealed record CacheEntry(WordDetailModel? Value, DateTimeOffset CachedAtUtc);
}
