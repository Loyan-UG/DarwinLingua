using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/topics")]
public sealed class TopicsController(IWebAdminOperationsQueryService operationsQueryService) : Controller
{
    [HttpGet("", Name = "Admin_Topics")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await operationsQueryService.GetTopicsAsync(cancellationToken).ConfigureAwait(false));
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

        return topic is null ? NotFound() : View(AdminTopicEditViewModel.FromItem(topic));
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
            return RedirectToAction(nameof(Edit), new { topicId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(form);
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
}
