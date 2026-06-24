namespace DarwinLingua.WebApi.Models;

public sealed record SaveLearnerConversationProfileRequest(
    string DisplayName,
    string? CityRegion,
    string InteractionPreference,
    string LearningLevel,
    IReadOnlyList<string> HelperLanguageCodes,
    string ConversationGoals,
    string? AvailabilityNotes,
    string Visibility,
    bool HasConfirmedAdult);

public sealed record LearnerConversationProfileVisibilityRequest(
    bool IsEnabled);

public sealed record LearnerConversationProfilePrivateResponse(
    Guid Id,
    string OwnerEmail,
    string DisplayName,
    string? CityRegion,
    string InteractionPreference,
    string LearningLevel,
    IReadOnlyList<string> HelperLanguageCodes,
    string ConversationGoals,
    string? AvailabilityNotes,
    string Visibility,
    bool HasConfirmedAdult,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record LearnerConversationProfilePublicResponse(
    string DisplayName,
    string? CityRegion,
    string InteractionPreference,
    string LearningLevel,
    IReadOnlyList<string> HelperLanguageCodes,
    string ConversationGoals);
