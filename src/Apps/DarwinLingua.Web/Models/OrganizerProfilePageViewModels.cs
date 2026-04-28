using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record OrganizerProfileIndexPageViewModel(
    IReadOnlyList<OrganizerProfileListItemModel> Profiles);

public sealed record OrganizerProfileDetailPageViewModel(
    OrganizerProfileDetailModel Profile);
