using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Route("organizers")]
public sealed class OrganizerProfilesController(IWebCatalogApiClient catalogApiClient) : Controller
{
    [HttpGet("", Name = "OrganizerProfiles_Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        IReadOnlyList<OrganizerProfileListItemModel> profiles = await catalogApiClient
            .GetOrganizerProfilesAsync(cancellationToken)
            .ConfigureAwait(false);

        return View(new OrganizerProfileIndexPageViewModel(profiles));
    }

    [HttpGet("{slug}", Name = "OrganizerProfiles_Detail")]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        OrganizerProfileDetailModel? profile = await catalogApiClient
            .GetOrganizerProfileBySlugAsync(slug, cancellationToken)
            .ConfigureAwait(false);

        return profile is null
            ? NotFound()
            : View(new OrganizerProfileDetailPageViewModel(profile));
    }
}
