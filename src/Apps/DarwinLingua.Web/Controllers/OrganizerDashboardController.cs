using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Identity;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Authorize(Policy = "Organizer")]
[Route("organizer")]
public sealed class OrganizerDashboardController(IWebCatalogApiClient catalogApiClient) : Controller
{
    private const int DashboardProfileLoadConcurrency = 4;
    private const int DashboardEventLoadConcurrency = 8;

    [HttpGet("", Name = "OrganizerDashboard_Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        string? ownerEmail = GetCurrentOwnerEmail();
        if (string.IsNullOrWhiteSpace(ownerEmail))
        {
            return Challenge();
        }

        IReadOnlyList<OrganizerProfileOwnerModel> ownerships = await catalogApiClient
            .GetOrganizerProfileOwnersByEmailAsync(ownerEmail, cancellationToken)
            .ConfigureAwait(false);

        OrganizerDashboardProfileViewModel?[] loadedProfiles = await LoadDashboardProfilesAsync(ownerships, cancellationToken)
            .ConfigureAwait(false);
        List<OrganizerDashboardProfileViewModel> profiles = loadedProfiles
            .Where(static profile => profile is not null)
            .Select(static profile => profile!)
            .ToList();

        return View(new OrganizerDashboardViewModel(ownerEmail, ownerships, profiles));
    }

    [HttpGet("profiles/{slug}/edit", Name = "OrganizerDashboard_EditProfile")]
    public async Task<IActionResult> EditProfile(string slug, CancellationToken cancellationToken)
    {
        OrganizerProfileDetailModel? profile = await LoadOwnedProfileAsync(slug, cancellationToken).ConfigureAwait(false);
        if (profile is null)
        {
            return Forbid();
        }

        return View(new OrganizerProfileEditPageViewModel(
            profile,
            CreateEditInput(profile),
            TempData["StatusMessage"] as string,
            TempData["ErrorMessage"] as string));
    }

    [HttpPost("profiles/{slug}/edit", Name = "OrganizerDashboard_SaveProfile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProfile(
        string slug,
        OrganizerProfileEditInputModel input,
        CancellationToken cancellationToken)
    {
        OrganizerProfileDetailModel? profile = await LoadOwnedProfileAsync(slug, cancellationToken).ConfigureAwait(false);
        if (profile is null)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View("EditProfile", new OrganizerProfileEditPageViewModel(
                profile,
                input,
                null,
                "Required profile fields are missing."));
        }

        try
        {
            OrganizerProfileDetailModel savedProfile = await catalogApiClient
                .SaveAdminOrganizerProfileAsync(
                    new AdminSaveOrganizerProfileRequest(
                        profile.Slug,
                        input.DisplayName,
                        input.OrganizerType,
                        input.Description,
                        input.CityRegion,
                        input.IsOnlineAvailable,
                        SplitCsv(input.SupportedLearnerLevels),
                        SplitCsv(input.HelperLanguageCodes),
                        input.WebsiteUrl,
                        input.PublicContactMethod,
                        profile.VerificationStatus,
                        profile.PlanKey,
                        profile.HistoricalEventCount),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = $"Saved {savedProfile.DisplayName}.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            return View("EditProfile", new OrganizerProfileEditPageViewModel(profile, input, null, exception.Message));
        }
    }

    [HttpGet("profiles/{profileSlug}/events/new", Name = "OrganizerDashboard_NewEvent")]
    public async Task<IActionResult> NewEvent(string profileSlug, CancellationToken cancellationToken)
    {
        OrganizerProfileDetailModel? profile = await LoadOwnedProfileAsync(profileSlug, cancellationToken).ConfigureAwait(false);
        if (profile is null)
        {
            return Forbid();
        }

        if (!await CanCreateMoreActiveEventsAsync(profile, cancellationToken).ConfigureAwait(false))
        {
            return RedirectToAction(nameof(Index));
        }

        return View("EditEvent", new OrganizerEventEditPageViewModel(
            profile,
            null,
            new OrganizerEventInputModel
            {
                CountryRegion = profile.CityRegion ?? "DE",
                HelperLanguageCodes = string.Join(",", profile.HelperLanguageCodes),
                SupportedLearnerLevels = profile.SupportedLearnerLevels.Count == 0
                    ? "A1"
                    : string.Join(",", profile.SupportedLearnerLevels),
            },
            true,
            null));
    }

    [HttpGet("profiles/{profileSlug}/events/{eventSlug}/edit", Name = "OrganizerDashboard_EditEvent")]
    public async Task<IActionResult> EditEvent(
        string profileSlug,
        string eventSlug,
        CancellationToken cancellationToken)
    {
        OrganizerProfileDetailModel? profile = await LoadOwnedProfileAsync(profileSlug, cancellationToken).ConfigureAwait(false);
        if (profile is null)
        {
            return Forbid();
        }

        OrganizerManagedConversationEventModel? conversationEvent = await LoadOwnedEventAsync(profileSlug, eventSlug, cancellationToken).ConfigureAwait(false);
        if (conversationEvent is null)
        {
            return NotFound();
        }

        return View(new OrganizerEventEditPageViewModel(
            profile,
            conversationEvent,
            CreateEventInput(conversationEvent),
            false,
            null));
    }

    [HttpPost("profiles/{profileSlug}/events/new", Name = "OrganizerDashboard_CreateEvent")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(
        string profileSlug,
        OrganizerEventInputModel input,
        CancellationToken cancellationToken)
    {
        OrganizerProfileDetailModel? profile = await LoadOwnedProfileAsync(profileSlug, cancellationToken).ConfigureAwait(false);
        if (profile is null)
        {
            return Forbid();
        }

        DarwinLinguaOrganizerPlanSnapshot plan = DarwinLinguaOrganizerPlanPolicy.Resolve(profile.PlanKey);
        IReadOnlyList<OrganizerManagedConversationEventModel> events = await catalogApiClient
            .GetAdminConversationEventsByOrganizerAsync(profile.Slug, cancellationToken)
            .ConfigureAwait(false);
        int activeEventCount = events.Count(item => string.Equals(item.PublicationStatus, "Active", StringComparison.OrdinalIgnoreCase));
        if (activeEventCount >= plan.ActiveEventLimit)
        {
            TempData["ErrorMessage"] = $"The {plan.PlanKey} organizer plan allows {plan.ActiveEventLimit} active event(s).";
            return RedirectToAction(nameof(Index));
        }

        return await SaveEventAsync(profile, null, input, isNew: true, cancellationToken).ConfigureAwait(false);
    }

    [HttpPost("profiles/{profileSlug}/events/{eventSlug}/edit", Name = "OrganizerDashboard_SaveEvent")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveEvent(
        string profileSlug,
        string eventSlug,
        OrganizerEventInputModel input,
        CancellationToken cancellationToken)
    {
        OrganizerProfileDetailModel? profile = await LoadOwnedProfileAsync(profileSlug, cancellationToken).ConfigureAwait(false);
        if (profile is null)
        {
            return Forbid();
        }

        OrganizerManagedConversationEventModel? conversationEvent = await LoadOwnedEventAsync(profileSlug, eventSlug, cancellationToken).ConfigureAwait(false);
        if (conversationEvent is null)
        {
            return NotFound();
        }

        input.Slug = conversationEvent.Slug;
        return await SaveEventAsync(profile, conversationEvent, input, isNew: false, cancellationToken).ConfigureAwait(false);
    }

    [HttpPost("profiles/{profileSlug}/events/{eventSlug}/publication-status", Name = "OrganizerDashboard_SetEventPublicationStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetEventPublicationStatus(
        string profileSlug,
        string eventSlug,
        string publicationStatus,
        CancellationToken cancellationToken)
    {
        OrganizerProfileDetailModel? profile = await LoadOwnedProfileAsync(profileSlug, cancellationToken).ConfigureAwait(false);
        if (profile is null)
        {
            return Forbid();
        }

        OrganizerManagedConversationEventModel? conversationEvent = await LoadOwnedEventAsync(profileSlug, eventSlug, cancellationToken).ConfigureAwait(false);
        if (conversationEvent is null)
        {
            return NotFound();
        }

        try
        {
            if (string.Equals(publicationStatus, "Active", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(conversationEvent.PublicationStatus, "Active", StringComparison.OrdinalIgnoreCase) &&
                !await CanCreateMoreActiveEventsAsync(profile, cancellationToken).ConfigureAwait(false))
            {
                return RedirectToAction(nameof(Index));
            }

            OrganizerManagedConversationEventModel updatedEvent = await catalogApiClient
                .SetAdminConversationEventPublicationStatusAsync(
                    eventSlug,
                    new AdminSetConversationEventPublicationStatusRequest(publicationStatus),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = $"Updated {updatedEvent.Name} to {updatedEvent.PublicationStatus}.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("profiles/{profileSlug}/events/{eventSlug}/rsvps/{rsvpId:guid}/status", Name = "OrganizerDashboard_SetRsvpStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRsvpStatus(
        string profileSlug,
        string eventSlug,
        Guid rsvpId,
        string status,
        CancellationToken cancellationToken)
    {
        OrganizerProfileDetailModel? profile = await LoadOwnedProfileAsync(profileSlug, cancellationToken).ConfigureAwait(false);
        if (profile is null)
        {
            return Forbid();
        }

        OrganizerManagedConversationEventModel? conversationEvent = await LoadOwnedEventAsync(profileSlug, eventSlug, cancellationToken).ConfigureAwait(false);
        if (conversationEvent is null)
        {
            return NotFound();
        }

        try
        {
            EventRsvpModel updatedRsvp = await catalogApiClient.SetAdminEventRsvpStatusAsync(
                    eventSlug,
                    rsvpId,
                    new AdminSetEventRsvpStatusRequest(status),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = $"Updated RSVP {rsvpId} to {updatedRsvp.Status}.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<OrganizerProfileDetailModel?> LoadOwnedProfileAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        string? ownerEmail = GetCurrentOwnerEmail();
        if (string.IsNullOrWhiteSpace(ownerEmail))
        {
            return null;
        }

        IReadOnlyList<OrganizerProfileOwnerModel> ownerships = await catalogApiClient
            .GetOrganizerProfileOwnersByEmailAsync(ownerEmail, cancellationToken)
            .ConfigureAwait(false);

        if (!ownerships.Any(ownership => string.Equals(ownership.OrganizerProfileSlug, slug, StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        return await catalogApiClient.GetOrganizerProfileBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
    }

    private async Task<OrganizerDashboardProfileViewModel?[]> LoadDashboardProfilesAsync(
        IReadOnlyList<OrganizerProfileOwnerModel> ownerships,
        CancellationToken cancellationToken)
    {
        using SemaphoreSlim gate = new(DashboardProfileLoadConcurrency);
        Task<OrganizerDashboardProfileViewModel?>[] tasks = ownerships
            .Select(ownership => LoadDashboardProfileWithGateAsync(ownership.OrganizerProfileSlug, gate, cancellationToken))
            .ToArray();

        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task<OrganizerDashboardProfileViewModel?> LoadDashboardProfileWithGateAsync(
        string organizerProfileSlug,
        SemaphoreSlim gate,
        CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await LoadDashboardProfileAsync(organizerProfileSlug, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<OrganizerDashboardProfileViewModel?> LoadDashboardProfileAsync(
        string organizerProfileSlug,
        CancellationToken cancellationToken)
    {
        OrganizerProfileDetailModel? profile = await catalogApiClient
            .GetOrganizerProfileBySlugAsync(organizerProfileSlug, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
        {
            return null;
        }

        IReadOnlyList<OrganizerManagedConversationEventModel> events = await catalogApiClient
            .GetAdminConversationEventsByOrganizerAsync(profile.Slug, cancellationToken)
            .ConfigureAwait(false);

        using SemaphoreSlim eventGate = new(DashboardEventLoadConcurrency);
        Task<KeyValuePair<string, DashboardEventRsvpData>>[] rsvpTasks = events
            .Select(conversationEvent => LoadEventRsvpDataAsync(conversationEvent.Slug, eventGate, cancellationToken))
            .ToArray();
        KeyValuePair<string, DashboardEventRsvpData>[] rsvpRows = await Task.WhenAll(rsvpTasks).ConfigureAwait(false);

        Dictionary<string, EventRsvpSummaryModel> rsvpSummaries = rsvpRows
            .ToDictionary(static item => item.Key, static item => item.Value.Summary, StringComparer.OrdinalIgnoreCase);
        Dictionary<string, IReadOnlyList<EventRsvpModel>> eventRsvps = rsvpRows
            .ToDictionary(static item => item.Key, static item => item.Value.Rsvps, StringComparer.OrdinalIgnoreCase);

        return new OrganizerDashboardProfileViewModel(
            profile,
            events,
            rsvpSummaries,
            eventRsvps,
            DarwinLinguaOrganizerPlanPolicy.Resolve(profile.PlanKey),
            CreateAnalytics(events));
    }

    private async Task<KeyValuePair<string, DashboardEventRsvpData>> LoadEventRsvpDataAsync(
        string eventSlug,
        SemaphoreSlim gate,
        CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Task<EventRsvpSummaryModel> summaryTask = catalogApiClient.GetEventRsvpSummaryAsync(eventSlug, cancellationToken);
            Task<IReadOnlyList<EventRsvpModel>> rsvpsTask = catalogApiClient.GetAdminEventRsvpsAsync(eventSlug, cancellationToken);
            await Task.WhenAll(summaryTask, rsvpsTask).ConfigureAwait(false);

            return new(
                eventSlug,
                new DashboardEventRsvpData(
                    await summaryTask.ConfigureAwait(false),
                    await rsvpsTask.ConfigureAwait(false)));
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<OrganizerManagedConversationEventModel?> LoadOwnedEventAsync(
        string profileSlug,
        string eventSlug,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<OrganizerManagedConversationEventModel> events = await catalogApiClient
            .GetAdminConversationEventsByOrganizerAsync(profileSlug, cancellationToken)
            .ConfigureAwait(false);

        return events.SingleOrDefault(item => string.Equals(item.Slug, eventSlug, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<bool> CanCreateMoreActiveEventsAsync(
        OrganizerProfileDetailModel profile,
        CancellationToken cancellationToken)
    {
        DarwinLinguaOrganizerPlanSnapshot plan = DarwinLinguaOrganizerPlanPolicy.Resolve(profile.PlanKey);
        IReadOnlyList<OrganizerManagedConversationEventModel> events = await catalogApiClient
            .GetAdminConversationEventsByOrganizerAsync(profile.Slug, cancellationToken)
            .ConfigureAwait(false);
        int activeEventCount = events.Count(item => string.Equals(item.PublicationStatus, "Active", StringComparison.OrdinalIgnoreCase));
        if (activeEventCount < plan.ActiveEventLimit)
        {
            return true;
        }

        TempData["ErrorMessage"] = $"The {plan.PlanKey} organizer plan allows {plan.ActiveEventLimit} active event(s).";
        return false;
    }

    private async Task<IActionResult> SaveEventAsync(
        OrganizerProfileDetailModel profile,
        OrganizerManagedConversationEventModel? existingEvent,
        OrganizerEventInputModel input,
        bool isNew,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("EditEvent", new OrganizerEventEditPageViewModel(
                profile,
                existingEvent,
                input,
                isNew,
                "Required event fields are missing."));
        }

        DateTime nowUtc = DateTime.UtcNow;
        try
        {
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
                        profile.DisplayName,
                        profile.Slug,
                        input.ExternalLink,
                        input.ContactMethod,
                        input.ScheduleText,
                        input.PriceType,
                        existingEvent?.VerificationStatus ?? "reviewed",
                        "organizer-dashboard",
                        profile.WebsiteUrl,
                        nowUtc,
                        SplitCsv(input.LinkedEventPreparationPackSlugs))
            {
                RecurrenceRule = input.RecurrenceRule,
                Capacity = input.Capacity,
            };

            ConversationEventDetailModel savedEvent = await catalogApiClient
                .SaveAdminConversationEventAsync(request, cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = $"Saved event {savedEvent.Name}.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            return View("EditEvent", new OrganizerEventEditPageViewModel(profile, existingEvent, input, isNew, exception.Message));
        }
    }

    private static OrganizerProfileEditInputModel CreateEditInput(OrganizerProfileDetailModel profile) =>
        new()
        {
            DisplayName = profile.DisplayName,
            OrganizerType = profile.OrganizerType,
            Description = profile.Description,
            CityRegion = profile.CityRegion,
            IsOnlineAvailable = profile.IsOnlineAvailable,
            SupportedLearnerLevels = string.Join(",", profile.SupportedLearnerLevels),
            HelperLanguageCodes = string.Join(",", profile.HelperLanguageCodes),
            WebsiteUrl = profile.WebsiteUrl,
            PublicContactMethod = profile.PublicContactMethod,
        };

    private static OrganizerEventInputModel CreateEventInput(OrganizerManagedConversationEventModel conversationEvent) =>
        new()
        {
            Slug = conversationEvent.Slug,
            Name = conversationEvent.Name,
            Description = conversationEvent.Description,
            City = conversationEvent.City,
            CountryRegion = conversationEvent.CountryRegion,
            ApproximateLocation = conversationEvent.ApproximateLocation,
            IsOnline = conversationEvent.IsOnline,
            Category = conversationEvent.Category,
            SupportedLearnerLevels = string.Join(",", conversationEvent.SupportedLearnerLevels),
            HelperLanguageCodes = string.Join(",", conversationEvent.HelperLanguageCodes),
            ExternalLink = conversationEvent.ExternalLink,
            ContactMethod = conversationEvent.ContactMethod,
            ScheduleText = conversationEvent.ScheduleText,
            RecurrenceRule = conversationEvent.RecurrenceRule,
            Capacity = conversationEvent.Capacity,
            PriceType = conversationEvent.PriceType,
            LinkedEventPreparationPackSlugs = string.Join(",", conversationEvent.LinkedEventPreparationPackSlugs),
        };

    private static OrganizerDashboardAnalyticsViewModel CreateAnalytics(IReadOnlyList<OrganizerManagedConversationEventModel> events) =>
        new(
            events.Count,
            events.Count(item => string.Equals(item.PublicationStatus, "Active", StringComparison.OrdinalIgnoreCase)),
            events.Count(item => string.Equals(item.PublicationStatus, "Archived", StringComparison.OrdinalIgnoreCase)),
            events.Count(item => item.IsOnline),
            events.Count(item => !item.IsOnline),
            events.Sum(item => item.Capacity ?? 0));

    private static string[] SplitCsv(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    private sealed record DashboardEventRsvpData(
        EventRsvpSummaryModel Summary,
        IReadOnlyList<EventRsvpModel> Rsvps);

    private string? GetCurrentOwnerEmail()
    {
        return WebUserIdentity.TryGetEmail(User);
    }
}
