using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IDialogueLessonRepository
{
    Task<IReadOnlyList<DialogueLessonListItemModel>> GetPublishedDialoguesAsync(CancellationToken cancellationToken);

    Task<DialogueLessonDetailModel?> GetPublishedDialogueBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken);
}
