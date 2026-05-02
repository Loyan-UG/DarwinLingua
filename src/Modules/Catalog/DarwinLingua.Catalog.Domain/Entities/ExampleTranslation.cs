using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents the translation of an example sentence into one target language.
/// </summary>
public sealed class ExampleTranslation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExampleTranslation"/> class for EF Core materialization.
    /// </summary>
    private ExampleTranslation()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExampleTranslation"/> class.
    /// </summary>
    internal ExampleTranslation(
        Guid id,
        Guid exampleSentenceId,
        LanguageCode languageCode,
        string translationText,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Example translation identifier cannot be empty.");
        }

        if (exampleSentenceId == Guid.Empty)
        {
            throw new DomainRuleException("Example sentence identifier cannot be empty for a translation.");
        }

        Id = id;
        ExampleSentenceId = exampleSentenceId;
        LanguageCode = languageCode;
        TranslationText = NormalizeRequiredText(translationText, nameof(translationText));
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ExampleSentenceId { get; private set; }

    public LanguageCode LanguageCode { get; private set; }

    public string TranslationText { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Updates the translation language and text.
    /// </summary>
    internal void Update(LanguageCode languageCode, string translationText, DateTime updatedAtUtc)
    {
        LanguageCode = languageCode;
        TranslationText = NormalizeRequiredText(translationText, nameof(translationText));
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
