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
    IWebProductAnalyticsService? analyticsService = null) : Controller
{
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
        ConversationEventListFilterModel filter = new(city, cefrLevel, helperLanguageCode, isOnline, priceType, category);
        IReadOnlyList<ConversationEventListItemModel> events = await catalogApiClient
            .GetConversationEventsAsync(filter, cancellationToken)
            .ConfigureAwait(false);

        return View(new ConversationEventIndexPageViewModel(events, filter));
    }

    [HttpGet("{slug}", Name = "ConversationEvents_Detail")]
    [OutputCache(NoStore = true)]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return RedirectToAction(nameof(Index));
        }

        ConversationEventDetailModel? conversationEvent = await catalogApiClient
            .GetConversationEventBySlugAsync(slug, cancellationToken)
            .ConfigureAwait(false);

        if (conversationEvent is null)
        {
            return NotFound();
        }

        analyticsService?.Record(WebProductAnalyticsEvents.EventViewed, $"event:{conversationEvent.Slug}");

        IReadOnlyList<EventPreparationPackListItemModel> preparationPacks = [];
        if (await featureAccessService.CanUseEventPreparationPacksAsync(cancellationToken).ConfigureAwait(false))
        {
            List<EventPreparationPackListItemModel> packs = [];
            foreach (string preparationPackSlug in conversationEvent.LinkedEventPreparationPackSlugs)
            {
                EventPreparationPackDetailModel? pack = await catalogApiClient
                    .GetEventPreparationPackBySlugAsync(preparationPackSlug, cancellationToken)
                    .ConfigureAwait(false);

                if (pack is not null)
                {
                    packs.Add(new EventPreparationPackListItemModel(
                        pack.Slug,
                        pack.Title,
                        pack.Description,
                        pack.CefrLevel,
                        pack.Category,
                        pack.EventType,
                        pack.TopicKeys,
                        pack.LinkedScenarioSlugs,
                        pack.LinkedConversationStarterPackSlugs));
                }
            }

            preparationPacks = packs;
        }

        EventRsvpSummaryModel rsvpSummary = await catalogApiClient
            .GetEventRsvpSummaryAsync(slug, cancellationToken)
            .ConfigureAwait(false);

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
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Required RSVP fields are missing or invalid.";
            return RedirectToAction(nameof(Detail), new { slug });
        }

        try
        {
            EventRsvpModel rsvp = await catalogApiClient.SubmitEventRsvpAsync(
                    slug,
                    new SubmitEventRsvpRequest(input.ParticipantName, input.ParticipantEmail, input.Status),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = $"RSVP saved as {rsvp.Status}.";
            analyticsService?.Record(WebProductAnalyticsEvents.EventRsvpSubmitted, $"event:{slug}:{rsvp.Status}");
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Detail), new { slug });
    }
}
