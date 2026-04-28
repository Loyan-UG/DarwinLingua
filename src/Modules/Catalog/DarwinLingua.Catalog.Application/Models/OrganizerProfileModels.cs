namespace DarwinLingua.Catalog.Application.Models;

public sealed record OrganizerProfileListItemModel(
    string Slug,
    string DisplayName,
    string OrganizerType,
    string Description,
    string? CityRegion,
    bool IsOnlineAvailable,
    IReadOnlyList<string> SupportedLearnerLevels,
    IReadOnlyList<string> HelperLanguageCodes,
    string VerificationStatus,
    string PlanKey,
    int HistoricalEventCount,
    int ActiveEventCount);

public sealed record OrganizerProfileDetailModel(
    string Slug,
    string DisplayName,
    string OrganizerType,
    string Description,
    string? CityRegion,
    bool IsOnlineAvailable,
    IReadOnlyList<string> SupportedLearnerLevels,
    IReadOnlyList<string> HelperLanguageCodes,
    string? WebsiteUrl,
    string? PublicContactMethod,
    string VerificationStatus,
    string PlanKey,
    int HistoricalEventCount,
    IReadOnlyList<ConversationEventListItemModel> ActiveEvents);
