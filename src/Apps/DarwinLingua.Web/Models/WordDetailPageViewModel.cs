namespace DarwinLingua.Web.Models;

public sealed record WordDetailPageViewModel(
    WordDetailContentViewModel Content,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode,
    string UiLanguageCode);
