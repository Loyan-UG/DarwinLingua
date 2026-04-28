using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class ConversationEventQueryService(IConversationEventRepository conversationEventRepository) : IConversationEventQueryService
{
    public Task<IReadOnlyList<ConversationEventListItemModel>> GetPublishedEventsAsync(
        ConversationEventListFilterModel filter,
        CancellationToken cancellationToken) =>
        conversationEventRepository.GetPublishedEventsAsync(filter, cancellationToken);

    public Task<ConversationEventDetailModel?> GetPublishedEventBySlugAsync(
        string slug,
        CancellationToken cancellationToken) =>
        conversationEventRepository.GetPublishedEventBySlugAsync(slug, cancellationToken);
}
