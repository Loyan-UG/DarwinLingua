using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Route("organizers")]
public sealed class OrganizerProfilesController(
    IWebCatalogApiClient catalogApiClient,
    IWebProductAnalyticsService? analyticsService = null) : Controller
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
            : RenderDetail(profile);
    }

    [HttpGet("{slug}/claim", Name = "OrganizerProfiles_Claim")]
    public async Task<IActionResult> Claim(string slug, CancellationToken cancellationToken)
    {
        OrganizerProfileDetailModel? profile = await catalogApiClient
            .GetOrganizerProfileBySlugAsync(slug, cancellationToken)
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
        OrganizerProfileDetailModel? profile = await catalogApiClient
            .GetOrganizerProfileBySlugAsync(slug, cancellationToken)
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

        try
        {
            await catalogApiClient.SubmitOrganizerClaimRequestAsync(
                    slug,
                    new SubmitOrganizerClaimRequest(
                        input.RequesterName,
                        input.RequesterEmail,
                        input.RelationshipToOrganizer,
                        input.EvidenceText),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = "Claim request submitted for review.";
            return RedirectToAction(nameof(Detail), new { slug });
        }
        catch (InvalidOperationException exception)
        {
            return View("Claim", new OrganizerProfileClaimPageViewModel(profile, input, null, exception.Message));
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
}
