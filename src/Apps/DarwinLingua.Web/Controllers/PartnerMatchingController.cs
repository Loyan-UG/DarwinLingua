using DarwinLingua.Identity;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Authorize]
[Route("partners")]
public sealed class PartnerMatchingController(
    IWebCatalogApiClient catalogApiClient,
    IWebEntitledFeatureAccessService featureAccessService,
    ICommunityNotificationEmailService notificationEmailService,
    IAccountEmailRateLimiter rateLimiter,
    ILogger<PartnerMatchingController> logger,
    IStringLocalizer<SharedResource> localizer,
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
                localizer[exception.Message].Value));
        }

        string ownerEmail = GetOwnerEmail();
        IReadOnlyList<PartnerMatchProfileModel> matches = [];
        IReadOnlyList<PartnerRequestModel> requests = [];
        string? errorMessage = TempData["ErrorMessage"] as string;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            Task<IReadOnlyList<PartnerMatchProfileModel>> matchesTask = catalogApiClient
                .SearchPartnerMatchesAsync(ownerEmail, ToRequest(search), catalogTimeout.Token);
            Task<IReadOnlyList<PartnerRequestModel>> requestsTask = catalogApiClient
                .GetPartnerRequestsAsync(ownerEmail, catalogTimeout.Token);
            await Task.WhenAll(matchesTask, requestsTask).ConfigureAwait(false);
            matches = await matchesTask.ConfigureAwait(false);
            requests = await requestsTask.ConfigureAwait(false);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested && exception is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(exception, "Partner matching data could not be loaded for {OwnerEmail}.", ownerEmail);
            errorMessage ??= localizer["Partner matching is temporarily unavailable. Please try again."].Value;
        }

        return View(new PartnerMatchingPageViewModel(
            search,
            matches,
            requests,
            TempData["StatusMessage"] as string,
            errorMessage));
    }

    [HttpPost("request", Name = "PartnerMatching_Request")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestPartner(
        PartnerRequestInputModel input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = localizer["The partner request is missing required fields."].Value;
            return RedirectToAction(nameof(Index));
        }

        if (!IsAllowedOpenerTemplateKey(input.OpenerTemplateKey))
        {
            TempData["ErrorMessage"] = localizer["The selected opener is not supported."].Value;
            return RedirectToAction(nameof(Index));
        }

        string ownerEmail = GetOwnerEmail();
        if (!rateLimiter.TryConsume("partner-request", ownerEmail, 10, TimeSpan.FromMinutes(15)))
        {
            TempData["ErrorMessage"] = localizer["Too many partner requests. Please wait a few minutes and try again."].Value;
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

            TempData["StatusMessage"] = localizer["Partner request sent to {0}.", request.OtherDisplayName].Value;
            analyticsService?.Record(WebProductAnalyticsEvents.PartnerRequestSent);
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = BuildPartnerRequestErrorMessage(exception);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested && exception is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(exception, "Partner request could not be submitted for {OwnerEmail}.", ownerEmail);
            TempData["ErrorMessage"] = localizer["The partner request could not be sent right now. Please try again."].Value;
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
                TempData["ErrorMessage"] = localizer["The selected partner request action is not supported."].Value;
                return RedirectToAction(nameof(Index));
            }

            await featureAccessService.EnsureCanUsePartnerMatchingAsync(cancellationToken).ConfigureAwait(false);
            PartnerRequestModel request = await catalogApiClient.UpdatePartnerRequestStateAsync(
                    GetOwnerEmail(),
                    requestId,
                    new PartnerRequestStateUpdateRequest(actionName),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = localizer["Partner request {0}.", request.Status].Value;
            if (string.Equals(request.Status, "accepted", StringComparison.OrdinalIgnoreCase))
            {
                analyticsService?.Record(WebProductAnalyticsEvents.PartnerRequestAccepted);
                if (!string.IsNullOrWhiteSpace(request.ContactEmail))
                {
                    try
                    {
                        await notificationEmailService.SendPartnerRequestAcceptedAsync(
                                request.ContactEmail,
                                request.OtherDisplayName,
                                ResolveCulture(),
                                HttpContext.TraceIdentifier,
                                cancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
                    {
                        logger.LogWarning(exception, "Partner acceptance email could not be sent for request {RequestId}.", requestId);
                    }
                }
            }
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = BuildPartnerRequestErrorMessage(exception);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested && exception is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(exception, "Partner request state could not be updated for {RequestId}.", requestId);
            TempData["ErrorMessage"] = localizer["The partner request could not be updated right now. Please try again."].Value;
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

    private string BuildPartnerRequestErrorMessage(InvalidOperationException exception) =>
        exception.Message.Contains("404", StringComparison.OrdinalIgnoreCase)
            ? localizer["The selected partner request or learner profile is no longer available."].Value
            : localizer["The partner request could not be updated right now. Please try again."].Value;

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
