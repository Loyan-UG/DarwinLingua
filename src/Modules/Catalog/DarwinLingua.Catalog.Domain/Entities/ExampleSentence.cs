using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents a German example sentence attached to a specific sense.
/// </summary>
public sealed class ExampleSentence
{
    private readonly List<ExampleTranslation> _translations = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ExampleSentence"/> class for EF Core materialization.
    /// </summary>
    private ExampleSentence()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExampleSentence"/> class.
    /// </summary>
    internal ExampleSentence(
        Guid id,
        Guid wordSenseId,
        int sentenceOrder,
        string germanText,
        bool isPrimaryExample,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Example sentence identifier cannot be empty.");
        }

        if (wordSenseId == Guid.Empty)
        {
            throw new DomainRuleException("Word sense identifier cannot be empty for an example sentence.");
        }

        if (sentenceOrder <= 0)
        {
            throw new DomainRuleException("Example sentence order must be greater than zero.");
        }

        Id = id;
        WordSenseId = wordSenseId;
        SentenceOrder = sentenceOrder;
        GermanText = NormalizeRequiredText(germanText, nameof(germanText));
        IsPrimaryExample = isPrimaryExample;
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid WordSenseId { get; private set; }

    public int SentenceOrder { get; private set; }

    public string GermanText { get; private set; } = string.Empty;

    public bool IsPrimaryExample { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<ExampleTranslation> Translations => _translations.AsReadOnly();

    /// <summary>
    /// Adds a translation to the example sentence.
    /// </summary>
    public ExampleTranslation AddTranslation(
        Guid id,
        LanguageCode languageCode,
        string translationText,
        DateTime createdAtUtc)
    {
        if (_translations.Any(existingTranslation => existingTranslation.Id == id))
        {
            throw new DomainRuleException("Duplicate example translation identifiers are not allowed.");
        }

        if (_translations.Any(existingTranslation => existingTranslation.LanguageCode == languageCode))
        {
            throw new DomainRuleException("Duplicate example translation languages are not allowed.");
        }

        ExampleTranslation translation = new(
            id,
            Id,
            languageCode,
            translationText,
            createdAtUtc);

        _translations.Add(translation);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));

        return translation;
    }

    /// <summary>
    /// Updates a translation attached to the example sentence.
    /// </summary>
    public bool UpdateTranslation(
        Guid translationId,
        LanguageCode languageCode,
        string translationText,
        DateTime updatedAtUtc)
    {
        ExampleTranslation? translation = _translations.SingleOrDefault(item => item.Id == translationId);
        if (translation is null)
        {
            return false;
        }

        if (_translations.Any(existingTranslation =>
                existingTranslation.Id != translationId &&
                existingTranslation.LanguageCode == languageCode))
        {
            throw new DomainRuleException("Duplicate example translation languages are not allowed.");
        }

        translation.Update(languageCode, translationText, updatedAtUtc);
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
        return true;
    }

    /// <summary>
    /// Removes a translation from the example sentence.
    /// </summary>
    public ExampleTranslation? RemoveTranslation(Guid translationId, DateTime updatedAtUtc)
    {
        ExampleTranslation? translation = _translations.SingleOrDefault(item => item.Id == translationId);
        if (translation is null)
        {
            return null;
        }

        _translations.Remove(translation);
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
        return translation;
    }

    /// <summary>
    /// Updates the primary-example flag.
    /// </summary>
    internal void SetPrimaryExample(bool isPrimaryExample, DateTime updatedAtUtc)
    {
        IsPrimaryExample = isPrimaryExample;
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    /// <summary>
    /// Updates the example sentence text and primary flag.
    /// </summary>
    internal void Update(string germanText, bool isPrimaryExample, DateTime updatedAtUtc)
    {
        GermanText = NormalizeRequiredText(germanText, nameof(germanText));
        IsPrimaryExample = isPrimaryExample;
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
