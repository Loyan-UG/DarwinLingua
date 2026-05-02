using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record WordCollectionIndexPageViewModel(
    IReadOnlyList<WordCollectionListItemModel> Collections,
    string MeaningLanguageCode);

public sealed record WordCollectionDetailPageViewModel(
    WordCollectionDetailModel Collection,
    IReadOnlyList<WordBrowseCardViewModel> Words,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode);

public sealed record HomePageViewModel(
    IReadOnlyList<WordCollectionListItemModel> FeaturedCollections);
