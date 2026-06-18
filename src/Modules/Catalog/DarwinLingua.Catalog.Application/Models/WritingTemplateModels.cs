namespace DarwinLingua.Catalog.Application.Models;

public sealed record WritingTemplateListFilterModel(
    string? CefrLevel,
    string? Category,
    string? Register,
    string? Situation,
    string? Query);

public sealed record WritingTemplateListItemModel(
    string Slug,
    string Title,
    string? LearnerLanguageTitle,
    string ShortDescription,
    string? LearnerLanguageShortDescription,
    string CefrLevel,
    string Category,
    string Situation,
    string? LearnerLanguageSituation,
    string Register);

public sealed record WritingTemplateDetailModel(
    string Slug,
    string Title,
    string? LearnerLanguageTitle,
    string ShortDescription,
    string? LearnerLanguageShortDescription,
    string CefrLevel,
    string Category,
    string Situation,
    string? LearnerLanguageSituation,
    string Register,
    string TemplateText,
    string? LearnerLanguageTemplateText,
    string Explanation,
    string? LearnerLanguageExplanation,
    IReadOnlyList<string> Variables,
    string SampleFilledVersion,
    string? LearnerLanguageSampleFilledVersion,
    IReadOnlyList<string> LinkedGrammarTopicSlugs,
    IReadOnlyList<string> LinkedWordSlugs,
    IReadOnlyList<string> LinkedExpressionSlugs,
    IReadOnlyList<string> LinkedExerciseSlugs,
    IReadOnlyList<string> LinkedCourseLessonSlugs);
