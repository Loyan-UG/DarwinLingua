namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedExerciseModel(
    string Slug,
    string Title,
    IReadOnlyList<ParsedContentMeaningModel> TitleTranslations,
    string Instruction,
    IReadOnlyList<ParsedContentMeaningModel> InstructionTranslations,
    string CefrLevel,
    string ExerciseType,
    string TargetSkill,
    string OwnerType,
    string? OwnerSlug,
    string PromptJson,
    string AnswerKeyJson,
    string CorrectExplanation,
    IReadOnlyList<ParsedContentMeaningModel> CorrectExplanationTranslations,
    string IncorrectExplanation,
    IReadOnlyList<ParsedContentMeaningModel> IncorrectExplanationTranslations,
    string? Hint,
    IReadOnlyList<ParsedContentMeaningModel> HintTranslations,
    string? CommonMistakeNote,
    IReadOnlyList<ParsedContentMeaningModel> CommonMistakeNoteTranslations,
    bool IsPublished,
    int SortOrder);

public sealed record ParsedExerciseSetModel(
    string Slug,
    string Title,
    IReadOnlyList<ParsedContentMeaningModel> TitleTranslations,
    string Description,
    IReadOnlyList<ParsedContentMeaningModel> DescriptionTranslations,
    string CefrLevel,
    string OwnerType,
    string? OwnerSlug,
    IReadOnlyList<string> ExerciseSlugs,
    bool IsPublished,
    int SortOrder);
