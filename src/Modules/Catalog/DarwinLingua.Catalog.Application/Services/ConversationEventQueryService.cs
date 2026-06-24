using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class ConversationEventQueryService(IConversationEventRepository conversationEventRepository) : IConversationEventQueryService
{
    public Task<IReadOnlyList<ConversationEventListItemModel>> GetPublishedEventsAsync(
        ConversationEventListFilterModel filter,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken) =>
        conversationEventRepository.GetPublishedEventsAsync(filter, targetLearningLanguageCode, cancellationToken);

    public Task<ConversationEventDetailModel?> GetPublishedEventBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken) =>
        conversationEventRepository.GetPublishedEventBySlugAsync(slug, targetLearningLanguageCode, cancellationToken);
}
