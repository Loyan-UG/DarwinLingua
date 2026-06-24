using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class EventPreparationQueryService(IEventPreparationRepository eventPreparationRepository) : IEventPreparationQueryService
{
    public Task<IReadOnlyList<EventPreparationPackListItemModel>> GetPublishedEventPreparationPacksAsync(
        EventPreparationListFilterModel filter,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken) =>
        eventPreparationRepository.GetPublishedEventPreparationPacksAsync(filter, targetLearningLanguageCode, cancellationToken);

    public Task<IReadOnlyList<EventPreparationPackListItemModel>> GetPublishedEventPreparationPacksForDialogueAsync(
        string dialogueSlug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken) =>
        eventPreparationRepository.GetPublishedEventPreparationPacksForDialogueAsync(dialogueSlug, targetLearningLanguageCode, cancellationToken);

    public Task<EventPreparationPackDetailModel?> GetPublishedEventPreparationPackBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken) =>
        eventPreparationRepository.GetPublishedEventPreparationPackBySlugAsync(slug, targetLearningLanguageCode, cancellationToken);
}
