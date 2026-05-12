using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IExerciseQueryService
{
    Task<IReadOnlyList<ExerciseSetListItemModel>> GetPublishedExerciseSetsAsync(ExerciseSetListFilterModel filter, CancellationToken cancellationToken);

    Task<ExerciseSetDetailModel?> GetPublishedExerciseSetBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<IReadOnlyList<ExerciseListItemModel>> GetPublishedExercisesAsync(ExerciseListFilterModel filter, CancellationToken cancellationToken);

    Task<ExerciseDetailModel?> GetPublishedExerciseBySlugAsync(string slug, CancellationToken cancellationToken);
}

public interface IExerciseAttemptService
{
    Task<ExerciseAttemptResultModel?> SubmitAttemptAsync(string slug, ExerciseAttemptRequestModel request, string userId, CancellationToken cancellationToken);
}
