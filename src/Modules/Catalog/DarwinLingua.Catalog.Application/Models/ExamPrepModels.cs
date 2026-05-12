namespace DarwinLingua.Catalog.Application.Models;

public sealed record ExamProfileModel(string Key, string DisplayName, string CefrRange, string Description);

public sealed record ExamPrepListFilterModel(string? ExamProfile, string? CefrLevel, string? SkillFocus, string? TaskType, string? Section, string? Query);

public sealed record ExamPrepUnitListItemModel(string Slug, string ExamProfileKey, string Title, string ShortDescription, string CefrLevel, string ExamSection, string TaskType, string SkillFocus);

public sealed record ExamPrepUnitDetailModel(
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
    IReadOnlyList<string> LinkedCourseLessonSlugs);
