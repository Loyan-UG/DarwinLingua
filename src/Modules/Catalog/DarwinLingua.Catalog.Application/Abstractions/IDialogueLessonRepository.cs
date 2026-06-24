using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IDialogueLessonRepository
{
    Task<IReadOnlyList<DialogueLessonListItemModel>> GetPublishedDialoguesAsync(
        DialogueLessonListFilterModel filter,
        CancellationToken cancellationToken) =>
        GetPublishedDialoguesAsync(filter, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, cancellationToken);

    Task<IReadOnlyList<DialogueLessonListItemModel>> GetPublishedDialoguesAsync(
        DialogueLessonListFilterModel filter,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);

    Task<DialogueLessonDetailModel?> GetPublishedDialogueBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        GetPublishedDialogueBySlugAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, primaryMeaningLanguageCode, secondaryMeaningLanguageCode, cancellationToken);

    Task<DialogueLessonDetailModel?> GetPublishedDialogueBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken);
}
