using System.Globalization;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Route("conversation-events")]
public sealed class ConversationEventsController(
    IWebCatalogApiClient catalogApiClient,
    IWebEntitledFeatureAccessService featureAccessService,
    ICommunityNotificationEmailService notificationEmailService,
    IAccountEmailRateLimiter rateLimiter,
    ILogger<ConversationEventsController> logger,
    IStringLocalizer<SharedResource> localizer,
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
        string? dateFromUtc,
        string? dateToUtc,
        CancellationToken cancellationToken)
    {
        ConversationEventListFilterModel filter = new(
            NormalizeFilter(city),
            NormalizeCefrLevel(cefrLevel),
            NormalizeLanguageCode(helperLanguageCode),
            isOnline,
            NormalizePriceType(priceType),
            NormalizeFilter(category),
            NormalizeDateFilter(dateFromUtc, endOfDay: false),
            NormalizeDateFilter(dateToUtc, endOfDay: true));
        IReadOnlyList<ConversationEventListItemModel> events;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            events = await catalogApiClient
                .GetConversationEventsAsync(filter, catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Conversation events could not be loaded.");
            events = [];
        }

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

        ConversationEventDetailModel? conversationEvent;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            conversationEvent = await catalogApiClient
                .GetConversationEventBySlugAsync(normalizedSlug, catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Conversation event could not be loaded for {Slug}.", normalizedSlug);
            return ServiceUnavailableView(
                localizer["Event is temporarily unavailable"],
                localizer["This conversation event could not be loaded right now. Please return to events and try again."]);
        }

        if (conversationEvent is null)
        {
            return NotFound();
        }

        analyticsService?.Record(WebProductAnalyticsEvents.EventViewed, $"event:{conversationEvent.Slug}");

        IReadOnlyList<EventPreparationPackListItemModel> preparationPacks = [];
        EventRsvpSummaryModel rsvpSummary = new(normalizedSlug, 0, 0, 0, conversationEvent.Capacity ?? 0, conversationEvent.Capacity);

        try
        {
            Task<IReadOnlyList<EventPreparationPackListItemModel>> preparationPacksTask = LoadPreparationPacksAsync(
                conversationEvent.LinkedEventPreparationPackSlugs,
                cancellationToken);
            Task<EventRsvpSummaryModel> rsvpSummaryTask = catalogApiClient
                .GetEventRsvpSummaryAsync(normalizedSlug, cancellationToken);
            await Task.WhenAll(preparationPacksTask, rsvpSummaryTask).ConfigureAwait(false);
            preparationPacks = await preparationPacksTask.ConfigureAwait(false);
            rsvpSummary = await rsvpSummaryTask.ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Conversation event supporting content could not be loaded for {Slug}.", normalizedSlug);
        }

        return View(new ConversationEventDetailPageViewModel(
            conversationEvent,
            preparationPacks,
            rsvpSummary,
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
            TempData["ErrorMessage"] = localizer["Required RSVP fields are missing or invalid."].Value;
            return RedirectToAction(nameof(Detail), new { slug = normalizedSlug });
        }

        if (!IsAllowedRsvpStatus(input.Status))
        {
            TempData["ErrorMessage"] = localizer["The selected RSVP status is not supported."].Value;
            return RedirectToAction(nameof(Detail), new { slug = normalizedSlug });
        }

        string participantEmail = input.ParticipantEmail.Trim();
        if (!rateLimiter.TryConsume("event-rsvp", $"{normalizedSlug}:{participantEmail}", 5, TimeSpan.FromMinutes(15)))
        {
            TempData["ErrorMessage"] = localizer["Too many RSVP attempts. Please wait a few minutes and try again."].Value;
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

            TempData["StatusMessage"] = localizer["RSVP saved as {0}.", rsvp.Status].Value;
            analyticsService?.Record(WebProductAnalyticsEvents.EventRsvpSubmitted, $"event:{normalizedSlug}:{rsvp.Status}");
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = BuildRsvpErrorMessage(exception);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested && exception is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(exception, "Event RSVP could not be submitted for {Slug}.", normalizedSlug);
            TempData["ErrorMessage"] = localizer["The RSVP could not be saved right now. Please try again."].Value;
        }

        return RedirectToAction(nameof(Detail), new { slug = normalizedSlug });
    }

    private ViewResult ServiceUnavailableView(string title, string message)
    {
        Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
        {
            Title = title,
            Message = message,
            RequestId = HttpContext.TraceIdentifier
        });
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

        string? actorEmail = WebUserIdentity.TryGetEmail(User);
        if (string.IsNullOrWhiteSpace(actorEmail))
        {
            return [];
        }

        Task<EventPreparationPackDetailModel?>[] packTasks = preparationPackSlugs
            .Select(preparationPackSlug => catalogApiClient.GetEventPreparationPackBySlugAsync(preparationPackSlug, actorEmail, cancellationToken))
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
                pack.LinkedDialogueSlugs,
                pack.LinkedConversationStarterPackSlugs))
            .ToArray();
    }

    private static bool IsAllowedRsvpStatus(string status) =>
        string.Equals(status, "interested", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "going", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "cancelled", StringComparison.OrdinalIgnoreCase);

    private string BuildRsvpErrorMessage(InvalidOperationException exception) =>
        exception.Message.Contains("404", StringComparison.OrdinalIgnoreCase)
            ? localizer["This conversation event is no longer available."].Value
            : localizer["The RSVP could not be saved right now. Please try again."].Value;

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

    private static DateTime? NormalizeDateFilter(string? value, bool endOfDay)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > MaxFilterLength)
        {
            return null;
        }

        string trimmed = value.Trim();
        if (!DateTime.TryParse(
                trimmed,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out DateTime parsed))
        {
            return null;
        }

        DateTime utc = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
        bool isDateOnly = !trimmed.Contains('T', StringComparison.Ordinal) && !trimmed.Contains(':', StringComparison.Ordinal);
        return endOfDay && isDateOnly
            ? utc.Date.AddDays(1).AddTicks(-1)
            : utc;
    }
}
