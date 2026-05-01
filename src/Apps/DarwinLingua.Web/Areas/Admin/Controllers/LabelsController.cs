using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/labels")]
public sealed class LabelsController(
    IWebAdminOperationsQueryService operationsQueryService,
    IOutputCacheStore outputCacheStore) : Controller
{
    private const string CatalogCacheTag = "catalog";

    [HttpGet("", Name = "Admin_Labels")]
    public async Task<IActionResult> Index(
        string? q,
        string? kind,
        string? system,
        string? sort,
        CancellationToken cancellationToken)
    {
        AdminLabelsPageViewModel page = await operationsQueryService.GetLabelsAsync(cancellationToken).ConfigureAwait(false);
        AdminLabelItemViewModel[] allLabels = page.Labels.ToArray();
        IEnumerable<AdminLabelItemViewModel> labels = allLabels;

        if (!string.IsNullOrWhiteSpace(q))
        {
            labels = labels.Where(label =>
                label.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                label.Key.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(kind))
        {
            labels = labels.Where(label => string.Equals(label.Kind, kind, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(system) && bool.TryParse(system, out bool isSystem))
        {
            labels = labels.Where(label => label.IsSystem == isSystem);
        }

        labels = sort switch
        {
            "displayName" => labels.OrderBy(label => label.DisplayName).ThenBy(label => label.SortOrder),
            "kind" => labels.OrderBy(label => label.Kind).ThenBy(label => label.SortOrder).ThenBy(label => label.DisplayName),
            "updated" => labels.OrderByDescending(label => label.UpdatedAtUtc).ThenBy(label => label.DisplayName),
            "words" => labels.OrderByDescending(label => label.WordCount).ThenBy(label => label.DisplayName),
            _ => labels.OrderBy(label => label.SortOrder).ThenBy(label => label.DisplayName),
        };

        ViewData["AdminLabelQuery"] = q ?? string.Empty;
        ViewData["AdminLabelKind"] = kind ?? string.Empty;
        ViewData["AdminLabelSystem"] = system ?? string.Empty;
        ViewData["AdminLabelSort"] = string.IsNullOrWhiteSpace(sort) ? "sortOrder" : sort;
        ViewData["AdminLabelKinds"] = allLabels.Select(item => item.Kind).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToArray();
        ViewData["AdminLabelTotalCount"] = allLabels.Length;

        return View(new AdminLabelsPageViewModel(labels.ToArray()));
    }

    [HttpGet("new", Name = "Admin_LabelNew")]
    public IActionResult New()
    {
        return View("Edit", AdminLabelEditViewModel.CreateNew());
    }

    [HttpPost("new", Name = "Admin_LabelNewPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> New(AdminLabelEditViewModel form, CancellationToken cancellationToken)
    {
        form.LabelId = Guid.Empty;

        if (!ModelState.IsValid)
        {
            return View("Edit", form);
        }

        try
        {
            AdminLabelItemViewModel label = await operationsQueryService
                .CreateLabelAsync(ToRequest(form), cancellationToken)
                .ConfigureAwait(false);

            TempData["AdminStatusMessage"] = "Label was created.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Edit), new { labelId = label.LabelId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Edit", form);
        }
    }

    [HttpGet("{labelId:guid}/edit", Name = "Admin_LabelEdit")]
    public async Task<IActionResult> Edit(Guid labelId, CancellationToken cancellationToken)
    {
        AdminLabelItemViewModel? label = await operationsQueryService
            .GetLabelAsync(labelId, cancellationToken)
            .ConfigureAwait(false);

        if (label is null)
        {
            return NotFound();
        }

        await SetMergeTargetsAsync(labelId, cancellationToken).ConfigureAwait(false);
        return View(AdminLabelEditViewModel.FromItem(label));
    }

    [HttpPost("{labelId:guid}/edit", Name = "Admin_LabelEditPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid labelId, AdminLabelEditViewModel form, CancellationToken cancellationToken)
    {
        if (labelId != form.LabelId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await SetMergeTargetsAsync(labelId, cancellationToken).ConfigureAwait(false);
            return View(form);
        }

        try
        {
            AdminLabelItemViewModel? label = await operationsQueryService
                .UpdateLabelAsync(labelId, ToRequest(form), cancellationToken)
                .ConfigureAwait(false);

            if (label is null)
            {
                return NotFound();
            }

            TempData["AdminStatusMessage"] = "Label was updated.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Edit), new { labelId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await SetMergeTargetsAsync(labelId, cancellationToken).ConfigureAwait(false);
            return View(form);
        }
    }

    [HttpPost("{labelId:guid}/rename", Name = "Admin_LabelRenamePost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rename(Guid labelId, AdminLabelEditViewModel form, CancellationToken cancellationToken)
    {
        if (labelId != form.LabelId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await SetMergeTargetsAsync(labelId, cancellationToken).ConfigureAwait(false);
            return View("Edit", form);
        }

        try
        {
            AdminLabelItemViewModel? label = await operationsQueryService
                .RenameLabelAsync(labelId, ToRequest(form), cancellationToken)
                .ConfigureAwait(false);

            if (label is null)
            {
                return NotFound();
            }

            TempData["AdminStatusMessage"] = "Label was renamed and attached words were updated.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Edit), new { labelId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await SetMergeTargetsAsync(labelId, cancellationToken).ConfigureAwait(false);
            return View("Edit", form);
        }
    }

    [HttpPost("{labelId:guid}/merge", Name = "Admin_LabelMergePost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Merge(Guid labelId, Guid targetLabelId, CancellationToken cancellationToken)
    {
        if (targetLabelId == Guid.Empty)
        {
            TempData["AdminErrorMessage"] = "Select a target label first.";
            return RedirectToAction(nameof(Edit), new { labelId });
        }

        try
        {
            AdminLabelItemViewModel? target = await operationsQueryService
                .MergeLabelAsync(labelId, new AdminMergeLabelRequest(targetLabelId), cancellationToken)
                .ConfigureAwait(false);

            if (target is null)
            {
                return NotFound();
            }

            TempData["AdminStatusMessage"] = "Label was merged and duplicate word labels were removed.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Edit), new { labelId = target.LabelId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["AdminErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Edit), new { labelId });
        }
    }

    private static AdminSaveLabelRequest ToRequest(AdminLabelEditViewModel form) =>
        new(
            form.Kind,
            form.Key,
            form.DisplayName,
            form.SortOrder,
            form.IsSystem);

    private async Task SetMergeTargetsAsync(Guid labelId, CancellationToken cancellationToken)
    {
        AdminLabelsPageViewModel labels = await operationsQueryService
            .GetLabelsAsync(cancellationToken)
            .ConfigureAwait(false);

        ViewData["MergeTargets"] = labels.Labels
            .Where(label => label.LabelId != labelId)
            .OrderBy(label => label.Kind)
            .ThenBy(label => label.SortOrder)
            .ThenBy(label => label.DisplayName)
            .ToArray();
    }

    private ValueTask EvictCatalogCacheAsync(CancellationToken cancellationToken) =>
        outputCacheStore.EvictByTagAsync(CatalogCacheTag, cancellationToken);
}
