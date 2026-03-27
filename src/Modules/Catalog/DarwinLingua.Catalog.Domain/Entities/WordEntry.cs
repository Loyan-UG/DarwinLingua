using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents the root lexical entry used for Phase 1 vocabulary content.
/// </summary>
public sealed partial class WordEntry
{
    private readonly List<WordSense> _senses = [];
    private readonly List<WordTopic> _topics = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="WordEntry"/> class for EF Core materialization.
    /// </summary>
    private WordEntry()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WordEntry"/> class.
    /// </summary>
    public WordEntry(
        Guid id,
        Guid publicId,
        string lemma,
        LanguageCode languageCode,
        CefrLevel primaryCefrLevel,
        PartOfSpeech partOfSpeech,
        PublicationStatus publicationStatus,
        ContentSourceType contentSourceType,
        DateTime createdAtUtc,
        string? article = null,
        string? pluralForm = null,
        string? infinitiveForm = null,
        string? pronunciationIpa = null,
        string? syllableBreak = null,
        string? sourceReference = null)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Word entry identifier cannot be empty.");
        }

        if (publicId == Guid.Empty)
        {
            throw new DomainRuleException("Word entry public identifier cannot be empty.");
        }

        Id = id;
        PublicId = publicId;
        Lemma = NormalizeRequiredText(lemma, nameof(lemma));
        NormalizedLemma = NormalizeLemma(Lemma);
        LanguageCode = languageCode;
        PrimaryCefrLevel = primaryCefrLevel;
        PartOfSpeech = partOfSpeech;
        PublicationStatus = publicationStatus;
        ContentSourceType = contentSourceType;
        Article = NormalizeOptionalText(article);
        PluralForm = NormalizeOptionalText(pluralForm);
        InfinitiveForm = NormalizeOptionalText(infinitiveForm);
        PronunciationIpa = NormalizeOptionalText(pronunciationIpa);
        SyllableBreak = NormalizeOptionalText(syllableBreak);
        SourceReference = NormalizeOptionalText(sourceReference);
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    /// <summary>
    /// Gets the internal identifier of the lexical entry.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the public identifier of the lexical entry.
    /// </summary>
    public Guid PublicId { get; private set; }

    /// <summary>
    /// Gets the visible lemma.
    /// </summary>
    public string Lemma { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the normalized lemma used for uniqueness and search preparation.
    /// </summary>
    public string NormalizedLemma { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the source language code of the lexical entry.
    /// </summary>
    public LanguageCode LanguageCode { get; private set; }

    /// <summary>
    /// Gets the primary CEFR level of the lexical entry.
    /// </summary>
    public CefrLevel PrimaryCefrLevel { get; private set; }

    /// <summary>
    /// Gets the part of speech of the lexical entry.
    /// </summary>
    public PartOfSpeech PartOfSpeech { get; private set; }

    /// <summary>
    /// Gets the optional article value for nouns.
    /// </summary>
    public string? Article { get; private set; }

    /// <summary>
    /// Gets the optional plural form.
    /// </summary>
    public string? PluralForm { get; private set; }

    /// <summary>
    /// Gets the optional infinitive form.
    /// </summary>
    public string? InfinitiveForm { get; private set; }

    /// <summary>
    /// Gets the optional IPA pronunciation.
    /// </summary>
    public string? PronunciationIpa { get; private set; }

    /// <summary>
    /// Gets the optional syllable break hint.
    /// </summary>
    public string? SyllableBreak { get; private set; }

    /// <summary>
    /// Gets the publication lifecycle state.
    /// </summary>
    public PublicationStatus PublicationStatus { get; private set; }

    /// <summary>
    /// Gets the source classification of the content row.
    /// </summary>
    public ContentSourceType ContentSourceType { get; private set; }

    /// <summary>
    /// Gets the optional source reference metadata.
    /// </summary>
    public string? SourceReference { get; private set; }

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC last update timestamp.
    /// </summary>
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the senses that belong to the lexical entry.
    /// </summary>
    public IReadOnlyCollection<WordSense> Senses => _senses.AsReadOnly();

    /// <summary>
    /// Gets the topic links that belong to the lexical entry.
    /// </summary>
    public IReadOnlyCollection<WordTopic> Topics => _topics.AsReadOnly();

    /// <summary>
    /// Adds a sense to the lexical entry.
    /// </summary>
    public WordSense AddSense(
        Guid id,
        int senseOrder,
        bool isPrimarySense,
        PublicationStatus publicationStatus,
        DateTime createdAtUtc,
        string? shortDefinitionDe = null,
        string? shortGloss = null)
    {
        if (_senses.Any(existingSense => existingSense.Id == id))
        {
            throw new DomainRuleException("Duplicate sense identifiers are not allowed within the same word entry.");
        }

        if (_senses.Any(existingSense => existingSense.SenseOrder == senseOrder))
        {
            throw new DomainRuleException("Duplicate sense order is not allowed within the same word entry.");
        }

        if (isPrimarySense)
        {
            foreach (WordSense existingSense in _senses.Where(existingSense => existingSense.IsPrimarySense))
            {
                existingSense.SetPrimarySense(false, createdAtUtc);
            }
        }

        WordSense sense = new(
            id,
            Id,
            senseOrder,
            isPrimarySense,
            publicationStatus,
            createdAtUtc,
            shortDefinitionDe,
            shortGloss);

        _senses.Add(sense);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));

        return sense;
    }

    /// <summary>
    /// Adds a topic link to the lexical entry.
    /// </summary>
    public WordTopic AddTopic(
        Guid id,
        Guid topicId,
        bool isPrimaryTopic,
        DateTime createdAtUtc)
    {
        if (_topics.Any(existingTopic => existingTopic.Id == id))
        {
            throw new DomainRuleException("Duplicate topic-link identifiers are not allowed within the same word entry.");
        }

        if (topicId == Guid.Empty)
        {
            throw new DomainRuleException("Topic identifier cannot be empty.");
        }

        if (_topics.Any(existingTopic => existingTopic.TopicId == topicId))
        {
            throw new DomainRuleException("Duplicate topic links are not allowed within the same word entry.");
        }

        if (isPrimaryTopic)
        {
            foreach (WordTopic existingTopic in _topics.Where(existingTopic => existingTopic.IsPrimaryTopic))
            {
                existingTopic.SetPrimaryTopic(false);
            }
        }

        WordTopic topicLink = new(id, Id, topicId, isPrimaryTopic, createdAtUtc);
        _topics.Add(topicLink);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));

        return topicLink;
    }

    /// <summary>
    /// Resolves the primary sense when one exists, otherwise falls back to the first ordered sense.
    /// </summary>
    public WordSense? GetPrimarySense()
    {
        return _senses
            .OrderByDescending(sense => sense.IsPrimarySense)
            .ThenBy(sense => sense.SenseOrder)
            .FirstOrDefault();
    }

    private static string NormalizeRequiredText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException($"{parameterName} cannot be empty.");
        }

        return CollapseWhitespace().Replace(value.Trim(), " ");
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : CollapseWhitespace().Replace(value.Trim(), " ");
    }

    private static string NormalizeLemma(string lemma)
    {
        return CollapseWhitespace().Replace(lemma.Trim(), " ").ToLowerInvariant();
    }

    private static DateTime NormalizeUtc(DateTime value, string parameterName)
    {
        if (value == default)
        {
            throw new DomainRuleException($"{parameterName} cannot be empty.");
        }

        return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }

    [GeneratedRegex("\\s+", RegexOptions.Compiled)]
    private static partial Regex CollapseWhitespace();
}
