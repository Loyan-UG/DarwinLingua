using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route("event-preparation-packs")]
public sealed class EventPreparationPacksController(
    IWebCatalogApiClient catalogApiClient,
    IWebEntitledFeatureAccessService featureAccessService) : Controller
{
    [HttpGet("{slug}", Name = "EventPreparationPacks_Detail")]
    [OutputCache(NoStore = true)]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return RedirectToAction("Index", "Scenarios");
        }

        if (!await featureAccessService.CanUseEventPreparationPacksAsync(cancellationToken).ConfigureAwait(false))
        {
            return Forbid();
        }

        EventPreparationPackDetailModel? preparationPack = await catalogApiClient
            .GetEventPreparationPackBySlugAsync(slug, cancellationToken)
            .ConfigureAwait(false);

        if (preparationPack is null)
        {
            return NotFound();
        }

        return View(new EventPreparationDetailPageViewModel(preparationPack));
    }
}
