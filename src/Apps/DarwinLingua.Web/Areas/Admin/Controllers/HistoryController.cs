using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/history")]
public sealed class HistoryController(IWebAdminOperationsQueryService operationsQueryService) : Controller
{
    [HttpGet("", Name = "Admin_History")]
    public async Task<IActionResult> Index(string? status, CancellationToken cancellationToken)
    {
        return View(await operationsQueryService.GetHistoryAsync(NormalizeStatus(status), cancellationToken));
    }

    [HttpGet("panel", Name = "Admin_HistoryPanel")]
    public async Task<IActionResult> Panel(string? status, CancellationToken cancellationToken)
    {
        return PartialView("_HistoryPanel", await operationsQueryService.GetHistoryAsync(NormalizeStatus(status), cancellationToken));
    }

    private static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        string trimmed = status.Trim();
        return IsAllowedPackageStatus(trimmed) ? trimmed : null;
    }

    private static bool IsAllowedPackageStatus(string status) =>
        string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Processing", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "CompletedWithWarnings", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase);
}
