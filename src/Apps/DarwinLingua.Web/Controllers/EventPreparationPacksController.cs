using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route("event-preparation-packs")]
public sealed class EventPreparationPacksController(
    IWebCatalogApiClient catalogApiClient,
    IWebEntitledFeatureAccessService featureAccessService,
    IWebProductAnalyticsService? analyticsService = null) : Controller
{
    [HttpGet("{slug}", Name = "EventPreparationPacks_Detail")]
    [OutputCache(NoStore = true)]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        string? normalizedSlug = WebRouteInput.NormalizeSlug(slug);
        if (normalizedSlug is null)
        {
            return RedirectToAction("Index", "Scenarios");
        }

        if (!await featureAccessService.CanUseEventPreparationPacksAsync(cancellationToken).ConfigureAwait(false))
        {
            analyticsService?.Record(WebProductAnalyticsEvents.PremiumFeatureDenied, "feature:event-preparation-packs");
            return Forbid();
        }

        EventPreparationPackDetailModel? preparationPack = await catalogApiClient
            .GetEventPreparationPackBySlugAsync(normalizedSlug, cancellationToken)
            .ConfigureAwait(false);

        if (preparationPack is null)
        {
            return NotFound();
        }

        analyticsService?.Record(WebProductAnalyticsEvents.EventPreparationPackViewed, $"pack:{preparationPack.Slug}");
        bool completionRecorded = TempData is not null &&
            string.Equals(TempData["PreparationPackCompleted"] as string, preparationPack.Slug, StringComparison.Ordinal);

        return View(new EventPreparationDetailPageViewModel(
            preparationPack,
            completionRecorded));
    }

    [HttpPost("{slug}/complete", Name = "EventPreparationPacks_Complete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(string slug, CancellationToken cancellationToken)
    {
        string? normalizedSlug = WebRouteInput.NormalizeSlug(slug);
        if (normalizedSlug is null)
        {
            return RedirectToAction("Index", "Scenarios");
        }

        if (!await featureAccessService.CanUseEventPreparationPacksAsync(cancellationToken).ConfigureAwait(false))
        {
            analyticsService?.Record(WebProductAnalyticsEvents.PremiumFeatureDenied, "feature:event-preparation-packs");
            return Forbid();
        }

        EventPreparationPackDetailModel? preparationPack = await catalogApiClient
            .GetEventPreparationPackBySlugAsync(normalizedSlug, cancellationToken)
            .ConfigureAwait(false);

        if (preparationPack is null)
        {
            return NotFound();
        }

        analyticsService?.Record(WebProductAnalyticsEvents.EventPreparationPackCompleted, $"pack:{preparationPack.Slug}");
        TempData["PreparationPackCompleted"] = preparationPack.Slug;

        return RedirectToAction(nameof(Detail), new { slug = preparationPack.Slug });
    }
}
