using System.Collections.Concurrent;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinDeutsch.Maui.Services.Browse;

/// <summary>
/// Adds a small L1 cache in front of localized topic-list queries.
/// </summary>
internal sealed class TopicCatalogCacheService : ITopicCatalogCacheService
{
    private static readonly TimeSpan CacheEntryLifetime = TimeSpan.FromMinutes(10);

    private readonly ITopicQueryService _topicQueryService;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="TopicCatalogCacheService"/> class.
    /// </summary>
    public TopicCatalogCacheService(ITopicQueryService topicQueryService)
    {
        ArgumentNullException.ThrowIfNull(topicQueryService);
        _topicQueryService = topicQueryService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TopicListItemModel>> GetTopicsAsync(string uiLanguageCode, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uiLanguageCode);

        string cacheKey = uiLanguageCode.Trim().ToLowerInvariant();
        if (TryGetCachedValue(cacheKey, out IReadOnlyList<TopicListItemModel>? cachedTopics))
        {
            return cachedTopics!;
        }

        SemaphoreSlim gate = _gates.GetOrAdd(cacheKey, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (TryGetCachedValue(cacheKey, out cachedTopics))
            {
                return cachedTopics!;
            }

            IReadOnlyList<TopicListItemModel> topics = await _topicQueryService
                .GetTopicsAsync(uiLanguageCode, cancellationToken)
                .ConfigureAwait(false);

            _cache[cacheKey] = new CacheEntry(topics, DateTimeOffset.UtcNow);
            return topics;
        }
        finally
        {
            gate.Release();
        }
    }

    /// <inheritdoc />
    public void ResetCache()
    {
        _cache.Clear();
    }

    private bool TryGetCachedValue(string cacheKey, out IReadOnlyList<TopicListItemModel>? topics)
    {
        if (_cache.TryGetValue(cacheKey, out CacheEntry? entry) && entry is not null)
        {
            if (DateTimeOffset.UtcNow - entry.CachedAtUtc <= CacheEntryLifetime)
            {
                topics = entry.Topics;
                return true;
            }

            _cache.TryRemove(cacheKey, out _);
        }

        topics = null;
        return false;
    }

    private sealed record CacheEntry(IReadOnlyList<TopicListItemModel> Topics, DateTimeOffset CachedAtUtc);
}
