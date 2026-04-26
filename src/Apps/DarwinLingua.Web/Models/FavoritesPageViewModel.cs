using DarwinLingua.Learning.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record FavoritesPageViewModel(
    IReadOnlyList<FavoriteWordListItemModel> Words,
    string MeaningLanguageCode,
    bool IsLocked,
    string? LockedMessage);
