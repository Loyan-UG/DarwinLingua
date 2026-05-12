using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record ExerciseIndexPageViewModel(
    IReadOnlyList<ExerciseSetListItemModel> ExerciseSets,
    IReadOnlyList<string> CefrLevels,
    string? SelectedCefrLevel,
    string? Query);

public sealed record ExerciseSetPageViewModel(
    ExerciseSetDetailModel ExerciseSet);

public sealed record ExerciseRunnerPageViewModel(
    ExerciseDetailModel Exercise,
    ExerciseAttemptResultModel? Result,
    string? SubmittedAnswerJson);
