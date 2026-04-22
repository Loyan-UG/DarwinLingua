using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DarwinLingua.Web.Services;
using DarwinLingua.Web.Models;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin")]
public sealed class DashboardController(IWebAdminDashboardQueryService adminDashboardQueryService) : Controller
{
    [HttpGet("", Name = "Admin_Dashboard")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        AdminDashboardViewModel viewModel = await adminDashboardQueryService.GetDashboardAsync(cancellationToken);
        return View(viewModel);
    }
}
