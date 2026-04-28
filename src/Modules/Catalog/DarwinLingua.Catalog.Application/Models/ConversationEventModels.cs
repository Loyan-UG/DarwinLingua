namespace DarwinLingua.Catalog.Application.Models;

public sealed record ConversationEventListFilterModel(
    string? City,
    string? CefrLevel,
    string? HelperLanguageCode,
    bool? IsOnline,
    string? PriceType,
    string? Category);

public sealed record ConversationEventListItemModel(
    string Slug,
    string Name,
    string Description,
    string? City,
    string CountryRegion,
    bool IsOnline,
    string Category,
    IReadOnlyList<string> SupportedLearnerLevels,
    IReadOnlyList<string> HelperLanguageCodes,
    string OrganizerName,
    string? OrganizerProfileSlug,
    string ScheduleText,
    string PriceType,
    string VerificationStatus,
    IReadOnlyList<string> LinkedEventPreparationPackSlugs);

public sealed record ConversationEventDetailModel(
    string Slug,
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
    string PriceType,
    string VerificationStatus,
    string? SourceName,
    string? SourceUrl,
    DateTime? LastVerifiedAtUtc,
    IReadOnlyList<string> LinkedEventPreparationPackSlugs);
