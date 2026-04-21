using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record SearchPageViewModel(
    string Query,
    IReadOnlyList<WordListItemModel> Results,
    string MeaningLanguageCode);
