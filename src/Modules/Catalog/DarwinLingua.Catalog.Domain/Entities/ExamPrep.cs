using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents a recognized German exam profile used by exam-preparation units.
/// </summary>
public sealed class ExamProfile
{
    private static readonly Regex KebabCaseRegex = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

    private ExamProfile()
    {
        Key = string.Empty;
        DisplayName = string.Empty;
        DisplayNameTranslationsJson = "[]";
        CefrRange = string.Empty;
        Description = string.Empty;
        DescriptionTranslationsJson = "[]";
    }

    public ExamProfile(
        Guid id,
        string key,
        string displayName,
        string cefrRange,
        string description,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime timestampUtc,
        string displayNameTranslationsJson = "[]",
        string descriptionTranslationsJson = "[]")
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Exam profile id is required.") : id;
        Key = NormalizeKebabKey(key, "Exam profile key");
        DisplayName = RequireText(displayName, "Exam profile display name", 256);
        DisplayNameTranslationsJson = RequireText(displayNameTranslationsJson, "Exam profile display name translations JSON", 12000);
        CefrRange = RequireText(cefrRange, "Exam profile CEFR range", 64);
        Description = RequireText(description, "Exam profile description", 1000);
        DescriptionTranslationsJson = RequireText(descriptionTranslationsJson, "Exam profile description translations JSON", 12000);
        PublicationStatus = publicationStatus;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public string Key { get; private set; }
    public string DisplayName { get; private set; }
    public string DisplayNameTranslationsJson { get; private set; }
    public string CefrRange { get; private set; }
    public string Description { get; private set; }
    public string DescriptionTranslationsJson { get; private set; }
    public PublicationStatus PublicationStatus { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    internal static string RequireText(string value, string fieldName, int maxLength)
    {
        string normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized)) throw new DomainRuleException($"{fieldName} is required.");
        if (normalized.Length > maxLength) throw new DomainRuleException($"{fieldName} must not exceed {maxLength} characters.");
        return normalized;
    }

    internal static string NormalizeKebabKey(string value, string fieldName)
    {
        string normalized = value.Trim().ToLowerInvariant();
        if (!KebabCaseRegex.IsMatch(normalized)) throw new DomainRuleException($"{fieldName} must use lowercase kebab-case.");
        return normalized;
    }
}

/// <summary>
/// Represents one original exam-preparation unit linked to reusable learning content.
/// </summary>
public sealed class ExamPrepUnit
{
    private ExamPrepUnit()
    {
        Slug = string.Empty;
        ExamProfileKey = string.Empty;
        Title = string.Empty;
        ShortDescription = string.Empty;
        ExamSection = string.Empty;
        TaskType = string.Empty;
        SkillFocus = string.Empty;
        Explanation = string.Empty;
        TitleTranslationsJson = "[]";
        ShortDescriptionTranslationsJson = "[]";
        ExplanationTranslationsJson = "[]";
        StrategyNotesJson = "[]";
        StrategyNotesTranslationsJson = "[]";
        ChecklistJson = "[]";
        ChecklistTranslationsJson = "[]";
        LinkedDialogueSlugsJson = "[]";
        LinkedTalkTopicSlugsJson = "[]";
        LinkedGrammarTopicSlugsJson = "[]";
        LinkedExpressionSlugsJson = "[]";
        LinkedWritingTemplateSlugsJson = "[]";
        LinkedExerciseSlugsJson = "[]";
        LinkedRoleplaySlugsJson = "[]";
        LinkedCourseLessonSlugsJson = "[]";
    }

    public ExamPrepUnit(
        Guid id,
        string slug,
        string examProfileKey,
        string title,
        string shortDescription,
        CefrLevel cefrLevel,
        string examSection,
        string taskType,
        string skillFocus,
        string explanation,
        string strategyNotesJson,
        string checklistJson,
        string linkedDialogueSlugsJson,
        string linkedTalkTopicSlugsJson,
        string linkedGrammarTopicSlugsJson,
        string linkedExpressionSlugsJson,
        string linkedWritingTemplateSlugsJson,
        string linkedExerciseSlugsJson,
        string linkedRoleplaySlugsJson,
        string linkedCourseLessonSlugsJson,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime timestampUtc,
        string titleTranslationsJson = "[]",
        string shortDescriptionTranslationsJson = "[]",
        string explanationTranslationsJson = "[]",
        string strategyNotesTranslationsJson = "[]",
        string checklistTranslationsJson = "[]")
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Exam prep unit id is required.") : id;
        Slug = ExamProfile.NormalizeKebabKey(slug, "Exam prep unit slug");
        ExamProfileKey = ExamProfile.NormalizeKebabKey(examProfileKey, "Exam profile key");
        Title = ExamProfile.RequireText(title, "Exam prep unit title", 256);
        ShortDescription = ExamProfile.RequireText(shortDescription, "Exam prep unit short description", 1000);
        CefrLevel = cefrLevel;
        ExamSection = ExamProfile.NormalizeKebabKey(examSection, "Exam section");
        TaskType = ExamProfile.NormalizeKebabKey(taskType, "Exam task type");
        SkillFocus = ExamProfile.NormalizeKebabKey(skillFocus, "Exam prep skill focus");
        Explanation = ExamProfile.RequireText(explanation, "Exam prep explanation", 12000);
        TitleTranslationsJson = ExamProfile.RequireText(titleTranslationsJson, "Exam prep title translations JSON", 12000);
        ShortDescriptionTranslationsJson = ExamProfile.RequireText(shortDescriptionTranslationsJson, "Exam prep short description translations JSON", 12000);
        ExplanationTranslationsJson = ExamProfile.RequireText(explanationTranslationsJson, "Exam prep explanation translations JSON", 24000);
        StrategyNotesJson = ExamProfile.RequireText(strategyNotesJson, "Exam prep strategy notes JSON", 12000);
        StrategyNotesTranslationsJson = ExamProfile.RequireText(strategyNotesTranslationsJson, "Exam prep strategy notes translations JSON", 24000);
        ChecklistJson = ExamProfile.RequireText(checklistJson, "Exam prep checklist JSON", 12000);
        ChecklistTranslationsJson = ExamProfile.RequireText(checklistTranslationsJson, "Exam prep checklist translations JSON", 24000);
        LinkedDialogueSlugsJson = ExamProfile.RequireText(linkedDialogueSlugsJson, "Linked dialogues JSON", 12000);
        LinkedTalkTopicSlugsJson = ExamProfile.RequireText(linkedTalkTopicSlugsJson, "Linked Talk Topics JSON", 12000);
        LinkedGrammarTopicSlugsJson = ExamProfile.RequireText(linkedGrammarTopicSlugsJson, "Linked grammar topics JSON", 12000);
        LinkedExpressionSlugsJson = ExamProfile.RequireText(linkedExpressionSlugsJson, "Linked expressions JSON", 12000);
        LinkedWritingTemplateSlugsJson = ExamProfile.RequireText(linkedWritingTemplateSlugsJson, "Linked writing templates JSON", 12000);
        LinkedExerciseSlugsJson = ExamProfile.RequireText(linkedExerciseSlugsJson, "Linked exercises JSON", 12000);
        LinkedRoleplaySlugsJson = ExamProfile.RequireText(linkedRoleplaySlugsJson, "Linked roleplays JSON", 12000);
        LinkedCourseLessonSlugsJson = ExamProfile.RequireText(linkedCourseLessonSlugsJson, "Linked course lessons JSON", 12000);
        PublicationStatus = publicationStatus;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public string Slug { get; private set; }
    public string ExamProfileKey { get; private set; }
    public string Title { get; private set; }
    public string ShortDescription { get; private set; }
    public CefrLevel CefrLevel { get; private set; }
    public string ExamSection { get; private set; }
    public string TaskType { get; private set; }
    public string SkillFocus { get; private set; }
    public string Explanation { get; private set; }
    public string TitleTranslationsJson { get; private set; }
    public string ShortDescriptionTranslationsJson { get; private set; }
    public string ExplanationTranslationsJson { get; private set; }
    public string StrategyNotesJson { get; private set; }
    public string StrategyNotesTranslationsJson { get; private set; }
    public string ChecklistJson { get; private set; }
    public string ChecklistTranslationsJson { get; private set; }
    public string LinkedDialogueSlugsJson { get; private set; }
    public string LinkedTalkTopicSlugsJson { get; private set; }
    public string LinkedGrammarTopicSlugsJson { get; private set; }
    public string LinkedExpressionSlugsJson { get; private set; }
    public string LinkedWritingTemplateSlugsJson { get; private set; }
    public string LinkedExerciseSlugsJson { get; private set; }
    public string LinkedRoleplaySlugsJson { get; private set; }
    public string LinkedCourseLessonSlugsJson { get; private set; }
    public PublicationStatus PublicationStatus { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
}
