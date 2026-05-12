using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record CulturalNoteIndexPageViewModel(
    IReadOnlyList<CulturalNoteListItemModel> Notes,
    IReadOnlyList<string> CefrLevels,
    IReadOnlyList<string> Categories,
    string? SelectedCefrLevel,
    string? SelectedCategory,
    string? Query);

public sealed record CulturalNoteDetailPageViewModel(
    CulturalNoteDetailModel Note);

public sealed record AdminCulturalNotesPageViewModel(
    IReadOnlyList<CulturalNoteListItemModel> Notes);
