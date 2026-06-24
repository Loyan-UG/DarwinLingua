using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

public interface IEventRsvpService
{
    Task<EventRsvpResponse> SubmitAsync(
        string eventSlug,
        string targetLearningLanguageCode,
        SubmitEventRsvpRequest request,
        CancellationToken cancellationToken);

    Task<EventRsvpSummaryResponse> GetSummaryAsync(
        string eventSlug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<EventRsvpResponse>> GetByEventAsync(
        string eventSlug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);

    Task<EventRsvpResponse> SetStatusAsync(
        string eventSlug,
        string targetLearningLanguageCode,
        Guid rsvpId,
        AdminSetEventRsvpStatusRequest request,
        CancellationToken cancellationToken);
}
