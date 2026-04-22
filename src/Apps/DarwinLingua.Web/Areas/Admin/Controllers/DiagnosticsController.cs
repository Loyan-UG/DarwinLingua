using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/diagnostics")]
public sealed class DiagnosticsController(IWebAdminDashboardQueryService adminDashboardQueryService) : Controller
{
    [HttpGet("", Name = "Admin_Diagnostics")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await adminDashboardQueryService.GetDashboardAsync(cancellationToken));
    }
}
