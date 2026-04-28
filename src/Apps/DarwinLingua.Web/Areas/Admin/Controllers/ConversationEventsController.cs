using System.Globalization;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/conversation-events")]
public sealed class ConversationEventsController(IWebCatalogApiClient catalogApiClient) : Controller
{
    [HttpGet("", Name = "Admin_ConversationEvents")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        AdminConversationEventsPageViewModel viewModel = await BuildViewModelAsync(
            new AdminConversationEventInputModel(),
            TempData["StatusMessage"] as string,
            TempData["ErrorMessage"] as string,
            cancellationToken);

        return View(viewModel);
    }

    [HttpPost("", Name = "Admin_ConversationEvents_Save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        AdminConversationEventInputModel input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildViewModelAsync(input, null, "Required event fields are missing.", cancellationToken));
        }

        DateTime? lastVerifiedAtUtc = null;
        if (!string.IsNullOrWhiteSpace(input.LastVerifiedAtUtc))
        {
            if (!DateTime.TryParse(
                    input.LastVerifiedAtUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out DateTime parsedLastVerifiedAtUtc))
            {
                return View("Index", await BuildViewModelAsync(input, null, "Last verified must be a valid UTC date/time value.", cancellationToken));
            }

            lastVerifiedAtUtc = DateTime.SpecifyKind(parsedLastVerifiedAtUtc, DateTimeKind.Utc);
        }

        AdminSaveConversationEventRequest request = new(
            input.Slug,
            input.Name,
            input.Description,
            input.City,
            input.CountryRegion,
            input.ApproximateLocation,
            input.IsOnline,
            input.Category,
            SplitCsv(input.SupportedLearnerLevels),
            SplitCsv(input.HelperLanguageCodes),
            input.OrganizerName,
            input.OrganizerProfileSlug,
            input.ExternalLink,
            input.ContactMethod,
            input.ScheduleText,
            input.PriceType,
            input.VerificationStatus,
            input.SourceName,
            input.SourceUrl,
            lastVerifiedAtUtc,
            SplitCsv(input.LinkedEventPreparationPackSlugs))
        {
            RecurrenceRule = input.RecurrenceRule,
            Capacity = input.Capacity,
        };

        try
        {
            ConversationEventDetailModel savedEvent = await catalogApiClient
                .SaveAdminConversationEventAsync(request, cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = $"Saved event '{savedEvent.Name}'.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            return View("Index", await BuildViewModelAsync(input, null, exception.Message, cancellationToken));
        }
    }

    private async Task<AdminConversationEventsPageViewModel> BuildViewModelAsync(
        AdminConversationEventInputModel input,
        string? statusMessage,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ConversationEventListItemModel> events = await catalogApiClient
            .GetConversationEventsAsync(new ConversationEventListFilterModel(null, null, null, null, null, null), cancellationToken)
            .ConfigureAwait(false);

        return new AdminConversationEventsPageViewModel(events, input, statusMessage, errorMessage);
    }

    private static string[] SplitCsv(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
