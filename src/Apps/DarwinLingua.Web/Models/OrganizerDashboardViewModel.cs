using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Identity;
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
    DarwinLinguaOrganizerPlanSnapshot Plan,
    OrganizerDashboardAnalyticsViewModel Analytics)
{
    public int ActiveEventCount => Analytics.ActiveEventCount;

    public bool CanCreateMoreActiveEvents => ActiveEventCount < Plan.ActiveEventLimit;

    public bool CanUseRsvpManagement => Plan.EnabledFeatures.Contains(DarwinLinguaFeatureKeys.OrganizerRsvpManagement, StringComparer.Ordinal);

    public bool CanUseAnalytics => Plan.EnabledFeatures.Contains(DarwinLinguaFeatureKeys.OrganizerAnalytics, StringComparer.Ordinal);
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
    [StringLength(160)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(64)]
    public string OrganizerType { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(128)]
    public string? CityRegion { get; set; }

    public bool IsOnlineAvailable { get; set; }

    [Required]
    [StringLength(128)]
    public string SupportedLearnerLevels { get; set; } = string.Empty;

    [StringLength(128)]
    public string HelperLanguageCodes { get; set; } = string.Empty;

    [Url]
    [StringLength(512)]
    public string? WebsiteUrl { get; set; }

    [StringLength(256)]
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
    [StringLength(128)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(128)]
    public string? City { get; set; }

    [Required]
    [StringLength(64)]
    public string CountryRegion { get; set; } = "DE";

    [StringLength(256)]
    public string? ApproximateLocation { get; set; }

    public bool IsOnline { get; set; }

    [Required]
    [StringLength(64)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public string Category { get; set; } = "conversation-cafe";

    [Required]
    [StringLength(128)]
    public string SupportedLearnerLevels { get; set; } = "A1";

    [StringLength(128)]
    public string HelperLanguageCodes { get; set; } = "en";

    [Url]
    [StringLength(512)]
    public string? ExternalLink { get; set; }

    [StringLength(256)]
    public string? ContactMethod { get; set; }

    [Required]
    [StringLength(512)]
    public string ScheduleText { get; set; } = string.Empty;

    [StringLength(256)]
    public string? RecurrenceRule { get; set; }

    [Range(1, 100000)]
    public int? Capacity { get; set; }

    [Required]
    [StringLength(32)]
    public string PriceType { get; set; } = "free";

    [StringLength(512)]
    public string? LinkedEventPreparationPackSlugs { get; set; }
}
