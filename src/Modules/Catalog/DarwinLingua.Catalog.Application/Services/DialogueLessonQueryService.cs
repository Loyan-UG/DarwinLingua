using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class DialogueLessonQueryService(IDialogueLessonRepository dialogueLessonRepository) : IDialogueLessonQueryService
{
    public Task<IReadOnlyList<DialogueLessonListItemModel>> GetPublishedDialoguesAsync(
        DialogueLessonListFilterModel filter,
        CancellationToken cancellationToken) =>
        dialogueLessonRepository.GetPublishedDialoguesAsync(filter, cancellationToken);

    public Task<DialogueLessonDetailModel?> GetPublishedDialogueBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        dialogueLessonRepository.GetPublishedDialogueBySlugAsync(
            slug,
            primaryMeaningLanguageCode,
            secondaryMeaningLanguageCode,
            cancellationToken);
}
