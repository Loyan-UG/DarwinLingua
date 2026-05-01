using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed partial class ScenarioLesson
{
    private readonly List<ScenarioLessonTopic> _topics = [];
    private readonly List<ScenarioDialogueTurn> _dialogueTurns = [];
    private readonly List<ScenarioPhrase> _usefulPhrases = [];
    private readonly List<ScenarioQuestion> _questions = [];

    private ScenarioLesson()
    {
    }

    public ScenarioLesson(
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
            throw new DomainRuleException("Scenario lesson identifier cannot be empty.");
        }

        Id = id;
        Slug = NormalizeKey(slug, "Scenario lesson slug");
        Title = NormalizeRequiredText(title, nameof(title), 256);
        Description = NormalizeRequiredText(description, nameof(description), 4000);
        LearnerGoal = NormalizeRequiredText(learnerGoal, nameof(learnerGoal), 1024);
        CefrLevel = cefrLevel;
        Category = NormalizeKey(category, "Scenario lesson category");
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

    public IReadOnlyCollection<ScenarioLessonTopic> Topics => _topics.AsReadOnly();

    public IReadOnlyCollection<ScenarioDialogueTurn> DialogueTurns => _dialogueTurns.AsReadOnly();

    public IReadOnlyCollection<ScenarioPhrase> UsefulPhrases => _usefulPhrases.AsReadOnly();

    public IReadOnlyCollection<ScenarioQuestion> Questions => _questions.AsReadOnly();

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
        Category = NormalizeKey(category, "Scenario lesson category");
        PublicationStatus = publicationStatus;
        SortOrder = NormalizeSortOrder(sortOrder);
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public void AddTopic(Guid id, Guid topicId, bool isPrimary, DateTime createdAtUtc)
    {
        _topics.Add(new ScenarioLessonTopic(id, Id, topicId, isPrimary, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public ScenarioDialogueTurn AddDialogueTurn(Guid id, int sortOrder, string speakerRole, string baseText, DateTime createdAtUtc)
    {
        ScenarioDialogueTurn turn = new(id, Id, sortOrder, speakerRole, baseText, createdAtUtc);
        _dialogueTurns.Add(turn);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        return turn;
    }

    public ScenarioPhrase AddUsefulPhrase(Guid id, int sortOrder, string baseText, string? usageNote, DateTime createdAtUtc)
    {
        ScenarioPhrase phrase = new(id, Id, sortOrder, baseText, usageNote, createdAtUtc);
        _usefulPhrases.Add(phrase);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        return phrase;
    }

    public ScenarioQuestion AddQuestion(Guid id, int sortOrder, string prompt, DateTime createdAtUtc)
    {
        ScenarioQuestion question = new(id, Id, sortOrder, prompt, createdAtUtc);
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
            throw new DomainRuleException("Scenario sort order cannot be negative.");
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

public sealed class ScenarioLessonTopic
{
    private ScenarioLessonTopic()
    {
    }

    internal ScenarioLessonTopic(Guid id, Guid scenarioLessonId, Guid topicId, bool isPrimary, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Scenario topic identifier cannot be empty.") : id;
        ScenarioLessonId = scenarioLessonId == Guid.Empty ? throw new DomainRuleException("Scenario topic lesson identifier cannot be empty.") : scenarioLessonId;
        TopicId = topicId == Guid.Empty ? throw new DomainRuleException("Scenario topic identifier cannot be empty.") : topicId;
        IsPrimary = isPrimary;
        CreatedAtUtc = ScenarioLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid ScenarioLessonId { get; private set; }

    public Guid TopicId { get; private set; }

    public bool IsPrimary { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public void ReassignTopic(Guid topicId)
    {
        TopicId = topicId == Guid.Empty ? throw new DomainRuleException("Scenario topic identifier cannot be empty.") : topicId;
    }
}

public sealed class ScenarioDialogueTurn
{
    private readonly List<ScenarioDialogueTurnTranslation> _translations = [];

    private ScenarioDialogueTurn()
    {
    }

    internal ScenarioDialogueTurn(Guid id, Guid scenarioLessonId, int sortOrder, string speakerRole, string baseText, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Scenario dialogue turn identifier cannot be empty.") : id;
        ScenarioLessonId = scenarioLessonId == Guid.Empty ? throw new DomainRuleException("Scenario dialogue turn lesson identifier cannot be empty.") : scenarioLessonId;
        SortOrder = ScenarioLesson.NormalizeSortOrder(sortOrder);
        SpeakerRole = ScenarioLesson.NormalizeKey(speakerRole, "Scenario speaker role");
        BaseText = ScenarioLesson.NormalizeRequiredText(baseText, nameof(baseText), 2000);
        CreatedAtUtc = ScenarioLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ScenarioLessonId { get; private set; }

    public int SortOrder { get; private set; }

    public string SpeakerRole { get; private set; } = string.Empty;

    public string BaseText { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<ScenarioDialogueTurnTranslation> Translations => _translations.AsReadOnly();

    public void AddTranslation(Guid id, LanguageCode languageCode, string text, DateTime createdAtUtc)
    {
        _translations.Add(new ScenarioDialogueTurnTranslation(id, Id, languageCode, text, createdAtUtc));
        UpdatedAtUtc = ScenarioLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }
}

public sealed class ScenarioPhrase
{
    private readonly List<ScenarioPhraseTranslation> _translations = [];

    private ScenarioPhrase()
    {
    }

    internal ScenarioPhrase(Guid id, Guid scenarioLessonId, int sortOrder, string baseText, string? usageNote, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Scenario phrase identifier cannot be empty.") : id;
        ScenarioLessonId = scenarioLessonId == Guid.Empty ? throw new DomainRuleException("Scenario phrase lesson identifier cannot be empty.") : scenarioLessonId;
        SortOrder = ScenarioLesson.NormalizeSortOrder(sortOrder);
        BaseText = ScenarioLesson.NormalizeRequiredText(baseText, nameof(baseText), 1024);
        UsageNote = ScenarioLesson.NormalizeOptionalText(usageNote, nameof(usageNote), 1024);
        CreatedAtUtc = ScenarioLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ScenarioLessonId { get; private set; }

    public int SortOrder { get; private set; }

    public string BaseText { get; private set; } = string.Empty;

    public string? UsageNote { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<ScenarioPhraseTranslation> Translations => _translations.AsReadOnly();

    public void AddTranslation(Guid id, LanguageCode languageCode, string text, DateTime createdAtUtc)
    {
        _translations.Add(new ScenarioPhraseTranslation(id, Id, languageCode, text, createdAtUtc));
        UpdatedAtUtc = ScenarioLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }
}

public sealed class ScenarioQuestion
{
    private readonly List<ScenarioQuestionTranslation> _translations = [];
    private readonly List<ScenarioAnswer> _answers = [];

    private ScenarioQuestion()
    {
    }

    internal ScenarioQuestion(Guid id, Guid scenarioLessonId, int sortOrder, string prompt, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Scenario question identifier cannot be empty.") : id;
        ScenarioLessonId = scenarioLessonId == Guid.Empty ? throw new DomainRuleException("Scenario question lesson identifier cannot be empty.") : scenarioLessonId;
        SortOrder = ScenarioLesson.NormalizeSortOrder(sortOrder);
        Prompt = ScenarioLesson.NormalizeRequiredText(prompt, nameof(prompt), 1024);
        CreatedAtUtc = ScenarioLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ScenarioLessonId { get; private set; }

    public int SortOrder { get; private set; }

    public string Prompt { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<ScenarioQuestionTranslation> Translations => _translations.AsReadOnly();

    public IReadOnlyCollection<ScenarioAnswer> Answers => _answers.AsReadOnly();

    public void AddTranslation(Guid id, LanguageCode languageCode, string text, DateTime createdAtUtc)
    {
        _translations.Add(new ScenarioQuestionTranslation(id, Id, languageCode, text, createdAtUtc));
        UpdatedAtUtc = ScenarioLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public ScenarioAnswer AddAnswer(Guid id, int sortOrder, string text, bool isCorrect, string? feedback, DateTime createdAtUtc)
    {
        ScenarioAnswer answer = new(id, Id, sortOrder, text, isCorrect, feedback, createdAtUtc);
        _answers.Add(answer);
        UpdatedAtUtc = ScenarioLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        return answer;
    }
}

public sealed class ScenarioAnswer
{
    private readonly List<ScenarioAnswerTranslation> _translations = [];

    private ScenarioAnswer()
    {
    }

    internal ScenarioAnswer(Guid id, Guid scenarioQuestionId, int sortOrder, string text, bool isCorrect, string? feedback, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Scenario answer identifier cannot be empty.") : id;
        ScenarioQuestionId = scenarioQuestionId == Guid.Empty ? throw new DomainRuleException("Scenario answer question identifier cannot be empty.") : scenarioQuestionId;
        SortOrder = ScenarioLesson.NormalizeSortOrder(sortOrder);
        Text = ScenarioLesson.NormalizeRequiredText(text, nameof(text), 1024);
        IsCorrect = isCorrect;
        Feedback = ScenarioLesson.NormalizeOptionalText(feedback, nameof(feedback), 1024);
        CreatedAtUtc = ScenarioLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ScenarioQuestionId { get; private set; }

    public int SortOrder { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public bool IsCorrect { get; private set; }

    public string? Feedback { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<ScenarioAnswerTranslation> Translations => _translations.AsReadOnly();

    public void AddTranslation(Guid id, LanguageCode languageCode, string text, DateTime createdAtUtc)
    {
        _translations.Add(new ScenarioAnswerTranslation(id, Id, languageCode, text, createdAtUtc));
        UpdatedAtUtc = ScenarioLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }
}

public sealed class ScenarioDialogueTurnTranslation : ScenarioTranslationBase
{
    private ScenarioDialogueTurnTranslation()
    {
    }

    internal ScenarioDialogueTurnTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime createdAtUtc)
        : base(id, ownerId, languageCode, text, createdAtUtc)
    {
    }

    public Guid ScenarioDialogueTurnId => OwnerId;
}

public sealed class ScenarioPhraseTranslation : ScenarioTranslationBase
{
    private ScenarioPhraseTranslation()
    {
    }

    internal ScenarioPhraseTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime createdAtUtc)
        : base(id, ownerId, languageCode, text, createdAtUtc)
    {
    }

    public Guid ScenarioPhraseId => OwnerId;
}

public sealed class ScenarioQuestionTranslation : ScenarioTranslationBase
{
    private ScenarioQuestionTranslation()
    {
    }

    internal ScenarioQuestionTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime createdAtUtc)
        : base(id, ownerId, languageCode, text, createdAtUtc)
    {
    }

    public Guid ScenarioQuestionId => OwnerId;
}

public sealed class ScenarioAnswerTranslation : ScenarioTranslationBase
{
    private ScenarioAnswerTranslation()
    {
    }

    internal ScenarioAnswerTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime createdAtUtc)
        : base(id, ownerId, languageCode, text, createdAtUtc)
    {
    }

    public Guid ScenarioAnswerId => OwnerId;
}

public abstract class ScenarioTranslationBase
{
    private protected ScenarioTranslationBase()
    {
    }

    private protected ScenarioTranslationBase(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Scenario translation identifier cannot be empty.") : id;
        OwnerId = ownerId == Guid.Empty ? throw new DomainRuleException("Scenario translation owner identifier cannot be empty.") : ownerId;
        LanguageCode = languageCode;
        Text = ScenarioLesson.NormalizeRequiredText(text, nameof(text), 2000);
        CreatedAtUtc = ScenarioLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid OwnerId { get; private set; }

    public LanguageCode LanguageCode { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }
}
