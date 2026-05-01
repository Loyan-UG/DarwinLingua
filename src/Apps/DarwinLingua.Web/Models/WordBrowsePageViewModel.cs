using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record WordBrowsePageViewModel(
    string Title,
    string Description,
    string? TopicKey,
    string? CefrLevel,
    IReadOnlyList<WordBrowseCardViewModel> Words,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode,
    int Skip,
    int Take,
    bool HasMore);

public sealed record WordBrowseCardViewModel(
    WordListItemModel Word,
    string? SecondaryMeaning,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode,
    WordInteractionPanelViewModel Interaction);
