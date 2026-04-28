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

    [Required]
    public string OrganizerName { get; set; } = string.Empty;

    public string? OrganizerProfileSlug { get; set; }

    public string? ExternalLink { get; set; }

    public string? ContactMethod { get; set; }

    [Required]
    public string ScheduleText { get; set; } = string.Empty;

    public string? RecurrenceRule { get; set; }

    public int? Capacity { get; set; }

    [Required]
    public string PriceType { get; set; } = "free";

    [Required]
    public string VerificationStatus { get; set; } = "reviewed";

    public string? SourceName { get; set; }

    public string? SourceUrl { get; set; }

    public string? LastVerifiedAtUtc { get; set; }

    public string? LinkedEventPreparationPackSlugs { get; set; }
}
