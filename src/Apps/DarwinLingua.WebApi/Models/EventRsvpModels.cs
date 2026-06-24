namespace DarwinLingua.WebApi.Models;

public sealed record SubmitEventRsvpRequest(
    string ParticipantName,
    string ParticipantEmail,
    string Status);

public sealed record AdminSetEventRsvpStatusRequest(
    string Status);

public sealed record EventRsvpResponse(
    Guid Id,
    string ConversationEventSlug,
    string TargetLearningLanguageCode,
    string ParticipantName,
    string ParticipantEmail,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record EventRsvpSummaryResponse(
    string ConversationEventSlug,
    string TargetLearningLanguageCode,
    int InterestedCount,
    int GoingCount,
    int CancelledCount,
    int Capacity,
    int? RemainingCapacity);
