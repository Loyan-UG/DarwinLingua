using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/publishing")]
public sealed class PublishingController(IWebAdminDashboardQueryService adminDashboardQueryService) : Controller
{
    [HttpGet("", Name = "Admin_Publishing")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await adminDashboardQueryService.GetDashboardAsync(cancellationToken));
    }
}
