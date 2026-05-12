namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedCoursePathModel(
    string Slug,
    string Title,
    string Description,
    string? CefrLevel,
    string? CefrRange,
    bool IsPublished,
    int SortOrder);

public sealed record ParsedCourseModuleModel(
    string Slug,
    string CoursePathSlug,
    string Title,
    string Description,
    int ModuleNumber,
    string CefrLevel,
    bool IsPublished,
    int SortOrder);

public sealed record ParsedCourseLessonModel(
    string Slug,
    string CoursePathSlug,
    string ModuleSlug,
    int LessonNumber,
    string Title,
    string ShortDescription,
    string Narrative,
    string CefrLevel,
    int EstimatedMinutes,
    IReadOnlyList<string> LearningGoals,
    IReadOnlyList<string> PrerequisiteLessonSlugs,
    string? NextLessonSlug,
    IReadOnlyList<string> LinkedGrammarTopicSlugs,
    IReadOnlyList<string> LinkedWordSlugs,
    IReadOnlyList<string> LinkedExpressionSlugs,
    IReadOnlyList<string> LinkedDialogueSlugs,
    IReadOnlyList<string> LinkedTalkTopicSlugs,
    IReadOnlyList<string> LinkedExerciseSetSlugs,
    IReadOnlyList<string> LinkedExamPrepSlugs,
    string? ReviewSummary,
    string? HomeworkTask,
    bool IsPublished,
    int SortOrder);
