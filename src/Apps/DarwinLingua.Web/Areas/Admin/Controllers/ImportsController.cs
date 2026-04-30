using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/imports")]
public sealed class ImportsController(IWebAdminOperationsQueryService operationsQueryService) : Controller
{
    [HttpGet("", Name = "Admin_Imports")]
    public async Task<IActionResult> Index(string? status, CancellationToken cancellationToken)
    {
        return View(await operationsQueryService.GetImportsAsync(NormalizeStatus(status), cancellationToken));
    }

    [HttpGet("table", Name = "Admin_ImportsTable")]
    public async Task<IActionResult> Table(string? status, CancellationToken cancellationToken)
    {
        return PartialView("_ImportsTable", await operationsQueryService.GetImportsAsync(NormalizeStatus(status), cancellationToken));
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
