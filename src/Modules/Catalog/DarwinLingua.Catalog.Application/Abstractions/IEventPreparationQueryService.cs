using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IEventPreparationQueryService
{
    Task<IReadOnlyList<EventPreparationPackListItemModel>> GetPublishedEventPreparationPacksAsync(
        EventPreparationListFilterModel filter,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<EventPreparationPackListItemModel>> GetPublishedEventPreparationPacksForDialogueAsync(
        string dialogueSlug,
        CancellationToken cancellationToken);

    Task<EventPreparationPackDetailModel?> GetPublishedEventPreparationPackBySlugAsync(
        string slug,
        CancellationToken cancellationToken);
}
