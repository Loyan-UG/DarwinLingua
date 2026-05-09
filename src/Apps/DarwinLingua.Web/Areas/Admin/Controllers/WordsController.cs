using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;
using System.Text.Json;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/words")]
public sealed class WordsController(
    IWebAdminOperationsQueryService operationsQueryService,
    IOutputCacheStore outputCacheStore) : Controller
{
    private const string CatalogCacheTag = "catalog";
    private const int DefaultTake = 25;
    private const int MaxQueryLength = 128;
    private const int MaxTake = 100;
    private const long MaxImportFileBytes = 2 * 1024 * 1024;

    [HttpGet("", Name = "Admin_Words")]
    public async Task<IActionResult> Index(
        string? q,
        string? status,
        string? sort,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        return View(await GetPageAsync(q, status, sort, skip, take, cancellationToken));
    }

    [HttpGet("table", Name = "Admin_WordsTable")]
    public async Task<IActionResult> Table(
        string? q,
        string? status,
        string? sort,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        return PartialView("_WordsTable", await GetPageAsync(q, status, sort, skip, take, cancellationToken));
    }

    [HttpGet("new", Name = "Admin_WordNew")]
    public IActionResult New()
    {
        return View("Edit", Models.AdminWordEditViewModel.CreateNew());
    }

    [HttpGet("import", Name = "Admin_WordImport")]
    public IActionResult Import()
    {
        return View();
    }

    [HttpPost("import", Name = "Admin_WordImportPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(
        Models.AdminBulkWordImportFormModel form,
        CancellationToken cancellationToken)
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
            Models.AdminBulkWordImportRequest? request = await JsonSerializer
                .DeserializeAsync<Models.AdminBulkWordImportRequest>(stream, JsonSerializerOptions.Web, cancellationToken)
                .ConfigureAwait(false);

            if (request is null || request.Words.Count == 0)
            {
                ModelState.AddModelError(nameof(form.JsonFile), "The JSON file must contain at least one word in the words array.");
                return View();
            }

            Models.AdminBulkWordImportResponse result = await operationsQueryService
                .ImportWordsAsync(request, cancellationToken)
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

    [HttpPost("new", Name = "Admin_WordNewPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> New(
        Models.AdminWordEditViewModel form,
        CancellationToken cancellationToken)
    {
        form.PublicId = Guid.Empty;

        if (!ModelState.IsValid)
        {
            return View("Edit", form);
        }

        try
        {
            Models.AdminWordDetailViewModel word = await operationsQueryService
                .CreateWordAsync(ToMetadataRequest(form), cancellationToken)
                .ConfigureAwait(false);

            TempData["AdminStatusMessage"] = "Word was created.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Details), new { publicId = word.PublicId });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "A word with this lemma already exists.");
            return View("Edit", form);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Edit", form);
        }
    }

    [HttpGet("{publicId:guid}", Name = "Admin_WordDetails")]
    public async Task<IActionResult> Details(Guid publicId, CancellationToken cancellationToken)
    {
        Models.AdminWordDetailViewModel? word = await operationsQueryService.GetWordAsync(publicId, cancellationToken);

        if (word is not null)
        {
            ViewData["AdminTopics"] = await operationsQueryService.GetTopicsAsync(cancellationToken).ConfigureAwait(false);
            ViewData["AdminLabels"] = await operationsQueryService.GetLabelsAsync(cancellationToken).ConfigureAwait(false);
        }

        return word is null ? NotFound() : View(word);
    }

    [HttpGet("{publicId:guid}/edit", Name = "Admin_WordEdit")]
    public async Task<IActionResult> Edit(Guid publicId, CancellationToken cancellationToken)
    {
        Models.AdminWordDetailViewModel? word = await operationsQueryService.GetWordAsync(publicId, cancellationToken);

        return word is null ? NotFound() : View(Models.AdminWordEditViewModel.FromDetail(word));
    }

    [HttpPost("{publicId:guid}/edit", Name = "Admin_WordEditPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid publicId,
        Models.AdminWordEditViewModel form,
        CancellationToken cancellationToken)
    {
        if (publicId != form.PublicId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(form);
        }

        try
        {
            await operationsQueryService.UpdateWordMetadataAsync(
                    publicId,
                    ToMetadataRequest(form),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "This update conflicts with another word that already has the same lemma.");
            return View(form);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(form);
        }

        TempData["AdminStatusMessage"] = "Word metadata was updated.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { publicId });
    }

    [HttpPost("{publicId:guid}/senses", Name = "Admin_WordAddSense")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSense(
        Guid publicId,
        Models.AdminWordSenseFormModel form,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["AdminErrorMessage"] = "The sense form contains invalid values.";
            return RedirectToAction(nameof(Details), new { publicId });
        }

        try
        {
            Models.AdminWordDetailViewModel? word = await operationsQueryService
                .AddWordSenseAsync(publicId, ToAddSenseRequest(form), cancellationToken)
                .ConfigureAwait(false);

            if (word is null)
            {
                return NotFound();
            }

            TempData["AdminStatusMessage"] = "Sense was added.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Details), new { publicId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["AdminErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { publicId });
        }
    }

    [HttpPost("{publicId:guid}/senses/{senseId:guid}/translations", Name = "Admin_WordAddSenseTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSenseTranslation(
        Guid publicId,
        Guid senseId,
        Models.AdminWordSenseTranslationFormModel form,
        CancellationToken cancellationToken)
    {
        form.SenseId = senseId;

        if (!ModelState.IsValid)
        {
            TempData["AdminErrorMessage"] = "The translation form contains invalid values.";
            return RedirectToAction(nameof(Details), new { publicId });
        }

        try
        {
            Models.AdminWordDetailViewModel? word = await operationsQueryService
                .AddWordSenseTranslationAsync(
                    publicId,
                    senseId,
                    new Models.AdminAddWordSenseTranslationRequest(
                        form.LanguageCode,
                        form.TranslationText,
                        form.IsPrimary),
                    cancellationToken)
                .ConfigureAwait(false);

            if (word is null)
            {
                return NotFound();
            }

            TempData["AdminStatusMessage"] = "Translation was added.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Details), new { publicId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["AdminErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { publicId });
        }
    }

    [HttpPost("{publicId:guid}/senses/{senseId:guid}/examples", Name = "Admin_WordAddSenseExample")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSenseExample(
        Guid publicId,
        Guid senseId,
        Models.AdminWordSenseExampleFormModel form,
        CancellationToken cancellationToken)
    {
        form.SenseId = senseId;

        if (!ModelState.IsValid)
        {
            TempData["AdminErrorMessage"] = "The example form contains invalid values.";
            return RedirectToAction(nameof(Details), new { publicId });
        }

        try
        {
            Models.AdminWordDetailViewModel? word = await operationsQueryService
                .AddWordSenseExampleAsync(
                    publicId,
                    senseId,
                    new Models.AdminAddWordSenseExampleRequest(
                        form.GermanText,
                        form.IsPrimaryExample,
                        NormalizeOptional(form.TranslationLanguageCode),
                        NormalizeOptional(form.TranslationText)),
                    cancellationToken)
                .ConfigureAwait(false);

            if (word is null)
            {
                return NotFound();
            }

            TempData["AdminStatusMessage"] = "Example was added.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Details), new { publicId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["AdminErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { publicId });
        }
    }

    [HttpPost("{publicId:guid}/senses/{senseId:guid}/translations/{translationId:guid}", Name = "Admin_WordUpdateSenseTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSenseTranslation(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        Models.AdminWordSenseTranslationFormModel form,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["AdminErrorMessage"] = "The translation form contains invalid values.";
            return RedirectToAction(nameof(Details), new { publicId });
        }

        try
        {
            Models.AdminWordDetailViewModel? word = await operationsQueryService
                .UpdateWordSenseTranslationAsync(
                    publicId,
                    senseId,
                    translationId,
                    new Models.AdminUpdateWordSenseTranslationRequest(form.LanguageCode, form.TranslationText, form.IsPrimary),
                    cancellationToken)
                .ConfigureAwait(false);

            if (word is null)
            {
                return NotFound();
            }

            TempData["AdminStatusMessage"] = "Translation was updated.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Details), new { publicId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["AdminErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { publicId });
        }
    }

    [HttpPost("{publicId:guid}/senses/{senseId:guid}/translations/{translationId:guid}/delete", Name = "Admin_WordDeleteSenseTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSenseTranslation(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        CancellationToken cancellationToken)
    {
        Models.AdminWordDetailViewModel? word = await operationsQueryService
            .DeleteWordSenseTranslationAsync(publicId, senseId, translationId, cancellationToken)
            .ConfigureAwait(false);

        if (word is null)
        {
            return NotFound();
        }

        TempData["AdminStatusMessage"] = "Translation was deleted.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { publicId });
    }

    [HttpPost("{publicId:guid}/senses/{senseId:guid}/examples/{exampleId:guid}", Name = "Admin_WordUpdateSenseExample")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSenseExample(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        Models.AdminWordSenseExampleFormModel form,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["AdminErrorMessage"] = "The example form contains invalid values.";
            return RedirectToAction(nameof(Details), new { publicId });
        }

        try
        {
            Models.AdminWordDetailViewModel? word = await operationsQueryService
                .UpdateWordSenseExampleAsync(
                    publicId,
                    senseId,
                    exampleId,
                    new Models.AdminUpdateWordSenseExampleRequest(form.GermanText, form.IsPrimaryExample),
                    cancellationToken)
                .ConfigureAwait(false);

            if (word is null)
            {
                return NotFound();
            }

            TempData["AdminStatusMessage"] = "Example was updated.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Details), new { publicId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["AdminErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { publicId });
        }
    }

    [HttpPost("{publicId:guid}/senses/{senseId:guid}/examples/{exampleId:guid}/delete", Name = "Admin_WordDeleteSenseExample")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSenseExample(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        CancellationToken cancellationToken)
    {
        Models.AdminWordDetailViewModel? word = await operationsQueryService
            .DeleteWordSenseExampleAsync(publicId, senseId, exampleId, cancellationToken)
            .ConfigureAwait(false);

        if (word is null)
        {
            return NotFound();
        }

        TempData["AdminStatusMessage"] = "Example was deleted.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { publicId });
    }

    [HttpPost("{publicId:guid}/senses/{senseId:guid}/examples/{exampleId:guid}/translations", Name = "Admin_WordAddSenseExampleTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSenseExampleTranslation(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        Models.AdminWordSenseTranslationFormModel form,
        CancellationToken cancellationToken)
    {
        form.SenseId = senseId;

        if (!ModelState.IsValid)
        {
            TempData["AdminErrorMessage"] = "The example translation form contains invalid values.";
            return RedirectToAction(nameof(Details), new { publicId });
        }

        try
        {
            Models.AdminWordDetailViewModel? word = await operationsQueryService
                .AddWordSenseExampleTranslationAsync(
                    publicId,
                    senseId,
                    exampleId,
                    new Models.AdminAddWordSenseExampleTranslationRequest(form.LanguageCode, form.TranslationText),
                    cancellationToken)
                .ConfigureAwait(false);

            if (word is null)
            {
                return NotFound();
            }

            TempData["AdminStatusMessage"] = "Example translation was added.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Details), new { publicId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["AdminErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { publicId });
        }
    }

    [HttpPost("{publicId:guid}/senses/{senseId:guid}/examples/{exampleId:guid}/translations/{translationId:guid}", Name = "Admin_WordUpdateSenseExampleTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSenseExampleTranslation(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        Guid translationId,
        Models.AdminWordSenseTranslationFormModel form,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["AdminErrorMessage"] = "The example translation form contains invalid values.";
            return RedirectToAction(nameof(Details), new { publicId });
        }

        try
        {
            Models.AdminWordDetailViewModel? word = await operationsQueryService
                .UpdateWordSenseExampleTranslationAsync(
                    publicId,
                    senseId,
                    exampleId,
                    translationId,
                    new Models.AdminUpdateWordSenseExampleTranslationRequest(form.LanguageCode, form.TranslationText),
                    cancellationToken)
                .ConfigureAwait(false);

            if (word is null)
            {
                return NotFound();
            }

            TempData["AdminStatusMessage"] = "Example translation was updated.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Details), new { publicId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["AdminErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { publicId });
        }
    }

    [HttpPost("{publicId:guid}/senses/{senseId:guid}/examples/{exampleId:guid}/translations/{translationId:guid}/delete", Name = "Admin_WordDeleteSenseExampleTranslation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSenseExampleTranslation(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        Guid translationId,
        CancellationToken cancellationToken)
    {
        Models.AdminWordDetailViewModel? word = await operationsQueryService
            .DeleteWordSenseExampleTranslationAsync(publicId, senseId, exampleId, translationId, cancellationToken)
            .ConfigureAwait(false);

        if (word is null)
        {
            return NotFound();
        }

        TempData["AdminStatusMessage"] = "Example translation was deleted.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { publicId });
    }

    [HttpPost("{publicId:guid}/topics", Name = "Admin_WordAddTopic")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTopic(
        Guid publicId,
        Guid topicId,
        bool isPrimaryTopic,
        CancellationToken cancellationToken)
    {
        if (topicId == Guid.Empty)
        {
            TempData["AdminErrorMessage"] = "Select a topic first.";
            return RedirectToAction(nameof(Details), new { publicId });
        }

        Models.AdminWordDetailViewModel? word = await operationsQueryService
            .AddWordTopicAsync(publicId, new Models.AdminAddWordTopicRequest(topicId, isPrimaryTopic), cancellationToken)
            .ConfigureAwait(false);

        if (word is null)
        {
            return NotFound();
        }

        TempData["AdminStatusMessage"] = "Topic was attached to the word.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { publicId });
    }

    [HttpPost("{publicId:guid}/topics/{topicId:guid}/delete", Name = "Admin_WordDeleteTopic")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTopic(
        Guid publicId,
        Guid topicId,
        CancellationToken cancellationToken)
    {
        Models.AdminWordDetailViewModel? word = await operationsQueryService
            .DeleteWordTopicAsync(publicId, topicId, cancellationToken)
            .ConfigureAwait(false);

        if (word is null)
        {
            return NotFound();
        }

        TempData["AdminStatusMessage"] = "Topic was removed from the word.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { publicId });
    }

    [HttpPost("{publicId:guid}/labels", Name = "Admin_WordAddLabel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLabel(
        Guid publicId,
        string labelKey,
        CancellationToken cancellationToken)
    {
        if (!TrySplitLabelKey(labelKey, out string kind, out string key))
        {
            TempData["AdminErrorMessage"] = "Select a label first.";
            return RedirectToAction(nameof(Details), new { publicId });
        }

        try
        {
            Models.AdminWordDetailViewModel? word = await operationsQueryService
                .AddWordLabelAsync(publicId, new Models.AdminAddWordLabelRequest(kind, key), cancellationToken)
                .ConfigureAwait(false);

            if (word is null)
            {
                return NotFound();
            }

            TempData["AdminStatusMessage"] = "Label was attached to the word.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Details), new { publicId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["AdminErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { publicId });
        }
    }

    [HttpPost("{publicId:guid}/labels/{kind}/{key}/delete", Name = "Admin_WordDeleteLabel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLabel(
        Guid publicId,
        string kind,
        string key,
        CancellationToken cancellationToken)
    {
        Models.AdminWordDetailViewModel? word = await operationsQueryService
            .DeleteWordLabelAsync(publicId, kind, key, cancellationToken)
            .ConfigureAwait(false);

        if (word is null)
        {
            return NotFound();
        }

        TempData["AdminStatusMessage"] = "Label was removed from the word.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { publicId });
    }

    private ValueTask EvictCatalogCacheAsync(CancellationToken cancellationToken) =>
        outputCacheStore.EvictByTagAsync(CatalogCacheTag, cancellationToken);

    private Task<Models.AdminWordsPageViewModel> GetPageAsync(
        string? q,
        string? status,
        string? sort,
        int? skip,
        int? take,
        CancellationToken cancellationToken) =>
        operationsQueryService.GetWordsAsync(
            NormalizeQuery(q),
            NormalizeStatus(status),
            NormalizeWordSort(sort),
            Math.Max(0, skip ?? 0),
            Math.Clamp(take ?? DefaultTake, 10, MaxTake),
            cancellationToken);

    private static string? NormalizeQuery(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length <= MaxQueryLength ? trimmed : trimmed[..MaxQueryLength];
    }

    private static string? NormalizeStatus(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeWordSort(string? value)
    {
        string? normalized = NormalizeOptional(value)?.ToLowerInvariant();
        return normalized is "updated" or "cefr" or "status" ? normalized : "lemma";
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool TrySplitLabelKey(string? value, out string kind, out string key)
    {
        kind = string.Empty;
        key = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string[] parts = value.Split("::", 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
            return false;
        }

        kind = parts[0];
        key = parts[1];
        return true;
    }

    private static Models.AdminUpdateWordMetadataRequest ToMetadataRequest(Models.AdminWordEditViewModel form) =>
        new(
            form.Lemma,
            form.LanguageCode,
            NormalizeOptional(form.Article),
            NormalizeOptional(form.PluralForm),
            NormalizeOptional(form.InfinitiveForm),
            NormalizeOptional(form.PronunciationIpa),
            NormalizeOptional(form.SyllableBreak),
            form.PartOfSpeech,
            form.CefrLevel,
            form.PublicationStatus,
            form.ContentSourceType,
            NormalizeOptional(form.SourceReference));

    private static Models.AdminAddWordSenseRequest ToAddSenseRequest(Models.AdminWordSenseFormModel form) =>
        new(
            form.PublicationStatus,
            form.IsPrimarySense,
            NormalizeOptional(form.ShortDefinitionDe),
            NormalizeOptional(form.ShortGloss),
            NormalizeOptional(form.TranslationLanguageCode),
            NormalizeOptional(form.TranslationText),
            form.IsPrimaryTranslation,
            NormalizeOptional(form.ExampleGermanText),
            NormalizeOptional(form.ExampleTranslationLanguageCode),
            NormalizeOptional(form.ExampleTranslationText),
            form.IsPrimaryExample);
}
