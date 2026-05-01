using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents the translation of a single sense into one target language.
/// </summary>
public sealed class SenseTranslation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SenseTranslation"/> class for EF Core materialization.
    /// </summary>
    private SenseTranslation()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SenseTranslation"/> class.
    /// </summary>
    internal SenseTranslation(
        Guid id,
        Guid wordSenseId,
        LanguageCode languageCode,
        string translationText,
        bool isPrimary,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Sense translation identifier cannot be empty.");
        }

        if (wordSenseId == Guid.Empty)
        {
            throw new DomainRuleException("Word sense identifier cannot be empty for a translation.");
        }

        Id = id;
        WordSenseId = wordSenseId;
        LanguageCode = languageCode;
        TranslationText = NormalizeRequiredText(translationText, nameof(translationText));
        IsPrimary = isPrimary;
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid WordSenseId { get; private set; }

    public LanguageCode LanguageCode { get; private set; }

    public string TranslationText { get; private set; } = string.Empty;

    public bool IsPrimary { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Updates the primary flag of the translation.
    /// </summary>
    internal void SetPrimary(bool isPrimary, DateTime updatedAtUtc)
    {
        IsPrimary = isPrimary;
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    /// <summary>
    /// Updates the translation language and text.
    /// </summary>
    internal void Update(LanguageCode languageCode, string translationText, bool isPrimary, DateTime updatedAtUtc)
    {
        LanguageCode = languageCode;
        TranslationText = NormalizeRequiredText(translationText, nameof(translationText));
        IsPrimary = isPrimary;
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    private static string NormalizeRequiredText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException($"{parameterName} cannot be empty.");
        }

        return value.Trim();
    }

    private static DateTime NormalizeUtc(DateTime value, string parameterName)
    {
        if (value == default)
        {
            throw new DomainRuleException($"{parameterName} cannot be empty.");
        }

        return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }
}
