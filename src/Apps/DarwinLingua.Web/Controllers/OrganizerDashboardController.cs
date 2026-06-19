using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Identity;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Authorize(Policy = "Organizer")]
[Route("organizer")]
public sealed class OrganizerDashboardController(
    IWebCatalogApiClient catalogApiClient,
    IStringLocalizer<SharedResource> localizer,
    ILogger<OrganizerDashboardController> logger) : Controller
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

        IReadOnlyList<OrganizerProfileOwnerModel> ownerships;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            ownerships = await catalogApiClient
                .GetOrganizerProfileOwnersByEmailAsync(ownerEmail, catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested && exception is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(exception, "Organizer ownerships could not be loaded for {OwnerEmail}.", ownerEmail);
            TempData["ErrorMessage"] = localizer["Organizer dashboard is temporarily unavailable. Please try again."].Value;
            return View(new OrganizerDashboardViewModel(ownerEmail, [], []));
        }

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

        string[] supportedLearnerLevels = SplitCsv(input.SupportedLearnerLevels);
        string[] helperLanguageCodes = SplitCsv(input.HelperLanguageCodes);
        if (!ModelState.IsValid ||
            !HasAllowedCefrLevels(supportedLearnerLevels) ||
            !HasAllowedLanguageCodes(helperLanguageCodes))
        {
            return View("EditProfile", new OrganizerProfileEditPageViewModel(
                profile,
                input,
                null,
                localizer["Required profile fields are missing."].Value));
        }

        try
        {
            OrganizerProfileDetailModel savedProfile = await catalogApiClient
                .SaveAdminOrganizerProfileAsync(
                    new AdminSaveOrganizerProfileRequest(
                        profile.Slug,
                        input.DisplayName.Trim(),
                        input.OrganizerType.Trim(),
                        input.Description.Trim(),
                        TrimToNull(input.CityRegion),
                        input.IsOnlineAvailable,
                        supportedLearnerLevels,
                        helperLanguageCodes,
                        TrimToNull(input.WebsiteUrl),
                        TrimToNull(input.PublicContactMethod),
                        profile.VerificationStatus,
                        profile.PlanKey,
                        profile.HistoricalEventCount),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = localizer["Saved {0}.", savedProfile.DisplayName].Value;
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            return View("EditProfile", new OrganizerProfileEditPageViewModel(
                profile,
                input,
                null,
                BuildOrganizerOperationErrorMessage(exception, "profile")));
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested && exception is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(exception, "Organizer profile could not be saved for {Slug}.", slug);
            return View("EditProfile", new OrganizerProfileEditPageViewModel(
                profile,
                input,
                null,
                localizer["The organizer profile could not be saved right now. Please try again."].Value));
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
            TempData["ErrorMessage"] = localizer["The {0} organizer plan allows {1} active event(s).", plan.PlanKey, plan.ActiveEventLimit].Value;
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
            if (!IsAllowedPublicationStatus(publicationStatus))
            {
                TempData["ErrorMessage"] = localizer["The selected publication status is not supported."].Value;
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(publicationStatus, "Active", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(conversationEvent.PublicationStatus, "Active", StringComparison.OrdinalIgnoreCase) &&
                !await CanCreateMoreActiveEventsAsync(profile, cancellationToken).ConfigureAwait(false))
            {
                return RedirectToAction(nameof(Index));
            }

            OrganizerManagedConversationEventModel updatedEvent = await catalogApiClient
                .SetAdminConversationEventPublicationStatusAsync(
                    conversationEvent.Slug,
                    new AdminSetConversationEventPublicationStatusRequest(publicationStatus.Trim()),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = localizer["Updated {0} to {1}.", updatedEvent.Name, updatedEvent.PublicationStatus].Value;
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = BuildOrganizerOperationErrorMessage(exception, "event publication");
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested && exception is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(exception, "Organizer event publication status could not be updated for {EventSlug}.", eventSlug);
            TempData["ErrorMessage"] = localizer["The event publication status could not be updated right now. Please try again."].Value;
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
            if (!IsAllowedRsvpStatus(status))
            {
                TempData["ErrorMessage"] = localizer["The selected RSVP status is not supported."].Value;
                return RedirectToAction(nameof(Index));
            }

            EventRsvpModel updatedRsvp = await catalogApiClient.SetAdminEventRsvpStatusAsync(
                    conversationEvent.Slug,
                    rsvpId,
                    new AdminSetEventRsvpStatusRequest(status.Trim()),
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = localizer["Updated RSVP {0} to {1}.", rsvpId, updatedRsvp.Status].Value;
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = BuildOrganizerOperationErrorMessage(exception, "RSVP update");
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested && exception is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(exception, "Organizer RSVP status could not be updated for {EventSlug}/{RsvpId}.", eventSlug, rsvpId);
            TempData["ErrorMessage"] = localizer["The RSVP status could not be updated right now. Please try again."].Value;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<OrganizerProfileDetailModel?> LoadOwnedProfileAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        string? normalizedSlug = NormalizeSlug(slug);
        if (normalizedSlug is null)
        {
            return null;
        }

        string? ownerEmail = GetCurrentOwnerEmail();
        if (string.IsNullOrWhiteSpace(ownerEmail))
        {
            return null;
        }

        IReadOnlyList<OrganizerProfileOwnerModel> ownerships = await catalogApiClient
            .GetOrganizerProfileOwnersByEmailAsync(ownerEmail, cancellationToken)
            .ConfigureAwait(false);

        if (!ownerships.Any(ownership => string.Equals(ownership.OrganizerProfileSlug, normalizedSlug, StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        return await catalogApiClient.GetOrganizerProfileBySlugAsync(normalizedSlug, cancellationToken).ConfigureAwait(false);
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
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested && exception is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(exception, "Organizer dashboard profile could not be loaded for {Slug}.", organizerProfileSlug);
            TempData["ErrorMessage"] = localizer["Some organizer dashboard data could not be loaded."].Value;
            return null;
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
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested && exception is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(exception, "Organizer dashboard RSVP data could not be loaded for {EventSlug}.", eventSlug);
            return new(
                eventSlug,
                new DashboardEventRsvpData(
                    new EventRsvpSummaryModel(eventSlug, 0, 0, 0, 0, null),
                    []));
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
        string? normalizedProfileSlug = NormalizeSlug(profileSlug);
        string? normalizedEventSlug = NormalizeSlug(eventSlug);
        if (normalizedProfileSlug is null || normalizedEventSlug is null)
        {
            return null;
        }

        IReadOnlyList<OrganizerManagedConversationEventModel> events = await catalogApiClient
            .GetAdminConversationEventsByOrganizerAsync(normalizedProfileSlug, cancellationToken)
            .ConfigureAwait(false);

        return events.SingleOrDefault(item => string.Equals(item.Slug, normalizedEventSlug, StringComparison.OrdinalIgnoreCase));
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

        TempData["ErrorMessage"] = localizer["The {0} organizer plan allows {1} active event(s).", plan.PlanKey, plan.ActiveEventLimit].Value;
        return false;
    }

    private async Task<IActionResult> SaveEventAsync(
        OrganizerProfileDetailModel profile,
        OrganizerManagedConversationEventModel? existingEvent,
        OrganizerEventInputModel input,
        bool isNew,
        CancellationToken cancellationToken)
    {
        string[] supportedLearnerLevels = SplitCsv(input.SupportedLearnerLevels);
        string[] helperLanguageCodes = SplitCsv(input.HelperLanguageCodes);
        string[] linkedPreparationPackSlugs = SplitCsv(input.LinkedEventPreparationPackSlugs);
        if (!ModelState.IsValid ||
            !IsAllowedPriceType(input.PriceType) ||
            !HasAllowedCefrLevels(supportedLearnerLevels) ||
            !HasAllowedLanguageCodes(helperLanguageCodes) ||
            !HasAllowedSlugs(linkedPreparationPackSlugs))
        {
            return View("EditEvent", new OrganizerEventEditPageViewModel(
                profile,
                existingEvent,
                input,
                isNew,
                localizer["Required event fields are missing."].Value));
        }

        DateTime nowUtc = DateTime.UtcNow;
        try
        {
            AdminSaveConversationEventRequest request = new(
                        input.Slug.Trim(),
                        input.Name.Trim(),
                        input.Description.Trim(),
                        TrimToNull(input.City),
                        input.CountryRegion.Trim(),
                        TrimToNull(input.ApproximateLocation),
                        input.IsOnline,
                        input.Category.Trim(),
                        supportedLearnerLevels,
                        helperLanguageCodes,
                        profile.DisplayName,
                        profile.Slug,
                        TrimToNull(input.ExternalLink),
                        TrimToNull(input.ContactMethod),
                        input.ScheduleText.Trim(),
                        null,
                        null,
                        input.PriceType,
                        existingEvent?.VerificationStatus ?? "reviewed",
                        "organizer-dashboard",
                        profile.WebsiteUrl,
                        nowUtc,
                        linkedPreparationPackSlugs)
            {
                RecurrenceRule = TrimToNull(input.RecurrenceRule),
                Capacity = input.Capacity,
            };

            ConversationEventDetailModel savedEvent = await catalogApiClient
                .SaveAdminConversationEventAsync(request, cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = localizer["Saved event {0}.", savedEvent.Name].Value;
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            return View("EditEvent", new OrganizerEventEditPageViewModel(
                profile,
                existingEvent,
                input,
                isNew,
                BuildOrganizerOperationErrorMessage(exception, "event")));
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested && exception is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(exception, "Organizer event could not be saved for profile {Slug}.", profile.Slug);
            return View("EditEvent", new OrganizerEventEditPageViewModel(
                profile,
                existingEvent,
                input,
                isNew,
                localizer["The organizer event could not be saved right now. Please try again."].Value));
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

    private static string? TrimToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static bool HasAllowedCefrLevels(IReadOnlyCollection<string> levels) =>
        levels.Count > 0 && levels.All(static level =>
            string.Equals(level, "A1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(level, "A2", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(level, "B1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(level, "B2", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(level, "C1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(level, "C2", StringComparison.OrdinalIgnoreCase));

    private static bool HasAllowedLanguageCodes(IReadOnlyCollection<string> languageCodes) =>
        languageCodes.Count > 0 && languageCodes.All(static code =>
            code.Length is >= 2 and <= 8 &&
            code.All(static character =>
                (character >= 'a' && character <= 'z') ||
                (character >= 'A' && character <= 'Z') ||
                character == '-'));

    private static bool HasAllowedSlugs(IReadOnlyCollection<string> slugs) =>
        slugs.Count == 0 || slugs.All(IsSlug);

    private static bool IsSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128)
        {
            return false;
        }

        string slug = value.Trim();
        bool previousWasDash = true;
        foreach (char character in slug)
        {
            bool isAlphaNumeric = (character >= 'a' && character <= 'z') ||
                (character >= '0' && character <= '9');
            if (isAlphaNumeric)
            {
                previousWasDash = false;
                continue;
            }

            if (character != '-' || previousWasDash)
            {
                return false;
            }

            previousWasDash = true;
        }

        return !previousWasDash;
    }

    private static string? NormalizeSlug(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return IsSlug(trimmed) ? trimmed : null;
    }

    private static bool IsAllowedPriceType(string priceType) =>
        string.Equals(priceType, "free", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(priceType, "donation", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(priceType, "paid", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(priceType, "unknown", StringComparison.OrdinalIgnoreCase);

    private static bool IsAllowedPublicationStatus(string status) =>
        string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Draft", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Archived", StringComparison.OrdinalIgnoreCase);

    private static bool IsAllowedRsvpStatus(string status) =>
        string.Equals(status, "interested", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "going", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "cancelled", StringComparison.OrdinalIgnoreCase);

    private string BuildOrganizerOperationErrorMessage(Exception exception, string operationName) =>
        exception.Message.Contains("409", StringComparison.OrdinalIgnoreCase)
            ? localizer["The organizer {0} could not be saved because it conflicts with existing data.", operationName].Value
            : localizer["The organizer {0} operation could not be completed. Review the fields and try again.", operationName].Value;

    private sealed record DashboardEventRsvpData(
        EventRsvpSummaryModel Summary,
        IReadOnlyList<EventRsvpModel> Rsvps);

    private string? GetCurrentOwnerEmail()
    {
        return WebUserIdentity.TryGetEmail(User);
    }
}
