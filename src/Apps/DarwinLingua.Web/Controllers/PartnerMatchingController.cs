using System.Security.Claims;
using DarwinLingua.Identity;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Authorize]
[Route("partners")]
public sealed class PartnerMatchingController(
    IWebCatalogApiClient catalogApiClient,
    IWebEntitledFeatureAccessService featureAccessService,
    IWebProductAnalyticsService? analyticsService = null) : Controller
{
    [HttpGet("", Name = "PartnerMatching_Index")]
    public async Task<IActionResult> Index(
        PartnerMatchSearchInputModel search,
        CancellationToken cancellationToken)
    {
        try
        {
            await featureAccessService.EnsureCanUsePartnerMatchingAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (FeatureAccessDeniedException exception)
        {
            analyticsService?.Record(WebProductAnalyticsEvents.PremiumFeatureDenied, "feature:partner-matching");
            return View(new PartnerMatchingPageViewModel(
                search,
                [],
                [],
                null,
                exception.Message));
        }

        string ownerEmail = GetOwnerEmail();
        IReadOnlyList<PartnerMatchProfileModel> matches = await catalogApiClient
            .SearchPartnerMatchesAsync(ownerEmail, ToRequest(search), cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<PartnerRequestModel> requests = await catalogApiClient
            .GetPartnerRequestsAsync(ownerEmail, cancellationToken)
            .ConfigureAwait(false);

        return View(new PartnerMatchingPageViewModel(
            search,
            matches,
            requests,
            TempData["StatusMessage"] as string,
            TempData["ErrorMessage"] as string));
    }

    [HttpPost("request", Name = "PartnerMatching_Request")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestPartner(
        PartnerRequestInputModel input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "The partner request is missing required fields.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await featureAccessService.EnsureCanUsePartnerMatchingAsync(cancellationToken).ConfigureAwait(false);
            PartnerRequestModel request = await catalogApiClient.SubmitPartnerRequestAsync(
                    GetOwnerEmail(),
                    new SubmitPartnerRequestRequest(input.TargetLearnerProfileId, input.OpenerTemplateKey, input.Note),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = $"Partner request sent to {request.OtherDisplayName}.";
            analyticsService?.Record(WebProductAnalyticsEvents.PartnerRequestSent);
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("requests/{requestId:guid}/state", Name = "PartnerMatching_UpdateState")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateState(
        Guid requestId,
        string actionName,
        CancellationToken cancellationToken)
    {
        try
        {
            await featureAccessService.EnsureCanUsePartnerMatchingAsync(cancellationToken).ConfigureAwait(false);
            PartnerRequestModel request = await catalogApiClient.UpdatePartnerRequestStateAsync(
                    GetOwnerEmail(),
                    requestId,
                    new PartnerRequestStateUpdateRequest(actionName),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = $"Partner request {request.Status}.";
            if (string.Equals(request.Status, "accepted", StringComparison.OrdinalIgnoreCase))
            {
                analyticsService?.Record(WebProductAnalyticsEvents.PartnerRequestAccepted);
            }
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private string GetOwnerEmail() =>
        User.FindFirstValue(ClaimTypes.Email)
        ?? User.Identity?.Name
        ?? throw new InvalidOperationException("The authenticated learner does not have an email address.");

    private static PartnerMatchSearchRequest ToRequest(PartnerMatchSearchInputModel search) =>
        new(
            search.CityRegion,
            search.InteractionPreference,
            search.GermanLevel,
            search.HelperLanguageCode,
            search.GoalKeyword);
}
