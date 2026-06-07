using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IExerciseQueryService
{
    Task<IReadOnlyList<ExerciseSetListItemModel>> GetPublishedExerciseSetsAsync(ExerciseSetListFilterModel filter, CancellationToken cancellationToken) =>
        GetPublishedExerciseSetsAsync(filter, "en", cancellationToken);

    Task<IReadOnlyList<ExerciseSetListItemModel>> GetPublishedExerciseSetsAsync(ExerciseSetListFilterModel filter, string primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<ExerciseSetDetailModel?> GetPublishedExerciseSetBySlugAsync(string slug, CancellationToken cancellationToken) =>
        GetPublishedExerciseSetBySlugAsync(slug, "en", cancellationToken);

    Task<ExerciseSetDetailModel?> GetPublishedExerciseSetBySlugAsync(string slug, string primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<ExerciseListItemModel>> GetPublishedExercisesAsync(ExerciseListFilterModel filter, CancellationToken cancellationToken) =>
        GetPublishedExercisesAsync(filter, "en", cancellationToken);

    Task<IReadOnlyList<ExerciseListItemModel>> GetPublishedExercisesAsync(ExerciseListFilterModel filter, string primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<ExerciseDetailModel?> GetPublishedExerciseBySlugAsync(string slug, CancellationToken cancellationToken) =>
        GetPublishedExerciseBySlugAsync(slug, "en", cancellationToken);

    Task<ExerciseDetailModel?> GetPublishedExerciseBySlugAsync(string slug, string primaryMeaningLanguageCode, CancellationToken cancellationToken);
}

public interface IExerciseAttemptService
{
    Task<ExerciseAttemptResultModel?> SubmitAttemptAsync(string slug, ExerciseAttemptRequestModel request, string userId, string primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<ExerciseAttemptResultModel?> EvaluateAttemptAsync(string slug, ExerciseAttemptRequestModel request, string primaryMeaningLanguageCode, CancellationToken cancellationToken);
}
