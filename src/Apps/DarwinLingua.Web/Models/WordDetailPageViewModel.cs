using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record WordDetailPageViewModel(
    WordDetailModel Word,
    bool IsFavorite,
    UserWordStateModel WordState,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode,
    string UiLanguageCode);
