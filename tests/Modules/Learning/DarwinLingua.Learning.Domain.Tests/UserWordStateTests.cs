using DarwinLingua.Learning.Domain.Entities;
using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Learning.Domain.Tests;

/// <summary>
/// Verifies domain invariants and state transitions for <see cref="UserWordState"/>.
/// </summary>
public sealed class UserWordStateTests
{
    /// <summary>
    /// Verifies that constructor validation rejects empty lexical-entry identifiers.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyWordEntryPublicId()
    {
        Assert.Throws<DomainRuleException>(() => new UserWordState(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.Empty,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that an empty internal identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyIdentifier()
    {
        Assert.Throws<DomainRuleException>(() => new UserWordState(
            Guid.Empty,
            "local-installation-user",
            Guid.NewGuid(),
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a blank user identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyUserId()
    {
        Assert.Throws<DomainRuleException>(() => new UserWordState(
            Guid.NewGuid(),
            "   ",
            Guid.NewGuid(),
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that the first view sets <see cref="UserWordState.FirstViewedAtUtc"/> and that subsequent
    /// views do not change it while still updating <see cref="UserWordState.LastViewedAtUtc"/>.
    /// </summary>
    [Fact]
    public void TrackViewed_FirstViewSetsFirstViewedAndSubsequentViewsDoNotChangeIt()
    {
        UserWordState userWordState = new(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(-10));

        DateTime firstView = DateTime.UtcNow.AddMinutes(-3);
        DateTime secondView = DateTime.UtcNow;

        userWordState.TrackViewed(firstView);
        userWordState.TrackViewed(secondView);

        Assert.Equal(firstView, userWordState.FirstViewedAtUtc);
        Assert.Equal(secondView, userWordState.LastViewedAtUtc);
    }

    /// <summary>
    /// Verifies that tracking views sets first/last timestamps and increments counters.
    /// </summary>
    [Fact]
    public void TrackViewed_ShouldSetViewTimestampsAndIncrementCount()
    {
        UserWordState userWordState = new(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(-10));

        DateTime firstView = DateTime.UtcNow.AddMinutes(-2);
        DateTime secondView = DateTime.UtcNow;

        userWordState.TrackViewed(firstView);
        userWordState.TrackViewed(secondView);

        Assert.Equal(firstView, userWordState.FirstViewedAtUtc);
        Assert.Equal(secondView, userWordState.LastViewedAtUtc);
        Assert.Equal(2, userWordState.ViewCount);
        Assert.Equal(secondView, userWordState.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that known and difficult markers can be toggled independently.
    /// </summary>
    [Fact]
    public void KnownAndDifficultMarkers_ShouldSupportSetAndClearTransitions()
    {
        UserWordState userWordState = new(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(-5));

        DateTime markKnownAt = DateTime.UtcNow.AddMinutes(-4);
        DateTime markDifficultAt = DateTime.UtcNow.AddMinutes(-3);
        DateTime clearKnownAt = DateTime.UtcNow.AddMinutes(-2);
        DateTime clearDifficultAt = DateTime.UtcNow.AddMinutes(-1);

        userWordState.MarkKnown(markKnownAt);
        userWordState.MarkDifficult(markDifficultAt);
        userWordState.ClearKnown(clearKnownAt);
        userWordState.ClearDifficult(clearDifficultAt);

        Assert.False(userWordState.IsKnown);
        Assert.False(userWordState.IsDifficult);
        Assert.Equal(clearDifficultAt, userWordState.UpdatedAtUtc);
    }
}
