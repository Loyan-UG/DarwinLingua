using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IExerciseRepository
{
    Task<IReadOnlyList<ExerciseSetListItemModel>> GetPublishedExerciseSetsAsync(ExerciseSetListFilterModel filter, CancellationToken cancellationToken);

    Task<ExerciseSetDetailModel?> GetPublishedExerciseSetBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<IReadOnlyList<ExerciseListItemModel>> GetPublishedExercisesAsync(ExerciseListFilterModel filter, CancellationToken cancellationToken);

    Task<ExerciseDetailModel?> GetPublishedExerciseBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<Exercise?> GetPublishedExerciseEntityBySlugAsync(string slug, CancellationToken cancellationToken);

    Task SaveAttemptAsync(UserExerciseAttempt attempt, CancellationToken cancellationToken);
}
