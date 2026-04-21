using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
public sealed class DiagnosticsController(IWebAdminDashboardQueryService adminDashboardQueryService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await adminDashboardQueryService.GetDashboardAsync(cancellationToken));
    }
}
