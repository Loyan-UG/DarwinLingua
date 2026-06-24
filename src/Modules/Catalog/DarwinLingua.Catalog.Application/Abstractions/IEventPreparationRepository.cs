using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IEventPreparationRepository
{
    Task<IReadOnlyList<EventPreparationPackListItemModel>> GetPublishedEventPreparationPacksAsync(
        EventPreparationListFilterModel filter,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<EventPreparationPackListItemModel>> GetPublishedEventPreparationPacksForDialogueAsync(
        string dialogueSlug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);

    Task<EventPreparationPackDetailModel?> GetPublishedEventPreparationPackBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);
}
