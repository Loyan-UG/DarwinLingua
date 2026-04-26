using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record WordInteractionPanelViewModel(
    Guid WordPublicId,
    bool IsFavorite,
    UserWordStateModel WordState,
    string ReturnUrl,
    bool CanUseFavorites,
    string? FavoriteLockedMessage);

public sealed record WordDetailContentViewModel(
    WordDetailModel Word,
    WordInteractionPanelViewModel Interaction);
