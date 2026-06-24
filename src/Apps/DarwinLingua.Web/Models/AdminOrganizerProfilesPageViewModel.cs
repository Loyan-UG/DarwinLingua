using System.ComponentModel.DataAnnotations;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.SharedKernel.Globalization;
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
    [StringLength(128)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [StringLength(16)]
    [RegularExpression("^[a-z]{2,3}(?:-[a-z0-9]{2,8})?$")]
    public string TargetLearningLanguageCode { get; set; } = ContentLanguageRequirements.DefaultTargetLearningLanguageCode;

    [Required]
    [StringLength(160)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(64)]
    public string OrganizerType { get; set; } = "club";

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(128)]
    public string? CityRegion { get; set; }

    public bool IsOnlineAvailable { get; set; }

    [Required]
    [StringLength(128)]
    public string SupportedLearnerLevels { get; set; } = "A1";

    [StringLength(128)]
    public string HelperLanguageCodes { get; set; } = "en";

    [Url]
    [StringLength(512)]
    public string? WebsiteUrl { get; set; }

    [StringLength(256)]
    public string? PublicContactMethod { get; set; }

    [Required]
    [StringLength(32)]
    public string VerificationStatus { get; set; } = "reviewed";

    [Required]
    [StringLength(64)]
    public string PlanKey { get; set; } = "free-organizer";

    [Range(0, 100000)]
    public int HistoricalEventCount { get; set; }
}

public sealed class AdminOrganizerProfileOwnerInputModel
{
    [Required]
    [StringLength(128)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public string OrganizerProfileSlug { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string OwnerEmail { get; set; } = string.Empty;
}

public sealed class AdminOrganizerClaimDecisionInputModel
{
    [Required]
    [StringLength(32)]
    public string Status { get; set; } = string.Empty;
}
