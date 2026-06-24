using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed partial class ConversationStarterPack
{
    private readonly List<ConversationStarterPackTopic> _topics = [];
    private readonly List<ConversationStarterLinkedDialogue> _linkedDialogues = [];
    private readonly List<ConversationStarterLinkedEventPreparationPack> _linkedEventPreparationPacks = [];
    private readonly List<ConversationStarterPhrase> _phrases = [];

    private ConversationStarterPack()
    {
    }

    public ConversationStarterPack(
        Guid id,
        string slug,
        string title,
        string description,
        CefrLevel cefrLevel,
        string category,
        string situation,
        string tone,
        string conversationGoal,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime createdAtUtc,
        string? targetLearningLanguageCode = null)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Conversation starter pack identifier cannot be empty.");
        }

        Id = id;
        TargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode);
        Slug = NormalizeKey(slug, "Conversation starter pack slug");
        Title = NormalizeRequiredText(title, nameof(title), 256);
        Description = NormalizeRequiredText(description, nameof(description), 4000);
        CefrLevel = cefrLevel;
        Category = NormalizeKey(category, "Conversation starter category");
        Situation = NormalizeKey(situation, "Conversation starter situation");
        Tone = NormalizeKey(tone, "Conversation starter tone");
        ConversationGoal = NormalizeKey(conversationGoal, "Conversation starter goal");
        PublicationStatus = publicationStatus;
        SortOrder = NormalizeSortOrder(sortOrder);
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string TargetLearningLanguageCode { get; private set; } = ContentLanguageRequirements.DefaultTargetLearningLanguageCode;

    public string Slug { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public CefrLevel CefrLevel { get; private set; }

    public string Category { get; private set; } = string.Empty;

    public string Situation { get; private set; } = string.Empty;

    public string Tone { get; private set; } = string.Empty;

    public string ConversationGoal { get; private set; } = string.Empty;

    public PublicationStatus PublicationStatus { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<ConversationStarterPackTopic> Topics => _topics.AsReadOnly();

    public IReadOnlyCollection<ConversationStarterLinkedDialogue> LinkedDialogues => _linkedDialogues.AsReadOnly();

    public IReadOnlyCollection<ConversationStarterLinkedEventPreparationPack> LinkedEventPreparationPacks => _linkedEventPreparationPacks.AsReadOnly();

    public IReadOnlyCollection<ConversationStarterPhrase> Phrases => _phrases.AsReadOnly();

    public void AddTopic(Guid id, Guid topicId, bool isPrimary, DateTime createdAtUtc)
    {
        _topics.Add(new ConversationStarterPackTopic(id, Id, topicId, isPrimary, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void AddLinkedDialogue(Guid id, string dialogueSlug, int sortOrder, DateTime createdAtUtc)
    {
        _linkedDialogues.Add(new ConversationStarterLinkedDialogue(id, Id, dialogueSlug, sortOrder, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void AddLinkedEventPreparationPack(Guid id, string eventPreparationPackSlug, int sortOrder, DateTime createdAtUtc)
    {
        _linkedEventPreparationPacks.Add(new ConversationStarterLinkedEventPreparationPack(id, Id, eventPreparationPackSlug, sortOrder, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public ConversationStarterPhrase AddPhrase(
        Guid id,
        int sortOrder,
        string baseText,
        string function,
        string? usageNote,
        string? register,
        string? commonMistake,
        DateTime createdAtUtc)
    {
        ConversationStarterPhrase phrase = new(id, Id, sortOrder, baseText, function, usageNote, register, commonMistake, createdAtUtc);
        _phrases.Add(phrase);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        return phrase;
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
            throw new DomainRuleException("Conversation starter sort order cannot be negative.");
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

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled)]
    private static partial Regex KeyPattern();
}

public sealed class ConversationStarterPackTopic
{
    private ConversationStarterPackTopic()
    {
    }

    internal ConversationStarterPackTopic(Guid id, Guid conversationStarterPackId, Guid topicId, bool isPrimary, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Conversation starter topic identifier cannot be empty.") : id;
        ConversationStarterPackId = conversationStarterPackId == Guid.Empty ? throw new DomainRuleException("Conversation starter topic pack identifier cannot be empty.") : conversationStarterPackId;
        TopicId = topicId == Guid.Empty ? throw new DomainRuleException("Conversation starter topic identifier cannot be empty.") : topicId;
        IsPrimary = isPrimary;
        CreatedAtUtc = ConversationStarterPack.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid ConversationStarterPackId { get; private set; }

    public Guid TopicId { get; private set; }

    public bool IsPrimary { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public void ReassignTopic(Guid topicId)
    {
        TopicId = topicId == Guid.Empty ? throw new DomainRuleException("Conversation starter topic identifier cannot be empty.") : topicId;
    }
}

public sealed class ConversationStarterLinkedDialogue
{
    private ConversationStarterLinkedDialogue()
    {
    }

    internal ConversationStarterLinkedDialogue(Guid id, Guid conversationStarterPackId, string dialogueSlug, int sortOrder, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Conversation starter linked dialogue identifier cannot be empty.") : id;
        ConversationStarterPackId = conversationStarterPackId == Guid.Empty ? throw new DomainRuleException("Conversation starter linked dialogue pack identifier cannot be empty.") : conversationStarterPackId;
        DialogueSlug = ConversationStarterPack.NormalizeKey(dialogueSlug, "Conversation starter linked dialogue slug");
        SortOrder = ConversationStarterPack.NormalizeSortOrder(sortOrder);
        CreatedAtUtc = ConversationStarterPack.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid ConversationStarterPackId { get; private set; }

    public string DialogueSlug { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class ConversationStarterLinkedEventPreparationPack
{
    private ConversationStarterLinkedEventPreparationPack()
    {
    }

    internal ConversationStarterLinkedEventPreparationPack(Guid id, Guid conversationStarterPackId, string eventPreparationPackSlug, int sortOrder, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Conversation starter linked event pack identifier cannot be empty.") : id;
        ConversationStarterPackId = conversationStarterPackId == Guid.Empty ? throw new DomainRuleException("Conversation starter linked event pack owner identifier cannot be empty.") : conversationStarterPackId;
        EventPreparationPackSlug = ConversationStarterPack.NormalizeKey(eventPreparationPackSlug, "Conversation starter linked event pack slug");
        SortOrder = ConversationStarterPack.NormalizeSortOrder(sortOrder);
        CreatedAtUtc = ConversationStarterPack.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid ConversationStarterPackId { get; private set; }

    public string EventPreparationPackSlug { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class ConversationStarterPhrase
{
    private readonly List<ConversationStarterPhraseTranslation> _translations = [];
    private readonly List<ConversationStarterPhraseAlternative> _alternativeBaseTexts = [];

    private ConversationStarterPhrase()
    {
    }

    internal ConversationStarterPhrase(
        Guid id,
        Guid conversationStarterPackId,
        int sortOrder,
        string baseText,
        string function,
        string? usageNote,
        string? register,
        string? commonMistake,
        DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Conversation starter phrase identifier cannot be empty.") : id;
        ConversationStarterPackId = conversationStarterPackId == Guid.Empty ? throw new DomainRuleException("Conversation starter phrase pack identifier cannot be empty.") : conversationStarterPackId;
        SortOrder = ConversationStarterPack.NormalizeSortOrder(sortOrder);
        BaseText = ConversationStarterPack.NormalizeRequiredText(baseText, nameof(baseText), 1024);
        Function = ConversationStarterPack.NormalizeKey(function, "Conversation starter phrase function");
        UsageNote = ConversationStarterPack.NormalizeOptionalText(usageNote, nameof(usageNote), 1024);
        Register = ConversationStarterPack.NormalizeOptionalText(register, nameof(register), 64);
        CommonMistake = ConversationStarterPack.NormalizeOptionalText(commonMistake, nameof(commonMistake), 1024);
        CreatedAtUtc = ConversationStarterPack.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ConversationStarterPackId { get; private set; }

    public int SortOrder { get; private set; }

    public string BaseText { get; private set; } = string.Empty;

    public string Function { get; private set; } = string.Empty;

    public string? UsageNote { get; private set; }

    public string? Register { get; private set; }

    public string? CommonMistake { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<ConversationStarterPhraseTranslation> Translations => _translations.AsReadOnly();

    public IReadOnlyCollection<ConversationStarterPhraseAlternative> AlternativeBaseTexts => _alternativeBaseTexts.AsReadOnly();

    public void AddTranslation(Guid id, LanguageCode languageCode, string text, DateTime createdAtUtc)
    {
        _translations.Add(new ConversationStarterPhraseTranslation(id, Id, languageCode, text, createdAtUtc));
        UpdatedAtUtc = ConversationStarterPack.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void AddAlternativeBaseText(Guid id, int sortOrder, string baseText, DateTime createdAtUtc)
    {
        _alternativeBaseTexts.Add(new ConversationStarterPhraseAlternative(id, Id, sortOrder, baseText, createdAtUtc));
        UpdatedAtUtc = ConversationStarterPack.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }
}

public sealed class ConversationStarterPhraseAlternative
{
    private ConversationStarterPhraseAlternative()
    {
    }

    internal ConversationStarterPhraseAlternative(Guid id, Guid conversationStarterPhraseId, int sortOrder, string baseText, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Conversation starter alternative identifier cannot be empty.") : id;
        ConversationStarterPhraseId = conversationStarterPhraseId == Guid.Empty ? throw new DomainRuleException("Conversation starter alternative phrase identifier cannot be empty.") : conversationStarterPhraseId;
        SortOrder = ConversationStarterPack.NormalizeSortOrder(sortOrder);
        BaseText = ConversationStarterPack.NormalizeRequiredText(baseText, nameof(baseText), 1024);
        CreatedAtUtc = ConversationStarterPack.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid ConversationStarterPhraseId { get; private set; }

    public int SortOrder { get; private set; }

    public string BaseText { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class ConversationStarterPhraseTranslation
{
    private ConversationStarterPhraseTranslation()
    {
    }

    internal ConversationStarterPhraseTranslation(Guid id, Guid conversationStarterPhraseId, LanguageCode languageCode, string text, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Conversation starter translation identifier cannot be empty.") : id;
        ConversationStarterPhraseId = conversationStarterPhraseId == Guid.Empty ? throw new DomainRuleException("Conversation starter translation phrase identifier cannot be empty.") : conversationStarterPhraseId;
        LanguageCode = languageCode;
        Text = ConversationStarterPack.NormalizeRequiredText(text, nameof(text), 2000);
        CreatedAtUtc = ConversationStarterPack.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ConversationStarterPhraseId { get; private set; }

    public LanguageCode LanguageCode { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }
}
