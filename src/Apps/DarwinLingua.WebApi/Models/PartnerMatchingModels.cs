namespace DarwinLingua.WebApi.Models;

public sealed record PartnerMatchSearchRequest(
    string? CityRegion,
    string? InteractionPreference,
    string? LearningLevel,
    string? HelperLanguageCode,
    string? GoalKeyword);

public sealed record PartnerMatchProfileResponse(
    Guid ProfileId,
    string DisplayName,
    string? CityRegion,
    string InteractionPreference,
    string LearningLevel,
    IReadOnlyList<string> HelperLanguageCodes,
    string ConversationGoals,
    string Visibility);

public sealed record SubmitPartnerRequestRequest(
    Guid TargetLearnerProfileId,
    string OpenerTemplateKey,
    string? Note);

public sealed record PartnerRequestStateUpdateRequest(
    string Action);

public sealed record PartnerRequestResponse(
    Guid Id,
    string Direction,
    Guid TargetLearnerProfileId,
    string OtherDisplayName,
    string? OtherCityRegion,
    string OpenerTemplateKey,
    string? Note,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime ExpiresAtUtc,
    DateTime? RespondedAtUtc,
    string? ContactEmail);
