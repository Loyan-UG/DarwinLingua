namespace DarwinLingua.WebApi.Models;

public sealed record AdminSaveConversationEventRequest(
    string Slug,
    string TargetLearningLanguageCode,
    string Name,
    string Description,
    string? City,
    string CountryRegion,
    string? ApproximateLocation,
    bool IsOnline,
    string Category,
    IReadOnlyList<string> SupportedLearnerLevels,
    IReadOnlyList<string> HelperLanguageCodes,
    string OrganizerName,
    string? OrganizerProfileSlug,
    string? ExternalLink,
    string? ContactMethod,
    string ScheduleText,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc,
    string PriceType,
    string VerificationStatus,
    string? SourceName,
    string? SourceUrl,
    DateTime? LastVerifiedAtUtc,
    IReadOnlyList<string> LinkedEventPreparationPackSlugs)
{
    public string? RecurrenceRule { get; init; }

    public int? Capacity { get; init; }
}
