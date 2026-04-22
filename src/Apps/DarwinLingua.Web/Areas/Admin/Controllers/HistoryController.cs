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
        return View(await operationsQueryService.GetHistoryAsync(status, cancellationToken));
    }

    [HttpGet("panel", Name = "Admin_HistoryPanel")]
    public async Task<IActionResult> Panel(string? status, CancellationToken cancellationToken)
    {
        return PartialView("_HistoryPanel", await operationsQueryService.GetHistoryAsync(status, cancellationToken));
    }
}
