using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Domain.Tests;

/// <summary>
/// Tests the <see cref="Topic"/> aggregate behavior.
/// </summary>
public sealed class TopicTests
{
    /// <summary>
    /// Verifies that duplicate localizations for the same language are merged rather than duplicated.
    /// </summary>
    [Fact]
    public void AddOrUpdateLocalization_ShouldUpdateExistingLanguageRow()
    {
        Topic topic = new(Guid.NewGuid(), "shopping", 10, true, DateTime.UtcNow);

        topic.AddOrUpdateLocalization(Guid.NewGuid(), LanguageCode.From("en"), "Shopping", DateTime.UtcNow);
        topic.AddOrUpdateLocalization(Guid.Empty, LanguageCode.From("en"), "Everyday Shopping", DateTime.UtcNow);

        TopicLocalization localization = Assert.Single(topic.Localizations);
        Assert.Equal("Everyday Shopping", localization.DisplayName);
    }

    /// <summary>
    /// Verifies that a conflicting localization identifier is rejected for an existing language row.
    /// </summary>
    [Fact]
    public void AddOrUpdateLocalization_ShouldRejectMismatchedIdentifierForExistingLanguage()
    {
        Topic topic = new(Guid.NewGuid(), "shopping", 10, true, DateTime.UtcNow);
        Guid localizationId = Guid.NewGuid();

        topic.AddOrUpdateLocalization(localizationId, LanguageCode.From("en"), "Shopping", DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() => topic.AddOrUpdateLocalization(
            Guid.NewGuid(),
            LanguageCode.From("en"),
            "Everyday Shopping",
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that invalid topic keys are rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectInvalidKey()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Topic(Guid.NewGuid(), "Shopping List", 0, true, DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that an empty identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyIdentifier()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Topic(Guid.Empty, "shopping", 0, true, DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a negative sort order is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectNegativeSortOrder()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Topic(Guid.NewGuid(), "shopping", -1, true, DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that the sort order can be updated and that the timestamp is refreshed.
    /// </summary>
    [Fact]
    public void UpdateSortOrder_ShouldPersistNewOrderAndTimestamp()
    {
        Topic topic = new(Guid.NewGuid(), "shopping", 10, true, DateTime.UtcNow.AddMinutes(-5));
        DateTime updatedAt = DateTime.UtcNow;

        topic.UpdateSortOrder(20, updatedAt);

        Assert.Equal(20, topic.SortOrder);
        Assert.Equal(updatedAt, topic.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that a negative sort-order update is rejected.
    /// </summary>
    [Fact]
    public void UpdateSortOrder_ShouldRejectNegativeValue()
    {
        Topic topic = new(Guid.NewGuid(), "shopping", 10, true, DateTime.UtcNow);

        Assert.Throws<DomainRuleException>(() => topic.UpdateSortOrder(-1, DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that <see cref="Topic.FindLocalization"/> returns null when no localization exists.
    /// </summary>
    [Fact]
    public void FindLocalization_ShouldReturnNullWhenLanguageIsAbsent()
    {
        Topic topic = new(Guid.NewGuid(), "shopping", 0, true, DateTime.UtcNow);

        TopicLocalization? result = topic.FindLocalization(LanguageCode.From("fr"));

        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that <see cref="Topic.FindLocalization"/> returns the correct row for an existing language.
    /// </summary>
    [Fact]
    public void FindLocalization_ShouldReturnExistingLocalization()
    {
        Topic topic = new(Guid.NewGuid(), "shopping", 0, true, DateTime.UtcNow);
        topic.AddOrUpdateLocalization(Guid.NewGuid(), LanguageCode.From("en"), "Shopping", DateTime.UtcNow);

        TopicLocalization? result = topic.FindLocalization(LanguageCode.From("en"));

        Assert.NotNull(result);
        Assert.Equal("Shopping", result.DisplayName);
    }
}
