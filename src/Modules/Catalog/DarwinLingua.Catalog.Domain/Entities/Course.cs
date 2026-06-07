using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents one structured CEFR learning path.
/// </summary>
public sealed class CoursePath
{
    private static readonly Regex KebabCaseRegex = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);
    private readonly List<CourseModule> _modules = [];

    private CoursePath()
    {
        Slug = string.Empty;
        Title = string.Empty;
        Description = string.Empty;
        TitleTranslationsJson = "[]";
        DescriptionTranslationsJson = "[]";
        CefrRange = string.Empty;
    }

    public CoursePath(
        Guid id,
        string slug,
        string title,
        string description,
        CefrLevel? cefrLevel,
        string? cefrRange,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime timestampUtc,
        string titleTranslationsJson = "[]",
        string descriptionTranslationsJson = "[]")
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Course path id is required.") : id;
        Slug = NormalizeKebabKey(slug, "Course path slug");
        Title = RequireText(title, "Course path title", 256);
        Description = RequireText(description, "Course path description", 2000);
        TitleTranslationsJson = RequireText(titleTranslationsJson, "Course path title translations JSON", 12000);
        DescriptionTranslationsJson = RequireText(descriptionTranslationsJson, "Course path description translations JSON", 12000);
        CefrLevel = cefrLevel;
        CefrRange = NormalizeOptionalText(cefrRange, 32, "Course path CEFR range") ?? cefrLevel?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(CefrRange))
        {
            throw new DomainRuleException("Course path CEFR level or range is required.");
        }

        PublicationStatus = publicationStatus;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public string Slug { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string TitleTranslationsJson { get; private set; }
    public string DescriptionTranslationsJson { get; private set; }
    public CefrLevel? CefrLevel { get; private set; }
    public string CefrRange { get; private set; }
    public PublicationStatus PublicationStatus { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<CourseModule> Modules => _modules;

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

/// <summary>
/// Represents one ordered module inside a course path.
/// </summary>
public sealed class CourseModule
{
    private readonly List<CourseLesson> _lessons = [];

    private CourseModule()
    {
        CoursePathSlug = string.Empty;
        Slug = string.Empty;
        Title = string.Empty;
        Description = string.Empty;
        TitleTranslationsJson = "[]";
        DescriptionTranslationsJson = "[]";
    }

    public CourseModule(
        Guid id,
        string coursePathSlug,
        string slug,
        string title,
        string description,
        int moduleNumber,
        CefrLevel cefrLevel,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime timestampUtc,
        string titleTranslationsJson = "[]",
        string descriptionTranslationsJson = "[]")
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Course module id is required.") : id;
        CoursePathSlug = CoursePath.NormalizeKebabKey(coursePathSlug, "Course path slug");
        Slug = CoursePath.NormalizeKebabKey(slug, "Course module slug");
        Title = CoursePath.RequireText(title, "Course module title", 256);
        Description = CoursePath.RequireText(description, "Course module description", 2000);
        TitleTranslationsJson = CoursePath.RequireText(titleTranslationsJson, "Course module title translations JSON", 12000);
        DescriptionTranslationsJson = CoursePath.RequireText(descriptionTranslationsJson, "Course module description translations JSON", 12000);
        ModuleNumber = moduleNumber <= 0 ? throw new DomainRuleException("Course module number must be positive.") : moduleNumber;
        CefrLevel = cefrLevel;
        PublicationStatus = publicationStatus;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public Guid CoursePathId { get; private set; }
    public string CoursePathSlug { get; private set; }
    public string Slug { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string TitleTranslationsJson { get; private set; }
    public string DescriptionTranslationsJson { get; private set; }
    public int ModuleNumber { get; private set; }
    public CefrLevel CefrLevel { get; private set; }
    public PublicationStatus PublicationStatus { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<CourseLesson> Lessons => _lessons;

    public void AttachToCoursePath(Guid coursePathId)
    {
        if (coursePathId == Guid.Empty)
        {
            throw new DomainRuleException("Course path id is required.");
        }

        CoursePathId = coursePathId;
    }
}

/// <summary>
/// Represents one content-driven lesson that orchestrates links to other learning content.
/// </summary>
public sealed class CourseLesson
{
    private CourseLesson()
    {
        CoursePathSlug = string.Empty;
        ModuleSlug = string.Empty;
        Slug = string.Empty;
        Title = string.Empty;
        ShortDescription = string.Empty;
        Narrative = string.Empty;
        TitleTranslationsJson = "[]";
        ShortDescriptionTranslationsJson = "[]";
        NarrativeTranslationsJson = "[]";
        LearningGoalsJson = "[]";
        LearningGoalsTranslationsJson = "[]";
        PrerequisiteLessonSlugsJson = "[]";
        LinkedGrammarTopicSlugsJson = "[]";
        LinkedWordSlugsJson = "[]";
        LinkedExpressionSlugsJson = "[]";
        LinkedDialogueSlugsJson = "[]";
        LinkedTalkTopicSlugsJson = "[]";
        LinkedExerciseSetSlugsJson = "[]";
        LinkedExamPrepSlugsJson = "[]";
        ReviewSummaryTranslationsJson = "[]";
        HomeworkTaskTranslationsJson = "[]";
    }

    public CourseLesson(
        Guid id,
        string coursePathSlug,
        string moduleSlug,
        string slug,
        int lessonNumber,
        string title,
        string shortDescription,
        string narrative,
        CefrLevel cefrLevel,
        int estimatedMinutes,
        string learningGoalsJson,
        string prerequisiteLessonSlugsJson,
        string? nextLessonSlug,
        string linkedGrammarTopicSlugsJson,
        string linkedWordSlugsJson,
        string linkedExpressionSlugsJson,
        string linkedDialogueSlugsJson,
        string linkedTalkTopicSlugsJson,
        string linkedExerciseSetSlugsJson,
        string linkedExamPrepSlugsJson,
        string? reviewSummary,
        string? homeworkTask,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime timestampUtc,
        string titleTranslationsJson = "[]",
        string shortDescriptionTranslationsJson = "[]",
        string narrativeTranslationsJson = "[]",
        string learningGoalsTranslationsJson = "[]",
        string reviewSummaryTranslationsJson = "[]",
        string homeworkTaskTranslationsJson = "[]")
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Course lesson id is required.") : id;
        CoursePathSlug = CoursePath.NormalizeKebabKey(coursePathSlug, "Course path slug");
        ModuleSlug = CoursePath.NormalizeKebabKey(moduleSlug, "Course module slug");
        Slug = CoursePath.NormalizeKebabKey(slug, "Course lesson slug");
        LessonNumber = lessonNumber <= 0 ? throw new DomainRuleException("Course lesson number must be positive.") : lessonNumber;
        Title = CoursePath.RequireText(title, "Course lesson title", 256);
        ShortDescription = CoursePath.RequireText(shortDescription, "Course lesson short description", 1000);
        Narrative = CoursePath.RequireText(narrative, "Course lesson narrative", 4000);
        TitleTranslationsJson = CoursePath.RequireText(titleTranslationsJson, "Course lesson title translations JSON", 12000);
        ShortDescriptionTranslationsJson = CoursePath.RequireText(shortDescriptionTranslationsJson, "Course lesson short description translations JSON", 12000);
        NarrativeTranslationsJson = CoursePath.RequireText(narrativeTranslationsJson, "Course lesson narrative translations JSON", 24000);
        CefrLevel = cefrLevel;
        EstimatedMinutes = estimatedMinutes <= 0 ? throw new DomainRuleException("Course lesson estimated minutes must be positive.") : estimatedMinutes;
        LearningGoalsJson = CoursePath.RequireText(learningGoalsJson, "Course lesson learning goals JSON", 12000);
        LearningGoalsTranslationsJson = CoursePath.RequireText(learningGoalsTranslationsJson, "Course lesson learning goals translations JSON", 24000);
        PrerequisiteLessonSlugsJson = CoursePath.RequireText(prerequisiteLessonSlugsJson, "Course lesson prerequisite JSON", 8000);
        NextLessonSlug = CoursePath.NormalizeOptionalKebabKey(nextLessonSlug, "Course lesson next slug");
        LinkedGrammarTopicSlugsJson = CoursePath.RequireText(linkedGrammarTopicSlugsJson, "Linked grammar JSON", 12000);
        LinkedWordSlugsJson = CoursePath.RequireText(linkedWordSlugsJson, "Linked words JSON", 12000);
        LinkedExpressionSlugsJson = CoursePath.RequireText(linkedExpressionSlugsJson, "Linked expressions JSON", 12000);
        LinkedDialogueSlugsJson = CoursePath.RequireText(linkedDialogueSlugsJson, "Linked dialogues JSON", 12000);
        LinkedTalkTopicSlugsJson = CoursePath.RequireText(linkedTalkTopicSlugsJson, "Linked Talk Topics JSON", 12000);
        LinkedExerciseSetSlugsJson = CoursePath.RequireText(linkedExerciseSetSlugsJson, "Linked exercise sets JSON", 12000);
        LinkedExamPrepSlugsJson = CoursePath.RequireText(linkedExamPrepSlugsJson, "Linked exam prep JSON", 12000);
        ReviewSummary = CoursePath.NormalizeOptionalText(reviewSummary, 2000, "Course lesson review summary");
        HomeworkTask = CoursePath.NormalizeOptionalText(homeworkTask, 2000, "Course lesson homework task");
        ReviewSummaryTranslationsJson = CoursePath.RequireText(reviewSummaryTranslationsJson, "Course lesson review summary translations JSON", 12000);
        HomeworkTaskTranslationsJson = CoursePath.RequireText(homeworkTaskTranslationsJson, "Course lesson homework task translations JSON", 12000);
        PublicationStatus = publicationStatus;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public Guid CourseModuleId { get; private set; }
    public string CoursePathSlug { get; private set; }
    public string ModuleSlug { get; private set; }
    public string Slug { get; private set; }
    public int LessonNumber { get; private set; }
    public string Title { get; private set; }
    public string ShortDescription { get; private set; }
    public string Narrative { get; private set; }
    public string TitleTranslationsJson { get; private set; }
    public string ShortDescriptionTranslationsJson { get; private set; }
    public string NarrativeTranslationsJson { get; private set; }
    public CefrLevel CefrLevel { get; private set; }
    public int EstimatedMinutes { get; private set; }
    public string LearningGoalsJson { get; private set; }
    public string LearningGoalsTranslationsJson { get; private set; }
    public string PrerequisiteLessonSlugsJson { get; private set; }
    public string? NextLessonSlug { get; private set; }
    public string LinkedGrammarTopicSlugsJson { get; private set; }
    public string LinkedWordSlugsJson { get; private set; }
    public string LinkedExpressionSlugsJson { get; private set; }
    public string LinkedDialogueSlugsJson { get; private set; }
    public string LinkedTalkTopicSlugsJson { get; private set; }
    public string LinkedExerciseSetSlugsJson { get; private set; }
    public string LinkedExamPrepSlugsJson { get; private set; }
    public string? ReviewSummary { get; private set; }
    public string? HomeworkTask { get; private set; }
    public string ReviewSummaryTranslationsJson { get; private set; }
    public string HomeworkTaskTranslationsJson { get; private set; }
    public PublicationStatus PublicationStatus { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public void AttachToCourseModule(Guid courseModuleId)
    {
        if (courseModuleId == Guid.Empty)
        {
            throw new DomainRuleException("Course module id is required.");
        }

        CourseModuleId = courseModuleId;
    }
}
