using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record BrowseIndexViewModel(
    IReadOnlyList<TopicListItemModel> Topics,
    IReadOnlyList<string> CefrLevels,
    string UiLanguageCode,
    string MeaningLanguageCode);
