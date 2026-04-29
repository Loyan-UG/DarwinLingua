using Microsoft.Extensions.Caching.Memory;

namespace DarwinLingua.Web.Services;

public interface IAccountEmailRateLimiter
{
    bool TryConsume(string purpose, string key, int maxAttempts, TimeSpan window);
}

public sealed class AccountEmailRateLimiter(IMemoryCache memoryCache)
    : IAccountEmailRateLimiter
{
    public bool TryConsume(string purpose, string key, int maxAttempts, TimeSpan window)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        string cacheKey = $"account-email-rate:{purpose}:{key.Trim().ToUpperInvariant()}";
        int attempts = memoryCache.Get<int?>(cacheKey) ?? 0;
        if (attempts >= maxAttempts)
        {
            return false;
        }

        memoryCache.Set(cacheKey, attempts + 1, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = window,
            Size = 1,
        });
        return true;
    }
}
