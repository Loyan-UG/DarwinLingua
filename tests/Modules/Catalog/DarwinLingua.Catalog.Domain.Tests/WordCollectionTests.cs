using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Tests;

/// <summary>
/// Verifies the <see cref="WordCollection"/> aggregate behavior.
/// </summary>
public sealed class WordCollectionTests
{
    /// <summary>
    /// Verifies that a valid word collection is created with expected property values.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateCollectionWithNormalizedProperties()
    {
        Guid id = Guid.NewGuid();
        DateTime createdAt = DateTime.UtcNow;

        WordCollection collection = new(
            id,
            "beginners-pack",
            "  Beginners Pack  ",
            "A starter pack for learners.",
            null,
            PublicationStatus.Active,
            5,
            createdAt);

        Assert.Equal(id, collection.Id);
        Assert.Equal("beginners-pack", collection.Slug);
        Assert.Equal("Beginners Pack", collection.Name);
        Assert.Equal("A starter pack for learners.", collection.Description);
        Assert.Null(collection.ImageUrl);
        Assert.Equal(PublicationStatus.Active, collection.PublicationStatus);
        Assert.Equal(5, collection.SortOrder);
        Assert.Empty(collection.Entries);
    }

    /// <summary>
    /// Verifies that an empty identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyIdentifier()
    {
        Assert.Throws<DomainRuleException>(() => new WordCollection(
            Guid.Empty,
            "my-pack",
            "My Pack",
            null,
            null,
            PublicationStatus.Active,
            0,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that an empty slug is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptySlug()
    {
        Assert.Throws<DomainRuleException>(() => new WordCollection(
            Guid.NewGuid(),
            "   ",
            "My Pack",
            null,
            null,
            PublicationStatus.Active,
            0,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a slug containing spaces or uppercase characters is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectSlugWithInvalidCharacters()
    {
        Assert.Throws<DomainRuleException>(() => new WordCollection(
            Guid.NewGuid(),
            "My Pack",
            "My Pack",
            null,
            null,
            PublicationStatus.Active,
            0,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a negative sort order is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectNegativeSortOrder()
    {
        Assert.Throws<DomainRuleException>(() => new WordCollection(
            Guid.NewGuid(),
            "my-pack",
            "My Pack",
            null,
            null,
            PublicationStatus.Active,
            -1,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that an empty name is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyName()
    {
        Assert.Throws<DomainRuleException>(() => new WordCollection(
            Guid.NewGuid(),
            "my-pack",
            "   ",
            null,
            null,
            PublicationStatus.Active,
            0,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a word is added correctly to the collection.
    /// </summary>
    [Fact]
    public void AddWord_ShouldAddEntryToCollection()
    {
        WordCollection collection = CreateWordCollection();
        Guid wordEntryId = Guid.NewGuid();

        collection.AddWord(Guid.NewGuid(), wordEntryId, 1, DateTime.UtcNow);

        WordCollectionEntry entry = Assert.Single(collection.Entries);
        Assert.Equal(wordEntryId, entry.WordEntryId);
        Assert.Equal(1, entry.SortOrder);
    }

    /// <summary>
    /// Verifies that duplicate word entries are rejected.
    /// </summary>
    [Fact]
    public void AddWord_ShouldRejectDuplicateWordEntry()
    {
        WordCollection collection = CreateWordCollection();
        Guid wordEntryId = Guid.NewGuid();
        collection.AddWord(Guid.NewGuid(), wordEntryId, 1, DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() =>
            collection.AddWord(Guid.NewGuid(), wordEntryId, 2, DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that duplicate sort orders are rejected.
    /// </summary>
    [Fact]
    public void AddWord_ShouldRejectDuplicateSortOrder()
    {
        WordCollection collection = CreateWordCollection();
        collection.AddWord(Guid.NewGuid(), Guid.NewGuid(), 1, DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() =>
            collection.AddWord(Guid.NewGuid(), Guid.NewGuid(), 1, DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that an empty entry identifier is rejected.
    /// </summary>
    [Fact]
    public void AddWord_ShouldRejectEmptyEntryIdentifier()
    {
        WordCollection collection = CreateWordCollection();

        Assert.Throws<DomainRuleException>(() =>
            collection.AddWord(Guid.Empty, Guid.NewGuid(), 1, DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that an empty word entry identifier is rejected.
    /// </summary>
    [Fact]
    public void AddWord_ShouldRejectEmptyWordEntryIdentifier()
    {
        WordCollection collection = CreateWordCollection();

        Assert.Throws<DomainRuleException>(() =>
            collection.AddWord(Guid.NewGuid(), Guid.Empty, 1, DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that replacing entries clears old entries and adds the new set in order.
    /// </summary>
    [Fact]
    public void ReplaceEntries_ShouldClearAndReplaceAllEntries()
    {
        WordCollection collection = CreateWordCollection();
        collection.AddWord(Guid.NewGuid(), Guid.NewGuid(), 1, DateTime.UtcNow);
        collection.AddWord(Guid.NewGuid(), Guid.NewGuid(), 2, DateTime.UtcNow);

        Guid firstWordId = Guid.NewGuid();
        Guid secondWordId = Guid.NewGuid();
        DateTime updatedAt = DateTime.UtcNow.AddMinutes(1);

        collection.ReplaceEntries(
            [(firstWordId, 10), (secondWordId, 20)],
            updatedAt);

        Assert.Equal(2, collection.Entries.Count);
        Assert.Contains(collection.Entries, entry => entry.WordEntryId == firstWordId);
        Assert.Contains(collection.Entries, entry => entry.WordEntryId == secondWordId);
        Assert.Equal(updatedAt, collection.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that updating metadata persists all changes.
    /// </summary>
    [Fact]
    public void UpdateMetadata_ShouldUpdateAllEditableProperties()
    {
        WordCollection collection = CreateWordCollection();
        DateTime updatedAt = DateTime.UtcNow.AddMinutes(5);

        collection.UpdateMetadata(
            "Advanced Pack",
            "For advanced learners.",
            "https://example.com/image.jpg",
            PublicationStatus.Draft,
            99,
            updatedAt);

        Assert.Equal("Advanced Pack", collection.Name);
        Assert.Equal("For advanced learners.", collection.Description);
        Assert.Equal("https://example.com/image.jpg", collection.ImageUrl);
        Assert.Equal(PublicationStatus.Draft, collection.PublicationStatus);
        Assert.Equal(99, collection.SortOrder);
        Assert.Equal(updatedAt, collection.UpdatedAtUtc);
    }

    private static WordCollection CreateWordCollection()
    {
        return new WordCollection(
            Guid.NewGuid(),
            "starters-pack",
            "Starters Pack",
            null,
            null,
            PublicationStatus.Active,
            0,
            DateTime.UtcNow);
    }
}
