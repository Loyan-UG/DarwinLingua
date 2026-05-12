using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Route("exercises")]
public sealed class ExercisesController(
    IWebCatalogApiClient catalogApiClient,
    IStringLocalizer<SharedResource> localizer,
    ILogger<ExercisesController> logger) : Controller
{
    [HttpGet("", Name = "Exercises_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(string? cefrLevel, string? q, CancellationToken cancellationToken)
    {
        ExerciseSetListFilterModel filter = new(LearningPortalFilterConventions.NormalizeCefrLevel(cefrLevel), null, null, string.IsNullOrWhiteSpace(q) ? null : q.Trim());
        IReadOnlyList<ExerciseSetListItemModel> sets;
        try
        {
            sets = await catalogApiClient.GetExerciseSetsAsync(filter, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is HttpRequestException or OperationCanceledException)
        {
            logger.LogWarning(ex, "Exercise sets could not be loaded.");
            sets = [];
        }

        return View(new ExerciseIndexPageViewModel(sets, LearningPortalFilterConventions.CefrLevels, filter.CefrLevel, filter.Query));
    }

    [HttpGet("sets/{slug}", Name = "ExerciseSets_Detail")]
    public async Task<IActionResult> Set(string slug, CancellationToken cancellationToken)
    {
        ExerciseSetDetailModel? set = await catalogApiClient.GetExerciseSetBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
        return set is null ? NotFound() : View("Set", new ExerciseSetPageViewModel(set));
    }

    [HttpGet("{slug}", Name = "Exercises_Detail")]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        ExerciseDetailModel? exercise = await catalogApiClient.GetExerciseBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
        return exercise is null ? NotFound() : View(new ExerciseRunnerPageViewModel(exercise, null, null));
    }

    [HttpPost("{slug}", Name = "Exercises_Submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(string slug, string submittedAnswerJson, CancellationToken cancellationToken)
    {
        ExerciseDetailModel? exercise = await catalogApiClient.GetExerciseBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
        if (exercise is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(submittedAnswerJson))
        {
            ModelState.AddModelError(nameof(submittedAnswerJson), localizer["Answer is required."].Value);
            return View("Detail", new ExerciseRunnerPageViewModel(exercise, null, submittedAnswerJson));
        }

        ExerciseAttemptResultModel? result = await catalogApiClient
            .SubmitExerciseAttemptAsync(slug, new ExerciseAttemptRequestModel(submittedAnswerJson), cancellationToken)
            .ConfigureAwait(false);

        return result is null
            ? NotFound()
            : View("Detail", new ExerciseRunnerPageViewModel(exercise, result, submittedAnswerJson));
    }
}
