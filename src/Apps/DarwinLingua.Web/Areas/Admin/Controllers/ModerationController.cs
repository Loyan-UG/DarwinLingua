using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/moderation")]
public sealed class ModerationController(
    IWebCatalogApiClient catalogApiClient,
    ICommunityNotificationEmailService notificationEmailService) : Controller
{
    [HttpGet("", Name = "Admin_Moderation_Index")]
    public async Task<IActionResult> Index(string? status, CancellationToken cancellationToken)
    {
        string? normalizedStatus = NormalizeStatus(status);
        Task<IReadOnlyList<UserReportModel>> reportsTask = catalogApiClient
            .GetAdminUserReportsAsync(normalizedStatus, cancellationToken);
        Task<IReadOnlyList<ModerationDecisionAuditModel>> auditsTask = catalogApiClient
            .GetAdminModerationDecisionAuditsAsync(cancellationToken);

        await Task.WhenAll(reportsTask, auditsTask).ConfigureAwait(false);

        return View(new AdminModerationPageViewModel(
            normalizedStatus,
            await reportsTask.ConfigureAwait(false),
            await auditsTask.ConfigureAwait(false),
            TempData["StatusMessage"] as string,
            TempData["ErrorMessage"] as string));
    }

    [HttpPost("{reportId:guid}/decision", Name = "Admin_Moderation_Decide")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decide(
        Guid reportId,
        AdminModerationDecisionInputModel input,
        [FromForm] string? returnStatus,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || !IsAllowedDecisionStatus(input.Status))
        {
            TempData["ErrorMessage"] = "Required decision fields are missing or invalid.";
            return RedirectToAction(nameof(Index), new { status = NormalizeStatus(returnStatus) });
        }

        try
        {
            UserReportModel report = await catalogApiClient.DecideAdminUserReportAsync(
                    reportId,
                    new ModerationDecisionRequest(input.Status.Trim(), TrimToNull(input.DecisionNote), GetAdminEmail()),
                    cancellationToken)
                .ConfigureAwait(false);

            await notificationEmailService.SendModerationReportOutcomeAsync(
                    report.ReporterEmail,
                    report.TargetType,
                    report.Status,
                    ResolveCulture(),
                    HttpContext.TraceIdentifier,
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = "Moderation decision saved.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = BuildDecisionErrorMessage(exception);
        }

        return RedirectToAction(nameof(Index), new { status = NormalizeStatus(returnStatus) });
    }

    private static bool IsAllowedDecisionStatus(string status) =>
        string.Equals(status, "reviewed", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "dismissed", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "action-taken", StringComparison.OrdinalIgnoreCase);

    private static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        string trimmed = status.Trim();
        return string.Equals(trimmed, "pending", StringComparison.OrdinalIgnoreCase) ||
            IsAllowedDecisionStatus(trimmed)
            ? trimmed
            : null;
    }

    private string GetAdminEmail() =>
        WebUserIdentity.TryGetEmail(User) ?? "admin@local";

    private string ResolveCulture() =>
        Request.HttpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()
            ?.RequestCulture.UICulture.Name
        ?? Request.Headers.AcceptLanguage.ToString()
        ?? "en";

    private static string BuildDecisionErrorMessage(InvalidOperationException exception) =>
        exception.Message.Contains("404", StringComparison.OrdinalIgnoreCase)
            ? "This moderation report is no longer available."
            : "The moderation decision could not be saved right now. Please try again.";

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
