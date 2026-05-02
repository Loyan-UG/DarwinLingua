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
[Route("admin/collections")]
public sealed class CollectionsController(
    IWebAdminOperationsQueryService operationsQueryService,
    IOutputCacheStore outputCacheStore) : Controller
{
    private const string CatalogCacheTag = "catalog";
    private const int WordPickerTake = 100;
    private const long MaxImportFileBytes = 2 * 1024 * 1024;

    [HttpGet("", Name = "Admin_Collections")]
    public async Task<IActionResult> Index(
        string? q,
        string? status,
        string? sort,
        CancellationToken cancellationToken)
    {
        AdminCollectionsPageViewModel page = await operationsQueryService.GetCollectionsAsync(cancellationToken).ConfigureAwait(false);
        AdminCollectionItemViewModel[] allCollections = page.Collections.ToArray();
        IEnumerable<AdminCollectionItemViewModel> collections = allCollections;

        if (!string.IsNullOrWhiteSpace(q))
        {
            collections = collections.Where(collection =>
                collection.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                collection.Slug.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            collections = collections.Where(collection => string.Equals(collection.PublicationStatus, status, StringComparison.OrdinalIgnoreCase));
        }

        collections = sort switch
        {
            "name" => collections.OrderBy(collection => collection.Name).ThenBy(collection => collection.SortOrder),
            "updated" => collections.OrderByDescending(collection => collection.UpdatedAtUtc).ThenBy(collection => collection.Name),
            "words" => collections.OrderByDescending(collection => collection.WordCount).ThenBy(collection => collection.Name),
            _ => collections.OrderBy(collection => collection.SortOrder).ThenBy(collection => collection.Name),
        };

        ViewData["AdminCollectionQuery"] = q ?? string.Empty;
        ViewData["AdminCollectionStatus"] = status ?? string.Empty;
        ViewData["AdminCollectionSort"] = string.IsNullOrWhiteSpace(sort) ? "sortOrder" : sort;
        ViewData["AdminCollectionStatuses"] = allCollections.Select(item => item.PublicationStatus).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToArray();
        ViewData["AdminCollectionTotalCount"] = allCollections.Length;

        return View(new AdminCollectionsPageViewModel(collections.ToArray()));
    }

    [HttpGet("new", Name = "Admin_CollectionNew")]
    public IActionResult New() => View("Edit", AdminCollectionEditViewModel.CreateNew());

    [HttpGet("import", Name = "Admin_CollectionImport")]
    public IActionResult Import() => View();

    [HttpPost("import", Name = "Admin_CollectionImportPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(AdminBulkCollectionImportFormModel form, CancellationToken cancellationToken)
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
            AdminBulkCollectionImportRequest? request = await JsonSerializer
                .DeserializeAsync<AdminBulkCollectionImportRequest>(stream, JsonSerializerOptions.Web, cancellationToken)
                .ConfigureAwait(false);

            if (request is null || request.Collections.Count == 0)
            {
                ModelState.AddModelError(nameof(form.JsonFile), "The JSON file must contain at least one collection in the collections array.");
                return View();
            }

            AdminBulkCollectionImportResponse result = await operationsQueryService
                .ImportCollectionsAsync(request, cancellationToken)
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

    [HttpPost("new", Name = "Admin_CollectionNewPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> New(AdminCollectionEditViewModel form, CancellationToken cancellationToken)
    {
        form.CollectionId = Guid.Empty;

        if (!ModelState.IsValid)
        {
            return View("Edit", form);
        }

        try
        {
            AdminCollectionDetailViewModel collection = await operationsQueryService
                .CreateCollectionAsync(ToRequest(form), cancellationToken)
                .ConfigureAwait(false);

            TempData["AdminStatusMessage"] = "Collection was created.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Details), new { collectionId = collection.CollectionId });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "A collection with this slug already exists.");
            return View("Edit", form);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Edit", form);
        }
    }

    [HttpGet("{collectionId:guid}", Name = "Admin_CollectionDetails")]
    public async Task<IActionResult> Details(Guid collectionId, CancellationToken cancellationToken)
    {
        AdminCollectionDetailViewModel? collection = await operationsQueryService
            .GetCollectionAsync(collectionId, cancellationToken)
            .ConfigureAwait(false);

        if (collection is not null)
        {
            ViewData["AdminWordPicker"] = await BuildWordPickerAsync(collection, null, cancellationToken)
                .ConfigureAwait(false);
        }

        return collection is null ? NotFound() : View(collection);
    }

    [HttpGet("{collectionId:guid}/word-options", Name = "Admin_CollectionWordOptions")]
    public async Task<IActionResult> WordOptions(Guid collectionId, string? q, CancellationToken cancellationToken)
    {
        AdminCollectionDetailViewModel? collection = await operationsQueryService
            .GetCollectionAsync(collectionId, cancellationToken)
            .ConfigureAwait(false);

        if (collection is null)
        {
            return NotFound();
        }

        AdminCollectionWordPickerViewModel picker = await BuildWordPickerAsync(collection, q, cancellationToken)
            .ConfigureAwait(false);

        return PartialView("_WordPickerOptions", picker);
    }

    [HttpGet("{collectionId:guid}/edit", Name = "Admin_CollectionEdit")]
    public async Task<IActionResult> Edit(Guid collectionId, CancellationToken cancellationToken)
    {
        AdminCollectionDetailViewModel? collection = await operationsQueryService
            .GetCollectionAsync(collectionId, cancellationToken)
            .ConfigureAwait(false);

        return collection is null ? NotFound() : View(AdminCollectionEditViewModel.FromDetail(collection));
    }

    [HttpPost("{collectionId:guid}/edit", Name = "Admin_CollectionEditPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid collectionId, AdminCollectionEditViewModel form, CancellationToken cancellationToken)
    {
        if (collectionId != form.CollectionId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(form);
        }

        try
        {
            await operationsQueryService.UpdateCollectionAsync(collectionId, ToRequest(form), cancellationToken).ConfigureAwait(false);
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(form);
        }

        TempData["AdminStatusMessage"] = "Collection was updated.";
        return RedirectToAction(nameof(Details), new { collectionId });
    }

    [HttpPost("{collectionId:guid}/delete", Name = "Admin_CollectionDelete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid collectionId, CancellationToken cancellationToken)
    {
        await operationsQueryService.DeleteCollectionAsync(collectionId, cancellationToken).ConfigureAwait(false);
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        TempData["AdminStatusMessage"] = "Collection was deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{collectionId:guid}/words", Name = "Admin_CollectionAddWord")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddWord(Guid collectionId, Guid wordPublicId, int sortOrder, CancellationToken cancellationToken)
    {
        if (wordPublicId == Guid.Empty)
        {
            TempData["AdminErrorMessage"] = "Word public ID is required.";
            return RedirectToAction(nameof(Details), new { collectionId });
        }

        AdminCollectionDetailViewModel? collection = await operationsQueryService
            .AddCollectionWordAsync(collectionId, new AdminAddCollectionWordRequest(wordPublicId, sortOrder), cancellationToken)
            .ConfigureAwait(false);

        if (collection is null)
        {
            return NotFound();
        }

        TempData["AdminStatusMessage"] = "Word was added to the collection.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { collectionId });
    }

    [HttpPost("{collectionId:guid}/words/{entryId:guid}/delete", Name = "Admin_CollectionDeleteWord")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWord(Guid collectionId, Guid entryId, CancellationToken cancellationToken)
    {
        AdminCollectionDetailViewModel? collection = await operationsQueryService
            .DeleteCollectionWordAsync(collectionId, entryId, cancellationToken)
            .ConfigureAwait(false);

        if (collection is null)
        {
            return NotFound();
        }

        TempData["AdminStatusMessage"] = "Word was removed from the collection.";
        await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Details), new { collectionId });
    }

    private ValueTask EvictCatalogCacheAsync(CancellationToken cancellationToken) =>
        outputCacheStore.EvictByTagAsync(CatalogCacheTag, cancellationToken);

    private async Task<AdminCollectionWordPickerViewModel> BuildWordPickerAsync(
        AdminCollectionDetailViewModel collection,
        string? query,
        CancellationToken cancellationToken)
    {
        string? normalizedQuery = string.IsNullOrWhiteSpace(query) ? null : query.Trim();
        AdminWordsPageViewModel words = await operationsQueryService
            .GetWordsAsync(normalizedQuery, "Active", "lemma", 0, WordPickerTake, cancellationToken)
            .ConfigureAwait(false);

        return new AdminCollectionWordPickerViewModel(
            normalizedQuery,
            collection.Entries.Select(entry => entry.WordPublicId).ToHashSet(),
            words.Words);
    }

    private static AdminSaveCollectionRequest ToRequest(AdminCollectionEditViewModel form) =>
        new(
            form.Slug,
            form.Name,
            string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
            null,
            string.IsNullOrWhiteSpace(form.ImageUrl) ? null : form.ImageUrl.Trim(),
            form.PublicationStatus,
            form.SortOrder);
}
