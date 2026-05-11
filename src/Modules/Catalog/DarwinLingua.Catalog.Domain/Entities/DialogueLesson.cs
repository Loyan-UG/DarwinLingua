using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed partial class DialogueLesson
{
    private readonly List<DialogueLessonTopic> _topics = [];
    private readonly List<DialogueExamProfile> _examProfiles = [];
    private readonly List<DialogueSkillFocus> _skillFocus = [];
    private readonly List<DialogueSpeakingFunction> _speakingFunctions = [];
    private readonly List<DialogueUsefulWord> _usefulWords = [];
    private readonly List<DialogueSpeakingPrompt> _speakingPrompts = [];
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

    public string TaskType { get; private set; } = "exam-roleplay";

    public string InteractionMode { get; private set; } = "face-to-face";

    public string Register { get; private set; } = "neutral";

    public int EstimatedPracticeMinutes { get; private set; } = 15;

    public string? DifficultyNote { get; private set; }

    public string? ExamRelevance { get; private set; }

    public PublicationStatus PublicationStatus { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<DialogueLessonTopic> Topics => _topics.AsReadOnly();

    public IReadOnlyCollection<DialogueExamProfile> ExamProfiles => _examProfiles.AsReadOnly();

    public IReadOnlyCollection<DialogueSkillFocus> SkillFocus => _skillFocus.AsReadOnly();

    public IReadOnlyCollection<DialogueSpeakingFunction> SpeakingFunctions => _speakingFunctions.AsReadOnly();

    public IReadOnlyCollection<DialogueUsefulWord> UsefulWords => _usefulWords.AsReadOnly();

    public IReadOnlyCollection<DialogueSpeakingPrompt> SpeakingPrompts => _speakingPrompts.AsReadOnly();

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

    public void UpdateExamMetadata(
        string taskType,
        string interactionMode,
        string register,
        int estimatedPracticeMinutes,
        string? difficultyNote,
        string? examRelevance,
        DateTime updatedAtUtc)
    {
        TaskType = NormalizeKey(taskType, "Dialogue task type");
        InteractionMode = NormalizeKey(interactionMode, "Dialogue interaction mode");
        Register = NormalizeKey(register, "Dialogue register");
        EstimatedPracticeMinutes = NormalizePositiveMinutes(estimatedPracticeMinutes, nameof(estimatedPracticeMinutes));
        DifficultyNote = NormalizeOptionalText(difficultyNote, nameof(difficultyNote), 1024);
        ExamRelevance = NormalizeOptionalText(examRelevance, nameof(examRelevance), 1024);
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public void AddTopic(Guid id, Guid topicId, bool isPrimary, DateTime createdAtUtc)
    {
        _topics.Add(new DialogueLessonTopic(id, Id, topicId, isPrimary, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void AddExamProfile(Guid id, string examProfile, int sortOrder, DateTime createdAtUtc)
    {
        _examProfiles.Add(new DialogueExamProfile(id, Id, examProfile, sortOrder, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void AddSkillFocus(Guid id, string skillFocus, int sortOrder, DateTime createdAtUtc)
    {
        _skillFocus.Add(new DialogueSkillFocus(id, Id, skillFocus, sortOrder, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void AddSpeakingFunction(Guid id, string speakingFunction, int sortOrder, DateTime createdAtUtc)
    {
        _speakingFunctions.Add(new DialogueSpeakingFunction(id, Id, speakingFunction, sortOrder, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public void AddUsefulWord(Guid id, string lemma, string? wordSlug, CefrLevel? cefrLevel, int sortOrder, DateTime createdAtUtc)
    {
        _usefulWords.Add(new DialogueUsefulWord(id, Id, lemma, wordSlug, cefrLevel, sortOrder, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public DialogueSpeakingPrompt AddSpeakingPrompt(Guid id, int sortOrder, string promptType, string prompt, DateTime createdAtUtc)
    {
        DialogueSpeakingPrompt speakingPrompt = new(id, Id, sortOrder, promptType, prompt, createdAtUtc);
        _speakingPrompts.Add(speakingPrompt);
        UpdatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        return speakingPrompt;
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

    internal static int NormalizePositiveMinutes(int value, string parameterName)
    {
        if (value <= 0)
        {
            throw new DomainRuleException($"{parameterName} must be greater than zero.");
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

public sealed class DialogueExamProfile
{
    private DialogueExamProfile()
    {
    }

    internal DialogueExamProfile(Guid id, Guid dialogueLessonId, string examProfile, int sortOrder, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Dialogue exam profile identifier cannot be empty.") : id;
        DialogueLessonId = dialogueLessonId == Guid.Empty ? throw new DomainRuleException("Dialogue exam profile lesson identifier cannot be empty.") : dialogueLessonId;
        ExamProfile = DialogueLesson.NormalizeKey(examProfile, "Dialogue exam profile");
        SortOrder = DialogueLesson.NormalizeSortOrder(sortOrder);
        CreatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid DialogueLessonId { get; private set; }

    public string ExamProfile { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class DialogueSkillFocus
{
    private DialogueSkillFocus()
    {
    }

    internal DialogueSkillFocus(Guid id, Guid dialogueLessonId, string skillFocus, int sortOrder, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Dialogue skill focus identifier cannot be empty.") : id;
        DialogueLessonId = dialogueLessonId == Guid.Empty ? throw new DomainRuleException("Dialogue skill focus lesson identifier cannot be empty.") : dialogueLessonId;
        SkillFocus = DialogueLesson.NormalizeKey(skillFocus, "Dialogue skill focus");
        SortOrder = DialogueLesson.NormalizeSortOrder(sortOrder);
        CreatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid DialogueLessonId { get; private set; }

    public string SkillFocus { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class DialogueSpeakingFunction
{
    private DialogueSpeakingFunction()
    {
    }

    internal DialogueSpeakingFunction(Guid id, Guid dialogueLessonId, string speakingFunction, int sortOrder, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Dialogue speaking function identifier cannot be empty.") : id;
        DialogueLessonId = dialogueLessonId == Guid.Empty ? throw new DomainRuleException("Dialogue speaking function lesson identifier cannot be empty.") : dialogueLessonId;
        SpeakingFunction = DialogueLesson.NormalizeKey(speakingFunction, "Dialogue speaking function");
        SortOrder = DialogueLesson.NormalizeSortOrder(sortOrder);
        CreatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid DialogueLessonId { get; private set; }

    public string SpeakingFunction { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class DialogueUsefulWord
{
    private DialogueUsefulWord()
    {
    }

    internal DialogueUsefulWord(Guid id, Guid dialogueLessonId, string lemma, string? wordSlug, CefrLevel? cefrLevel, int sortOrder, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Dialogue useful word identifier cannot be empty.") : id;
        DialogueLessonId = dialogueLessonId == Guid.Empty ? throw new DomainRuleException("Dialogue useful word lesson identifier cannot be empty.") : dialogueLessonId;
        Lemma = DialogueLesson.NormalizeRequiredText(lemma, nameof(lemma), 256);
        WordSlug = string.IsNullOrWhiteSpace(wordSlug) ? null : DialogueLesson.NormalizeKey(wordSlug, "Dialogue useful word slug");
        CefrLevel = cefrLevel;
        SortOrder = DialogueLesson.NormalizeSortOrder(sortOrder);
        CreatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid DialogueLessonId { get; private set; }

    public string Lemma { get; private set; } = string.Empty;

    public string? WordSlug { get; private set; }

    public CefrLevel? CefrLevel { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
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

public sealed class DialogueSpeakingPrompt
{
    private readonly List<DialogueSpeakingPromptTranslation> _translations = [];

    private DialogueSpeakingPrompt()
    {
    }

    internal DialogueSpeakingPrompt(Guid id, Guid dialogueLessonId, int sortOrder, string promptType, string prompt, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Dialogue speaking prompt identifier cannot be empty.") : id;
        DialogueLessonId = dialogueLessonId == Guid.Empty ? throw new DomainRuleException("Dialogue speaking prompt lesson identifier cannot be empty.") : dialogueLessonId;
        SortOrder = DialogueLesson.NormalizeSortOrder(sortOrder);
        PromptType = DialogueLesson.NormalizeKey(promptType, "Dialogue speaking prompt type");
        Prompt = DialogueLesson.NormalizeRequiredText(prompt, nameof(prompt), 2000);
        CreatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid DialogueLessonId { get; private set; }

    public int SortOrder { get; private set; }

    public string PromptType { get; private set; } = string.Empty;

    public string Prompt { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<DialogueSpeakingPromptTranslation> Translations => _translations.AsReadOnly();

    public void AddTranslation(Guid id, LanguageCode languageCode, string text, DateTime createdAtUtc)
    {
        _translations.Add(new DialogueSpeakingPromptTranslation(id, Id, languageCode, text, createdAtUtc));
        UpdatedAtUtc = DialogueLesson.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
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

public sealed class DialogueSpeakingPromptTranslation : DialogueTranslationBase
{
    private DialogueSpeakingPromptTranslation()
    {
    }

    internal DialogueSpeakingPromptTranslation(Guid id, Guid ownerId, LanguageCode languageCode, string text, DateTime createdAtUtc)
        : base(id, ownerId, languageCode, text, createdAtUtc)
    {
    }

    public Guid DialogueSpeakingPromptId => OwnerId;
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
