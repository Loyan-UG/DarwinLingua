using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Admin")]
[Route("admin/rollback")]
public sealed class RollbackController(IWebAdminOperationsQueryService operationsQueryService) : Controller
{
    [HttpGet("", Name = "Admin_Rollback")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await operationsQueryService.GetRollbackPreviewAsync(cancellationToken));
    }
}
