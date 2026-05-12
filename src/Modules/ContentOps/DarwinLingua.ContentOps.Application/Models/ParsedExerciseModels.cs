namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedExerciseModel(
    string Slug,
    string Title,
    string Instruction,
    string CefrLevel,
    string ExerciseType,
    string TargetSkill,
    string OwnerType,
    string? OwnerSlug,
    string PromptJson,
    string AnswerKeyJson,
    string CorrectExplanation,
    string IncorrectExplanation,
    string? Hint,
    string? CommonMistakeNote,
    bool IsPublished,
    int SortOrder);

public sealed record ParsedExerciseSetModel(
    string Slug,
    string Title,
    string Description,
    string CefrLevel,
    string OwnerType,
    string? OwnerSlug,
    IReadOnlyList<string> ExerciseSlugs,
    bool IsPublished,
    int SortOrder);
