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
[Route("admin/dialogues")]
public sealed class DialoguesController(
    IWebAdminOperationsQueryService operationsQueryService,
    IOutputCacheStore outputCacheStore) : Controller
{
    private const string CatalogCacheTag = "catalog";
    private const long MaxImportFileBytes = 2 * 1024 * 1024;

    [HttpGet("", Name = "Admin_Dialogues")]
    public async Task<IActionResult> Index(
        string? q,
        string? status,
        string? cefr,
        string? category,
        string? sort,
        CancellationToken cancellationToken)
    {
        AdminDialoguesPageViewModel page = await operationsQueryService.GetDialoguesAsync(cancellationToken).ConfigureAwait(false);
        AdminDialogueItemViewModel[] allDialogues = page.Dialogues.ToArray();
        IEnumerable<AdminDialogueItemViewModel> dialogues = allDialogues;

        if (!string.IsNullOrWhiteSpace(q))
        {
            dialogues = dialogues.Where(dialogue =>
                dialogue.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                dialogue.Slug.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                dialogue.Category.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            dialogues = dialogues.Where(dialogue => string.Equals(dialogue.PublicationStatus, status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(cefr))
        {
            dialogues = dialogues.Where(dialogue => string.Equals(dialogue.CefrLevel, cefr, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            dialogues = dialogues.Where(dialogue => string.Equals(dialogue.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        dialogues = sort switch
        {
            "title" => dialogues.OrderBy(dialogue => dialogue.Title).ThenBy(dialogue => dialogue.SortOrder),
            "updated" => dialogues.OrderByDescending(dialogue => dialogue.UpdatedAtUtc).ThenBy(dialogue => dialogue.Title),
            "assets" => dialogues.OrderByDescending(dialogue => dialogue.DialogueTurnCount + dialogue.PhraseCount + dialogue.QuestionCount).ThenBy(dialogue => dialogue.Title),
            _ => dialogues.OrderBy(dialogue => dialogue.SortOrder).ThenBy(dialogue => dialogue.Title),
        };

        ViewData["AdminDialogueQuery"] = q ?? string.Empty;
        ViewData["AdminDialogueStatus"] = status ?? string.Empty;
        ViewData["AdminDialogueCefr"] = cefr ?? string.Empty;
        ViewData["AdminDialogueCategory"] = category ?? string.Empty;
        ViewData["AdminDialogueSort"] = string.IsNullOrWhiteSpace(sort) ? "sortOrder" : sort;
        ViewData["AdminDialogueStatuses"] = allDialogues.Select(item => item.PublicationStatus).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToArray();
        ViewData["AdminDialogueCefrLevels"] = allDialogues.Select(item => item.CefrLevel).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToArray();
        ViewData["AdminDialogueCategories"] = allDialogues.Select(item => item.Category).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToArray();
        ViewData["AdminDialogueTotalCount"] = allDialogues.Length;

        return View(new AdminDialoguesPageViewModel(dialogues.ToArray()));
    }

    [HttpGet("new", Name = "Admin_DialogueNew")]
    public IActionResult New() => View("Edit", AdminDialogueEditViewModel.CreateNew());

    [HttpGet("import", Name = "Admin_DialogueImport")]
    public IActionResult Import() => View();

    [HttpPost("import", Name = "Admin_DialogueImportPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(AdminBulkDialogueImportFormModel form, CancellationToken cancellationToken)
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
            AdminBulkDialogueImportRequest? request = await JsonSerializer
                .DeserializeAsync<AdminBulkDialogueImportRequest>(stream, JsonSerializerOptions.Web, cancellationToken)
                .ConfigureAwait(false);

            if (request is null || request.Dialogues.Count == 0)
            {
                ModelState.AddModelError(nameof(form.JsonFile), "The JSON file must contain at least one dialogue in the dialogues array.");
                return View();
            }

            AdminBulkDialogueImportResponse result = await operationsQueryService
                .ImportDialoguesAsync(request, cancellationToken)
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

    [HttpPost("new", Name = "Admin_DialogueNewPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> New(AdminDialogueEditViewModel form, CancellationToken cancellationToken)
    {
        form.DialogueId = Guid.Empty;
        if (!ModelState.IsValid)
        {
            return View("Edit", form);
        }

        try
        {
            AdminDialogueDetailViewModel dialogue = await operationsQueryService
                .CreateDialogueAsync(ToRequest(form), cancellationToken)
                .ConfigureAwait(false);

            TempData["AdminStatusMessage"] = "Dialogue was created.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Details), new { dialogueId = dialogue.DialogueId });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "A dialogue with this slug already exists.");
            return View("Edit", form);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Edit", form);
        }
    }

    [HttpGet("{dialogueId:guid}", Name = "Admin_DialogueDetails")]
    public async Task<IActionResult> Details(Guid dialogueId, CancellationToken cancellationToken)
    {
        AdminDialogueDetailViewModel? dialogue = await operationsQueryService.GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
        return dialogue is null ? NotFound() : View(dialogue);
    }

    [HttpGet("{dialogueId:guid}/edit", Name = "Admin_DialogueEdit")]
    public async Task<IActionResult> Edit(Guid dialogueId, CancellationToken cancellationToken)
    {
        AdminDialogueDetailViewModel? dialogue = await operationsQueryService.GetDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
        return dialogue is null ? NotFound() : View(AdminDialogueEditViewModel.FromDetail(dialogue));
    }

    [HttpPost("{dialogueId:guid}/edit", Name = "Admin_DialogueEditPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid dialogueId, AdminDialogueEditViewModel form, CancellationToken cancellationToken)
    {
        if (dialogueId != form.DialogueId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(form);
        }

        try
        {
            await operationsQueryService.UpdateDialogueAsync(dialogueId, ToRequest(form), cancellationToken).ConfigureAwait(false);
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(form);
        }

        TempData["AdminStatusMessage"] = "Dialogue was updated.";
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    [HttpPost("{dialogueId:guid}/delete", Name = "Admin_DialogueDelete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid dialogueId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteDialogueAsync(dialogueId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Dialogue was deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{dialogueId:guid}/dialogue-turns", Name = "Admin_DialogueAddDialogueTurn")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDialogueTurn(Guid dialogueId, int sortOrder, string speakerRole, string baseText, CancellationToken cancellationToken)
    {
        await operationsQueryService
            .AddDialogueTurnAsync(dialogueId, new AdminAddDialogueTurnRequest(sortOrder, speakerRole, baseText), cancellationToken)
            .ConfigureAwait(false);

        TempData["AdminStatusMessage"] = "Dialogue turn was added.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    [HttpPost("{dialogueId:guid}/dialogue-turns/{turnId:guid}/delete", Name = "Admin_DialogueDeleteDialogueTurn")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDialogueTurn(Guid dialogueId, Guid turnId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteDialogueTurnAsync(dialogueId, turnId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Dialogue turn was removed.";
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    [HttpPost("{dialogueId:guid}/phrases", Name = "Admin_DialogueAddPhrase")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPhrase(Guid dialogueId, int sortOrder, string baseText, string? usageNote, CancellationToken cancellationToken)
    {
        await operationsQueryService
            .AddDialoguePhraseAsync(dialogueId, new AdminAddDialoguePhraseRequest(sortOrder, baseText, usageNote), cancellationToken)
            .ConfigureAwait(false);

        TempData["AdminStatusMessage"] = "Phrase was added.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    [HttpPost("{dialogueId:guid}/phrases/{phraseId:guid}/delete", Name = "Admin_DialogueDeletePhrase")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhrase(Guid dialogueId, Guid phraseId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteDialoguePhraseAsync(dialogueId, phraseId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Phrase was removed.";
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    [HttpPost("{dialogueId:guid}/questions", Name = "Admin_DialogueAddQuestion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(Guid dialogueId, int sortOrder, string prompt, CancellationToken cancellationToken)
    {
        await operationsQueryService
            .AddDialogueQuestionAsync(dialogueId, new AdminAddDialogueQuestionRequest(sortOrder, prompt), cancellationToken)
            .ConfigureAwait(false);

        TempData["AdminStatusMessage"] = "Question was added.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    [HttpPost("{dialogueId:guid}/questions/{questionId:guid}/delete", Name = "Admin_DialogueDeleteQuestion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(Guid dialogueId, Guid questionId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteDialogueQuestionAsync(dialogueId, questionId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Question was removed.";
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    [HttpPost("{dialogueId:guid}/questions/{questionId:guid}/answers", Name = "Admin_DialogueAddAnswer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAnswer(
        Guid dialogueId,
        Guid questionId,
        int sortOrder,
        string text,
        bool isCorrect,
        string? feedback,
        CancellationToken cancellationToken)
    {
        await operationsQueryService
            .AddDialogueAnswerAsync(dialogueId, questionId, new AdminAddDialogueAnswerRequest(sortOrder, text, isCorrect, feedback), cancellationToken)
            .ConfigureAwait(false);

        TempData["AdminStatusMessage"] = "Answer was added.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    [HttpPost("{dialogueId:guid}/questions/{questionId:guid}/answers/{answerId:guid}/delete", Name = "Admin_DialogueDeleteAnswer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAnswer(Guid dialogueId, Guid questionId, Guid answerId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteDialogueAnswerAsync(dialogueId, questionId, answerId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Answer was removed.";
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    [HttpPost("{dialogueId:guid}/dialogue-turns/{turnId:guid}/translations", Name = "Admin_DialogueAddDialogueTurnTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDialogueTurnTranslation(Guid dialogueId, Guid turnId, string languageCode, string text, CancellationToken cancellationToken)
    {
        await operationsQueryService
            .AddDialogueTurnTranslationAsync(dialogueId, turnId, new AdminAddDialogueTranslationRequest(languageCode, text), cancellationToken)
            .ConfigureAwait(false);

        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Dialogue translation was added.";
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    [HttpPost("{dialogueId:guid}/dialogue-turns/{turnId:guid}/translations/{translationId:guid}/delete", Name = "Admin_DialogueDeleteDialogueTurnTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDialogueTurnTranslation(Guid dialogueId, Guid turnId, Guid translationId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteDialogueTurnTranslationAsync(dialogueId, turnId, translationId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Dialogue translation was removed.";
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    [HttpPost("{dialogueId:guid}/phrases/{phraseId:guid}/translations", Name = "Admin_DialogueAddPhraseTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPhraseTranslation(Guid dialogueId, Guid phraseId, string languageCode, string text, CancellationToken cancellationToken)
    {
        await operationsQueryService
            .AddDialoguePhraseTranslationAsync(dialogueId, phraseId, new AdminAddDialogueTranslationRequest(languageCode, text), cancellationToken)
            .ConfigureAwait(false);

        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Phrase translation was added.";
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    [HttpPost("{dialogueId:guid}/phrases/{phraseId:guid}/translations/{translationId:guid}/delete", Name = "Admin_DialogueDeletePhraseTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhraseTranslation(Guid dialogueId, Guid phraseId, Guid translationId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteDialoguePhraseTranslationAsync(dialogueId, phraseId, translationId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Phrase translation was removed.";
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    [HttpPost("{dialogueId:guid}/questions/{questionId:guid}/translations", Name = "Admin_DialogueAddQuestionTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestionTranslation(Guid dialogueId, Guid questionId, string languageCode, string text, CancellationToken cancellationToken)
    {
        await operationsQueryService
            .AddDialogueQuestionTranslationAsync(dialogueId, questionId, new AdminAddDialogueTranslationRequest(languageCode, text), cancellationToken)
            .ConfigureAwait(false);

        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Question translation was added.";
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    [HttpPost("{dialogueId:guid}/questions/{questionId:guid}/translations/{translationId:guid}/delete", Name = "Admin_DialogueDeleteQuestionTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestionTranslation(Guid dialogueId, Guid questionId, Guid translationId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteDialogueQuestionTranslationAsync(dialogueId, questionId, translationId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Question translation was removed.";
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    [HttpPost("{dialogueId:guid}/questions/{questionId:guid}/answers/{answerId:guid}/translations", Name = "Admin_DialogueAddAnswerTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAnswerTranslation(Guid dialogueId, Guid questionId, Guid answerId, string languageCode, string text, CancellationToken cancellationToken)
    {
        await operationsQueryService
            .AddDialogueAnswerTranslationAsync(dialogueId, questionId, answerId, new AdminAddDialogueTranslationRequest(languageCode, text), cancellationToken)
            .ConfigureAwait(false);

        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Answer translation was added.";
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    [HttpPost("{dialogueId:guid}/questions/{questionId:guid}/answers/{answerId:guid}/translations/{translationId:guid}/delete", Name = "Admin_DialogueDeleteAnswerTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAnswerTranslation(Guid dialogueId, Guid questionId, Guid answerId, Guid translationId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteDialogueAnswerTranslationAsync(dialogueId, questionId, answerId, translationId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Answer translation was removed.";
        return RedirectToAction(nameof(Details), new { dialogueId });
    }

    private ValueTask EvictCatalogCacheAsync(CancellationToken cancellationToken) =>
        outputCacheStore.EvictByTagAsync(CatalogCacheTag, cancellationToken);

    private static AdminSaveDialogueRequest ToRequest(AdminDialogueEditViewModel form) =>
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
