using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record AdminExerciseSetsPageViewModel(
    IReadOnlyList<ExerciseSetListItemModel> ExerciseSets);
