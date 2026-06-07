using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Models;
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
    IWebLearningProfileAccessor profileAccessor,
    IStringLocalizer<SharedResource> localizer,
    ILogger<ExercisesController> logger) : Controller
{
    private const int MaxSubmittedAnswerJsonLength = 4096;

    [HttpGet("", Name = "Exercises_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(string? cefrLevel, string? q, CancellationToken cancellationToken)
    {
        ExerciseSetListFilterModel filter = new(LearningPortalFilterConventions.NormalizeCefrLevel(cefrLevel), null, null, string.IsNullOrWhiteSpace(q) ? null : q.Trim());
        UserLearningProfileModel profile = await profileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string primaryMeaningLanguageCode = ResolveMeaningLanguage(profile.PreferredMeaningLanguage1);
        IReadOnlyList<ExerciseSetListItemModel> sets;
        try
        {
            sets = await catalogApiClient.GetExerciseSetsAsync(filter, primaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is HttpRequestException or OperationCanceledException)
        {
            logger.LogWarning(ex, "Exercise sets could not be loaded.");
            sets = [];
        }

        return View(new ExerciseIndexPageViewModel(sets, LearningPortalFilterConventions.CefrLevels, filter.CefrLevel, filter.Query, primaryMeaningLanguageCode));
    }

    [HttpGet("sets/{slug}", Name = "ExerciseSets_Detail")]
    public async Task<IActionResult> Set(string slug, CancellationToken cancellationToken)
    {
        UserLearningProfileModel profile = await profileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string primaryMeaningLanguageCode = ResolveMeaningLanguage(profile.PreferredMeaningLanguage1);
        ExerciseSetDetailModel? set = await catalogApiClient.GetExerciseSetBySlugAsync(slug, primaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);
        return set is null ? NotFound() : View("Set", new ExerciseSetPageViewModel(set, primaryMeaningLanguageCode));
    }

    [HttpGet("{slug}", Name = "Exercises_Detail")]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        UserLearningProfileModel profile = await profileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string primaryMeaningLanguageCode = ResolveMeaningLanguage(profile.PreferredMeaningLanguage1);
        ExerciseDetailModel? exercise = await catalogApiClient.GetExerciseBySlugAsync(slug, primaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);
        return exercise is null ? NotFound() : View(new ExerciseRunnerPageViewModel(exercise, null, null, primaryMeaningLanguageCode));
    }

    [HttpPost("{slug}", Name = "Exercises_Submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(
        string slug,
        string? submittedAnswerJson,
        string[]? selectedOptionIds,
        string? answer,
        string? correctedText,
        string? orderedSegmentsText,
        string? pairsText,
        CancellationToken cancellationToken)
    {
        UserLearningProfileModel profile = await profileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string primaryMeaningLanguageCode = ResolveMeaningLanguage(profile.PreferredMeaningLanguage1);
        ExerciseDetailModel? exercise = await catalogApiClient.GetExerciseBySlugAsync(slug, primaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);
        if (exercise is null)
        {
            return NotFound();
        }

        string normalizedSubmittedAnswerJson = BuildSubmittedAnswerJson(
            exercise.ExerciseType,
            submittedAnswerJson,
            selectedOptionIds,
            answer,
            correctedText,
            orderedSegmentsText,
            pairsText);

        if (string.IsNullOrWhiteSpace(normalizedSubmittedAnswerJson))
        {
            ModelState.AddModelError(nameof(submittedAnswerJson), localizer["Answer is required."].Value);
            return View("Detail", new ExerciseRunnerPageViewModel(exercise, null, submittedAnswerJson, primaryMeaningLanguageCode));
        }

        if (!IsSubmittedAnswerJsonValid(normalizedSubmittedAnswerJson, out string? validationMessage))
        {
            ModelState.AddModelError(nameof(submittedAnswerJson), validationMessage);
            return View("Detail", new ExerciseRunnerPageViewModel(exercise, null, normalizedSubmittedAnswerJson, primaryMeaningLanguageCode));
        }

        ExerciseAttemptResultModel? result;
        try
        {
            result = await catalogApiClient
                .SubmitExerciseAttemptAsync(slug, new ExerciseAttemptRequestModel(normalizedSubmittedAnswerJson.Trim()), primaryMeaningLanguageCode, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is HttpRequestException or InvalidOperationException)
        {
            logger.LogWarning(ex, "Exercise answer evaluation failed for exercise {ExerciseSlug}.", slug);
            ModelState.AddModelError(nameof(submittedAnswerJson), localizer["The answer could not be evaluated right now."].Value);
            return View("Detail", new ExerciseRunnerPageViewModel(exercise, null, normalizedSubmittedAnswerJson, primaryMeaningLanguageCode));
        }

        return result is null
            ? NotFound()
            : View("Detail", new ExerciseRunnerPageViewModel(exercise, result, normalizedSubmittedAnswerJson, primaryMeaningLanguageCode));
    }

    private static string BuildSubmittedAnswerJson(
        string exerciseType,
        string? submittedAnswerJson,
        string[]? selectedOptionIds,
        string? answer,
        string? correctedText,
        string? orderedSegmentsText,
        string? pairsText)
    {
        string type = exerciseType.Trim().ToLowerInvariant();
        if (IsChoiceExercise(type) && selectedOptionIds is { Length: > 0 })
        {
            return JsonSerializer.Serialize(new
            {
                selectedOptionIds = selectedOptionIds.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToArray(),
            });
        }

        if (IsSingleAnswerExercise(type) && !string.IsNullOrWhiteSpace(answer))
        {
            return JsonSerializer.Serialize(new { answer = answer.Trim() });
        }

        if (string.Equals(type, "error-correction", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(correctedText))
        {
            return JsonSerializer.Serialize(new { correctedText = correctedText.Trim() });
        }

        if (string.Equals(type, "sentence-ordering", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(orderedSegmentsText))
        {
            return JsonSerializer.Serialize(new
            {
                orderedSegments = SplitLines(orderedSegmentsText),
            });
        }

        if (string.Equals(type, "matching", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(pairsText))
        {
            return JsonSerializer.Serialize(new
            {
                pairs = SplitLines(pairsText),
            });
        }

        return submittedAnswerJson?.Trim() ?? string.Empty;
    }

    private static bool IsChoiceExercise(string exerciseType) =>
        exerciseType is "multiple-choice" or "article-selection" or "case-selection" or "vocabulary-choice" or "grammar-choice" or "dialogue-completion";

    private static bool IsSingleAnswerExercise(string exerciseType) =>
        exerciseType is "fill-in-the-blank" or "conjugation" or "translation-controlled";

    private static string[] SplitLines(string value) =>
        value.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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

    private static string ResolveMeaningLanguage(string? languageCode) =>
        string.IsNullOrWhiteSpace(languageCode) ? "en" : languageCode.Trim().ToLowerInvariant();
}
