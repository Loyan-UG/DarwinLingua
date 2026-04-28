using System.Security.Claims;
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
        IReadOnlyList<UserReportModel> reports = await catalogApiClient
            .GetAdminUserReportsAsync(status, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<ModerationDecisionAuditModel> audits = await catalogApiClient
            .GetAdminModerationDecisionAuditsAsync(cancellationToken)
            .ConfigureAwait(false);

        return View(new AdminModerationPageViewModel(
            status,
            reports,
            audits,
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

    private string GetAdminEmail()
    {
        string? candidate = User.FindFirstValue(ClaimTypes.Email)
            ?? User.Identity?.Name;

        return !string.IsNullOrWhiteSpace(candidate) && candidate.Contains('@', StringComparison.Ordinal)
            ? candidate
            : "admin@local";
    }
}
