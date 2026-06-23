using DarwinLingua.Web.Services;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class AccountEmailRateLimiterTests
{
    [Fact]
    public void TryConsume_BlocksSamePurposeAndKeyAfterLimit()
    {
        using MemoryCache cache = CreateCache();
        AccountEmailRateLimiter limiter = new(cache);

        Assert.True(limiter.TryConsume("forgot-password", " learner@example.com ", 2, TimeSpan.FromMinutes(5)));
        Assert.True(limiter.TryConsume("forgot-password", "LEARNER@example.com", 2, TimeSpan.FromMinutes(5)));
        Assert.False(limiter.TryConsume("forgot-password", "learner@example.com", 2, TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void TryConsume_SeparatesPurposeAndRecipientBuckets()
    {
        using MemoryCache cache = CreateCache();
        AccountEmailRateLimiter limiter = new(cache);

        Assert.True(limiter.TryConsume("forgot-password", "learner@example.com", 1, TimeSpan.FromMinutes(5)));
        Assert.False(limiter.TryConsume("forgot-password", "learner@example.com", 1, TimeSpan.FromMinutes(5)));
        Assert.True(limiter.TryConsume("resend-confirmation", "learner@example.com", 1, TimeSpan.FromMinutes(5)));
        Assert.True(limiter.TryConsume("forgot-password", "other@example.com", 1, TimeSpan.FromMinutes(5)));
    }

    private static MemoryCache CreateCache() => new(new MemoryCacheOptions
    {
        SizeLimit = 32,
    });
}
