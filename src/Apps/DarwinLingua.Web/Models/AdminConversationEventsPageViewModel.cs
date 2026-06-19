using System.ComponentModel.DataAnnotations;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record AdminConversationEventsPageViewModel(
    IReadOnlyList<ConversationEventListItemModel> Events,
    AdminConversationEventInputModel Input,
    string? StatusMessage,
    string? ErrorMessage);

public sealed class AdminConversationEventInputModel
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

    [Required]
    [StringLength(160)]
    public string OrganizerName { get; set; } = string.Empty;

    [StringLength(128)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public string? OrganizerProfileSlug { get; set; }

    [Url]
    [StringLength(512)]
    public string? ExternalLink { get; set; }

    [StringLength(256)]
    public string? ContactMethod { get; set; }

    [Required]
    [StringLength(512)]
    public string ScheduleText { get; set; } = string.Empty;

    [StringLength(64)]
    public string? StartsAtUtc { get; set; }

    [StringLength(64)]
    public string? EndsAtUtc { get; set; }

    [StringLength(256)]
    public string? RecurrenceRule { get; set; }

    [Range(1, 100000)]
    public int? Capacity { get; set; }

    [Required]
    [StringLength(32)]
    public string PriceType { get; set; } = "free";

    [Required]
    [StringLength(32)]
    public string VerificationStatus { get; set; } = "reviewed";

    [StringLength(160)]
    public string? SourceName { get; set; }

    [Url]
    [StringLength(512)]
    public string? SourceUrl { get; set; }

    [StringLength(64)]
    public string? LastVerifiedAtUtc { get; set; }

    [StringLength(512)]
    public string? LinkedEventPreparationPackSlugs { get; set; }
}
