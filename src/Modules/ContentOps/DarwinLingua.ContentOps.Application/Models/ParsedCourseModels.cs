namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedCoursePathModel(
    string Slug,
    string Title,
    IReadOnlyList<ParsedContentMeaningModel> TitleTranslations,
    string Description,
    IReadOnlyList<ParsedContentMeaningModel> DescriptionTranslations,
    string? CefrLevel,
    string? CefrRange,
    bool IsPublished,
    int SortOrder);

public sealed record ParsedCourseModuleModel(
    string Slug,
    string CoursePathSlug,
    string Title,
    IReadOnlyList<ParsedContentMeaningModel> TitleTranslations,
    string Description,
    IReadOnlyList<ParsedContentMeaningModel> DescriptionTranslations,
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
    IReadOnlyList<ParsedContentMeaningModel> TitleTranslations,
    string ShortDescription,
    IReadOnlyList<ParsedContentMeaningModel> ShortDescriptionTranslations,
    string Narrative,
    IReadOnlyList<ParsedContentMeaningModel> NarrativeTranslations,
    string CefrLevel,
    int EstimatedMinutes,
    IReadOnlyList<string> LearningGoals,
    IReadOnlyList<ParsedCourseTextListTranslationModel> LearningGoalsTranslations,
    IReadOnlyList<string> PrerequisiteLessonSlugs,
    string? NextLessonSlug,
    IReadOnlyList<string> LinkedGrammarTopicSlugs,
    IReadOnlyList<string> LinkedWordSlugs,
    IReadOnlyList<string> LinkedExpressionSlugs,
    IReadOnlyList<string> LinkedDialogueSlugs,
    IReadOnlyList<string> LinkedTalkTopicSlugs,
    IReadOnlyList<string> LinkedExerciseSetSlugs,
    IReadOnlyList<string> LinkedExamPrepSlugs,
    IReadOnlyList<ParsedCourseLessonActivityBlockModel> ActivityBlocks,
    string? ReviewSummary,
    IReadOnlyList<ParsedContentMeaningModel> ReviewSummaryTranslations,
    string? HomeworkTask,
    IReadOnlyList<ParsedContentMeaningModel> HomeworkTaskTranslations,
    bool IsPublished,
    int SortOrder);

public sealed record ParsedCourseTextListTranslationModel(
    string Language,
    IReadOnlyList<string> Texts);

public sealed record ParsedCourseLessonActivityBlockModel(
    string Kind,
    string Title,
    IReadOnlyList<ParsedContentMeaningModel> TitleTranslations,
    string Instruction,
    IReadOnlyList<ParsedContentMeaningModel> InstructionTranslations,
    string TargetType,
    string? TargetSlug,
    int EstimatedMinutes,
    int SortOrder,
    bool IsRequired);
