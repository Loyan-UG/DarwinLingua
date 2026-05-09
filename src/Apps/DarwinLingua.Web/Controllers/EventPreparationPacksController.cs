using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Route("event-preparation-packs")]
public sealed class EventPreparationPacksController(
    IWebCatalogApiClient catalogApiClient,
    IWebEntitledFeatureAccessService featureAccessService,
    ILogger<EventPreparationPacksController> logger,
    IStringLocalizer<SharedResource> localizer,
    IWebProductAnalyticsService? analyticsService = null) : Controller
{
    [HttpGet("{slug}", Name = "EventPreparationPacks_Detail")]
    [OutputCache(NoStore = true)]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        string? normalizedSlug = WebRouteInput.NormalizeSlug(slug);
        if (normalizedSlug is null)
        {
            return RedirectToAction("Index", "Dialogues");
        }

        if (!await featureAccessService.CanUseEventPreparationPacksAsync(cancellationToken).ConfigureAwait(false))
        {
            analyticsService?.Record(WebProductAnalyticsEvents.PremiumFeatureDenied, "feature:event-preparation-packs");
            return Forbid();
        }

        EventPreparationPackDetailModel? preparationPack;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            preparationPack = await catalogApiClient
                .GetEventPreparationPackBySlugAsync(normalizedSlug, catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Event preparation pack could not be loaded for {Slug}.", normalizedSlug);
            return ServiceUnavailableView(
                localizer["Preparation pack is temporarily unavailable"],
                localizer["This preparation pack could not be loaded right now. Please try again from dialogues or events."]);
        }

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
            return RedirectToAction("Index", "Dialogues");
        }

        if (!await featureAccessService.CanUseEventPreparationPacksAsync(cancellationToken).ConfigureAwait(false))
        {
            analyticsService?.Record(WebProductAnalyticsEvents.PremiumFeatureDenied, "feature:event-preparation-packs");
            return Forbid();
        }

        EventPreparationPackDetailModel? preparationPack;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            preparationPack = await catalogApiClient
                .GetEventPreparationPackBySlugAsync(normalizedSlug, catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Event preparation completion could not load pack {Slug}.", normalizedSlug);
            TempData["ErrorMessage"] = localizer["Preparation progress could not be saved right now. Please try again."].Value;
            return RedirectToAction("Index", "Dialogues");
        }

        if (preparationPack is null)
        {
            return NotFound();
        }

        analyticsService?.Record(WebProductAnalyticsEvents.EventPreparationPackCompleted, $"pack:{preparationPack.Slug}");
        TempData["PreparationPackCompleted"] = preparationPack.Slug;

        return RedirectToAction(nameof(Detail), new { slug = preparationPack.Slug });
    }

    private ViewResult ServiceUnavailableView(string title, string message)
    {
        Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
        {
            Title = title,
            Message = message,
            RequestId = HttpContext.TraceIdentifier
        });
    }
}
