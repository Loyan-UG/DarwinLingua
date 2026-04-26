using DarwinLingua.Learning.Domain.Entities;
using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Learning.Domain.Tests;

/// <summary>
/// Verifies the <see cref="UserFavoriteWord"/> entity invariants.
/// </summary>
public sealed class UserFavoriteWordTests
{
    /// <summary>
    /// Verifies that a valid favorite word record is created with the expected property values.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateFavoriteWordWithExpectedValues()
    {
        Guid id = Guid.NewGuid();
        Guid wordPublicId = Guid.NewGuid();
        DateTime createdAt = DateTime.UtcNow;

        UserFavoriteWord favoriteWord = new(id, "local-installation-user", wordPublicId, createdAt);

        Assert.Equal(id, favoriteWord.Id);
        Assert.Equal("local-installation-user", favoriteWord.UserId);
        Assert.Equal(wordPublicId, favoriteWord.WordEntryPublicId);
        Assert.Equal(createdAt, favoriteWord.CreatedAtUtc);
    }

    /// <summary>
    /// Verifies that an empty identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyIdentifier()
    {
        Assert.Throws<DomainRuleException>(() => new UserFavoriteWord(
            Guid.Empty,
            "local-installation-user",
            Guid.NewGuid(),
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that an empty lexical-entry identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyWordEntryPublicId()
    {
        Assert.Throws<DomainRuleException>(() => new UserFavoriteWord(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.Empty,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a blank user identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyUserId()
    {
        Assert.Throws<DomainRuleException>(() => new UserFavoriteWord(
            Guid.NewGuid(),
            "   ",
            Guid.NewGuid(),
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that the user identifier is trimmed on construction.
    /// </summary>
    [Fact]
    public void Constructor_ShouldTrimUserId()
    {
        UserFavoriteWord favoriteWord = new(
            Guid.NewGuid(),
            "  local-installation-user  ",
            Guid.NewGuid(),
            DateTime.UtcNow);

        Assert.Equal("local-installation-user", favoriteWord.UserId);
    }

    /// <summary>
    /// Verifies that a default (uninitialized) creation timestamp is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectDefaultCreatedAtUtc()
    {
        Assert.Throws<DomainRuleException>(() => new UserFavoriteWord(
            Guid.NewGuid(),
            "local-installation-user",
            Guid.NewGuid(),
            default));
    }
}
