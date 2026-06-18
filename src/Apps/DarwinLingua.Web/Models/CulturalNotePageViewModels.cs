using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record CulturalNoteIndexPageViewModel(
    IReadOnlyList<CulturalNoteListItemPageViewModel> Notes,
    IReadOnlyList<string> CefrLevels,
    IReadOnlyList<string> Categories,
    string? SelectedCefrLevel,
    string? SelectedCategory,
    string? Query,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode);

public sealed record CulturalNoteDetailPageViewModel(
    CulturalNoteDetailModel Note,
    CulturalNoteDetailModel? SecondaryLanguageNote,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode);

public sealed record CulturalNoteListItemPageViewModel(
    CulturalNoteListItemModel Note,
    CulturalNoteListItemModel? SecondaryLanguageNote);

public sealed record AdminCulturalNotesPageViewModel(
    IReadOnlyList<CulturalNoteListItemModel> Notes);
