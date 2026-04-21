using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record WordBrowsePageViewModel(
    string Title,
    string Description,
    string? TopicKey,
    string? CefrLevel,
    IReadOnlyList<WordListItemModel> Words,
    string MeaningLanguageCode,
    int Skip,
    int Take,
    bool HasMore);
