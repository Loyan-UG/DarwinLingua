using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;
using System.Text.Json;

namespace DarwinLingua.Web.Controllers;

[Route("exercises")]
public sealed class ExercisesController(
    IWebCatalogApiClient catalogApiClient,
    IStringLocalizer<SharedResource> localizer,
    ILogger<ExercisesController> logger) : Controller
{
    private const int MaxSubmittedAnswerJsonLength = 4096;

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

        if (!IsSubmittedAnswerJsonValid(submittedAnswerJson, out string? validationMessage))
        {
            ModelState.AddModelError(nameof(submittedAnswerJson), validationMessage);
            return View("Detail", new ExerciseRunnerPageViewModel(exercise, null, submittedAnswerJson));
        }

        ExerciseAttemptResultModel? result;
        try
        {
            result = await catalogApiClient
                .SubmitExerciseAttemptAsync(slug, new ExerciseAttemptRequestModel(submittedAnswerJson.Trim()), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is HttpRequestException or InvalidOperationException)
        {
            logger.LogWarning(ex, "Exercise answer evaluation failed for exercise {ExerciseSlug}.", slug);
            ModelState.AddModelError(nameof(submittedAnswerJson), localizer["The answer could not be evaluated right now."].Value);
            return View("Detail", new ExerciseRunnerPageViewModel(exercise, null, submittedAnswerJson));
        }

        return result is null
            ? NotFound()
            : View("Detail", new ExerciseRunnerPageViewModel(exercise, result, submittedAnswerJson));
    }

    private bool IsSubmittedAnswerJsonValid(string submittedAnswerJson, out string validationMessage)
    {
        validationMessage = string.Empty;
        string trimmed = submittedAnswerJson.Trim();
        if (trimmed.Length > MaxSubmittedAnswerJsonLength)
        {
            validationMessage = localizer["Answer data is too large."].Value;
            return false;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(trimmed);
            JsonValueKind kind = document.RootElement.ValueKind;
            if (kind is JsonValueKind.Object or JsonValueKind.Array)
            {
                return true;
            }

            validationMessage = localizer["Answer data must be a JSON object or array."].Value;
            return false;
        }
        catch (JsonException)
        {
            validationMessage = localizer["Answer data is not valid JSON."].Value;
            return false;
        }
    }
}
