using Microsoft.AspNetCore.Mvc;
using DarwinLingua.Web.Services;
using DarwinLingua.Web.Models;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
public sealed class DashboardController(IWebAdminDashboardQueryService adminDashboardQueryService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        AdminDashboardViewModel viewModel = await adminDashboardQueryService.GetDashboardAsync(cancellationToken);
        return View(viewModel);
    }
}
