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

    /// <summary>
    /// Verifies that two <see cref="LanguageCode"/> instances with the same value are equal.
    /// </summary>
    [Fact]
    public void From_ShouldSupportValueEquality()
    {
        LanguageCode first = LanguageCode.From("en");
        LanguageCode second = LanguageCode.From("EN");

        Assert.Equal(first, second);
    }

    /// <summary>
    /// Verifies that <see cref="LanguageCode.ToString"/> returns the normalized value.
    /// </summary>
    [Fact]
    public void ToString_ShouldReturnNormalizedValue()
    {
        LanguageCode code = LanguageCode.From("DE");

        Assert.Equal("de", code.ToString());
    }

    /// <summary>
    /// Verifies that a minimal two-letter code is accepted.
    /// </summary>
    [Fact]
    public void From_ShouldAcceptTwoLetterCode()
    {
        LanguageCode code = LanguageCode.From("de");

        Assert.Equal("de", code.Value);
    }

    /// <summary>
    /// Verifies that a single-letter string is rejected.
    /// </summary>
    [Fact]
    public void From_ShouldRejectSingleLetterValue()
    {
        Assert.Throws<DomainRuleException>(() => LanguageCode.From("e"));
    }

    /// <summary>
    /// Verifies that a three-letter ISO 639-2 code is accepted and normalized.
    /// </summary>
    [Fact]
    public void From_ShouldAcceptThreeLetterCode()
    {
        LanguageCode code = LanguageCode.From("deu");

        Assert.Equal("deu", code.Value);
    }

    /// <summary>
    /// Verifies that a subtag containing numeric characters is accepted and normalized.
    /// </summary>
    [Fact]
    public void From_ShouldAcceptNumericSubtag()
    {
        LanguageCode code = LanguageCode.From("es-419");

        Assert.Equal("es-419", code.Value);
    }
}
