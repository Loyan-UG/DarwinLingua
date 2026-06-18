namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedExamProfileModel(
    string Key,
    string DisplayName,
    IReadOnlyList<ParsedContentMeaningModel> DisplayNameTranslations,
    string CefrRange,
    string Description,
    IReadOnlyList<ParsedContentMeaningModel> DescriptionTranslations,
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
    IReadOnlyList<ParsedContentMeaningModel> TitleTranslations,
    IReadOnlyList<ParsedContentMeaningModel> ShortDescriptionTranslations,
    IReadOnlyList<ParsedContentMeaningModel> ExplanationTranslations,
    IReadOnlyList<string> StrategyNotes,
    IReadOnlyList<ParsedExamPrepTextListTranslationModel> StrategyNotesTranslations,
    IReadOnlyList<string> Checklist,
    IReadOnlyList<ParsedExamPrepTextListTranslationModel> ChecklistTranslations,
    IReadOnlyList<string> LinkedDialogueSlugs,
    IReadOnlyList<string> LinkedTalkTopicSlugs,
    IReadOnlyList<string> LinkedGrammarTopicSlugs,
    IReadOnlyList<string> LinkedExpressionSlugs,
    IReadOnlyList<string> LinkedWritingTemplateSlugs,
    IReadOnlyList<string> LinkedExerciseSlugs,
    IReadOnlyList<string> LinkedRoleplaySlugs,
    IReadOnlyList<string> LinkedCourseLessonSlugs,
    bool IsPublished,
    int SortOrder);

public sealed record ParsedExamPrepTextListTranslationModel(
    string Language,
    IReadOnlyList<string> Texts);
