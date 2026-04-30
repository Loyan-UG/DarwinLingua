using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route("conversation-events")]
public sealed class ConversationEventsController(
    IWebCatalogApiClient catalogApiClient,
    IWebEntitledFeatureAccessService featureAccessService,
    ICommunityNotificationEmailService notificationEmailService,
    IAccountEmailRateLimiter rateLimiter,
    IWebProductAnalyticsService? analyticsService = null) : Controller
{
    private const int MaxFilterLength = 128;

    [HttpGet("", Name = "ConversationEvents_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(
        string? city,
        string? cefrLevel,
        string? helperLanguageCode,
        bool? isOnline,
        string? priceType,
        string? category,
        CancellationToken cancellationToken)
    {
        ConversationEventListFilterModel filter = new(
            NormalizeFilter(city),
            NormalizeCefrLevel(cefrLevel),
            NormalizeLanguageCode(helperLanguageCode),
            isOnline,
            NormalizePriceType(priceType),
            NormalizeFilter(category));
        IReadOnlyList<ConversationEventListItemModel> events = await catalogApiClient
            .GetConversationEventsAsync(filter, cancellationToken)
            .ConfigureAwait(false);

        return View(new ConversationEventIndexPageViewModel(events, filter));
    }

    [HttpGet("{slug}", Name = "ConversationEvents_Detail")]
    [OutputCache(NoStore = true)]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        string? normalizedSlug = WebRouteInput.NormalizeSlug(slug);
        if (normalizedSlug is null)
        {
            return RedirectToAction(nameof(Index));
        }

        ConversationEventDetailModel? conversationEvent = await catalogApiClient
            .GetConversationEventBySlugAsync(normalizedSlug, cancellationToken)
            .ConfigureAwait(false);

        if (conversationEvent is null)
        {
            return NotFound();
        }

        analyticsService?.Record(WebProductAnalyticsEvents.EventViewed, $"event:{conversationEvent.Slug}");

        Task<IReadOnlyList<EventPreparationPackListItemModel>> preparationPacksTask = LoadPreparationPacksAsync(
            conversationEvent.LinkedEventPreparationPackSlugs,
            cancellationToken);
        Task<EventRsvpSummaryModel> rsvpSummaryTask = catalogApiClient
            .GetEventRsvpSummaryAsync(normalizedSlug, cancellationToken);
        await Task.WhenAll(preparationPacksTask, rsvpSummaryTask).ConfigureAwait(false);

        return View(new ConversationEventDetailPageViewModel(
            conversationEvent,
            await preparationPacksTask.ConfigureAwait(false),
            await rsvpSummaryTask.ConfigureAwait(false),
            new EventRsvpInputModel
            {
                ParticipantEmail = WebUserIdentity.TryGetEmail(User) ?? string.Empty,
            },
            TempData["StatusMessage"] as string,
            TempData["ErrorMessage"] as string));
    }

    [HttpPost("{slug}/rsvp", Name = "ConversationEvents_Rsvp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rsvp(
        string slug,
        EventRsvpInputModel input,
        CancellationToken cancellationToken)
    {
        string? normalizedSlug = WebRouteInput.NormalizeSlug(slug);
        if (normalizedSlug is null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Required RSVP fields are missing or invalid.";
            return RedirectToAction(nameof(Detail), new { slug = normalizedSlug });
        }

        if (!IsAllowedRsvpStatus(input.Status))
        {
            TempData["ErrorMessage"] = "The selected RSVP status is not supported.";
            return RedirectToAction(nameof(Detail), new { slug = normalizedSlug });
        }

        string participantEmail = input.ParticipantEmail.Trim();
        if (!rateLimiter.TryConsume("event-rsvp", $"{normalizedSlug}:{participantEmail}", 5, TimeSpan.FromMinutes(15)))
        {
            TempData["ErrorMessage"] = "Too many RSVP attempts. Please wait a few minutes and try again.";
            return RedirectToAction(nameof(Detail), new { slug = normalizedSlug });
        }

        try
        {
            EventRsvpModel rsvp = await catalogApiClient.SubmitEventRsvpAsync(
                    normalizedSlug,
                    new SubmitEventRsvpRequest(input.ParticipantName.Trim(), participantEmail, input.Status),
                    cancellationToken)
                .ConfigureAwait(false);
            ConversationEventDetailModel? conversationEvent = await catalogApiClient
                .GetConversationEventBySlugAsync(normalizedSlug, cancellationToken)
                .ConfigureAwait(false);
            await notificationEmailService.SendEventRsvpConfirmationAsync(
                    rsvp.ParticipantEmail,
                    conversationEvent?.Name ?? slug,
                    rsvp.Status,
                    ResolveCulture(),
                    HttpContext.TraceIdentifier,
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = $"RSVP saved as {rsvp.Status}.";
            analyticsService?.Record(WebProductAnalyticsEvents.EventRsvpSubmitted, $"event:{normalizedSlug}:{rsvp.Status}");
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = BuildRsvpErrorMessage(exception);
        }

        return RedirectToAction(nameof(Detail), new { slug = normalizedSlug });
    }

    private string ResolveCulture() =>
        Request.HttpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()
            ?.RequestCulture.UICulture.Name
        ?? Request.Headers.AcceptLanguage.ToString()
        ?? "en";

    private async Task<IReadOnlyList<EventPreparationPackListItemModel>> LoadPreparationPacksAsync(
        IReadOnlyList<string> preparationPackSlugs,
        CancellationToken cancellationToken)
    {
        if (preparationPackSlugs.Count == 0 ||
            !await featureAccessService.CanUseEventPreparationPacksAsync(cancellationToken).ConfigureAwait(false))
        {
            return [];
        }

        Task<EventPreparationPackDetailModel?>[] packTasks = preparationPackSlugs
            .Select(preparationPackSlug => catalogApiClient.GetEventPreparationPackBySlugAsync(preparationPackSlug, cancellationToken))
            .ToArray();
        EventPreparationPackDetailModel?[] packs = await Task.WhenAll(packTasks).ConfigureAwait(false);

        return packs
            .Where(static pack => pack is not null)
            .Select(static pack => new EventPreparationPackListItemModel(
                pack!.Slug,
                pack.Title,
                pack.Description,
                pack.CefrLevel,
                pack.Category,
                pack.EventType,
                pack.TopicKeys,
                pack.LinkedScenarioSlugs,
                pack.LinkedConversationStarterPackSlugs))
            .ToArray();
    }

    private static bool IsAllowedRsvpStatus(string status) =>
        string.Equals(status, "interested", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "going", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "cancelled", StringComparison.OrdinalIgnoreCase);

    private static string BuildRsvpErrorMessage(InvalidOperationException exception) =>
        exception.Message.Contains("404", StringComparison.OrdinalIgnoreCase)
            ? "This conversation event is no longer available."
            : "The RSVP could not be saved right now. Please try again.";

    private static string? NormalizeFilter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length <= MaxFilterLength ? trimmed : trimmed[..MaxFilterLength];
    }

    private static string? NormalizeCefrLevel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return string.Equals(trimmed, "A1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "A2", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "B1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "B2", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "C1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "C2", StringComparison.OrdinalIgnoreCase)
            ? trimmed
            : null;
    }

    private static string? NormalizePriceType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return string.Equals(trimmed, "free", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "donation", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "paid", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "unknown", StringComparison.OrdinalIgnoreCase)
            ? trimmed
            : null;
    }

    private static string? NormalizeLanguageCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length is >= 2 and <= 8 &&
            trimmed.All(static character =>
                (character >= 'a' && character <= 'z') ||
                (character >= 'A' && character <= 'Z') ||
                character == '-')
            ? trimmed
            : null;
    }
}
