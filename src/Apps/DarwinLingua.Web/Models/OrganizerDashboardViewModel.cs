using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace DarwinLingua.Web.Models;

public sealed record OrganizerDashboardViewModel(
    string OwnerEmail,
    IReadOnlyList<OrganizerProfileOwnerModel> Ownerships,
    IReadOnlyList<OrganizerDashboardProfileViewModel> Profiles);

public sealed record OrganizerDashboardProfileViewModel(
    OrganizerProfileDetailModel Profile,
    IReadOnlyList<OrganizerManagedConversationEventModel> Events,
    IReadOnlyDictionary<string, EventRsvpSummaryModel> RsvpSummaries,
    IReadOnlyDictionary<string, IReadOnlyList<EventRsvpModel>> EventRsvps,
    OrganizerDashboardAnalyticsViewModel Analytics)
{
    public int ActiveEventCount => Analytics.ActiveEventCount;
}

public sealed record OrganizerDashboardAnalyticsViewModel(
    int TotalEventCount,
    int ActiveEventCount,
    int ArchivedEventCount,
    int OnlineEventCount,
    int InPersonEventCount,
    int TotalCapacity);

public sealed record OrganizerProfileEditPageViewModel(
    OrganizerProfileDetailModel Profile,
    OrganizerProfileEditInputModel Input,
    string? StatusMessage,
    string? ErrorMessage);

public sealed class OrganizerProfileEditInputModel
{
    [Required]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    public string OrganizerType { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? CityRegion { get; set; }

    public bool IsOnlineAvailable { get; set; }

    [Required]
    public string SupportedLearnerLevels { get; set; } = string.Empty;

    public string HelperLanguageCodes { get; set; } = string.Empty;

    public string? WebsiteUrl { get; set; }

    public string? PublicContactMethod { get; set; }
}

public sealed record OrganizerEventEditPageViewModel(
    OrganizerProfileDetailModel Profile,
    OrganizerManagedConversationEventModel? Event,
    OrganizerEventInputModel Input,
    bool IsNew,
    string? ErrorMessage);

public sealed class OrganizerEventInputModel
{
    [Required]
    public string Slug { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? City { get; set; }

    [Required]
    public string CountryRegion { get; set; } = "DE";

    public string? ApproximateLocation { get; set; }

    public bool IsOnline { get; set; }

    [Required]
    public string Category { get; set; } = "conversation-cafe";

    [Required]
    public string SupportedLearnerLevels { get; set; } = "A1";

    public string HelperLanguageCodes { get; set; } = "en";

    public string? ExternalLink { get; set; }

    public string? ContactMethod { get; set; }

    [Required]
    public string ScheduleText { get; set; } = string.Empty;

    public string? RecurrenceRule { get; set; }

    public int? Capacity { get; set; }

    [Required]
    public string PriceType { get; set; } = "free";

    public string? LinkedEventPreparationPackSlugs { get; set; }
}
