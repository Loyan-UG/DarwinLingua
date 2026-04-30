using Microsoft.Extensions.Caching.Memory;

namespace DarwinLingua.Web.Services;

public interface IBillingOperationRateLimiter
{
    bool TryConsume(string operationKey, string actorKey, int limit, TimeSpan window);
}

public sealed class BillingOperationRateLimiter(IMemoryCache memoryCache) : IBillingOperationRateLimiter
{
    public bool TryConsume(string operationKey, string actorKey, int limit, TimeSpan window)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorKey);

        int boundedLimit = Math.Clamp(limit, 1, 100);
        TimeSpan boundedWindow = window <= TimeSpan.Zero ? TimeSpan.FromMinutes(1) : window;
        string cacheKey = $"billing-rate-limit:{operationKey}:{actorKey.Trim().ToUpperInvariant()}";
        BillingRateLimitBucket bucket = memoryCache.GetOrCreate(
            cacheKey,
            entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = boundedWindow;
                entry.SetSize(1);
                return new BillingRateLimitBucket();
            }) ?? new BillingRateLimitBucket();

        lock (bucket)
        {
            if (bucket.Count >= boundedLimit)
            {
                return false;
            }

            bucket.Count++;
            return true;
        }
    }

    private sealed class BillingRateLimitBucket
    {
        public int Count { get; set; }
    }
}
