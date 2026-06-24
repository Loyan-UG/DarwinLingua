using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents one reusable deterministic learning exercise definition.
/// </summary>
public sealed class Exercise
{
    private static readonly Regex KebabCaseRegex = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

    private Exercise()
    {
        Slug = string.Empty;
        Title = string.Empty;
        Instruction = string.Empty;
        ExerciseType = string.Empty;
        TargetSkill = string.Empty;
        OwnerType = string.Empty;
        PromptJson = "{}";
        AnswerKeyJson = "{}";
        TitleTranslationsJson = "[]";
        InstructionTranslationsJson = "[]";
        CorrectExplanation = string.Empty;
        CorrectExplanationTranslationsJson = "[]";
        IncorrectExplanation = string.Empty;
        IncorrectExplanationTranslationsJson = "[]";
        HintTranslationsJson = "[]";
        CommonMistakeNoteTranslationsJson = "[]";
        TargetLearningLanguageCode = ContentLanguageRequirements.DefaultTargetLearningLanguageCode;
    }

    public Exercise(
        Guid id,
        string slug,
        string title,
        string instruction,
        CefrLevel cefrLevel,
        string exerciseType,
        string targetSkill,
        string ownerType,
        string? ownerSlug,
        string promptJson,
        string answerKeyJson,
        string correctExplanation,
        string incorrectExplanation,
        string? hint,
        string? commonMistakeNote,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime timestampUtc,
        string? titleTranslationsJson = null,
        string? instructionTranslationsJson = null,
        string? correctExplanationTranslationsJson = null,
        string? incorrectExplanationTranslationsJson = null,
        string? hintTranslationsJson = null,
        string? commonMistakeNoteTranslationsJson = null,
        string? targetLearningLanguageCode = null)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Exercise id is required.") : id;
        TargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode, "Exercise target learning language");
        Slug = NormalizeKebabKey(slug, "Exercise slug");
        Title = RequireText(title, "Exercise title", 256);
        Instruction = RequireText(instruction, "Exercise instruction", 2000);
        CefrLevel = cefrLevel;
        ExerciseType = NormalizeKebabKey(exerciseType, "Exercise type");
        TargetSkill = NormalizeKebabKey(targetSkill, "Exercise target skill");
        OwnerType = NormalizeKebabKey(ownerType, "Exercise owner type");
        OwnerSlug = NormalizeOptionalKebabKey(ownerSlug, "Exercise owner slug");
        PromptJson = RequireText(promptJson, "Exercise prompt JSON", 20000);
        AnswerKeyJson = RequireText(answerKeyJson, "Exercise answer key JSON", 20000);
        TitleTranslationsJson = NormalizeJsonArray(titleTranslationsJson);
        InstructionTranslationsJson = NormalizeJsonArray(instructionTranslationsJson);
        CorrectExplanation = RequireText(correctExplanation, "Correct explanation", 2000);
        CorrectExplanationTranslationsJson = NormalizeJsonArray(correctExplanationTranslationsJson);
        IncorrectExplanation = RequireText(incorrectExplanation, "Incorrect explanation", 2000);
        IncorrectExplanationTranslationsJson = NormalizeJsonArray(incorrectExplanationTranslationsJson);
        Hint = NormalizeOptionalText(hint, 1000, "Exercise hint");
        HintTranslationsJson = NormalizeJsonArray(hintTranslationsJson);
        CommonMistakeNote = NormalizeOptionalText(commonMistakeNote, 1000, "Common mistake note");
        CommonMistakeNoteTranslationsJson = NormalizeJsonArray(commonMistakeNoteTranslationsJson);
        PublicationStatus = publicationStatus;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public string TargetLearningLanguageCode { get; private set; }
    public string Slug { get; private set; }
    public string Title { get; private set; }
    public string Instruction { get; private set; }
    public CefrLevel CefrLevel { get; private set; }
    public string ExerciseType { get; private set; }
    public string TargetSkill { get; private set; }
    public string OwnerType { get; private set; }
    public string? OwnerSlug { get; private set; }
    public string PromptJson { get; private set; }
    public string AnswerKeyJson { get; private set; }
    public string TitleTranslationsJson { get; private set; }
    public string InstructionTranslationsJson { get; private set; }
    public string CorrectExplanation { get; private set; }
    public string CorrectExplanationTranslationsJson { get; private set; }
    public string IncorrectExplanation { get; private set; }
    public string IncorrectExplanationTranslationsJson { get; private set; }
    public string? Hint { get; private set; }
    public string HintTranslationsJson { get; private set; }
    public string? CommonMistakeNote { get; private set; }
    public string CommonMistakeNoteTranslationsJson { get; private set; }
    public PublicationStatus PublicationStatus { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

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

    internal static string NormalizeJsonArray(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "[]" : value.Trim();
}

/// <summary>
/// Represents an ordered reusable group of exercises.
/// </summary>
public sealed class ExerciseSet
{
    private readonly List<ExerciseSetItem> _items = [];

    private ExerciseSet()
    {
        Slug = string.Empty;
        Title = string.Empty;
        TitleTranslationsJson = "[]";
        Description = string.Empty;
        DescriptionTranslationsJson = "[]";
        OwnerType = string.Empty;
        TargetLearningLanguageCode = ContentLanguageRequirements.DefaultTargetLearningLanguageCode;
    }

    public ExerciseSet(
        Guid id,
        string slug,
        string title,
        string description,
        CefrLevel cefrLevel,
        string ownerType,
        string? ownerSlug,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime timestampUtc,
        string? titleTranslationsJson = null,
        string? descriptionTranslationsJson = null,
        string? targetLearningLanguageCode = null)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Exercise set id is required.") : id;
        TargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode, "Exercise set target learning language");
        Slug = Exercise.NormalizeKebabKey(slug, "Exercise set slug");
        Title = Exercise.RequireText(title, "Exercise set title", 256);
        TitleTranslationsJson = Exercise.NormalizeJsonArray(titleTranslationsJson);
        Description = Exercise.RequireText(description, "Exercise set description", 2000);
        DescriptionTranslationsJson = Exercise.NormalizeJsonArray(descriptionTranslationsJson);
        CefrLevel = cefrLevel;
        OwnerType = Exercise.NormalizeKebabKey(ownerType, "Exercise set owner type");
        OwnerSlug = Exercise.NormalizeOptionalKebabKey(ownerSlug, "Exercise set owner slug");
        PublicationStatus = publicationStatus;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public string TargetLearningLanguageCode { get; private set; }
    public string Slug { get; private set; }
    public string Title { get; private set; }
    public string TitleTranslationsJson { get; private set; }
    public string Description { get; private set; }
    public string DescriptionTranslationsJson { get; private set; }
    public CefrLevel CefrLevel { get; private set; }
    public string OwnerType { get; private set; }
    public string? OwnerSlug { get; private set; }
    public PublicationStatus PublicationStatus { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<ExerciseSetItem> Items => _items;

    public void AddExercise(Guid id, string exerciseSlug, int sortOrder, DateTime timestampUtc) =>
        _items.Add(new ExerciseSetItem(id, Id, Exercise.NormalizeKebabKey(exerciseSlug, "Exercise slug"), sortOrder, timestampUtc));
}

/// <summary>
/// Represents an exercise reference inside an exercise set.
/// </summary>
public sealed class ExerciseSetItem
{
    private ExerciseSetItem() { ExerciseSlug = string.Empty; }

    internal ExerciseSetItem(Guid id, Guid exerciseSetId, string exerciseSlug, int sortOrder, DateTime timestampUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Exercise set item id is required.") : id;
        ExerciseSetId = exerciseSetId;
        ExerciseSlug = exerciseSlug;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public Guid ExerciseSetId { get; private set; }
    public string ExerciseSlug { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}

/// <summary>
/// Represents one persisted user attempt against a deterministic exercise.
/// </summary>
public sealed class UserExerciseAttempt
{
    private UserExerciseAttempt()
    {
        UserId = string.Empty;
        TargetLearningLanguageCode = ContentLanguageRequirements.DefaultTargetLearningLanguageCode;
        ExerciseSlug = string.Empty;
        SubmittedAnswerJson = "{}";
        FeedbackExplanation = string.Empty;
    }

    public UserExerciseAttempt(
        Guid id,
        string userId,
        string targetLearningLanguageCode,
        string exerciseSlug,
        string submittedAnswerJson,
        bool isCorrect,
        string feedbackExplanation,
        DateTime attemptedAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Exercise attempt id is required.") : id;
        UserId = Exercise.RequireText(userId, "Exercise attempt user id", 256);
        TargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode, "Exercise attempt target learning language");
        ExerciseSlug = Exercise.NormalizeKebabKey(exerciseSlug, "Exercise attempt slug");
        SubmittedAnswerJson = Exercise.RequireText(submittedAnswerJson, "Submitted answer JSON", 20000);
        IsCorrect = isCorrect;
        FeedbackExplanation = Exercise.RequireText(feedbackExplanation, "Feedback explanation", 2000);
        AttemptedAtUtc = attemptedAtUtc == default ? throw new DomainRuleException("Attempt timestamp is required.") : attemptedAtUtc;
        CreatedAtUtc = AttemptedAtUtc;
    }

    public Guid Id { get; private set; }
    public string UserId { get; private set; }
    public string TargetLearningLanguageCode { get; private set; }
    public string ExerciseSlug { get; private set; }
    public string SubmittedAnswerJson { get; private set; }
    public bool IsCorrect { get; private set; }
    public string FeedbackExplanation { get; private set; }
    public DateTime AttemptedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}
