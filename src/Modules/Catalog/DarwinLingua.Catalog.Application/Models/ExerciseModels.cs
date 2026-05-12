namespace DarwinLingua.Catalog.Application.Models;

public sealed record ExerciseSetListFilterModel(
    string? CefrLevel,
    string? OwnerType,
    string? OwnerSlug,
    string? Query);

public sealed record ExerciseListFilterModel(
    string? CefrLevel,
    string? ExerciseType,
    string? TargetSkill,
    string? OwnerType,
    string? OwnerSlug,
    string? Query);

public sealed record ExerciseSetListItemModel(
    string Slug,
    string Title,
    string Description,
    string CefrLevel,
    string OwnerType,
    string? OwnerSlug,
    int ExerciseCount);

public sealed record ExerciseSetDetailModel(
    string Slug,
    string Title,
    string Description,
    string CefrLevel,
    string OwnerType,
    string? OwnerSlug,
    IReadOnlyList<ExerciseListItemModel> Exercises);

public sealed record ExerciseListItemModel(
    string Slug,
    string Title,
    string Instruction,
    string CefrLevel,
    string ExerciseType,
    string TargetSkill,
    string OwnerType,
    string? OwnerSlug);

public sealed record ExerciseDetailModel(
    string Slug,
    string Title,
    string Instruction,
    string CefrLevel,
    string ExerciseType,
    string TargetSkill,
    string OwnerType,
    string? OwnerSlug,
    string PromptJson,
    string? Hint);

public sealed record ExerciseAttemptRequestModel(
    string SubmittedAnswerJson);

public sealed record ExerciseAttemptResultModel(
    string ExerciseSlug,
    bool IsCorrect,
    string Explanation,
    string? Hint,
    string? CommonMistakeNote,
    DateTime AttemptedAtUtc);
