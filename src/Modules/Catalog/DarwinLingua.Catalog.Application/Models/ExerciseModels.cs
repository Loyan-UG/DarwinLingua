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
    string? LearnerLanguageTitle,
    string Description,
    string? LearnerLanguageDescription,
    string CefrLevel,
    string OwnerType,
    string? OwnerSlug,
    int ExerciseCount);

public sealed record ExerciseSetDetailModel(
    string Slug,
    string Title,
    string? LearnerLanguageTitle,
    string Description,
    string? LearnerLanguageDescription,
    string CefrLevel,
    string OwnerType,
    string? OwnerSlug,
    IReadOnlyList<ExerciseListItemModel> Exercises);

public sealed record ExerciseListItemModel(
    string Slug,
    string Title,
    string? LearnerLanguageTitle,
    string Instruction,
    string? LearnerLanguageInstruction,
    string CefrLevel,
    string ExerciseType,
    string TargetSkill,
    string OwnerType,
    string? OwnerSlug);

public sealed record ExerciseDetailModel(
    string Slug,
    string Title,
    string? LearnerLanguageTitle,
    string Instruction,
    string? LearnerLanguageInstruction,
    string CefrLevel,
    string ExerciseType,
    string TargetSkill,
    string OwnerType,
    string? OwnerSlug,
    string PromptJson,
    string? Hint,
    string? LearnerLanguageHint);

public sealed record ExerciseAttemptRequestModel(
    string SubmittedAnswerJson);

public sealed record ExerciseAttemptResultModel(
    string ExerciseSlug,
    bool IsCorrect,
    string Explanation,
    string? LearnerLanguageExplanation,
    string? Hint,
    string? LearnerLanguageHint,
    string? CommonMistakeNote,
    string? LearnerLanguageCommonMistakeNote,
    DateTime AttemptedAtUtc);
