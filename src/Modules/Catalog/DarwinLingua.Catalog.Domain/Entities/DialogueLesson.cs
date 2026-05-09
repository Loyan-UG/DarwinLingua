using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed partial class DialogueLesson
{
    private readonly List<DialogueLessonTopic> _topics = [];
    private readonly List<DialogueTurn> _dialogueTurns = [];
    private readonly List<DialoguePhrase> _usefulPhrases = [];
    private readonly List<DialogueQuestion> _questions = [];

    private DialogueLesson()
    {
    }

    public DialogueLesson(
        Guid id,
        string slug,
        string title,
        string description,
        string learnerGoal,
        CefrLevel cefrLevel,
        string category,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Dialogue lesson identifier cannot be empty.");
        }

        Id = id;
        Slug = NormalizeKey(slug, "Dialogue lesson slug");
        Title = NormalizeRequiredText(title, nameof(title), 256);
        Description = NormalizeRequiredText(description, nameof(description), 4000);
        LearnerGoal = NormalizeRequiredText(learnerGoal, nameof(learnerGoal), 1024);
        CefrLevel = cefrLevel;
        Category = NormalizeKey(category, "Dialogue lesson category");
        PublicationStatus = publicationStatus;
        SortOrder = NormalizeSortOrder(sortOrder);
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string Slug { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string LearnerGoal { get; private set; } = string.Empty;

    public CefrLevel CefrLevel { get; private set; }

    public string Category { get; private set; } = string.Empty;

    public PublicationStatus PublicationStatus { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<DialogueLessonTopic> Topics => _topics.AsReadOnly();

    public IReadOnlyCollection<DialogueTurn> DialogueTurns => _dialogueTurns.AsReadOnly();

    public IReadOnlyCollection<DialoguePhrase> UsefulPhrases => _usefulPhrases.AsReadOnly();

    public IReadOnlyCollection<DialogueQuestion> Questions => _questions.AsReadOnly();

    public void UpdateMetadata(
        string title,
        string description,
        string learnerGoal,
        CefrLevel cefrLevel,
        string category,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime updatedAtUtc)
    {
        Title = NormalizeRequiredText(title, nameof(title), 256);
        Description = NormalizeRequiredText(description, nameof(description), 4000);
        LearnerGoal = NormalizeRequiredText(learnerGoal, nameof(learnerGoal), 1024);
        CefrLevel = cefrLevel;
        Category = NormalizeKey(category, "Dialogue lesson category");
        PublicationStatus = publicationStatus;
        SortOrder = NormalizeSortOrder(sortOrder);
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public void AddTopic(Guid id, Guid topicId, bool isPrimary, DateTime createdAtUtc)
    {
        _topics.Add(new DialogueLessonTopic(id, Id, topicId, isPrimary, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public DialogueTurn AddDialogueTurn(Guid id, int sortOrder, string speakerRole, string baseText, DateTime createdAtUtc)
    {
        DialogueTurn turn = new(id, Id, sortOrder, speakerRole, baseText, createdAtUtc);
        _dialogueTurns.Add(turn);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        return turn;
    }

    public DialoguePhrase AddUsefulPhrase(Guid id, int sortOrder, string baseText, string? usageNote, DateTime createdAtUtc)
    {
        DialoguePhrase phrase = new(id, Id, sortOrder, baseText, usageNote, createdAtUtc);
        _usefulPhrases.Add(phrase);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        return phrase;
    }

    public DialogueQuestion AddQuestion(Guid id, int sortOrder, string prompt, DateTime createdAtUtc)
    {
        DialogueQuestion question = new(id, Id, sortOrder, prompt, createdAtUtc);
        _questions.Add(question);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        return question;
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
            throw new DomainRuleException("Dialogue sort order cannot be negative.");
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

public sealed class DialogueLessonTopic
{
    private DialogueLessonTopic()
    {
    }

    internal DialogueLessonTopic(Guid id, Guid dialogueLessonId, Guid topicId, bool isPrimary, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Dialogue topic identifier cannot be empty.") : id;
        DialogueLessonId = dialogueLessonId == Guid.Empty ? throw new DomainRuleException("Dialogue topic lesson identifier cannot be empty.") : dialogueLessonId;
        TopicId = topicId == Guid.Empty ? throw new DomainRuleException("Dialogue topic identifier cannot be empty.") : topicId;
        IsPrimary = isPrimary;
        CreatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid DialogueLessonId { get; private set; }

    public Guid TopicId { get; private set; }

    public bool IsPrimary { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public void ReassignTopic(Guid topicId)
    {
        TopicId = topicId == Guid.Empty ? throw new DomainRuleException("Dialogue topic identifier cannot be empty.") : topicId;
    }
}

public sealed class DialogueTurn
{
    private readonly List<DialogueTurnTranslation> _translations = [];

    private DialogueTurn()
    {
    }

    internal DialogueTurn(Guid id, Guid dialogueLessonId, int sortOrder, string speakerRole, string baseText, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Dialogue dialogue turn identifier cannot be empty.") : id;
        DialogueLessonId = dialogueLessonId == Guid.Empty ? throw new DomainRuleException("Dialogue dialogue turn lesson identifier cannot be empty.") : dialogueLessonId;
        SortOrder = DialogueLesson.NormalizeSortOrder(sortOrder);
        SpeakerRole = DialogueLesson.NormalizeKey(speakerRole, "Dialogue speaker role");
        BaseText = DialogueLesson.NormalizeRequiredText(baseText, nameof(baseText), 2000);
        CreatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid DialogueLessonId { get; private set; }

    public int SortOrder { get; private set; }

    public string SpeakerRole { get; private set; } = string.Empty;

    public string BaseText { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<DialogueTurnTranslation> Translations => _translations.AsReadOnly();

    public void AddTranslation(Guid id, LanguageCode languageCode, string text, DateTime createdAtUtc)
    {
        _translations.Add(new DialogueTurnTranslation(id, Id, languageCode, text, createdAtUtc));
        UpdatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }
}

public sealed class DialoguePhrase
{
    private readonly List<DialoguePhraseTranslation> _translations = [];

    private DialoguePhrase()
    {
    }

    internal DialoguePhrase(Guid id, Guid dialogueLessonId, int sortOrder, string baseText, string? usageNote, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Dialogue phrase identifier cannot be empty.") : id;
        DialogueLessonId = dialogueLessonId == Guid.Empty ? throw new DomainRuleException("Dialogue phrase lesson identifier cannot be empty.") : dialogueLessonId;
        SortOrder = DialogueLesson.NormalizeSortOrder(sortOrder);
        BaseText = DialogueLesson.NormalizeRequiredText(baseText, nameof(baseText), 1024);
        UsageNote = DialogueLesson.NormalizeOptionalText(usageNote, nameof(usageNote), 1024);
        CreatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid DialogueLessonId { get; private set; }

    public int SortOrder { get; private set; }

    public string BaseText { get; private set; } = string.Empty;

    public string? UsageNote { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<DialoguePhraseTranslation> Translations => _translations.AsReadOnly();

    public void AddTranslation(Guid id, LanguageCode languageCode, string text, DateTime createdAtUtc)
    {
        _translations.Add(new DialoguePhraseTranslation(id, Id, languageCode, text, createdAtUtc));
        UpdatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }
}

public sealed class DialogueQuestion
{
    private readonly List<DialogueQuestionTranslation> _translations = [];
    private readonly List<DialogueAnswer> _answers = [];

    private DialogueQuestion()
    {
    }

    internal DialogueQuestion(Guid id, Guid dialogueLessonId, int sortOrder, string prompt, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Dialogue question identifier cannot be empty.") : id;
        DialogueLessonId = dialogueLessonId == Guid.Empty ? throw new DomainRuleException("Dialogue question lesson identifier cannot be empty.") : dialogueLessonId;
        SortOrder = DialogueLesson.NormalizeSortOrder(sortOrder);
        Prompt = DialogueLesson.NormalizeRequiredText(prompt, nameof(prompt), 1024);
        CreatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid DialogueLessonId { get; private set; }

    public int SortOrder { get; private set; }

    public string Prompt { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<DialogueQuestionTranslation> Translations => _translations.AsReadOnly();

    public IReadOnlyCollection<DialogueAnswer> Answers => _answers.AsReadOnly();

    public void AddTranslation(Guid id, LanguageCode languageCode, string text, DateTime createdAtUtc)
    {
        _translations.Add(new DialogueQuestionTranslation(id, Id, languageCode, text, createdAtUtc));
        UpdatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public DialogueAnswer AddAnswer(Guid id, int sortOrder, string text, bool isCorrect, string? feedback, DateTime createdAtUtc)
    {
        DialogueAnswer answer = new(id, Id, sortOrder, text, isCorrect, feedback, createdAtUtc);
        _answers.Add(answer);
        UpdatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        return answer;
    }
}

public sealed class DialogueAnswer
{
    private readonly List<DialogueAnswerTranslation> _translations = [];

    private DialogueAnswer()
    {
    }

    internal DialogueAnswer(Guid id, Guid dialogueQuestionId, int sortOrder, string text, bool isCorrect, string? feedback, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Dialogue answer identifier cannot be empty.") : id;
        DialogueQuestionId = dialogueQuestionId == Guid.Empty ? throw new DomainRuleException("Dialogue answer question identifier cannot be empty.") : dialogueQuestionId;
        SortOrder = DialogueLesson.NormalizeSortOrder(sortOrder);
        Text = DialogueLesson.NormalizeRequiredText(text, nameof(text), 1024);
        IsCorrect = isCorrect;
        Feedback = DialogueLesson.NormalizeOptionalText(feedback, nameof(feedback), 1024);
        CreatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid DialogueQuestionId { get; private set; }

    public int SortOrder { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public bool IsCorrect { get; private set; }

    public string? Feedback { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<DialogueAnswerTranslation> Translations => _translations.AsReadOnly();

    public void AddTranslation(Guid id, LanguageCode languageCode, string text, DateTime createdAtUtc)
    {
        _translations.Add(new DialogueAnswerTranslation(id, Id, languageCode, text, createdAtUtc));
        UpdatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }
}

public sealed class DialogueTurnTranslation : DialogueTranslationBase
{
    private DialogueTurnTranslation()
    {
    }

    internal DialogueTurnTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime createdAtUtc)
        : base(id, ownerId, languageCode, text, createdAtUtc)
    {
    }

    public Guid DialogueTurnId => OwnerId;
}

public sealed class DialoguePhraseTranslation : DialogueTranslationBase
{
    private DialoguePhraseTranslation()
    {
    }

    internal DialoguePhraseTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime createdAtUtc)
        : base(id, ownerId, languageCode, text, createdAtUtc)
    {
    }

    public Guid DialoguePhraseId => OwnerId;
}

public sealed class DialogueQuestionTranslation : DialogueTranslationBase
{
    private DialogueQuestionTranslation()
    {
    }

    internal DialogueQuestionTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime createdAtUtc)
        : base(id, ownerId, languageCode, text, createdAtUtc)
    {
    }

    public Guid DialogueQuestionId => OwnerId;
}

public sealed class DialogueAnswerTranslation : DialogueTranslationBase
{
    private DialogueAnswerTranslation()
    {
    }

    internal DialogueAnswerTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime createdAtUtc)
        : base(id, ownerId, languageCode, text, createdAtUtc)
    {
    }

    public Guid DialogueAnswerId => OwnerId;
}

public abstract class DialogueTranslationBase
{
    private protected DialogueTranslationBase()
    {
    }

    private protected DialogueTranslationBase(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Dialogue translation identifier cannot be empty.") : id;
        OwnerId = ownerId == Guid.Empty ? throw new DomainRuleException("Dialogue translation owner identifier cannot be empty.") : ownerId;
        LanguageCode = languageCode;
        Text = DialogueLesson.NormalizeRequiredText(text, nameof(text), 2000);
        CreatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid OwnerId { get; private set; }

    public LanguageCode LanguageCode { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }
}
