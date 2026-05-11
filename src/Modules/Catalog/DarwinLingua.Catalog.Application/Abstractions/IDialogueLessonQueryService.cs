using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IDialogueLessonQueryService
{
    Task<IReadOnlyList<DialogueLessonListItemModel>> GetPublishedDialoguesAsync(
        DialogueLessonListFilterModel filter,
        CancellationToken cancellationToken);

    Task<DialogueLessonDetailModel?> GetPublishedDialogueBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken);
}
