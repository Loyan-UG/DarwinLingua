using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents a single meaning-aware sense under a lexical entry.
/// </summary>
public sealed class WordSense
{
    private readonly List<SenseTranslation> _translations = [];
    private readonly List<ExampleSentence> _examples = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="WordSense"/> class for EF Core materialization.
    /// </summary>
    private WordSense()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WordSense"/> class.
    /// </summary>
    internal WordSense(
        Guid id,
        Guid wordEntryId,
        int senseOrder,
        bool isPrimarySense,
        PublicationStatus publicationStatus,
        DateTime createdAtUtc,
        string? shortDefinitionDe,
        string? shortGloss)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Word sense identifier cannot be empty.");
        }

        if (wordEntryId == Guid.Empty)
        {
            throw new DomainRuleException("Word entry identifier cannot be empty for a sense.");
        }

        if (senseOrder <= 0)
        {
            throw new DomainRuleException("Sense order must be greater than zero.");
        }

        Id = id;
        WordEntryId = wordEntryId;
        SenseOrder = senseOrder;
        IsPrimarySense = isPrimarySense;
        PublicationStatus = publicationStatus;
        ShortDefinitionDe = NormalizeOptionalText(shortDefinitionDe);
        ShortGloss = NormalizeOptionalText(shortGloss);
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid WordEntryId { get; private set; }

    public int SenseOrder { get; private set; }

    public bool IsPrimarySense { get; private set; }

    public string? ShortDefinitionDe { get; private set; }

    public string? ShortGloss { get; private set; }

    public PublicationStatus PublicationStatus { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<SenseTranslation> Translations => _translations.AsReadOnly();

    public IReadOnlyCollection<ExampleSentence> Examples => _examples.AsReadOnly();

    /// <summary>
    /// Adds a translation to the sense.
    /// </summary>
    public SenseTranslation AddTranslation(
        Guid id,
        LanguageCode languageCode,
        string translationText,
        bool isPrimary,
        DateTime createdAtUtc)
    {
        if (_translations.Any(existingTranslation => existingTranslation.Id == id))
        {
            throw new DomainRuleException("Duplicate translation identifiers are not allowed within the same sense.");
        }

        if (_translations.Any(existingTranslation => existingTranslation.LanguageCode == languageCode))
        {
            throw new DomainRuleException("Duplicate translation languages are not allowed within the same sense.");
        }

        if (isPrimary)
        {
            foreach (SenseTranslation existingTranslation in _translations.Where(existingTranslation => existingTranslation.IsPrimary))
            {
                existingTranslation.SetPrimary(false, createdAtUtc);
            }
        }

        SenseTranslation translation = new(
            id,
            Id,
            languageCode,
            translationText,
            isPrimary,
            createdAtUtc);

        _translations.Add(translation);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));

        return translation;
    }

    /// <summary>
    /// Adds a usage example to the sense.
    /// </summary>
    public ExampleSentence AddExample(
        Guid id,
        int sentenceOrder,
        string germanText,
        bool isPrimaryExample,
        DateTime createdAtUtc)
    {
        if (_examples.Any(existingExample => existingExample.Id == id))
        {
            throw new DomainRuleException("Duplicate example identifiers are not allowed within the same sense.");
        }

        if (_examples.Any(existingExample => existingExample.SentenceOrder == sentenceOrder))
        {
            throw new DomainRuleException("Duplicate example sentence orders are not allowed within the same sense.");
        }

        if (isPrimaryExample)
        {
            foreach (ExampleSentence existingExample in _examples.Where(existingExample => existingExample.IsPrimaryExample))
            {
                existingExample.SetPrimaryExample(false, createdAtUtc);
            }
        }

        ExampleSentence example = new(
            id,
            Id,
            sentenceOrder,
            germanText,
            isPrimaryExample,
            createdAtUtc);

        _examples.Add(example);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));

        return example;
    }

    /// <summary>
    /// Updates the primary flag of the sense.
    /// </summary>
    internal void SetPrimarySense(bool isPrimarySense, DateTime updatedAtUtc)
    {
        IsPrimarySense = isPrimarySense;
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
