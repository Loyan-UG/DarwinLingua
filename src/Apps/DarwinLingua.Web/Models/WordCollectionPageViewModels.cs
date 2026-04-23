using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record WordCollectionIndexPageViewModel(
    IReadOnlyList<WordCollectionListItemModel> Collections,
    string MeaningLanguageCode);

public sealed record WordCollectionDetailPageViewModel(
    WordCollectionDetailModel Collection,
    string MeaningLanguageCode);

public sealed record HomePageViewModel(
    IReadOnlyList<WordCollectionListItemModel> FeaturedCollections);
