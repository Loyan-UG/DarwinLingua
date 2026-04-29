using System.ComponentModel.DataAnnotations;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Services;

namespace DarwinLingua.Web.Models;

public sealed record AdminOrganizerProfilesPageViewModel(
    IReadOnlyList<OrganizerProfileListItemModel> Profiles,
    IReadOnlyList<OrganizerClaimRequestModel> ClaimRequests,
    IReadOnlyList<OrganizerProfileOwnerModel> Owners,
    AdminOrganizerProfileInputModel Input,
    AdminOrganizerProfileOwnerInputModel OwnerInput,
    string? StatusMessage,
    string? ErrorMessage);

public sealed class AdminOrganizerProfileInputModel
{
    [Required]
    public string Slug { get; set; } = string.Empty;

    [Required]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    public string OrganizerType { get; set; } = "club";

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? CityRegion { get; set; }

    public bool IsOnlineAvailable { get; set; }

    [Required]
    public string SupportedLearnerLevels { get; set; } = "A1";

    public string HelperLanguageCodes { get; set; } = "en";

    public string? WebsiteUrl { get; set; }

    public string? PublicContactMethod { get; set; }

    [Required]
    public string VerificationStatus { get; set; } = "reviewed";

    [Required]
    public string PlanKey { get; set; } = "free-organizer";

    public int HistoricalEventCount { get; set; }
}

public sealed class AdminOrganizerProfileOwnerInputModel
{
    [Required]
    public string OrganizerProfileSlug { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string OwnerEmail { get; set; } = string.Empty;
}

public sealed class AdminOrganizerClaimDecisionInputModel
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
