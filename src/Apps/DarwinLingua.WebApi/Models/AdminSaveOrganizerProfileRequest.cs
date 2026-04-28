namespace DarwinLingua.WebApi.Models;

public sealed record AdminSaveOrganizerProfileRequest(
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
    int HistoricalEventCount);
