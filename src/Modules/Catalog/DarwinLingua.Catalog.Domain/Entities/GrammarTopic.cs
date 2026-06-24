using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed class GrammarTopic
{
    private static readonly Regex KebabCaseRegex = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

    private readonly List<GrammarTopicTopic> _topics = [];
    private readonly List<GrammarSection> _sections = [];
    private readonly List<GrammarExample> _examples = [];
    private readonly List<GrammarCommonMistake> _commonMistakes = [];
    private readonly List<GrammarRuleSummary> _ruleSummaries = [];
    private readonly List<GrammarExceptionNote> _exceptionNotes = [];
    private readonly List<GrammarPrerequisiteLink> _prerequisites = [];
    private readonly List<GrammarRelatedTopicLink> _relatedTopics = [];
    private readonly List<GrammarLinkedWord> _linkedWords = [];
    private readonly List<GrammarLinkedDialogue> _linkedDialogues = [];
    private readonly List<GrammarLinkedTalkTopic> _linkedTalkTopics = [];
    private readonly List<GrammarLinkedExercise> _linkedExercises = [];

    private GrammarTopic()
    {
        Slug = string.Empty;
        Title = string.Empty;
        ShortDescription = string.Empty;
        GrammarCategory = string.Empty;
        TargetLearningLanguageCode = ContentLanguageRequirements.DefaultTargetLearningLanguageCode;
    }

    public GrammarTopic(
        Guid id,
        string slug,
        string title,
        string shortDescription,
        CefrLevel cefrLevel,
        string grammarCategory,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime timestampUtc,
        string? targetLearningLanguageCode = null)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Grammar topic id is required.");
        }

        Id = id;
        TargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode, "Grammar topic target learning language");
        Slug = NormalizeKebabKey(slug, "Grammar topic slug");
        Title = RequireText(title, "Grammar topic title", 256);
        ShortDescription = RequireText(shortDescription, "Grammar topic short description", 1024);
        CefrLevel = cefrLevel;
        GrammarCategory = NormalizeKebabKey(grammarCategory, "Grammar category");
        PublicationStatus = publicationStatus;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }

    public string TargetLearningLanguageCode { get; private set; }

    public string Slug { get; private set; }

    public string Title { get; private set; }

    public string ShortDescription { get; private set; }

    public int? ContentRevision { get; private set; }

    public string? TitleLocalizedJson { get; private set; }

    public string? ShortDescriptionLocalizedJson { get; private set; }

    public string? ImageSlotsJson { get; private set; }

    public CefrLevel CefrLevel { get; private set; }

    public string GrammarCategory { get; private set; }

    public PublicationStatus PublicationStatus { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<GrammarTopicTopic> Topics => _topics;

    public IReadOnlyCollection<GrammarSection> Sections => _sections;

    public IReadOnlyCollection<GrammarExample> Examples => _examples;

    public IReadOnlyCollection<GrammarCommonMistake> CommonMistakes => _commonMistakes;

    public IReadOnlyCollection<GrammarRuleSummary> RuleSummaries => _ruleSummaries;

    public IReadOnlyCollection<GrammarExceptionNote> ExceptionNotes => _exceptionNotes;

    public IReadOnlyCollection<GrammarPrerequisiteLink> Prerequisites => _prerequisites;

    public IReadOnlyCollection<GrammarRelatedTopicLink> RelatedTopics => _relatedTopics;

    public IReadOnlyCollection<GrammarLinkedWord> LinkedWords => _linkedWords;

    public IReadOnlyCollection<GrammarLinkedDialogue> LinkedDialogues => _linkedDialogues;

    public IReadOnlyCollection<GrammarLinkedTalkTopic> LinkedTalkTopics => _linkedTalkTopics;

    public IReadOnlyCollection<GrammarLinkedExercise> LinkedExercises => _linkedExercises;

    public void SetRichContentMetadata(
        int? contentRevision,
        string? titleLocalizedJson,
        string? shortDescriptionLocalizedJson,
        string? imageSlotsJson,
        DateTime timestampUtc)
    {
        ContentRevision = contentRevision;
        TitleLocalizedJson = NormalizeOptionalJson(titleLocalizedJson, "Grammar topic localized title JSON", 16000);
        ShortDescriptionLocalizedJson = NormalizeOptionalJson(shortDescriptionLocalizedJson, "Grammar topic localized short description JSON", 32000);
        ImageSlotsJson = NormalizeOptionalJson(imageSlotsJson, "Grammar topic image slots JSON", 32000);
        UpdatedAtUtc = timestampUtc;
    }

    public void AddTopic(Guid id, Guid topicId, bool isPrimary, DateTime timestampUtc)
    {
        if (topicId == Guid.Empty)
        {
            throw new DomainRuleException("Grammar topic link target topic id is required.");
        }

        if (_topics.Any(topic => topic.TopicId == topicId))
        {
            throw new DomainRuleException("Grammar topic cannot contain duplicate topic links.");
        }

        _topics.Add(new GrammarTopicTopic(id, Id, topicId, isPrimary, timestampUtc));
    }

    public GrammarSection AddSection(
        Guid id,
        int sortOrder,
        string heading,
        string explanation,
        DateTime timestampUtc,
        string? sectionKey = null,
        string? localizedBlocksJson = null)
    {
        GrammarSection section = new(id, Id, sortOrder, heading, explanation, timestampUtc, sectionKey, localizedBlocksJson);
        _sections.Add(section);
        return section;
    }

    public GrammarExample AddExample(Guid id, int sortOrder, string germanText, string? note, DateTime timestampUtc)
    {
        GrammarExample example = new(id, Id, sortOrder, germanText, note, timestampUtc);
        _examples.Add(example);
        return example;
    }

    public GrammarCommonMistake AddCommonMistake(Guid id, int sortOrder, string wrongText, string correctedText, string explanation, DateTime timestampUtc)
    {
        GrammarCommonMistake mistake = new(id, Id, sortOrder, wrongText, correctedText, explanation, timestampUtc);
        _commonMistakes.Add(mistake);
        return mistake;
    }

    public GrammarRuleSummary AddRuleSummary(Guid id, int sortOrder, string text, DateTime timestampUtc)
    {
        GrammarRuleSummary rule = new(id, Id, sortOrder, text, timestampUtc);
        _ruleSummaries.Add(rule);
        return rule;
    }

    public GrammarExceptionNote AddExceptionNote(Guid id, int sortOrder, string text, DateTime timestampUtc)
    {
        GrammarExceptionNote note = new(id, Id, sortOrder, text, timestampUtc);
        _exceptionNotes.Add(note);
        return note;
    }

    public void AddPrerequisite(Guid id, string targetSlug, int sortOrder, DateTime timestampUtc) =>
        _prerequisites.Add(new GrammarPrerequisiteLink(id, Id, NormalizeKebabKey(targetSlug, "Prerequisite grammar slug"), sortOrder, timestampUtc));

    public void AddRelatedTopic(Guid id, string targetSlug, int sortOrder, DateTime timestampUtc) =>
        _relatedTopics.Add(new GrammarRelatedTopicLink(id, Id, NormalizeKebabKey(targetSlug, "Related grammar slug"), sortOrder, timestampUtc));

    public void AddLinkedWord(Guid id, string lemma, string? wordSlug, int sortOrder, DateTime timestampUtc) =>
        _linkedWords.Add(new GrammarLinkedWord(id, Id, RequireText(lemma, "Linked word lemma", 128), NormalizeOptionalKebabKey(wordSlug, "Linked word slug"), sortOrder, timestampUtc));

    public void AddLinkedDialogue(Guid id, string slug, int sortOrder, DateTime timestampUtc) =>
        _linkedDialogues.Add(new GrammarLinkedDialogue(id, Id, NormalizeKebabKey(slug, "Linked dialogue slug"), sortOrder, timestampUtc));

    public void AddLinkedTalkTopic(Guid id, string slug, int sortOrder, DateTime timestampUtc) =>
        _linkedTalkTopics.Add(new GrammarLinkedTalkTopic(id, Id, NormalizeKebabKey(slug, "Linked Talk Topic slug"), sortOrder, timestampUtc));

    public void AddLinkedExercise(Guid id, string slug, int sortOrder, DateTime timestampUtc) =>
        _linkedExercises.Add(new GrammarLinkedExercise(id, Id, NormalizeKebabKey(slug, "Linked exercise slug"), sortOrder, timestampUtc));

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

    internal static string? NormalizeOptionalJson(string? value, string fieldName, int maxLength)
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

public sealed class GrammarTopicTopic
{
    private GrammarTopicTopic() { }

    internal GrammarTopicTopic(Guid id, Guid grammarTopicId, Guid topicId, bool isPrimary, DateTime timestampUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Grammar topic topic id is required.") : id;
        GrammarTopicId = grammarTopicId;
        TopicId = topicId;
        IsPrimary = isPrimary;
        CreatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public Guid GrammarTopicId { get; private set; }
    public Guid TopicId { get; private set; }
    public bool IsPrimary { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class GrammarSection
{
    private readonly List<GrammarSectionTranslation> _translations = [];
    private GrammarSection() { Heading = string.Empty; Explanation = string.Empty; }

    internal GrammarSection(
        Guid id,
        Guid grammarTopicId,
        int sortOrder,
        string heading,
        string explanation,
        DateTime timestampUtc,
        string? sectionKey = null,
        string? localizedBlocksJson = null)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Grammar section id is required.") : id;
        GrammarTopicId = grammarTopicId;
        SortOrder = Math.Max(0, sortOrder);
        SectionKey = string.IsNullOrWhiteSpace(sectionKey) ? null : GrammarTopic.NormalizeKebabKey(sectionKey, "Grammar section key");
        Heading = GrammarTopic.RequireText(heading, "Grammar section heading", 256);
        Explanation = GrammarTopic.RequireText(explanation, "Grammar section explanation", 12000);
        LocalizedBlocksJson = GrammarTopic.NormalizeOptionalJson(localizedBlocksJson, "Grammar section localized blocks JSON", 64000);
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public Guid GrammarTopicId { get; private set; }
    public int SortOrder { get; private set; }
    public string? SectionKey { get; private set; }
    public string Heading { get; private set; }
    public string Explanation { get; private set; }
    public string? LocalizedBlocksJson { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<GrammarSectionTranslation> Translations => _translations;

    public void AddTranslation(Guid id, LanguageCode languageCode, string heading, string explanation, DateTime timestampUtc) =>
        _translations.Add(new GrammarSectionTranslation(id, Id, languageCode, heading, explanation, timestampUtc));
}

public sealed class GrammarExample
{
    private readonly List<GrammarExampleTranslation> _translations = [];
    private GrammarExample() { GermanText = string.Empty; }

    internal GrammarExample(Guid id, Guid grammarTopicId, int sortOrder, string germanText, string? note, DateTime timestampUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Grammar example id is required.") : id;
        GrammarTopicId = grammarTopicId;
        SortOrder = Math.Max(0, sortOrder);
        GermanText = GrammarTopic.RequireText(germanText, "Grammar example German text", 1024);
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public Guid GrammarTopicId { get; private set; }
    public int SortOrder { get; private set; }
    public string GermanText { get; private set; }
    public string? Note { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<GrammarExampleTranslation> Translations => _translations;

    public void AddTranslation(Guid id, LanguageCode languageCode, string text, DateTime timestampUtc) =>
        _translations.Add(new GrammarExampleTranslation(id, Id, languageCode, text, timestampUtc));
}

public sealed class GrammarCommonMistake
{
    private readonly List<GrammarCommonMistakeTranslation> _translations = [];
    private GrammarCommonMistake() { WrongText = string.Empty; CorrectedText = string.Empty; Explanation = string.Empty; }

    internal GrammarCommonMistake(Guid id, Guid grammarTopicId, int sortOrder, string wrongText, string correctedText, string explanation, DateTime timestampUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Grammar mistake id is required.") : id;
        GrammarTopicId = grammarTopicId;
        SortOrder = Math.Max(0, sortOrder);
        WrongText = GrammarTopic.RequireText(wrongText, "Grammar mistake wrong text", 1024);
        CorrectedText = GrammarTopic.RequireText(correctedText, "Grammar mistake corrected text", 1024);
        Explanation = GrammarTopic.RequireText(explanation, "Grammar mistake explanation", 4000);
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public Guid GrammarTopicId { get; private set; }
    public int SortOrder { get; private set; }
    public string WrongText { get; private set; }
    public string CorrectedText { get; private set; }
    public string Explanation { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<GrammarCommonMistakeTranslation> Translations => _translations;

    public void AddTranslation(Guid id, LanguageCode languageCode, string explanation, DateTime timestampUtc) =>
        _translations.Add(new GrammarCommonMistakeTranslation(id, Id, languageCode, explanation, timestampUtc));
}

public sealed class GrammarRuleSummary : GrammarLocalizedTextOwner<GrammarRuleSummaryTranslation>
{
    private GrammarRuleSummary() { Text = string.Empty; }

    internal GrammarRuleSummary(Guid id, Guid grammarTopicId, int sortOrder, string text, DateTime timestampUtc)
        : base(id, grammarTopicId, sortOrder, text, 2000, timestampUtc)
    {
    }

    public override IReadOnlyCollection<GrammarRuleSummaryTranslation> Translations => Items;

    public void AddTranslation(Guid id, LanguageCode languageCode, string text, DateTime timestampUtc) =>
        Items.Add(new GrammarRuleSummaryTranslation(id, Id, languageCode, text, timestampUtc));
}

public sealed class GrammarExceptionNote : GrammarLocalizedTextOwner<GrammarExceptionNoteTranslation>
{
    private GrammarExceptionNote() { Text = string.Empty; }

    internal GrammarExceptionNote(Guid id, Guid grammarTopicId, int sortOrder, string text, DateTime timestampUtc)
        : base(id, grammarTopicId, sortOrder, text, 2000, timestampUtc)
    {
    }

    public override IReadOnlyCollection<GrammarExceptionNoteTranslation> Translations => Items;

    public void AddTranslation(Guid id, LanguageCode languageCode, string text, DateTime timestampUtc) =>
        Items.Add(new GrammarExceptionNoteTranslation(id, Id, languageCode, text, timestampUtc));
}

public abstract class GrammarLocalizedTextOwner<TTranslation>
{
    protected readonly List<TTranslation> Items = [];

    protected GrammarLocalizedTextOwner() { Text = string.Empty; }

    protected GrammarLocalizedTextOwner(Guid id, Guid grammarTopicId, int sortOrder, string text, int maxLength, DateTime timestampUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Grammar text item id is required.") : id;
        GrammarTopicId = grammarTopicId;
        SortOrder = Math.Max(0, sortOrder);
        Text = GrammarTopic.RequireText(text, "Grammar text", maxLength);
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; protected set; }
    public Guid GrammarTopicId { get; protected set; }
    public int SortOrder { get; protected set; }
    public string Text { get; protected set; }
    public DateTime CreatedAtUtc { get; protected set; }
    public DateTime UpdatedAtUtc { get; protected set; }
    public abstract IReadOnlyCollection<TTranslation> Translations { get; }
}

public abstract class GrammarTranslationBase
{
    protected GrammarTranslationBase() { Text = string.Empty; LanguageCode = LanguageCode.From("en"); }

    protected GrammarTranslationBase(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime timestampUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Grammar translation id is required.") : id;
        OwnerId = ownerId;
        LanguageCode = languageCode;
        Text = GrammarTopic.RequireText(text, "Grammar translation text", 12000);
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

public sealed class GrammarSectionTranslation : GrammarTranslationBase
{
    private GrammarSectionTranslation() { Heading = string.Empty; }

    internal GrammarSectionTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string heading, string text, DateTime timestampUtc)
        : base(id, ownerId, languageCode, text, timestampUtc)
    {
        Heading = GrammarTopic.RequireText(heading, "Grammar section translation heading", 256);
    }

    public string Heading { get; private set; }
}

public sealed class GrammarExampleTranslation : GrammarTranslationBase
{
    private GrammarExampleTranslation() { }
    internal GrammarExampleTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime timestampUtc)
        : base(id, ownerId, languageCode, text, timestampUtc) { }
}

public sealed class GrammarCommonMistakeTranslation : GrammarTranslationBase
{
    private GrammarCommonMistakeTranslation() { }
    internal GrammarCommonMistakeTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime timestampUtc)
        : base(id, ownerId, languageCode, text, timestampUtc) { }
}

public sealed class GrammarRuleSummaryTranslation : GrammarTranslationBase
{
    private GrammarRuleSummaryTranslation() { }
    internal GrammarRuleSummaryTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime timestampUtc)
        : base(id, ownerId, languageCode, text, timestampUtc) { }
}

public sealed class GrammarExceptionNoteTranslation : GrammarTranslationBase
{
    private GrammarExceptionNoteTranslation() { }
    internal GrammarExceptionNoteTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime timestampUtc)
        : base(id, ownerId, languageCode, text, timestampUtc) { }
}

public abstract class GrammarSlugLink
{
    protected GrammarSlugLink() { TargetSlug = string.Empty; }

    protected GrammarSlugLink(Guid id, Guid grammarTopicId, string targetSlug, int sortOrder, DateTime timestampUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Grammar link id is required.") : id;
        GrammarTopicId = grammarTopicId;
        TargetSlug = targetSlug;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = timestampUtc;
    }

    public Guid Id { get; protected set; }
    public Guid GrammarTopicId { get; protected set; }
    public string TargetSlug { get; protected set; }
    public int SortOrder { get; protected set; }
    public DateTime CreatedAtUtc { get; protected set; }
}

public sealed class GrammarPrerequisiteLink : GrammarSlugLink
{
    private GrammarPrerequisiteLink() { }
    internal GrammarPrerequisiteLink(Guid id, Guid grammarTopicId, string targetSlug, int sortOrder, DateTime timestampUtc)
        : base(id, grammarTopicId, targetSlug, sortOrder, timestampUtc) { }
}

public sealed class GrammarRelatedTopicLink : GrammarSlugLink
{
    private GrammarRelatedTopicLink() { }
    internal GrammarRelatedTopicLink(Guid id, Guid grammarTopicId, string targetSlug, int sortOrder, DateTime timestampUtc)
        : base(id, grammarTopicId, targetSlug, sortOrder, timestampUtc) { }
}

public sealed class GrammarLinkedDialogue : GrammarSlugLink
{
    private GrammarLinkedDialogue() { }
    internal GrammarLinkedDialogue(Guid id, Guid grammarTopicId, string targetSlug, int sortOrder, DateTime timestampUtc)
        : base(id, grammarTopicId, targetSlug, sortOrder, timestampUtc) { }
}

public sealed class GrammarLinkedTalkTopic : GrammarSlugLink
{
    private GrammarLinkedTalkTopic() { }
    internal GrammarLinkedTalkTopic(Guid id, Guid grammarTopicId, string targetSlug, int sortOrder, DateTime timestampUtc)
        : base(id, grammarTopicId, targetSlug, sortOrder, timestampUtc) { }
}

public sealed class GrammarLinkedExercise : GrammarSlugLink
{
    private GrammarLinkedExercise() { }
    internal GrammarLinkedExercise(Guid id, Guid grammarTopicId, string targetSlug, int sortOrder, DateTime timestampUtc)
        : base(id, grammarTopicId, targetSlug, sortOrder, timestampUtc) { }
}

public sealed class GrammarLinkedWord
{
    private GrammarLinkedWord() { Lemma = string.Empty; }

    internal GrammarLinkedWord(Guid id, Guid grammarTopicId, string lemma, string? wordSlug, int sortOrder, DateTime timestampUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Grammar linked word id is required.") : id;
        GrammarTopicId = grammarTopicId;
        Lemma = lemma;
        WordSlug = wordSlug;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public Guid GrammarTopicId { get; private set; }
    public string Lemma { get; private set; }
    public string? WordSlug { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}
