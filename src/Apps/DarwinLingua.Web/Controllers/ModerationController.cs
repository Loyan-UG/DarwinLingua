using System.Security.Claims;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Authorize]
[Route("moderation")]
public sealed class ModerationController(
    IWebCatalogApiClient catalogApiClient,
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

        try
        {
            await catalogApiClient.SubmitUserReportAsync(
                    GetOwnerEmail(),
                    new SubmitUserReportRequest(
                        input.TargetType,
                        input.TargetKey,
                        input.ReportedUserEmail,
                        input.Reason,
                        input.Details),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = "Report submitted for moderation review.";
            analyticsService?.Record(WebProductAnalyticsEvents.UserReported, $"target:{input.TargetType}");
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
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

        try
        {
            await catalogApiClient.BlockUserAsync(
                    GetOwnerEmail(),
                    new BlockUserRequest(input.BlockedEmail, input.Reason, input.SourcePartnerRequestId, input.TargetLearnerProfileId),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = "Learner blocked.";
            analyticsService?.Record(WebProductAnalyticsEvents.UserBlocked);
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToSafeReturn(input.ReturnUrl);
    }

    private IActionResult RedirectToSafeReturn(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Home");

    private string GetOwnerEmail() =>
        User.FindFirstValue(ClaimTypes.Email)
        ?? User.Identity?.Name
        ?? throw new InvalidOperationException("The authenticated learner does not have an email address.");
}
