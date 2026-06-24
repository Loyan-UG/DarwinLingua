using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IConversationEventQueryService
{
    Task<IReadOnlyList<ConversationEventListItemModel>> GetPublishedEventsAsync(
        ConversationEventListFilterModel filter,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);

    Task<ConversationEventDetailModel?> GetPublishedEventBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);
}
