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
    ICommunityNotificationEmailService notificationEmailService,
    IAccountEmailRateLimiter rateLimiter,
    IWebProductAnalyticsService? analyticsService = null) : Controller
{
    private const int MaxSearchTextLength = 128;

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
        Task<IReadOnlyList<PartnerMatchProfileModel>> matchesTask = catalogApiClient
            .SearchPartnerMatchesAsync(ownerEmail, ToRequest(search), cancellationToken);
        Task<IReadOnlyList<PartnerRequestModel>> requestsTask = catalogApiClient
            .GetPartnerRequestsAsync(ownerEmail, cancellationToken);
        await Task.WhenAll(matchesTask, requestsTask).ConfigureAwait(false);

        return View(new PartnerMatchingPageViewModel(
            search,
            await matchesTask.ConfigureAwait(false),
            await requestsTask.ConfigureAwait(false),
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

        if (!IsAllowedOpenerTemplateKey(input.OpenerTemplateKey))
        {
            TempData["ErrorMessage"] = "The selected opener is not supported.";
            return RedirectToAction(nameof(Index));
        }

        string ownerEmail = GetOwnerEmail();
        if (!rateLimiter.TryConsume("partner-request", ownerEmail, 10, TimeSpan.FromMinutes(15)))
        {
            TempData["ErrorMessage"] = "Too many partner requests. Please wait a few minutes and try again.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await featureAccessService.EnsureCanUsePartnerMatchingAsync(cancellationToken).ConfigureAwait(false);
            PartnerRequestModel request = await catalogApiClient.SubmitPartnerRequestAsync(
                    ownerEmail,
                    new SubmitPartnerRequestRequest(input.TargetLearnerProfileId, input.OpenerTemplateKey, input.Note?.Trim()),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = $"Partner request sent to {request.OtherDisplayName}.";
            analyticsService?.Record(WebProductAnalyticsEvents.PartnerRequestSent);
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = BuildPartnerRequestErrorMessage(exception);
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
            if (!IsAllowedPartnerRequestAction(actionName))
            {
                TempData["ErrorMessage"] = "The selected partner request action is not supported.";
                return RedirectToAction(nameof(Index));
            }

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
                if (!string.IsNullOrWhiteSpace(request.ContactEmail))
                {
                    await notificationEmailService.SendPartnerRequestAcceptedAsync(
                            request.ContactEmail,
                            request.OtherDisplayName,
                            ResolveCulture(),
                            HttpContext.TraceIdentifier,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
            }
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = BuildPartnerRequestErrorMessage(exception);
        }

        return RedirectToAction(nameof(Index));
    }

    private string GetOwnerEmail() =>
        WebUserIdentity.GetRequiredEmail(User, "The authenticated learner does not have an email address.");

    private string ResolveCulture() =>
        Request.HttpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()
            ?.RequestCulture.UICulture.Name
        ?? Request.Headers.AcceptLanguage.ToString()
        ?? "en";

    private static PartnerMatchSearchRequest ToRequest(PartnerMatchSearchInputModel search) =>
        new(
            NormalizeSearchText(search.CityRegion),
            NormalizeInteractionPreference(search.InteractionPreference),
            NormalizeGermanLevel(search.GermanLevel),
            NormalizeLanguageCode(search.HelperLanguageCode),
            NormalizeSearchText(search.GoalKeyword));

    private static bool IsAllowedOpenerTemplateKey(string openerTemplateKey) =>
        string.Equals(openerTemplateKey, "practice-goals", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(openerTemplateKey, "same-city", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(openerTemplateKey, "online-practice", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(openerTemplateKey, "event-follow-up", StringComparison.OrdinalIgnoreCase);

    private static bool IsAllowedPartnerRequestAction(string actionName) =>
        string.Equals(actionName, "accept", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(actionName, "decline", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(actionName, "block", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(actionName, "cancel", StringComparison.OrdinalIgnoreCase);

    private static string BuildPartnerRequestErrorMessage(InvalidOperationException exception) =>
        exception.Message.Contains("404", StringComparison.OrdinalIgnoreCase)
            ? "The selected partner request or learner profile is no longer available."
            : "The partner request could not be updated right now. Please try again.";

    private static string? NormalizeSearchText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length <= MaxSearchTextLength ? trimmed : trimmed[..MaxSearchTextLength];
    }

    private static string? NormalizeInteractionPreference(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return string.Equals(trimmed, "online", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "in-person", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "both", StringComparison.OrdinalIgnoreCase)
            ? trimmed
            : null;
    }

    private static string? NormalizeGermanLevel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return string.Equals(trimmed, "A1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "A2", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "B1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "B2", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "C1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "C2", StringComparison.OrdinalIgnoreCase)
            ? trimmed
            : null;
    }

    private static string? NormalizeLanguageCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length is >= 2 and <= 8 &&
            trimmed.All(static character =>
                (character >= 'a' && character <= 'z') ||
                (character >= 'A' && character <= 'Z') ||
                character == '-')
            ? trimmed
            : null;
    }
}
