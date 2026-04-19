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
    private readonly List<WordLexicalForm> _lexicalForms = [];
    private readonly List<WordSense> _senses = [];
    private readonly List<WordTopic> _topics = [];
    private readonly List<WordLabel> _labels = [];
    private readonly List<WordGrammarNote> _grammarNotes = [];
    private readonly List<WordCollocation> _collocations = [];
    private readonly List<WordFamilyMember> _familyMembers = [];
    private readonly List<WordRelation> _relations = [];

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

        AddLexicalForm(
            Guid.NewGuid(),
            partOfSpeech,
            true,
            CreatedAtUtc,
            article,
            pluralForm,
            infinitiveForm);
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
    /// Gets the lexical-role variants attached to the entry.
    /// </summary>
    public IReadOnlyCollection<WordLexicalForm> LexicalForms => _lexicalForms.AsReadOnly();

    /// <summary>
    /// Gets the senses that belong to the lexical entry.
    /// </summary>
    public IReadOnlyCollection<WordSense> Senses => _senses.AsReadOnly();

    /// <summary>
    /// Gets the topic links that belong to the lexical entry.
    /// </summary>
    public IReadOnlyCollection<WordTopic> Topics => _topics.AsReadOnly();

    /// <summary>
    /// Gets the lexical labels attached to the entry.
    /// </summary>
    public IReadOnlyCollection<WordLabel> Labels => _labels.AsReadOnly();

    /// <summary>
    /// Gets the grammar notes attached to the entry.
    /// </summary>
    public IReadOnlyCollection<WordGrammarNote> GrammarNotes => _grammarNotes.AsReadOnly();

    /// <summary>
    /// Gets the collocations attached to the entry.
    /// </summary>
    public IReadOnlyCollection<WordCollocation> Collocations => _collocations.AsReadOnly();

    /// <summary>
    /// Gets the word-family members attached to the entry.
    /// </summary>
    public IReadOnlyCollection<WordFamilyMember> FamilyMembers => _familyMembers.AsReadOnly();

    /// <summary>
    /// Gets the lexical relations attached to the entry.
    /// </summary>
    public IReadOnlyCollection<WordRelation> Relations => _relations.AsReadOnly();

    /// <summary>
    /// Adds a lexical-role variant to the entry.
    /// </summary>
    public WordLexicalForm AddLexicalForm(
        Guid id,
        PartOfSpeech partOfSpeech,
        bool isPrimary,
        DateTime createdAtUtc,
        string? article = null,
        string? pluralForm = null,
        string? infinitiveForm = null)
    {
        if (_lexicalForms.Any(existingForm => existingForm.Id == id))
        {
            throw new DomainRuleException("Duplicate lexical-form identifiers are not allowed within the same word entry.");
        }

        if (_lexicalForms.Any(existingForm => existingForm.PartOfSpeech == partOfSpeech))
        {
            throw new DomainRuleException("Duplicate lexical-form part of speech is not allowed within the same word entry.");
        }

        if (isPrimary)
        {
            foreach (WordLexicalForm existingForm in _lexicalForms.Where(existingForm => existingForm.IsPrimary))
            {
                existingForm.SetPrimary(false, createdAtUtc);
            }
        }

        WordLexicalForm lexicalForm = new(
            id,
            Id,
            partOfSpeech,
            _lexicalForms.Count + 1,
            isPrimary,
            createdAtUtc,
            article,
            pluralForm,
            infinitiveForm);

        _lexicalForms.Add(lexicalForm);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));

        if (isPrimary)
        {
            ApplyPrimaryLexicalForm(lexicalForm);
        }

        return lexicalForm;
    }

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
    /// Adds a lexical usage or context label to the entry.
    /// </summary>
    public WordLabel AddLabel(
        Guid id,
        WordLabelKind kind,
        string key,
        DateTime createdAtUtc)
    {
        if (_labels.Any(existingLabel => existingLabel.Id == id))
        {
            throw new DomainRuleException("Duplicate word-label identifiers are not allowed within the same word entry.");
        }

        string normalizedKey = NormalizeLabelKey(key);

        if (_labels.Any(existingLabel => existingLabel.Kind == kind && existingLabel.Key == normalizedKey))
        {
            throw new DomainRuleException("Duplicate word-label keys are not allowed within the same word entry and label kind.");
        }

        WordLabel label = new(
            id,
            Id,
            kind,
            normalizedKey,
            _labels.Count(existingLabel => existingLabel.Kind == kind) + 1,
            createdAtUtc);

        _labels.Add(label);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));

        return label;
    }

    /// <summary>
    /// Adds a grammar note to the entry.
    /// </summary>
    public WordGrammarNote AddGrammarNote(
        Guid id,
        string text,
        DateTime createdAtUtc)
    {
        if (_grammarNotes.Any(existingNote => existingNote.Id == id))
        {
            throw new DomainRuleException("Duplicate grammar-note identifiers are not allowed within the same word entry.");
        }

        string normalizedText = NormalizeGrammarNoteText(text);

        if (_grammarNotes.Any(existingNote => string.Equals(existingNote.Text, normalizedText, StringComparison.Ordinal)))
        {
            throw new DomainRuleException("Duplicate grammar-note text is not allowed within the same word entry.");
        }

        WordGrammarNote grammarNote = new(
            id,
            Id,
            normalizedText,
            _grammarNotes.Count + 1,
            createdAtUtc);

        _grammarNotes.Add(grammarNote);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));

        return grammarNote;
    }

    /// <summary>
    /// Adds a collocation to the entry.
    /// </summary>
    public WordCollocation AddCollocation(
        Guid id,
        string text,
        string? meaning,
        DateTime createdAtUtc)
    {
        if (_collocations.Any(existingCollocation => existingCollocation.Id == id))
        {
            throw new DomainRuleException("Duplicate collocation identifiers are not allowed within the same word entry.");
        }

        string normalizedText = NormalizeCollocationText(text);

        if (_collocations.Any(existingCollocation => string.Equals(existingCollocation.Text, normalizedText, StringComparison.Ordinal)))
        {
            throw new DomainRuleException("Duplicate collocation text is not allowed within the same word entry.");
        }

        WordCollocation collocation = new(
            id,
            Id,
            normalizedText,
            NormalizeCollocationMeaning(meaning),
            _collocations.Count + 1,
            createdAtUtc);

        _collocations.Add(collocation);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));

        return collocation;
    }

    /// <summary>
    /// Adds a word-family member to the entry.
    /// </summary>
    public WordFamilyMember AddFamilyMember(
        Guid id,
        string lemma,
        string relationLabel,
        string? note,
        DateTime createdAtUtc)
    {
        if (_familyMembers.Any(existingMember => existingMember.Id == id))
        {
            throw new DomainRuleException("Duplicate family-member identifiers are not allowed within the same word entry.");
        }

        string normalizedLemma = NormalizeFamilyMemberLemma(lemma);
        string normalizedRelationLabel = NormalizeFamilyMemberRelationLabel(relationLabel);

        if (_familyMembers.Any(existingMember =>
                string.Equals(existingMember.Lemma, normalizedLemma, StringComparison.Ordinal) &&
                string.Equals(existingMember.RelationLabel, normalizedRelationLabel, StringComparison.Ordinal)))
        {
            throw new DomainRuleException("Duplicate family-member lemma and relation label are not allowed within the same word entry.");
        }

        WordFamilyMember familyMember = new(
            id,
            Id,
            normalizedLemma,
            normalizedRelationLabel,
            NormalizeFamilyMemberNote(note),
            _familyMembers.Count + 1,
            createdAtUtc);

        _familyMembers.Add(familyMember);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));

        return familyMember;
    }

    /// <summary>
    /// Adds a lexical relation to the entry.
    /// </summary>
    public WordRelation AddRelation(
        Guid id,
        WordRelationKind kind,
        string lemma,
        string? note,
        DateTime createdAtUtc)
    {
        if (_relations.Any(existingRelation => existingRelation.Id == id))
        {
            throw new DomainRuleException("Duplicate relation identifiers are not allowed within the same word entry.");
        }

        string normalizedLemma = NormalizeRelationLemma(lemma);

        if (_relations.Any(existingRelation =>
                existingRelation.Kind == kind &&
                string.Equals(existingRelation.Lemma, normalizedLemma, StringComparison.Ordinal)))
        {
            throw new DomainRuleException("Duplicate relation lemma is not allowed within the same word entry and relation kind.");
        }

        int sortOrder = _relations.Count(existingRelation => existingRelation.Kind == kind) + 1;

        WordRelation relation = new(
            id,
            Id,
            kind,
            normalizedLemma,
            NormalizeRelationNote(note),
            sortOrder,
            createdAtUtc);

        _relations.Add(relation);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));

        return relation;
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

    /// <summary>
    /// Resolves the primary lexical form when one exists, otherwise falls back to the first ordered form.
    /// </summary>
    public WordLexicalForm? GetPrimaryLexicalForm()
    {
        return _lexicalForms
            .OrderByDescending(form => form.IsPrimary)
            .ThenBy(form => form.SortOrder)
            .FirstOrDefault();
    }

    private void ApplyPrimaryLexicalForm(WordLexicalForm lexicalForm)
    {
        ArgumentNullException.ThrowIfNull(lexicalForm);

        PartOfSpeech = lexicalForm.PartOfSpeech;
        Article = lexicalForm.Article;
        PluralForm = lexicalForm.PluralForm;
        InfinitiveForm = lexicalForm.InfinitiveForm;
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

    private static string NormalizeLabelKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Word label key cannot be empty.");
        }

        return value.Trim().ToLowerInvariant();
    }

    private static string NormalizeGrammarNoteText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Word grammar-note text cannot be empty.");
        }

        return CollapseWhitespace().Replace(value.Trim(), " ");
    }

    private static string NormalizeCollocationText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Word collocation text cannot be empty.");
        }

        return CollapseWhitespace().Replace(value.Trim(), " ");
    }

    private static string? NormalizeCollocationMeaning(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : CollapseWhitespace().Replace(value.Trim(), " ");
    }

    private static string NormalizeFamilyMemberLemma(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Word family-member lemma cannot be empty.");
        }

        return CollapseWhitespace().Replace(value.Trim(), " ");
    }

    private static string NormalizeFamilyMemberRelationLabel(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Word family-member relation label cannot be empty.");
        }

        return CollapseWhitespace().Replace(value.Trim(), " ");
    }

    private static string? NormalizeFamilyMemberNote(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : CollapseWhitespace().Replace(value.Trim(), " ");
    }

    private static string NormalizeRelationLemma(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Word relation lemma cannot be empty.");
        }

        return CollapseWhitespace().Replace(value.Trim(), " ");
    }

    private static string? NormalizeRelationNote(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : CollapseWhitespace().Replace(value.Trim(), " ");
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
