using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("admin/country-guidance")]
public sealed class CountryGuidanceController(IWebCatalogApiClient catalogApiClient) : Controller
{
    [HttpGet("", Name = "Admin_CountryGuidance")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        IReadOnlyList<CountryGuidanceNoteListItemModel> notes = await catalogApiClient
            .GetCountryGuidanceAsync(
                new CountryGuidanceNoteListFilterModel(null, null, null, null),
                ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                "DE",
                null,
                cancellationToken)
            .ConfigureAwait(false);

        return View(new AdminCountryGuidancePageViewModel(notes));
    }
}
