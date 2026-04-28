using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record ConversationEventIndexPageViewModel(
    IReadOnlyList<ConversationEventListItemModel> Events,
    ConversationEventListFilterModel Filter);

public sealed record ConversationEventDetailPageViewModel(
    ConversationEventDetailModel Event,
    IReadOnlyList<EventPreparationPackListItemModel> PreparationPacks);
