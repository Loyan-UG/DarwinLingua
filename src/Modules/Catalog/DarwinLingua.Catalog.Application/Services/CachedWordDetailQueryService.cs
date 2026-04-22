using System.Collections.Concurrent;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using Microsoft.Extensions.Caching.Memory;

namespace DarwinLingua.Catalog.Application.Services;

/// <summary>
/// Adds a bounded in-memory cache in front of expensive word-detail projections.
/// </summary>
internal sealed class CachedWordDetailQueryService(
    WordDetailQueryService innerService,
    IMemoryCache memoryCache) : IWordDetailQueryService
{
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(5);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Gates = new(StringComparer.OrdinalIgnoreCase);

    public async Task<WordDetailModel?> GetWordDetailsAsync(
        Guid publicId,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode,
        CancellationToken cancellationToken)
    {
        string cacheKey = BuildCacheKey(publicId, primaryMeaningLanguageCode, secondaryMeaningLanguageCode, uiLanguageCode);

        if (memoryCache.TryGetValue(cacheKey, out WordDetailModel? cachedModel))
        {
            return cachedModel;
        }

        SemaphoreSlim gate = Gates.GetOrAdd(cacheKey, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (memoryCache.TryGetValue(cacheKey, out cachedModel))
            {
                return cachedModel;
            }

            WordDetailModel? loadedModel = await innerService
                .GetWordDetailsAsync(
                    publicId,
                    primaryMeaningLanguageCode,
                    secondaryMeaningLanguageCode,
                    uiLanguageCode,
                    cancellationToken)
                .ConfigureAwait(false);

            memoryCache.Set(
                cacheKey,
                loadedModel,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheLifetime,
                    Size = 1
                });

            return loadedModel;
        }
        finally
        {
            gate.Release();
        }
    }

    private static string BuildCacheKey(
        Guid publicId,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode) =>
        string.Join(
            '|',
            publicId.ToString("D"),
            primaryMeaningLanguageCode.Trim().ToLowerInvariant(),
            (secondaryMeaningLanguageCode ?? string.Empty).Trim().ToLowerInvariant(),
            uiLanguageCode.Trim().ToLowerInvariant());
}
