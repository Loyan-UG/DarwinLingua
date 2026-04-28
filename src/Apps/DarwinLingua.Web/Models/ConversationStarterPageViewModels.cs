using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record ConversationStarterIndexPageViewModel(
    IReadOnlyList<ConversationStarterPackListItemModel> StarterPacks,
    ConversationStarterListFilterModel Filter);

public sealed record ConversationStarterDetailPageViewModel(
    ConversationStarterPackDetailModel StarterPack,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode);
