namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedExamProfileModel(
    string Key,
    string DisplayName,
    string CefrRange,
    string Description,
    bool IsPublished,
    int SortOrder);

public sealed record ParsedExamPrepUnitModel(
    string Slug,
    string ExamProfileKey,
    string Title,
    string ShortDescription,
    string CefrLevel,
    string ExamSection,
    string TaskType,
    string SkillFocus,
    string Explanation,
    IReadOnlyList<string> StrategyNotes,
    IReadOnlyList<string> Checklist,
    IReadOnlyList<string> LinkedDialogueSlugs,
    IReadOnlyList<string> LinkedTalkTopicSlugs,
    IReadOnlyList<string> LinkedGrammarTopicSlugs,
    IReadOnlyList<string> LinkedExpressionSlugs,
    IReadOnlyList<string> LinkedWritingTemplateSlugs,
    IReadOnlyList<string> LinkedExerciseSlugs,
    IReadOnlyList<string> LinkedCourseLessonSlugs,
    bool IsPublished,
    int SortOrder);
