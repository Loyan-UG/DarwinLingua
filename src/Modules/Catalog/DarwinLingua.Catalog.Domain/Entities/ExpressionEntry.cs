using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed class ExpressionEntry
{
    private static readonly Regex KebabCaseRegex = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

    private readonly List<ExpressionTopic> _topics = [];
    private readonly List<ExpressionMeaning> _meanings = [];
    private readonly List<ExpressionExample> _examples = [];
    private readonly List<ExpressionWarning> _warnings = [];
    private readonly List<ExpressionLinkedWord> _linkedWords = [];
    private readonly List<RelatedExpressionLink> _relatedExpressions = [];
    private readonly List<ExpressionLinkedExercise> _linkedExercises = [];

    private ExpressionEntry()
    {
        Slug = string.Empty;
        ExpressionText = string.Empty;
        ActualMeaningText = string.Empty;
        ExpressionType = string.Empty;
        Register = string.Empty;
        Category = string.Empty;
    }

    public ExpressionEntry(
        Guid id,
        string slug,
        string expressionText,
        string? literalMeaningText,
        string actualMeaningText,
        string? usageExplanation,
        CefrLevel cefrLevel,
        string expressionType,
        string register,
        string category,
        string? region,
        bool isRisky,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime timestampUtc,
        string? meaningTransparency = null,
        string? teachingReason = null,
        string? safetyRating = null,
        int minimumAge = 0,
        bool requiresAdultAccess = false,
        string? adultContentCategory = null)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Expression id is required.");
        }

        Id = id;
        Slug = NormalizeKebabKey(slug, "Expression slug");
        ExpressionText = RequireText(expressionText, "Expression text", 512);
        LiteralMeaningText = NormalizeOptionalText(literalMeaningText, 1024, "Literal meaning");
        ActualMeaningText = RequireText(actualMeaningText, "Actual meaning", 4000);
        UsageExplanation = NormalizeOptionalText(usageExplanation, 4000, "Usage explanation");
        CefrLevel = cefrLevel;
        ExpressionType = NormalizeKebabKey(expressionType, "Expression type");
        Register = NormalizeKebabKey(register, "Expression register");
        Category = NormalizeKebabKey(category, "Expression category");
        Region = NormalizeOptionalText(region, 128, "Expression region");
        IsRisky = isRisky;
        MeaningTransparency = NormalizeOptionalKebabKey(meaningTransparency, "Expression meaning transparency");
        TeachingReason = NormalizeOptionalText(teachingReason, 2000, "Expression teaching reason");
        SafetyRating = NormalizeOptionalKebabKey(safetyRating, "Expression safety rating") ?? "general";
        MinimumAge = minimumAge is 16 or 18 ? minimumAge : 0;
        RequiresAdultAccess = requiresAdultAccess;
        AdultContentCategory = NormalizeOptionalKebabKey(adultContentCategory, "Expression adult content category");
        PublicationStatus = publicationStatus;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public string Slug { get; private set; }
    public string ExpressionText { get; private set; }
    public string? LiteralMeaningText { get; private set; }
    public string ActualMeaningText { get; private set; }
    public string? UsageExplanation { get; private set; }
    public CefrLevel CefrLevel { get; private set; }
    public string ExpressionType { get; private set; }
    public string Register { get; private set; }
    public string Category { get; private set; }
    public string? Region { get; private set; }
    public bool IsRisky { get; private set; }
    public string? MeaningTransparency { get; private set; }
    public string? TeachingReason { get; private set; }
    public string SafetyRating { get; private set; } = "general";
    public int MinimumAge { get; private set; }
    public bool RequiresAdultAccess { get; private set; }
    public string? AdultContentCategory { get; private set; }
    public PublicationStatus PublicationStatus { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<ExpressionTopic> Topics => _topics;
    public IReadOnlyCollection<ExpressionMeaning> Meanings => _meanings;
    public IReadOnlyCollection<ExpressionExample> Examples => _examples;
    public IReadOnlyCollection<ExpressionWarning> Warnings => _warnings;
    public IReadOnlyCollection<ExpressionLinkedWord> LinkedWords => _linkedWords;
    public IReadOnlyCollection<RelatedExpressionLink> RelatedExpressions => _relatedExpressions;
    public IReadOnlyCollection<ExpressionLinkedExercise> LinkedExercises => _linkedExercises;

    public void AddTopic(Guid id, Guid topicId, bool isPrimary, DateTime timestampUtc)
    {
        if (_topics.Any(topic => topic.TopicId == topicId))
        {
            throw new DomainRuleException("Expression cannot contain duplicate topic links.");
        }

        _topics.Add(new ExpressionTopic(id, Id, topicId, isPrimary, timestampUtc));
    }

    public ExpressionMeaning AddMeaning(Guid id, LanguageCode languageCode, string actualMeaning, string? literalMeaning, string? usageExplanation, DateTime timestampUtc)
    {
        ExpressionMeaning meaning = new(id, Id, languageCode, actualMeaning, literalMeaning, usageExplanation, timestampUtc);
        _meanings.Add(meaning);
        return meaning;
    }

    public ExpressionExample AddExample(Guid id, int sortOrder, string germanText, string? note, DateTime timestampUtc)
    {
        ExpressionExample example = new(id, Id, sortOrder, germanText, note, timestampUtc);
        _examples.Add(example);
        return example;
    }

    public ExpressionWarning AddWarning(Guid id, string warningType, string text, DateTime timestampUtc)
    {
        ExpressionWarning warning = new(id, Id, warningType, text, timestampUtc);
        _warnings.Add(warning);
        return warning;
    }

    public void AddLinkedWord(Guid id, string lemma, string? wordSlug, int sortOrder, DateTime timestampUtc) =>
        _linkedWords.Add(new ExpressionLinkedWord(id, Id, RequireText(lemma, "Linked word lemma", 128), NormalizeOptionalKebabKey(wordSlug, "Linked word slug"), sortOrder, timestampUtc));

    public void AddRelatedExpression(Guid id, string slug, int sortOrder, DateTime timestampUtc) =>
        _relatedExpressions.Add(new RelatedExpressionLink(id, Id, NormalizeKebabKey(slug, "Related expression slug"), sortOrder, timestampUtc));

    public void AddLinkedExercise(Guid id, string slug, int sortOrder, DateTime timestampUtc) =>
        _linkedExercises.Add(new ExpressionLinkedExercise(id, Id, NormalizeKebabKey(slug, "Linked exercise slug"), sortOrder, timestampUtc));

    internal static string RequireText(string value, string fieldName, int maxLength)
    {
        string normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new DomainRuleException($"{fieldName} is required.");
        }

        if (normalized.Length > maxLength)
        {
            throw new DomainRuleException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }

    internal static string NormalizeKebabKey(string value, string fieldName)
    {
        string normalized = value.Trim().ToLowerInvariant();
        if (!KebabCaseRegex.IsMatch(normalized))
        {
            throw new DomainRuleException($"{fieldName} must use lowercase kebab-case.");
        }

        return normalized;
    }

    internal static string? NormalizeOptionalKebabKey(string? value, string fieldName) =>
        string.IsNullOrWhiteSpace(value) ? null : NormalizeKebabKey(value, fieldName);

    internal static string? NormalizeOptionalText(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainRuleException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }
}

public sealed class ExpressionTopic
{
    private ExpressionTopic() { }
    internal ExpressionTopic(Guid id, Guid expressionEntryId, Guid topicId, bool isPrimary, DateTime timestampUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Expression topic id is required.") : id;
        ExpressionEntryId = expressionEntryId;
        TopicId = topicId;
        IsPrimary = isPrimary;
        CreatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public Guid ExpressionEntryId { get; private set; }
    public Guid TopicId { get; private set; }
    public bool IsPrimary { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class ExpressionMeaning
{
    private ExpressionMeaning() { LanguageCode = LanguageCode.From("en"); ActualMeaningText = string.Empty; }
    internal ExpressionMeaning(Guid id, Guid expressionEntryId, LanguageCode languageCode, string actualMeaningText, string? literalMeaningText, string? usageExplanation, DateTime timestampUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Expression meaning id is required.") : id;
        ExpressionEntryId = expressionEntryId;
        LanguageCode = languageCode;
        ActualMeaningText = ExpressionEntry.RequireText(actualMeaningText, "Expression meaning actual text", 4000);
        LiteralMeaningText = ExpressionEntry.NormalizeOptionalText(literalMeaningText, 1024, "Expression literal meaning");
        UsageExplanation = ExpressionEntry.NormalizeOptionalText(usageExplanation, 4000, "Expression usage explanation");
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public Guid ExpressionEntryId { get; private set; }
    public LanguageCode LanguageCode { get; private set; }
    public string ActualMeaningText { get; private set; }
    public string? LiteralMeaningText { get; private set; }
    public string? UsageExplanation { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
}

public sealed class ExpressionExample
{
    private readonly List<ExpressionExampleTranslation> _translations = [];
    private ExpressionExample() { GermanText = string.Empty; }
    internal ExpressionExample(Guid id, Guid expressionEntryId, int sortOrder, string germanText, string? note, DateTime timestampUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Expression example id is required.") : id;
        ExpressionEntryId = expressionEntryId;
        SortOrder = Math.Max(0, sortOrder);
        GermanText = ExpressionEntry.RequireText(germanText, "Expression example German text", 1024);
        Note = ExpressionEntry.NormalizeOptionalText(note, 512, "Expression example note");
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public Guid ExpressionEntryId { get; private set; }
    public int SortOrder { get; private set; }
    public string GermanText { get; private set; }
    public string? Note { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<ExpressionExampleTranslation> Translations => _translations;

    public void AddTranslation(Guid id, LanguageCode languageCode, string text, DateTime timestampUtc) =>
        _translations.Add(new ExpressionExampleTranslation(id, Id, languageCode, text, timestampUtc));
}

public sealed class ExpressionWarning
{
    private readonly List<ExpressionWarningTranslation> _translations = [];
    private ExpressionWarning() { WarningType = string.Empty; Text = string.Empty; }
    internal ExpressionWarning(Guid id, Guid expressionEntryId, string warningType, string text, DateTime timestampUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Expression warning id is required.") : id;
        ExpressionEntryId = expressionEntryId;
        WarningType = ExpressionEntry.NormalizeKebabKey(warningType, "Expression warning type");
        Text = ExpressionEntry.RequireText(text, "Expression warning text", 2000);
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public Guid ExpressionEntryId { get; private set; }
    public string WarningType { get; private set; }
    public string Text { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<ExpressionWarningTranslation> Translations => _translations;

    public void AddTranslation(Guid id, LanguageCode languageCode, string text, DateTime timestampUtc) =>
        _translations.Add(new ExpressionWarningTranslation(id, Id, languageCode, text, timestampUtc));
}

public abstract class ExpressionTranslationBase
{
    protected ExpressionTranslationBase() { LanguageCode = LanguageCode.From("en"); Text = string.Empty; }
    protected ExpressionTranslationBase(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime timestampUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Expression translation id is required.") : id;
        OwnerId = ownerId;
        LanguageCode = languageCode;
        Text = ExpressionEntry.RequireText(text, "Expression translation text", 4000);
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; protected set; }
    public Guid OwnerId { get; protected set; }
    public LanguageCode LanguageCode { get; protected set; }
    public string Text { get; protected set; }
    public DateTime CreatedAtUtc { get; protected set; }
    public DateTime UpdatedAtUtc { get; protected set; }
}

public sealed class ExpressionExampleTranslation : ExpressionTranslationBase
{
    private ExpressionExampleTranslation() { }
    internal ExpressionExampleTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime timestampUtc)
        : base(id, ownerId, languageCode, text, timestampUtc) { }
}

public sealed class ExpressionWarningTranslation : ExpressionTranslationBase
{
    private ExpressionWarningTranslation() { }
    internal ExpressionWarningTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime timestampUtc)
        : base(id, ownerId, languageCode, text, timestampUtc) { }
}

public abstract class ExpressionSlugLink
{
    protected ExpressionSlugLink() { TargetSlug = string.Empty; }
    protected ExpressionSlugLink(Guid id, Guid expressionEntryId, string targetSlug, int sortOrder, DateTime timestampUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Expression link id is required.") : id;
        ExpressionEntryId = expressionEntryId;
        TargetSlug = targetSlug;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = timestampUtc;
    }

    public Guid Id { get; protected set; }
    public Guid ExpressionEntryId { get; protected set; }
    public string TargetSlug { get; protected set; }
    public int SortOrder { get; protected set; }
    public DateTime CreatedAtUtc { get; protected set; }
}

public sealed class RelatedExpressionLink : ExpressionSlugLink
{
    private RelatedExpressionLink() { }
    internal RelatedExpressionLink(Guid id, Guid expressionEntryId, string targetSlug, int sortOrder, DateTime timestampUtc)
        : base(id, expressionEntryId, targetSlug, sortOrder, timestampUtc) { }
}

public sealed class ExpressionLinkedExercise : ExpressionSlugLink
{
    private ExpressionLinkedExercise() { }
    internal ExpressionLinkedExercise(Guid id, Guid expressionEntryId, string targetSlug, int sortOrder, DateTime timestampUtc)
        : base(id, expressionEntryId, targetSlug, sortOrder, timestampUtc) { }
}

public sealed class ExpressionLinkedWord
{
    private ExpressionLinkedWord() { Lemma = string.Empty; }
    internal ExpressionLinkedWord(Guid id, Guid expressionEntryId, string lemma, string? wordSlug, int sortOrder, DateTime timestampUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Expression linked word id is required.") : id;
        ExpressionEntryId = expressionEntryId;
        Lemma = lemma;
        WordSlug = wordSlug;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public Guid ExpressionEntryId { get; private set; }
    public string Lemma { get; private set; }
    public string? WordSlug { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}
