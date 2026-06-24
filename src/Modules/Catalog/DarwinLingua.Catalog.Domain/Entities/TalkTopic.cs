using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents a reading-based conversation topic for language cafés and speaking groups.
/// </summary>
public sealed partial class TalkTopic
{
    private readonly List<TalkTopicTopic> _topics = [];
    private readonly List<TalkTopicArticleTranslation> _articleTranslations = [];
    private readonly List<TalkTopicQuestion> _questions = [];
    private readonly List<TalkTopicVocabularyItem> _vocabularyItems = [];
    private readonly List<TalkTopicSpeakingGoalLink> _speakingGoals = [];

    private TalkTopic()
    {
        TargetLearningLanguageCode = ContentLanguageRequirements.DefaultTargetLearningLanguageCode;
    }

    public TalkTopic(
        Guid id,
        string slug,
        string topicGroupKey,
        string title,
        string description,
        CefrLevel cefrLevel,
        string category,
        TalkTopicContentType contentType,
        string articleBaseText,
        int estimatedReadingMinutes,
        int estimatedDiscussionMinutes,
        bool isSensitive,
        string? sensitivityNote,
        bool recommendedForModeratedGroupsOnly,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime createdAtUtc,
        string? targetLearningLanguageCode = null)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Talk topic identifier cannot be empty.") : id;
        TargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode, "Talk topic target learning language");
        Slug = NormalizeKey(slug, "Talk topic slug");
        TopicGroupKey = NormalizeKey(topicGroupKey, "Talk topic group key");
        Title = NormalizeRequiredText(title, nameof(title), 256);
        Description = NormalizeRequiredText(description, nameof(description), 4000);
        CefrLevel = cefrLevel;
        Category = NormalizeKey(category, "Talk topic category");
        ContentType = contentType;
        ArticleBaseText = NormalizeRequiredText(articleBaseText, nameof(articleBaseText), 12000);
        EstimatedReadingMinutes = NormalizePositiveMinutes(estimatedReadingMinutes, nameof(estimatedReadingMinutes));
        EstimatedDiscussionMinutes = NormalizePositiveMinutes(estimatedDiscussionMinutes, nameof(estimatedDiscussionMinutes));
        IsSensitive = isSensitive;
        SensitivityNote = NormalizeOptionalText(sensitivityNote, nameof(sensitivityNote), 1024);
        RecommendedForModeratedGroupsOnly = recommendedForModeratedGroupsOnly;
        PublicationStatus = publicationStatus;
        SortOrder = NormalizeSortOrder(sortOrder);
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string TargetLearningLanguageCode { get; private set; }

    public string Slug { get; private set; } = string.Empty;

    public string TopicGroupKey { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public CefrLevel CefrLevel { get; private set; }

    public string Category { get; private set; } = string.Empty;

    public TalkTopicContentType ContentType { get; private set; }

    public string ArticleBaseText { get; private set; } = string.Empty;

    public int EstimatedReadingMinutes { get; private set; }

    public int EstimatedDiscussionMinutes { get; private set; }

    public bool IsSensitive { get; private set; }

    public string? SensitivityNote { get; private set; }

    public bool RecommendedForModeratedGroupsOnly { get; private set; }

    public PublicationStatus PublicationStatus { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<TalkTopicTopic> Topics => _topics.AsReadOnly();

    public IReadOnlyCollection<TalkTopicArticleTranslation> ArticleTranslations => _articleTranslations.AsReadOnly();

    public IReadOnlyCollection<TalkTopicQuestion> Questions => _questions.AsReadOnly();

    public IReadOnlyCollection<TalkTopicQuestion> WarmupQuestions => _questions
        .Where(question => question.Kind == TalkTopicQuestionKind.Warmup)
        .ToArray();

    public IReadOnlyCollection<TalkTopicQuestion> DiscussionQuestions => _questions
        .Where(question => question.Kind == TalkTopicQuestionKind.Discussion)
        .ToArray();

    public IReadOnlyCollection<TalkTopicVocabularyItem> VocabularyItems => _vocabularyItems.AsReadOnly();

    public IReadOnlyCollection<TalkTopicSpeakingGoalLink> SpeakingGoals => _speakingGoals.AsReadOnly();

    public void AddTopic(Guid id, Guid topicId, bool isPrimary, DateTime createdAtUtc)
    {
        _topics.Add(new TalkTopicTopic(id, Id, topicId, isPrimary, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void AddArticleTranslation(Guid id, LanguageCode languageCode, string text, DateTime createdAtUtc)
    {
        _articleTranslations.Add(new TalkTopicArticleTranslation(id, Id, languageCode, text, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public TalkTopicQuestion AddWarmupQuestion(Guid id, int sortOrder, string prompt, DateTime createdAtUtc)
    {
        TalkTopicQuestion question = new(id, Id, TalkTopicQuestionKind.Warmup, null, sortOrder, prompt, createdAtUtc);
        _questions.Add(question);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        return question;
    }

    public TalkTopicQuestion AddDiscussionQuestion(Guid id, TalkTopicQuestionType questionType, int sortOrder, string prompt, DateTime createdAtUtc)
    {
        TalkTopicQuestion question = new(id, Id, TalkTopicQuestionKind.Discussion, questionType, sortOrder, prompt, createdAtUtc);
        _questions.Add(question);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        return question;
    }

    public void AddVocabularyItem(Guid id, string lemma, string? wordSlug, CefrLevel? cefrLevel, int sortOrder, DateTime createdAtUtc)
    {
        _vocabularyItems.Add(new TalkTopicVocabularyItem(id, Id, lemma, wordSlug, cefrLevel, sortOrder, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void AddSpeakingGoal(Guid id, TalkTopicSpeakingGoal speakingGoal, int sortOrder, DateTime createdAtUtc)
    {
        _speakingGoals.Add(new TalkTopicSpeakingGoalLink(id, Id, speakingGoal, sortOrder, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    internal static string NormalizeRequiredText(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException($"{parameterName} cannot be empty.");
        }

        string normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainRuleException($"{parameterName} cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    internal static string? NormalizeOptionalText(string? value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainRuleException($"{parameterName} cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    internal static string NormalizeKey(string value, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException($"{label} cannot be empty.");
        }

        string normalized = value.Trim().ToLowerInvariant();
        if (!KeyPattern().IsMatch(normalized))
        {
            throw new DomainRuleException($"{label} must use lowercase kebab-case characters only.");
        }

        return normalized;
    }

    internal static int NormalizeSortOrder(int value)
    {
        if (value < 0)
        {
            throw new DomainRuleException("Talk topic sort order cannot be negative.");
        }

        return value;
    }

    internal static DateTime NormalizeUtc(DateTime value, string parameterName)
    {
        if (value == default)
        {
            throw new DomainRuleException($"{parameterName} cannot be empty.");
        }

        return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }

    private static int NormalizePositiveMinutes(int value, string parameterName)
    {
        if (value <= 0)
        {
            throw new DomainRuleException($"{parameterName} must be greater than zero.");
        }

        return value;
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled)]
    private static partial Regex KeyPattern();
}

public sealed class TalkTopicTopic
{
    private TalkTopicTopic()
    {
    }

    internal TalkTopicTopic(Guid id, Guid talkTopicId, Guid topicId, bool isPrimary, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Talk topic topic link identifier cannot be empty.") : id;
        TalkTopicId = talkTopicId == Guid.Empty ? throw new DomainRuleException("Talk topic topic link owner identifier cannot be empty.") : talkTopicId;
        TopicId = topicId == Guid.Empty ? throw new DomainRuleException("Talk topic topic identifier cannot be empty.") : topicId;
        IsPrimary = isPrimary;
        CreatedAtUtc = TalkTopic.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid TalkTopicId { get; private set; }

    public Guid TopicId { get; private set; }

    public bool IsPrimary { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class TalkTopicArticleTranslation : TalkTopicTranslationBase
{
    private TalkTopicArticleTranslation()
    {
    }

    internal TalkTopicArticleTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime createdAtUtc)
        : base(id, ownerId, languageCode, text, 12000, createdAtUtc)
    {
    }

    public Guid TalkTopicId => OwnerId;
}

public sealed class TalkTopicQuestion
{
    private readonly List<TalkTopicQuestionTranslation> _translations = [];

    private TalkTopicQuestion()
    {
    }

    internal TalkTopicQuestion(
        Guid id,
        Guid talkTopicId,
        TalkTopicQuestionKind kind,
        TalkTopicQuestionType? questionType,
        int sortOrder,
        string prompt,
        DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Talk topic question identifier cannot be empty.") : id;
        TalkTopicId = talkTopicId == Guid.Empty ? throw new DomainRuleException("Talk topic question owner identifier cannot be empty.") : talkTopicId;
        Kind = kind;
        QuestionType = questionType;
        SortOrder = TalkTopic.NormalizeSortOrder(sortOrder);
        Prompt = TalkTopic.NormalizeRequiredText(prompt, nameof(prompt), 1024);
        CreatedAtUtc = TalkTopic.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid TalkTopicId { get; private set; }

    public TalkTopicQuestionKind Kind { get; private set; }

    public TalkTopicQuestionType? QuestionType { get; private set; }

    public int SortOrder { get; private set; }

    public string Prompt { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<TalkTopicQuestionTranslation> Translations => _translations.AsReadOnly();

    public void AddTranslation(Guid id, LanguageCode languageCode, string text, DateTime createdAtUtc)
    {
        _translations.Add(new TalkTopicQuestionTranslation(id, Id, languageCode, text, createdAtUtc));
        UpdatedAtUtc = TalkTopic.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }
}

public sealed class TalkTopicQuestionTranslation : TalkTopicTranslationBase
{
    private TalkTopicQuestionTranslation()
    {
    }

    internal TalkTopicQuestionTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime createdAtUtc)
        : base(id, ownerId, languageCode, text, 2000, createdAtUtc)
    {
    }

    public Guid TalkTopicQuestionId => OwnerId;
}

public sealed class TalkTopicVocabularyItem
{
    private TalkTopicVocabularyItem()
    {
    }

    internal TalkTopicVocabularyItem(Guid id, Guid talkTopicId, string lemma, string? wordSlug, CefrLevel? cefrLevel, int sortOrder, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Talk topic vocabulary identifier cannot be empty.") : id;
        TalkTopicId = talkTopicId == Guid.Empty ? throw new DomainRuleException("Talk topic vocabulary owner identifier cannot be empty.") : talkTopicId;
        Lemma = TalkTopic.NormalizeRequiredText(lemma, nameof(lemma), 128);
        WordSlug = TalkTopic.NormalizeOptionalText(wordSlug, nameof(wordSlug), 128)?.ToLowerInvariant();
        CefrLevel = cefrLevel;
        SortOrder = TalkTopic.NormalizeSortOrder(sortOrder);
        CreatedAtUtc = TalkTopic.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid TalkTopicId { get; private set; }

    public string Lemma { get; private set; } = string.Empty;

    public string? WordSlug { get; private set; }

    public CefrLevel? CefrLevel { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class TalkTopicSpeakingGoalLink
{
    private TalkTopicSpeakingGoalLink()
    {
    }

    internal TalkTopicSpeakingGoalLink(Guid id, Guid talkTopicId, TalkTopicSpeakingGoal speakingGoal, int sortOrder, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Talk topic speaking goal identifier cannot be empty.") : id;
        TalkTopicId = talkTopicId == Guid.Empty ? throw new DomainRuleException("Talk topic speaking goal owner identifier cannot be empty.") : talkTopicId;
        SpeakingGoal = speakingGoal;
        SortOrder = TalkTopic.NormalizeSortOrder(sortOrder);
        CreatedAtUtc = TalkTopic.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid TalkTopicId { get; private set; }

    public TalkTopicSpeakingGoal SpeakingGoal { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

public abstract class TalkTopicTranslationBase
{
    private protected TalkTopicTranslationBase()
    {
    }

    private protected TalkTopicTranslationBase(Guid id, Guid ownerId, LanguageCode languageCode, string text, int maxLength, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Talk topic translation identifier cannot be empty.") : id;
        OwnerId = ownerId == Guid.Empty ? throw new DomainRuleException("Talk topic translation owner identifier cannot be empty.") : ownerId;
        LanguageCode = languageCode;
        Text = TalkTopic.NormalizeRequiredText(text, nameof(text), maxLength);
        CreatedAtUtc = TalkTopic.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid OwnerId { get; private set; }

    public LanguageCode LanguageCode { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }
}
