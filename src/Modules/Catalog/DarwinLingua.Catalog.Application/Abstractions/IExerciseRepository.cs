using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IExerciseRepository
{
    Task<IReadOnlyList<ExerciseSetListItemModel>> GetPublishedExerciseSetsAsync(ExerciseSetListFilterModel filter, string primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<ExerciseSetDetailModel?> GetPublishedExerciseSetBySlugAsync(string slug, string primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<ExerciseListItemModel>> GetPublishedExercisesAsync(ExerciseListFilterModel filter, string primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<ExerciseDetailModel?> GetPublishedExerciseBySlugAsync(string slug, string primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<Exercise?> GetPublishedExerciseEntityBySlugAsync(string slug, CancellationToken cancellationToken);

    Task SaveAttemptAsync(UserExerciseAttempt attempt, CancellationToken cancellationToken);
}
