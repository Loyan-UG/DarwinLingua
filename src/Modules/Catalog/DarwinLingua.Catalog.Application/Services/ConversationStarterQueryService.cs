using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class ConversationStarterQueryService(IConversationStarterRepository conversationStarterRepository) : IConversationStarterQueryService
{
    public Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetPublishedStarterPacksAsync(
        ConversationStarterListFilterModel filter,
        CancellationToken cancellationToken) =>
        conversationStarterRepository.GetPublishedStarterPacksAsync(filter, cancellationToken);

    public Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetPublishedStarterPacksForScenarioAsync(
        string scenarioSlug,
        CancellationToken cancellationToken) =>
        conversationStarterRepository.GetPublishedStarterPacksForScenarioAsync(scenarioSlug, cancellationToken);

    public Task<ConversationStarterPackDetailModel?> GetPublishedStarterPackBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        conversationStarterRepository.GetPublishedStarterPackBySlugAsync(
            slug,
            primaryMeaningLanguageCode,
            secondaryMeaningLanguageCode,
            cancellationToken);
}
