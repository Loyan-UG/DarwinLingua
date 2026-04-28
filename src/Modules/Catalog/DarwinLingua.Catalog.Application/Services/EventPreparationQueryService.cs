using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class EventPreparationQueryService(IEventPreparationRepository eventPreparationRepository) : IEventPreparationQueryService
{
    public Task<IReadOnlyList<EventPreparationPackListItemModel>> GetPublishedEventPreparationPacksAsync(
        EventPreparationListFilterModel filter,
        CancellationToken cancellationToken) =>
        eventPreparationRepository.GetPublishedEventPreparationPacksAsync(filter, cancellationToken);

    public Task<IReadOnlyList<EventPreparationPackListItemModel>> GetPublishedEventPreparationPacksForScenarioAsync(
        string scenarioSlug,
        CancellationToken cancellationToken) =>
        eventPreparationRepository.GetPublishedEventPreparationPacksForScenarioAsync(scenarioSlug, cancellationToken);

    public Task<EventPreparationPackDetailModel?> GetPublishedEventPreparationPackBySlugAsync(
        string slug,
        CancellationToken cancellationToken) =>
        eventPreparationRepository.GetPublishedEventPreparationPackBySlugAsync(slug, cancellationToken);
}
