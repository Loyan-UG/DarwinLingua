using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/topics")]
public sealed class TopicsController(
    IWebAdminOperationsQueryService operationsQueryService,
    IOutputCacheStore outputCacheStore) : Controller
{
    private const string CatalogCacheTag = "catalog";

    [HttpGet("", Name = "Admin_Topics")]
    public async Task<IActionResult> Index(
        string? q,
        string? system,
        string? sort,
        CancellationToken cancellationToken)
    {
        AdminTopicsPageViewModel page = await operationsQueryService.GetTopicsAsync(cancellationToken).ConfigureAwait(false);
        AdminTopicItemViewModel[] allTopics = page.Topics.ToArray();
        IEnumerable<AdminTopicItemViewModel> topics = allTopics;

        if (!string.IsNullOrWhiteSpace(q))
        {
            topics = topics.Where(topic =>
                topic.Key.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                topic.Localizations.Any(localization =>
                    localization.LanguageCode.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    localization.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(system) && bool.TryParse(system, out bool isSystem))
        {
            topics = topics.Where(topic => topic.IsSystem == isSystem);
        }

        topics = sort switch
        {
            "key" => topics.OrderBy(topic => topic.Key).ThenBy(topic => topic.SortOrder),
            "updated" => topics.OrderByDescending(topic => topic.UpdatedAtUtc).ThenBy(topic => topic.Key),
            "words" => topics.OrderByDescending(topic => topic.WordCount).ThenBy(topic => topic.Key),
            _ => topics.OrderBy(topic => topic.SortOrder).ThenBy(topic => topic.Key),
        };

        ViewData["AdminTopicQuery"] = q ?? string.Empty;
        ViewData["AdminTopicSystem"] = system ?? string.Empty;
        ViewData["AdminTopicSort"] = string.IsNullOrWhiteSpace(sort) ? "sortOrder" : sort;
        ViewData["AdminTopicTotalCount"] = allTopics.Length;

        return View(new AdminTopicsPageViewModel(topics.ToArray()));
    }

    [HttpGet("new", Name = "Admin_TopicNew")]
    public IActionResult New()
    {
        return View("Edit", AdminTopicEditViewModel.CreateNew());
    }

    [HttpPost("new", Name = "Admin_TopicNewPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> New(AdminTopicEditViewModel form, CancellationToken cancellationToken)
    {
        form.TopicId = Guid.Empty;

        if (!ModelState.IsValid)
        {
            return View("Edit", form);
        }

        try
        {
            AdminTopicItemViewModel topic = await operationsQueryService
                .CreateTopicAsync(ToRequest(form), cancellationToken)
                .ConfigureAwait(false);

            TempData["AdminStatusMessage"] = "Topic was created.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Edit), new { topicId = topic.TopicId });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "A topic with the same key already exists.");
            return View("Edit", form);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Edit", form);
        }
    }

    [HttpGet("{topicId:guid}/edit", Name = "Admin_TopicEdit")]
    public async Task<IActionResult> Edit(Guid topicId, CancellationToken cancellationToken)
    {
        AdminTopicItemViewModel? topic = await operationsQueryService
            .GetTopicAsync(topicId, cancellationToken)
            .ConfigureAwait(false);

        if (topic is null)
        {
            return NotFound();
        }

        await SetMergeTargetsAsync(topicId, cancellationToken).ConfigureAwait(false);
        return View(AdminTopicEditViewModel.FromItem(topic));
    }

    [HttpPost("{topicId:guid}/edit", Name = "Admin_TopicEditPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid topicId, AdminTopicEditViewModel form, CancellationToken cancellationToken)
    {
        if (topicId != form.TopicId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await SetMergeTargetsAsync(topicId, cancellationToken).ConfigureAwait(false);
            return View(form);
        }

        try
        {
            AdminTopicItemViewModel? topic = await operationsQueryService
                .UpdateTopicAsync(topicId, ToRequest(form), cancellationToken)
                .ConfigureAwait(false);

            if (topic is null)
            {
                return NotFound();
            }

            TempData["AdminStatusMessage"] = "Topic was updated.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Edit), new { topicId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await SetMergeTargetsAsync(topicId, cancellationToken).ConfigureAwait(false);
            return View(form);
        }
    }

    [HttpPost("{topicId:guid}/merge", Name = "Admin_TopicMergePost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Merge(Guid topicId, Guid targetTopicId, CancellationToken cancellationToken)
    {
        if (targetTopicId == Guid.Empty)
        {
            TempData["AdminErrorMessage"] = "Select a target topic first.";
            return RedirectToAction(nameof(Edit), new { topicId });
        }

        try
        {
            AdminTopicItemViewModel? target = await operationsQueryService
                .MergeTopicAsync(topicId, new AdminMergeTopicRequest(targetTopicId), cancellationToken)
                .ConfigureAwait(false);

            if (target is null)
            {
                return NotFound();
            }

            TempData["AdminStatusMessage"] = "Topic was merged and duplicate links were removed.";
            await EvictCatalogCacheAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(Edit), new { topicId = target.TopicId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["AdminErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Edit), new { topicId });
        }
    }

    private static AdminSaveTopicRequest ToRequest(AdminTopicEditViewModel form)
    {
        List<AdminTopicLocalizationRequest> localizations = [];
        AddLocalization(localizations, form.PrimaryLanguageCode, form.PrimaryDisplayName);
        AddLocalization(localizations, form.SecondaryLanguageCode, form.SecondaryDisplayName);

        return new AdminSaveTopicRequest(
            form.Key,
            form.SortOrder,
            form.IsSystem,
            localizations);
    }

    private static void AddLocalization(List<AdminTopicLocalizationRequest> localizations, string? languageCode, string? displayName)
    {
        if (!string.IsNullOrWhiteSpace(languageCode) && !string.IsNullOrWhiteSpace(displayName))
        {
            localizations.Add(new AdminTopicLocalizationRequest(languageCode.Trim(), displayName.Trim()));
        }
    }

    private async Task SetMergeTargetsAsync(Guid topicId, CancellationToken cancellationToken)
    {
        AdminTopicsPageViewModel topics = await operationsQueryService
            .GetTopicsAsync(cancellationToken)
            .ConfigureAwait(false);

        ViewData["MergeTargets"] = topics.Topics
            .Where(topic => topic.TopicId != topicId)
            .OrderBy(topic => topic.SortOrder)
            .ThenBy(topic => topic.Key)
            .ToArray();
    }

    private ValueTask EvictCatalogCacheAsync(CancellationToken cancellationToken) =>
        outputCacheStore.EvictByTagAsync(CatalogCacheTag, cancellationToken);
}
