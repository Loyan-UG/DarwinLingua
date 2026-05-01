using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;
using System.Text.Json;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/scenarios")]
public sealed class ScenariosController(
    IWebAdminOperationsQueryService operationsQueryService,
    IOutputCacheStore outputCacheStore) : Controller
{
    private const string CatalogCacheTag = "catalog";
    private const long MaxImportFileBytes = 2 * 1024 * 1024;

    [HttpGet("", Name = "Admin_Scenarios")]
    public async Task<IActionResult> Index(
        string? q,
        string? status,
        string? cefr,
        string? category,
        string? sort,
        CancellationToken cancellationToken)
    {
        AdminScenariosPageViewModel page = await operationsQueryService.GetScenariosAsync(cancellationToken).ConfigureAwait(false);
        AdminScenarioItemViewModel[] allScenarios = page.Scenarios.ToArray();
        IEnumerable<AdminScenarioItemViewModel> scenarios = allScenarios;

        if (!string.IsNullOrWhiteSpace(q))
        {
            scenarios = scenarios.Where(scenario =>
                scenario.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                scenario.Slug.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                scenario.Category.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            scenarios = scenarios.Where(scenario => string.Equals(scenario.PublicationStatus, status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(cefr))
        {
            scenarios = scenarios.Where(scenario => string.Equals(scenario.CefrLevel, cefr, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            scenarios = scenarios.Where(scenario => string.Equals(scenario.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        scenarios = sort switch
        {
            "title" => scenarios.OrderBy(scenario => scenario.Title).ThenBy(scenario => scenario.SortOrder),
            "updated" => scenarios.OrderByDescending(scenario => scenario.UpdatedAtUtc).ThenBy(scenario => scenario.Title),
            "assets" => scenarios.OrderByDescending(scenario => scenario.DialogueTurnCount + scenario.PhraseCount + scenario.QuestionCount).ThenBy(scenario => scenario.Title),
            _ => scenarios.OrderBy(scenario => scenario.SortOrder).ThenBy(scenario => scenario.Title),
        };

        ViewData["AdminScenarioQuery"] = q ?? string.Empty;
        ViewData["AdminScenarioStatus"] = status ?? string.Empty;
        ViewData["AdminScenarioCefr"] = cefr ?? string.Empty;
        ViewData["AdminScenarioCategory"] = category ?? string.Empty;
        ViewData["AdminScenarioSort"] = string.IsNullOrWhiteSpace(sort) ? "sortOrder" : sort;
        ViewData["AdminScenarioStatuses"] = allScenarios.Select(item => item.PublicationStatus).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToArray();
        ViewData["AdminScenarioCefrLevels"] = allScenarios.Select(item => item.CefrLevel).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToArray();
        ViewData["AdminScenarioCategories"] = allScenarios.Select(item => item.Category).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToArray();
        ViewData["AdminScenarioTotalCount"] = allScenarios.Length;

        return View(new AdminScenariosPageViewModel(scenarios.ToArray()));
    }

    [HttpGet("new", Name = "Admin_ScenarioNew")]
    public IActionResult New() => View("Edit", AdminScenarioEditViewModel.CreateNew());

    [HttpGet("import", Name = "Admin_ScenarioImport")]
    public IActionResult Import() => View();

    [HttpPost("import", Name = "Admin_ScenarioImportPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(AdminBulkScenarioImportFormModel form, CancellationToken cancellationToken)
    {
        if (form.JsonFile is null || form.JsonFile.Length == 0)
        {
            ModelState.AddModelError(nameof(form.JsonFile), "Select a JSON file to import.");
            return View();
        }

        if (form.JsonFile.Length > MaxImportFileBytes)
        {
            ModelState.AddModelError(nameof(form.JsonFile), "The JSON file is too large. The current limit is 2 MB.");
            return View();
        }

        try
        {
            await using Stream stream = form.JsonFile.OpenReadStream();
            AdminBulkScenarioImportRequest? request = await JsonSerializer
                .DeserializeAsync<AdminBulkScenarioImportRequest>(stream, JsonSerializerOptions.Web, cancellationToken)
                .ConfigureAwait(false);

            if (request is null || request.Scenarios.Count == 0)
            {
                ModelState.AddModelError(nameof(form.JsonFile), "The JSON file must contain at least one scenario in the scenarios array.");
                return View();
            }

            AdminBulkScenarioImportResponse result = await operationsQueryService
                .ImportScenariosAsync(request, cancellationToken)
                .ConfigureAwait(false);

            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return View(result);
        }
        catch (JsonException ex)
        {
            ModelState.AddModelError(nameof(form.JsonFile), $"The selected file is not valid JSON: {ex.Message}");
            return View();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(form.JsonFile), ex.Message);
            return View();
        }
    }

    [HttpPost("new", Name = "Admin_ScenarioNewPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> New(AdminScenarioEditViewModel form, CancellationToken cancellationToken)
    {
        form.ScenarioId = Guid.Empty;
        if (!ModelState.IsValid)
        {
            return View("Edit", form);
        }

        try
        {
            AdminScenarioDetailViewModel scenario = await operationsQueryService
                .CreateScenarioAsync(ToRequest(form), cancellationToken)
                .ConfigureAwait(false);

            TempData["AdminStatusMessage"] = "Scenario was created.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Details), new { scenarioId = scenario.ScenarioId });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "A scenario with this slug already exists.");
            return View("Edit", form);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Edit", form);
        }
    }

    [HttpGet("{scenarioId:guid}", Name = "Admin_ScenarioDetails")]
    public async Task<IActionResult> Details(Guid scenarioId, CancellationToken cancellationToken)
    {
        AdminScenarioDetailViewModel? scenario = await operationsQueryService.GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
        return scenario is null ? NotFound() : View(scenario);
    }

    [HttpGet("{scenarioId:guid}/edit", Name = "Admin_ScenarioEdit")]
    public async Task<IActionResult> Edit(Guid scenarioId, CancellationToken cancellationToken)
    {
        AdminScenarioDetailViewModel? scenario = await operationsQueryService.GetScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
        return scenario is null ? NotFound() : View(AdminScenarioEditViewModel.FromDetail(scenario));
    }

    [HttpPost("{scenarioId:guid}/edit", Name = "Admin_ScenarioEditPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid scenarioId, AdminScenarioEditViewModel form, CancellationToken cancellationToken)
    {
        if (scenarioId != form.ScenarioId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(form);
        }

        try
        {
            await operationsQueryService.UpdateScenarioAsync(scenarioId, ToRequest(form), cancellationToken).ConfigureAwait(false);
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(form);
        }

        TempData["AdminStatusMessage"] = "Scenario was updated.";
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    [HttpPost("{scenarioId:guid}/delete", Name = "Admin_ScenarioDelete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid scenarioId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteScenarioAsync(scenarioId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Scenario was deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{scenarioId:guid}/dialogue-turns", Name = "Admin_ScenarioAddDialogueTurn")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDialogueTurn(Guid scenarioId, int sortOrder, string speakerRole, string baseText, CancellationToken cancellationToken)
    {
        await operationsQueryService
            .AddScenarioDialogueTurnAsync(scenarioId, new AdminAddScenarioDialogueTurnRequest(sortOrder, speakerRole, baseText), cancellationToken)
            .ConfigureAwait(false);

        TempData["AdminStatusMessage"] = "Dialogue turn was added.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    [HttpPost("{scenarioId:guid}/dialogue-turns/{turnId:guid}/delete", Name = "Admin_ScenarioDeleteDialogueTurn")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDialogueTurn(Guid scenarioId, Guid turnId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteScenarioDialogueTurnAsync(scenarioId, turnId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Dialogue turn was removed.";
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    [HttpPost("{scenarioId:guid}/phrases", Name = "Admin_ScenarioAddPhrase")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPhrase(Guid scenarioId, int sortOrder, string baseText, string? usageNote, CancellationToken cancellationToken)
    {
        await operationsQueryService
            .AddScenarioPhraseAsync(scenarioId, new AdminAddScenarioPhraseRequest(sortOrder, baseText, usageNote), cancellationToken)
            .ConfigureAwait(false);

        TempData["AdminStatusMessage"] = "Phrase was added.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    [HttpPost("{scenarioId:guid}/phrases/{phraseId:guid}/delete", Name = "Admin_ScenarioDeletePhrase")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhrase(Guid scenarioId, Guid phraseId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteScenarioPhraseAsync(scenarioId, phraseId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Phrase was removed.";
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    [HttpPost("{scenarioId:guid}/questions", Name = "Admin_ScenarioAddQuestion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(Guid scenarioId, int sortOrder, string prompt, CancellationToken cancellationToken)
    {
        await operationsQueryService
            .AddScenarioQuestionAsync(scenarioId, new AdminAddScenarioQuestionRequest(sortOrder, prompt), cancellationToken)
            .ConfigureAwait(false);

        TempData["AdminStatusMessage"] = "Question was added.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    [HttpPost("{scenarioId:guid}/questions/{questionId:guid}/delete", Name = "Admin_ScenarioDeleteQuestion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(Guid scenarioId, Guid questionId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteScenarioQuestionAsync(scenarioId, questionId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Question was removed.";
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    [HttpPost("{scenarioId:guid}/questions/{questionId:guid}/answers", Name = "Admin_ScenarioAddAnswer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAnswer(
        Guid scenarioId,
        Guid questionId,
        int sortOrder,
        string text,
        bool isCorrect,
        string? feedback,
        CancellationToken cancellationToken)
    {
        await operationsQueryService
            .AddScenarioAnswerAsync(scenarioId, questionId, new AdminAddScenarioAnswerRequest(sortOrder, text, isCorrect, feedback), cancellationToken)
            .ConfigureAwait(false);

        TempData["AdminStatusMessage"] = "Answer was added.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    [HttpPost("{scenarioId:guid}/questions/{questionId:guid}/answers/{answerId:guid}/delete", Name = "Admin_ScenarioDeleteAnswer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAnswer(Guid scenarioId, Guid questionId, Guid answerId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteScenarioAnswerAsync(scenarioId, questionId, answerId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Answer was removed.";
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    [HttpPost("{scenarioId:guid}/dialogue-turns/{turnId:guid}/translations", Name = "Admin_ScenarioAddDialogueTurnTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDialogueTurnTranslation(Guid scenarioId, Guid turnId, string languageCode, string text, CancellationToken cancellationToken)
    {
        await operationsQueryService
            .AddScenarioDialogueTurnTranslationAsync(scenarioId, turnId, new AdminAddScenarioTranslationRequest(languageCode, text), cancellationToken)
            .ConfigureAwait(false);

        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Dialogue translation was added.";
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    [HttpPost("{scenarioId:guid}/dialogue-turns/{turnId:guid}/translations/{translationId:guid}/delete", Name = "Admin_ScenarioDeleteDialogueTurnTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDialogueTurnTranslation(Guid scenarioId, Guid turnId, Guid translationId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteScenarioDialogueTurnTranslationAsync(scenarioId, turnId, translationId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Dialogue translation was removed.";
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    [HttpPost("{scenarioId:guid}/phrases/{phraseId:guid}/translations", Name = "Admin_ScenarioAddPhraseTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPhraseTranslation(Guid scenarioId, Guid phraseId, string languageCode, string text, CancellationToken cancellationToken)
    {
        await operationsQueryService
            .AddScenarioPhraseTranslationAsync(scenarioId, phraseId, new AdminAddScenarioTranslationRequest(languageCode, text), cancellationToken)
            .ConfigureAwait(false);

        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Phrase translation was added.";
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    [HttpPost("{scenarioId:guid}/phrases/{phraseId:guid}/translations/{translationId:guid}/delete", Name = "Admin_ScenarioDeletePhraseTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhraseTranslation(Guid scenarioId, Guid phraseId, Guid translationId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteScenarioPhraseTranslationAsync(scenarioId, phraseId, translationId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Phrase translation was removed.";
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    [HttpPost("{scenarioId:guid}/questions/{questionId:guid}/translations", Name = "Admin_ScenarioAddQuestionTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestionTranslation(Guid scenarioId, Guid questionId, string languageCode, string text, CancellationToken cancellationToken)
    {
        await operationsQueryService
            .AddScenarioQuestionTranslationAsync(scenarioId, questionId, new AdminAddScenarioTranslationRequest(languageCode, text), cancellationToken)
            .ConfigureAwait(false);

        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Question translation was added.";
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    [HttpPost("{scenarioId:guid}/questions/{questionId:guid}/translations/{translationId:guid}/delete", Name = "Admin_ScenarioDeleteQuestionTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestionTranslation(Guid scenarioId, Guid questionId, Guid translationId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteScenarioQuestionTranslationAsync(scenarioId, questionId, translationId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Question translation was removed.";
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    [HttpPost("{scenarioId:guid}/questions/{questionId:guid}/answers/{answerId:guid}/translations", Name = "Admin_ScenarioAddAnswerTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAnswerTranslation(Guid scenarioId, Guid questionId, Guid answerId, string languageCode, string text, CancellationToken cancellationToken)
    {
        await operationsQueryService
            .AddScenarioAnswerTranslationAsync(scenarioId, questionId, answerId, new AdminAddScenarioTranslationRequest(languageCode, text), cancellationToken)
            .ConfigureAwait(false);

        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Answer translation was added.";
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    [HttpPost("{scenarioId:guid}/questions/{questionId:guid}/answers/{answerId:guid}/translations/{translationId:guid}/delete", Name = "Admin_ScenarioDeleteAnswerTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAnswerTranslation(Guid scenarioId, Guid questionId, Guid answerId, Guid translationId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteScenarioAnswerTranslationAsync(scenarioId, questionId, answerId, translationId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Answer translation was removed.";
        return RedirectToAction(nameof(Details), new { scenarioId });
    }

    private ValueTask EvictCatalogCacheAsync(CancellationToken cancellationToken) =>
        outputCacheStore.EvictByTagAsync(CatalogCacheTag, cancellationToken);

    private static AdminSaveScenarioRequest ToRequest(AdminScenarioEditViewModel form) =>
        new(
            form.Slug,
            form.Title,
            form.Description,
            form.LearnerGoal,
            form.CefrLevel,
            form.Category,
            form.PublicationStatus,
            form.SortOrder);
}
