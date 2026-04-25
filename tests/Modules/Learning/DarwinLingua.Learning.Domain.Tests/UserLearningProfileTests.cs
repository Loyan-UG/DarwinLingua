using DarwinLingua.Learning.Domain.Entities;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Learning.Domain.Tests;

/// <summary>
/// Verifies the <see cref="UserLearningProfile"/> aggregate rules.
/// </summary>
public sealed class UserLearningProfileTests
{
    /// <summary>
    /// Verifies that the aggregate rejects duplicate primary and secondary meaning languages.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectDuplicateMeaningLanguages()
    {
        Assert.Throws<DomainRuleException>(() => new UserLearningProfile(
            Guid.NewGuid(),
            "local-installation-user",
            LanguageCode.From("en"),
            LanguageCode.From("en"),
            LanguageCode.From("de"),
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that an empty internal identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyIdentifier()
    {
        Assert.Throws<DomainRuleException>(() => new UserLearningProfile(
            Guid.Empty,
            "local-installation-user",
            LanguageCode.From("en"),
            preferredMeaningLanguage2: null,
            LanguageCode.From("en"),
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that a blank user identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyUserId()
    {
        Assert.Throws<DomainRuleException>(() => new UserLearningProfile(
            Guid.NewGuid(),
            "   ",
            LanguageCode.From("en"),
            preferredMeaningLanguage2: null,
            LanguageCode.From("en"),
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that UI language updates refresh the last-updated timestamp.
    /// </summary>
    [Fact]
    public void UpdateUiLanguage_ShouldUpdateStoredLanguageAndTimestamp()
    {
        UserLearningProfile profile = new(
            Guid.NewGuid(),
            "local-installation-user",
            LanguageCode.From("en"),
            preferredMeaningLanguage2: null,
            LanguageCode.From("en"),
            DateTime.UtcNow.AddMinutes(-5));

        DateTime updatedAtUtc = DateTime.UtcNow;

        profile.UpdateUiLanguage(LanguageCode.From("de"), updatedAtUtc);

        Assert.Equal(LanguageCode.From("de"), profile.UiLanguageCode);
        Assert.Equal(updatedAtUtc, profile.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that meaning-language preferences can be updated and that duplicate languages are rejected.
    /// </summary>
    [Fact]
    public void UpdateMeaningLanguagePreferences_ShouldRejectDuplicateLanguages()
    {
        UserLearningProfile profile = new(
            Guid.NewGuid(),
            "local-installation-user",
            LanguageCode.From("en"),
            preferredMeaningLanguage2: null,
            LanguageCode.From("en"),
            DateTime.UtcNow.AddMinutes(-5));

        Assert.Throws<DomainRuleException>(() => profile.UpdateMeaningLanguagePreferences(
            LanguageCode.From("tr"),
            LanguageCode.From("tr"),
            DateTime.UtcNow));
    }

    /// <summary>
    /// Verifies that valid meaning-language preference updates are persisted and the timestamp is refreshed.
    /// </summary>
    [Fact]
    public void UpdateMeaningLanguagePreferences_ShouldPersistNewLanguagesAndTimestamp()
    {
        UserLearningProfile profile = new(
            Guid.NewGuid(),
            "local-installation-user",
            LanguageCode.From("en"),
            preferredMeaningLanguage2: null,
            LanguageCode.From("en"),
            DateTime.UtcNow.AddMinutes(-5));

        DateTime updatedAt = DateTime.UtcNow;

        profile.UpdateMeaningLanguagePreferences(
            LanguageCode.From("de"),
            LanguageCode.From("tr"),
            updatedAt);

        Assert.Equal(LanguageCode.From("de"), profile.PreferredMeaningLanguage1);
        Assert.Equal(LanguageCode.From("tr"), profile.PreferredMeaningLanguage2);
        Assert.Equal(updatedAt, profile.UpdatedAtUtc);
    }
}
