using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Localization.Domain.Tests;

/// <summary>
/// Tests the <see cref="LanguageCode"/> value object behavior.
/// </summary>
public sealed class LanguageCodeTests
{
    /// <summary>
    /// Verifies that a language code is trimmed and normalized to lowercase.
    /// </summary>
    [Fact]
    public void From_ShouldNormalizeWhitespaceAndCasing()
    {
        LanguageCode code = LanguageCode.From(" EN ");

        Assert.Equal("en", code.Value);
    }

    /// <summary>
    /// Verifies that invalid language code values are rejected.
    /// </summary>
    [Fact]
    public void From_ShouldRejectInvalidValue()
    {
        Assert.Throws<DomainRuleException>(() => LanguageCode.From("english"));
    }

    /// <summary>
    /// Verifies that an empty or whitespace-only value is rejected.
    /// </summary>
    [Fact]
    public void From_ShouldRejectEmptyOrWhitespaceValue()
    {
        Assert.Throws<DomainRuleException>(() => LanguageCode.From("   "));
    }

    /// <summary>
    /// Verifies that a valid BCP 47 subtag (e.g. "zh-hans") is accepted and normalized.
    /// </summary>
    [Fact]
    public void From_ShouldAcceptValidSubtag()
    {
        LanguageCode code = LanguageCode.From("ZH-Hans");

        Assert.Equal("zh-hans", code.Value);
    }
}
