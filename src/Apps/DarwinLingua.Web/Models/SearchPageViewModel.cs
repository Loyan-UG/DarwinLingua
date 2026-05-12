namespace DarwinLingua.Web.Models;

using DarwinLingua.Catalog.Application.Models;

public sealed record SearchPageViewModel(
    string Query,
    IReadOnlyList<WordBrowseCardViewModel> Results,
    IReadOnlyList<UnifiedLearningSearchResultModel> LearningResults,
    IReadOnlyList<string> ResultTypes,
    IReadOnlyList<string> CefrLevels,
    string? SelectedResultType,
    string? SelectedCefrLevel,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode,
    string? StatusMessage,
    string? ErrorMessage);
