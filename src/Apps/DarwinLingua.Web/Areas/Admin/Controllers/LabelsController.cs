using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/labels")]
public sealed class LabelsController(IWebAdminOperationsQueryService operationsQueryService) : Controller
{
    [HttpGet("", Name = "Admin_Labels")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await operationsQueryService.GetLabelsAsync(cancellationToken).ConfigureAwait(false));
    }
}
