using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class ExamPrepController(IWebCatalogApiClient catalogApiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        IReadOnlyList<ExamProfileModel> profiles = await catalogApiClient.GetExamProfilesAsync("en", cancellationToken).ConfigureAwait(false);
        IReadOnlyList<ExamPrepUnitListItemModel> units = await catalogApiClient.GetExamPrepUnitsAsync(new ExamPrepListFilterModel(null, null, null, null, null, null), "en", cancellationToken).ConfigureAwait(false);
        return View(new AdminExamPrepPageViewModel(profiles, units));
    }
}
