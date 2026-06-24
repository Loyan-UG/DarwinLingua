using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class CoursesController(IWebCatalogApiClient catalogApiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        IReadOnlyList<CoursePathListItemModel> courses = await catalogApiClient
            .GetCoursesAsync(
                new CoursePathListFilterModel(null, null),
                ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
                "en",
                cancellationToken)
            .ConfigureAwait(false);

        return View(new AdminCoursesPageViewModel(courses));
    }
}
