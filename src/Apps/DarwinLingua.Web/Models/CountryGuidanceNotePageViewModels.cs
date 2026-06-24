using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record CountryGuidanceNoteIndexPageViewModel(
    IReadOnlyList<CountryGuidanceNoteListItemPageViewModel> Notes,
    string CountryContextCode,
    IReadOnlyList<string> CefrLevels,
    IReadOnlyList<string> Categories,
    string? SelectedCefrLevel,
    string? SelectedCategory,
    string? Query,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode);

public sealed record CountryGuidanceNoteDetailPageViewModel(
    CountryGuidanceNoteDetailModel Note,
    CountryGuidanceNoteDetailModel? SecondaryLanguageNote,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode);

public sealed record CountryGuidanceNoteListItemPageViewModel(
    CountryGuidanceNoteListItemModel Note,
    CountryGuidanceNoteListItemModel? SecondaryLanguageNote);

public sealed record AdminCountryGuidancePageViewModel(
    IReadOnlyList<CountryGuidanceNoteListItemModel> Notes);
