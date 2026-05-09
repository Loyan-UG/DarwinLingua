using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed partial class EventPreparationPack
{
    private readonly List<EventPreparationPackTopic> _topics = [];
    private readonly List<EventPreparationLinkedDialogue> _linkedDialogues = [];
    private readonly List<EventPreparationLinkedConversationStarterPack> _linkedConversationStarterPacks = [];
    private readonly List<EventPreparationVocabularyReference> _linkedVocabulary = [];
    private readonly List<EventPreparationPrompt> _prompts = [];

    private EventPreparationPack()
    {
    }

    public EventPreparationPack(
        Guid id,
        string slug,
        string title,
        string description,
        CefrLevel cefrLevel,
        string category,
        string eventType,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Event preparation pack identifier cannot be empty.");
        }

        Id = id;
        Slug = NormalizeKey(slug, "Event preparation pack slug");
        Title = NormalizeRequiredText(title, nameof(title), 256);
        Description = NormalizeRequiredText(description, nameof(description), 4000);
        CefrLevel = cefrLevel;
        Category = NormalizeKey(category, "Event preparation pack category");
        EventType = NormalizeKey(eventType, "Event preparation pack event type");
        PublicationStatus = publicationStatus;
        SortOrder = NormalizeSortOrder(sortOrder);
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string Slug { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public CefrLevel CefrLevel { get; private set; }

    public string Category { get; private set; } = string.Empty;

    public string EventType { get; private set; } = string.Empty;

    public PublicationStatus PublicationStatus { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<EventPreparationPackTopic> Topics => _topics.AsReadOnly();

    public IReadOnlyCollection<EventPreparationLinkedDialogue> LinkedDialogues => _linkedDialogues.AsReadOnly();

    public IReadOnlyCollection<EventPreparationLinkedConversationStarterPack> LinkedConversationStarterPacks => _linkedConversationStarterPacks.AsReadOnly();

    public IReadOnlyCollection<EventPreparationVocabularyReference> LinkedVocabulary => _linkedVocabulary.AsReadOnly();

    public IReadOnlyCollection<EventPreparationPrompt> Prompts => _prompts.AsReadOnly();

    public void AddTopic(Guid id, Guid topicId, bool isPrimary, DateTime createdAtUtc)
    {
        _topics.Add(new EventPreparationPackTopic(id, Id, topicId, isPrimary, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void AddLinkedDialogue(Guid id, string dialogueSlug, int sortOrder, DateTime createdAtUtc)
    {
        _linkedDialogues.Add(new EventPreparationLinkedDialogue(id, Id, dialogueSlug, sortOrder, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void AddLinkedConversationStarterPack(Guid id, string conversationStarterPackSlug, int sortOrder, DateTime createdAtUtc)
    {
        _linkedConversationStarterPacks.Add(new EventPreparationLinkedConversationStarterPack(id, Id, conversationStarterPackSlug, sortOrder, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void AddLinkedVocabulary(Guid id, string word, PartOfSpeech? partOfSpeech, CefrLevel? cefrLevel, int sortOrder, DateTime createdAtUtc)
    {
        _linkedVocabulary.Add(new EventPreparationVocabularyReference(id, Id, word, partOfSpeech, cefrLevel, sortOrder, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void AddPrompt(Guid id, string promptType, int sortOrder, string text, DateTime createdAtUtc)
    {
        _prompts.Add(new EventPreparationPrompt(id, Id, promptType, sortOrder, text, createdAtUtc));
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
            throw new DomainRuleException("Event preparation sort order cannot be negative.");
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

public sealed class EventPreparationPackTopic
{
    private EventPreparationPackTopic()
    {
    }

    internal EventPreparationPackTopic(Guid id, Guid eventPreparationPackId, Guid topicId, bool isPrimary, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Event preparation topic identifier cannot be empty.") : id;
        EventPreparationPackId = eventPreparationPackId == Guid.Empty ? throw new DomainRuleException("Event preparation topic pack identifier cannot be empty.") : eventPreparationPackId;
        TopicId = topicId == Guid.Empty ? throw new DomainRuleException("Event preparation topic identifier cannot be empty.") : topicId;
        IsPrimary = isPrimary;
        CreatedAtUtc = EventPreparationPack.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid EventPreparationPackId { get; private set; }

    public Guid TopicId { get; private set; }

    public bool IsPrimary { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public void ReassignTopic(Guid topicId)
    {
        TopicId = topicId == Guid.Empty ? throw new DomainRuleException("Event preparation topic identifier cannot be empty.") : topicId;
    }
}

public sealed class EventPreparationLinkedDialogue
{
    private EventPreparationLinkedDialogue()
    {
    }

    internal EventPreparationLinkedDialogue(Guid id, Guid eventPreparationPackId, string dialogueSlug, int sortOrder, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Event preparation linked dialogue identifier cannot be empty.") : id;
        EventPreparationPackId = eventPreparationPackId == Guid.Empty ? throw new DomainRuleException("Event preparation linked dialogue pack identifier cannot be empty.") : eventPreparationPackId;
        DialogueSlug = EventPreparationPack.NormalizeKey(dialogueSlug, "Event preparation linked dialogue slug");
        SortOrder = EventPreparationPack.NormalizeSortOrder(sortOrder);
        CreatedAtUtc = EventPreparationPack.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid EventPreparationPackId { get; private set; }

    public string DialogueSlug { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class EventPreparationLinkedConversationStarterPack
{
    private EventPreparationLinkedConversationStarterPack()
    {
    }

    internal EventPreparationLinkedConversationStarterPack(Guid id, Guid eventPreparationPackId, string conversationStarterPackSlug, int sortOrder, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Event preparation linked starter identifier cannot be empty.") : id;
        EventPreparationPackId = eventPreparationPackId == Guid.Empty ? throw new DomainRuleException("Event preparation linked starter pack identifier cannot be empty.") : eventPreparationPackId;
        ConversationStarterPackSlug = EventPreparationPack.NormalizeKey(conversationStarterPackSlug, "Event preparation linked starter slug");
        SortOrder = EventPreparationPack.NormalizeSortOrder(sortOrder);
        CreatedAtUtc = EventPreparationPack.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid EventPreparationPackId { get; private set; }

    public string ConversationStarterPackSlug { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class EventPreparationVocabularyReference
{
    private EventPreparationVocabularyReference()
    {
    }

    internal EventPreparationVocabularyReference(Guid id, Guid eventPreparationPackId, string word, PartOfSpeech? partOfSpeech, CefrLevel? cefrLevel, int sortOrder, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Event preparation vocabulary reference identifier cannot be empty.") : id;
        EventPreparationPackId = eventPreparationPackId == Guid.Empty ? throw new DomainRuleException("Event preparation vocabulary pack identifier cannot be empty.") : eventPreparationPackId;
        Word = EventPreparationPack.NormalizeRequiredText(word, nameof(word), 256);
        PartOfSpeech = partOfSpeech;
        CefrLevel = cefrLevel;
        SortOrder = EventPreparationPack.NormalizeSortOrder(sortOrder);
        CreatedAtUtc = EventPreparationPack.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid EventPreparationPackId { get; private set; }

    public string Word { get; private set; } = string.Empty;

    public PartOfSpeech? PartOfSpeech { get; private set; }

    public CefrLevel? CefrLevel { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class EventPreparationPrompt
{
    private EventPreparationPrompt()
    {
    }

    internal EventPreparationPrompt(Guid id, Guid eventPreparationPackId, string promptType, int sortOrder, string text, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Event preparation prompt identifier cannot be empty.") : id;
        EventPreparationPackId = eventPreparationPackId == Guid.Empty ? throw new DomainRuleException("Event preparation prompt pack identifier cannot be empty.") : eventPreparationPackId;
        PromptType = EventPreparationPack.NormalizeKey(promptType, "Event preparation prompt type");
        SortOrder = EventPreparationPack.NormalizeSortOrder(sortOrder);
        Text = EventPreparationPack.NormalizeRequiredText(text, nameof(text), 2000);
        CreatedAtUtc = EventPreparationPack.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid EventPreparationPackId { get; private set; }

    public string PromptType { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }
}
