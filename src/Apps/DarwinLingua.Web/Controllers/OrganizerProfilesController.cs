using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route("organizers")]
public sealed class OrganizerProfilesController(
    IWebCatalogApiClient catalogApiClient,
    ICommunityNotificationEmailService notificationEmailService,
    IAccountEmailRateLimiter rateLimiter,
    IWebProductAnalyticsService? analyticsService = null) : Controller
{
    [HttpGet("", Name = "OrganizerProfiles_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
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
        string? normalizedSlug = WebRouteInput.NormalizeSlug(slug);
        if (normalizedSlug is null)
        {
            return RedirectToAction(nameof(Index));
        }

        OrganizerProfileDetailModel? profile = await catalogApiClient
            .GetOrganizerProfileBySlugAsync(normalizedSlug, cancellationToken)
            .ConfigureAwait(false);

        return profile is null
            ? NotFound()
            : RenderDetail(profile);
    }

    [HttpGet("{slug}/claim", Name = "OrganizerProfiles_Claim")]
    public async Task<IActionResult> Claim(string slug, CancellationToken cancellationToken)
    {
        string? normalizedSlug = WebRouteInput.NormalizeSlug(slug);
        if (normalizedSlug is null)
        {
            return RedirectToAction(nameof(Index));
        }

        OrganizerProfileDetailModel? profile = await catalogApiClient
            .GetOrganizerProfileBySlugAsync(normalizedSlug, cancellationToken)
            .ConfigureAwait(false);

        return profile is null
            ? NotFound()
            : View(new OrganizerProfileClaimPageViewModel(
                profile,
                new OrganizerClaimInputModel
                {
                    RequesterEmail = WebUserIdentity.TryGetEmail(User) ?? string.Empty,
                },
                TempData["StatusMessage"] as string,
                TempData["ErrorMessage"] as string));
    }

    [HttpPost("{slug}/claim", Name = "OrganizerProfiles_SubmitClaim")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitClaim(
        string slug,
        OrganizerClaimInputModel input,
        CancellationToken cancellationToken)
    {
        string? normalizedSlug = WebRouteInput.NormalizeSlug(slug);
        if (normalizedSlug is null)
        {
            return RedirectToAction(nameof(Index));
        }

        OrganizerProfileDetailModel? profile = await catalogApiClient
            .GetOrganizerProfileBySlugAsync(normalizedSlug, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("Claim", new OrganizerProfileClaimPageViewModel(
                profile,
                input,
                null,
                "Required claim fields are missing or invalid."));
        }

        string requesterEmail = input.RequesterEmail.Trim();
        if (!rateLimiter.TryConsume("organizer-claim", $"{normalizedSlug}:{requesterEmail}", 3, TimeSpan.FromHours(1)))
        {
            return View("Claim", new OrganizerProfileClaimPageViewModel(
                profile,
                input,
                null,
                "Too many claim attempts. Please wait before submitting again."));
        }

        try
        {
            await catalogApiClient.SubmitOrganizerClaimRequestAsync(
                    normalizedSlug,
                    new SubmitOrganizerClaimRequest(
                        input.RequesterName.Trim(),
                        requesterEmail,
                        input.RelationshipToOrganizer.Trim(),
                        input.EvidenceText.Trim()),
                    cancellationToken)
                .ConfigureAwait(false);

            await notificationEmailService.SendOrganizerClaimSubmittedAsync(
                    requesterEmail,
                    profile.DisplayName,
                    ResolveCulture(),
                    HttpContext.TraceIdentifier,
                    cancellationToken)
                .ConfigureAwait(false);
            await notificationEmailService.SendAdminNewOrganizerClaimAsync(
                    profile.DisplayName,
                    input.RequesterName,
                    ResolveCulture(),
                    HttpContext.TraceIdentifier,
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = "Claim request submitted for review.";
            return RedirectToAction(nameof(Detail), new { slug = normalizedSlug });
        }
        catch (InvalidOperationException exception)
        {
            return View("Claim", new OrganizerProfileClaimPageViewModel(
                profile,
                input,
                null,
                BuildClaimErrorMessage(exception)));
        }
    }

    private ViewResult RenderDetail(OrganizerProfileDetailModel profile)
    {
        analyticsService?.Record(WebProductAnalyticsEvents.OrganizerProfileViewed, $"organizer:{profile.Slug}");

        return View(new OrganizerProfileDetailPageViewModel(
            profile,
            TempData["StatusMessage"] as string,
            TempData["ErrorMessage"] as string));
    }

    private string ResolveCulture() =>
        Request.HttpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()
            ?.RequestCulture.UICulture.Name
        ?? Request.Headers.AcceptLanguage.ToString()
        ?? "en";

    private static string BuildClaimErrorMessage(InvalidOperationException exception) =>
        exception.Message.Contains("404", StringComparison.OrdinalIgnoreCase)
            ? "This organizer profile is no longer available."
            : "The claim request could not be submitted right now. Please try again.";
}
