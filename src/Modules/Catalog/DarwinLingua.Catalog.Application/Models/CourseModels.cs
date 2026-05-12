namespace DarwinLingua.Catalog.Application.Models;

public sealed record CoursePathListFilterModel(
    string? CefrLevel,
    string? Query);

public sealed record CoursePathListItemModel(
    string Slug,
    string Title,
    string Description,
    string? CefrLevel,
    string CefrRange,
    int ModuleCount,
    int LessonCount);

public sealed record CoursePathDetailModel(
    string Slug,
    string Title,
    string Description,
    string? CefrLevel,
    string CefrRange,
    IReadOnlyList<CourseModuleModel> Modules);

public sealed record CourseModuleModel(
    string Slug,
    string Title,
    string Description,
    int ModuleNumber,
    string CefrLevel,
    IReadOnlyList<CourseLessonListItemModel> Lessons);

public sealed record CourseLessonListItemModel(
    string Slug,
    string CoursePathSlug,
    string ModuleSlug,
    int LessonNumber,
    string Title,
    string ShortDescription,
    string CefrLevel,
    int EstimatedMinutes);

public sealed record CourseLessonDetailModel(
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
    string? HomeworkTask);
