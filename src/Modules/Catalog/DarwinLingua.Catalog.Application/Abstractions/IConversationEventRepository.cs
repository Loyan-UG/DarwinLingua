using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IConversationEventRepository
{
    Task<IReadOnlyList<ConversationEventListItemModel>> GetPublishedEventsAsync(
        ConversationEventListFilterModel filter,
        CancellationToken cancellationToken);

    Task<ConversationEventDetailModel?> GetPublishedEventBySlugAsync(
        string slug,
        CancellationToken cancellationToken);
}
