using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/moderation")]
public sealed class ModerationController(IWebCatalogApiClient catalogApiClient) : Controller
{
    [HttpGet("", Name = "Admin_Moderation_Index")]
    public async Task<IActionResult> Index(string? status, CancellationToken cancellationToken)
    {
        Task<IReadOnlyList<UserReportModel>> reportsTask = catalogApiClient
            .GetAdminUserReportsAsync(status, cancellationToken);
        Task<IReadOnlyList<ModerationDecisionAuditModel>> auditsTask = catalogApiClient
            .GetAdminModerationDecisionAuditsAsync(cancellationToken);

        await Task.WhenAll(reportsTask, auditsTask).ConfigureAwait(false);

        return View(new AdminModerationPageViewModel(
            status,
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
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Required decision fields are missing or invalid.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await catalogApiClient.DecideAdminUserReportAsync(
                    reportId,
                    new ModerationDecisionRequest(input.Status, input.DecisionNote, GetAdminEmail()),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = "Moderation decision saved.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private string GetAdminEmail() =>
        WebUserIdentity.TryGetEmail(User) ?? "admin@local";
}
