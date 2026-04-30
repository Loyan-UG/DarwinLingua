using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Authorize]
[Route("moderation")]
public sealed class ModerationController(
    IWebCatalogApiClient catalogApiClient,
    ICommunityNotificationEmailService notificationEmailService,
    IAccountEmailRateLimiter rateLimiter,
    IWebProductAnalyticsService? analyticsService = null) : Controller
{
    [HttpPost("reports", Name = "Moderation_Report")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Report(
        UserReportInputModel input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Required report fields are missing or invalid.";
            return RedirectToSafeReturn(input.ReturnUrl);
        }

        string targetType = input.TargetType.Trim();
        string? targetKey = NormalizeTargetKey(targetType, input.TargetKey);
        string reason = input.Reason.Trim();
        if (!IsAllowedTargetType(targetType) || !IsAllowedReportReason(reason) || targetKey is null)
        {
            TempData["ErrorMessage"] = "The selected report target or reason is not supported.";
            return RedirectToSafeReturn(input.ReturnUrl);
        }

        string ownerEmail = GetOwnerEmail();
        if (!rateLimiter.TryConsume("moderation-report", ownerEmail, 10, TimeSpan.FromMinutes(15)))
        {
            TempData["ErrorMessage"] = "Too many reports submitted. Please wait a few minutes and try again.";
            return RedirectToSafeReturn(input.ReturnUrl);
        }

        try
        {
            await catalogApiClient.SubmitUserReportAsync(
                    ownerEmail,
                    new SubmitUserReportRequest(
                        targetType,
                        targetKey,
                        TrimToNull(input.ReportedUserEmail),
                        reason,
                        input.Details.Trim()),
                    cancellationToken)
                .ConfigureAwait(false);
            if (IsHighSeverityReason(reason))
            {
                await notificationEmailService.SendAdminHighSeverityReportAsync(
                        reason,
                        targetType,
                        targetKey,
                        ResolveCulture(),
                        HttpContext.TraceIdentifier,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            TempData["StatusMessage"] = "Report submitted for moderation review.";
            analyticsService?.Record(WebProductAnalyticsEvents.UserReported, $"target:{targetType}");
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = BuildModerationErrorMessage(exception);
        }

        return RedirectToSafeReturn(input.ReturnUrl);
    }

    [HttpPost("blocks", Name = "Moderation_Block")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(
        UserBlockInputModel input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Required block fields are missing or invalid.";
            return RedirectToSafeReturn(input.ReturnUrl);
        }

        if (string.IsNullOrWhiteSpace(input.BlockedEmail) &&
            !input.SourcePartnerRequestId.HasValue &&
            !input.TargetLearnerProfileId.HasValue)
        {
            TempData["ErrorMessage"] = "A learner email, partner request, or learner profile is required to block.";
            return RedirectToSafeReturn(input.ReturnUrl);
        }

        if (input.SourcePartnerRequestId == Guid.Empty || input.TargetLearnerProfileId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "The selected block target is not supported.";
            return RedirectToSafeReturn(input.ReturnUrl);
        }

        string ownerEmail = GetOwnerEmail();
        if (!rateLimiter.TryConsume("moderation-block", ownerEmail, 10, TimeSpan.FromMinutes(15)))
        {
            TempData["ErrorMessage"] = "Too many block attempts. Please wait a few minutes and try again.";
            return RedirectToSafeReturn(input.ReturnUrl);
        }

        try
        {
            await catalogApiClient.BlockUserAsync(
                    ownerEmail,
                    new BlockUserRequest(
                        TrimToNull(input.BlockedEmail),
                        TrimToNull(input.Reason),
                        input.SourcePartnerRequestId,
                        input.TargetLearnerProfileId),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = "Learner blocked.";
            analyticsService?.Record(WebProductAnalyticsEvents.UserBlocked);
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = BuildModerationErrorMessage(exception);
        }

        return RedirectToSafeReturn(input.ReturnUrl);
    }

    private IActionResult RedirectToSafeReturn(string? returnUrl)
    {
        string? normalizedReturnUrl = WebRouteInput.NormalizeLocalReturnUrl(returnUrl);
        return normalizedReturnUrl is not null && Url.IsLocalUrl(normalizedReturnUrl)
            ? LocalRedirect(normalizedReturnUrl)
            : RedirectToAction("Index", "Home");
    }

    private string GetOwnerEmail() =>
        WebUserIdentity.GetRequiredEmail(User, "The authenticated learner does not have an email address.");

    private string ResolveCulture() =>
        Request.HttpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()
            ?.RequestCulture.UICulture.Name
        ?? Request.Headers.AcceptLanguage.ToString()
        ?? "en";

    private static bool IsHighSeverityReason(string reason) =>
        string.Equals(reason, "harassment", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(reason, "unsafe-contact", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(reason, "impersonation", StringComparison.OrdinalIgnoreCase);

    private static bool IsAllowedTargetType(string targetType) =>
        string.Equals(targetType, "conversation-event", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(targetType, "learner-profile", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(targetType, "partner-request", StringComparison.OrdinalIgnoreCase);

    private static bool IsAllowedReportReason(string reason) =>
        string.Equals(reason, "inaccurate-listing", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(reason, "spam", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(reason, "harassment", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(reason, "unsafe-contact", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(reason, "impersonation", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(reason, "other", StringComparison.OrdinalIgnoreCase);

    private static string BuildModerationErrorMessage(InvalidOperationException exception) =>
        exception.Message.Contains("404", StringComparison.OrdinalIgnoreCase)
            ? "The selected moderation target is no longer available."
            : "The moderation request could not be saved right now. Please try again.";

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeTargetKey(string targetType, string targetKey)
    {
        if (string.Equals(targetType, "conversation-event", StringComparison.OrdinalIgnoreCase))
        {
            return WebRouteInput.NormalizeSlug(targetKey);
        }

        if (string.Equals(targetType, "learner-profile", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(targetType, "partner-request", StringComparison.OrdinalIgnoreCase))
        {
            return Guid.TryParse(targetKey.Trim(), out Guid parsedId) && parsedId != Guid.Empty
                ? parsedId.ToString("D")
                : null;
        }

        return null;
    }
}
