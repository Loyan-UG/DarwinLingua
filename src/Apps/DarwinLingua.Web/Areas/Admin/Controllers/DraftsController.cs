using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/drafts")]
public sealed class DraftsController(IWebAdminOperationsQueryService operationsQueryService) : Controller
{
    private const int MaxQueryLength = 128;

    [HttpGet("", Name = "Admin_Drafts")]
    public async Task<IActionResult> Index(string? q, CancellationToken cancellationToken)
    {
        return View(await operationsQueryService.GetDraftWordsAsync(NormalizeQuery(q), cancellationToken));
    }

    [HttpGet("table", Name = "Admin_DraftsTable")]
    public async Task<IActionResult> Table(string? q, CancellationToken cancellationToken)
    {
        return PartialView("_DraftsTable", await operationsQueryService.GetDraftWordsAsync(NormalizeQuery(q), cancellationToken));
    }

    private static string? NormalizeQuery(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length <= MaxQueryLength ? trimmed : trimmed[..MaxQueryLength];
    }
}
