using DarwinLingua.Localization.Domain.Entities;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Localization.Domain.Tests;

/// <summary>
/// Tests the <see cref="Language"/> entity behavior.
/// </summary>
public sealed class LanguageTests
{
    /// <summary>
    /// Verifies that a language requires at least one active capability.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectLanguageWithoutCapabilities()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Language(
                Guid.NewGuid(),
                LanguageCode.From("en"),
                "English",
                "English",
                isActive: true,
                supportsUserInterface: false,
                supportsMeanings: false));
    }

    /// <summary>
    /// Verifies that capability updates still require at least one active capability.
    /// </summary>
    [Fact]
    public void UpdateCapabilities_ShouldRejectWhenAllCapabilitiesAreDisabled()
    {
        Language language = new(
            Guid.NewGuid(),
            LanguageCode.From("de"),
            "German",
            "Deutsch",
            isActive: true,
            supportsUserInterface: true,
            supportsMeanings: true);

        Assert.Throws<DomainRuleException>(() => language.UpdateCapabilities(false, false));
    }

    /// <summary>
    /// Verifies that display names are normalized when updated.
    /// </summary>
    [Fact]
    public void UpdateNames_ShouldTrimIncomingValues()
    {
        Language language = new(
            Guid.NewGuid(),
            LanguageCode.From("de"),
            "German",
            "Deutsch",
            isActive: true,
            supportsUserInterface: true,
            supportsMeanings: false);

        language.UpdateNames(" German ", " Deutsch ");

        Assert.Equal("German", language.EnglishName);
        Assert.Equal("Deutsch", language.NativeName);
    }

    /// <summary>
    /// Verifies that an empty identifier is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyIdentifier()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Language(
                Guid.Empty,
                LanguageCode.From("en"),
                "English",
                "English",
                isActive: true,
                supportsUserInterface: true,
                supportsMeanings: false));
    }

    /// <summary>
    /// Verifies that an empty English name is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyEnglishName()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Language(
                Guid.NewGuid(),
                LanguageCode.From("en"),
                "   ",
                "English",
                isActive: true,
                supportsUserInterface: true,
                supportsMeanings: false));
    }

    /// <summary>
    /// Verifies that <see cref="Language.UpdateCapabilities"/> accepts changes when at least one capability remains.
    /// </summary>
    [Fact]
    public void UpdateCapabilities_ShouldSucceedWhenAtLeastOneCapabilityIsEnabled()
    {
        Language language = new(
            Guid.NewGuid(),
            LanguageCode.From("en"),
            "English",
            "English",
            isActive: true,
            supportsUserInterface: true,
            supportsMeanings: true);

        language.UpdateCapabilities(supportsUserInterface: false, supportsMeanings: true);

        Assert.False(language.SupportsUserInterface);
        Assert.True(language.SupportsMeanings);
    }

    /// <summary>
    /// Verifies that <see cref="Language.Activate"/> and <see cref="Language.Deactivate"/> toggle the active state.
    /// </summary>
    [Fact]
    public void ActivateAndDeactivate_ShouldToggleIsActive()
    {
        Language language = new(
            Guid.NewGuid(),
            LanguageCode.From("de"),
            "German",
            "Deutsch",
            isActive: false,
            supportsUserInterface: true,
            supportsMeanings: false);

        language.Activate();
        Assert.True(language.IsActive);

        language.Deactivate();
        Assert.False(language.IsActive);
    }

    /// <summary>
    /// Verifies that an empty native name is rejected.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectEmptyNativeName()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Language(
                Guid.NewGuid(),
                LanguageCode.From("de"),
                "German",
                "   ",
                isActive: true,
                supportsUserInterface: true,
                supportsMeanings: false));
    }

    /// <summary>
    /// Verifies that <see cref="Language.UpdateNames"/> rejects an empty English name.
    /// </summary>
    [Fact]
    public void UpdateNames_ShouldRejectEmptyEnglishName()
    {
        Language language = new(
            Guid.NewGuid(),
            LanguageCode.From("de"),
            "German",
            "Deutsch",
            isActive: true,
            supportsUserInterface: true,
            supportsMeanings: false);

        Assert.Throws<DomainRuleException>(() => language.UpdateNames("   ", "Deutsch"));
    }

    /// <summary>
    /// Verifies that <see cref="Language.UpdateNames"/> rejects an empty native name.
    /// </summary>
    [Fact]
    public void UpdateNames_ShouldRejectEmptyNativeName()
    {
        Language language = new(
            Guid.NewGuid(),
            LanguageCode.From("de"),
            "German",
            "Deutsch",
            isActive: true,
            supportsUserInterface: true,
            supportsMeanings: false);

        Assert.Throws<DomainRuleException>(() => language.UpdateNames("German", "   "));
    }

    /// <summary>
    /// Verifies that a valid language is created with the expected property values.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateLanguageWithExpectedProperties()
    {
        Guid id = Guid.NewGuid();
        LanguageCode code = LanguageCode.From("de");

        Language language = new(
            id,
            code,
            "German",
            "Deutsch",
            isActive: true,
            supportsUserInterface: true,
            supportsMeanings: true);

        Assert.Equal(id, language.Id);
        Assert.Equal(code, language.Code);
        Assert.Equal("German", language.EnglishName);
        Assert.Equal("Deutsch", language.NativeName);
        Assert.True(language.IsActive);
        Assert.True(language.SupportsUserInterface);
        Assert.True(language.SupportsMeanings);
    }
}
