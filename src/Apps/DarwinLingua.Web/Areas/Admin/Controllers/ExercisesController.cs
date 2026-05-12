using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class ExercisesController(IWebCatalogApiClient catalogApiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        IReadOnlyList<ExerciseSetListItemModel> exerciseSets = await catalogApiClient
            .GetExerciseSetsAsync(new ExerciseSetListFilterModel(null, null, null, null), cancellationToken)
            .ConfigureAwait(false);

        return View(new AdminExerciseSetsPageViewModel(exerciseSets));
    }
}
