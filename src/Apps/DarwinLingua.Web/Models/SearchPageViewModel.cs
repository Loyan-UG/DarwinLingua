namespace DarwinLingua.Web.Models;

public sealed record SearchPageViewModel(
    string Query,
    IReadOnlyList<WordBrowseCardViewModel> Results,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode,
    string? StatusMessage,
    string? ErrorMessage);
