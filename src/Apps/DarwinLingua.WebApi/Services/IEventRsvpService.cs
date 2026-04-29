using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

public interface IEventRsvpService
{
    Task<EventRsvpResponse> SubmitAsync(
        string eventSlug,
        SubmitEventRsvpRequest request,
        CancellationToken cancellationToken);

    Task<EventRsvpSummaryResponse> GetSummaryAsync(
        string eventSlug,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<EventRsvpResponse>> GetByEventAsync(
        string eventSlug,
        CancellationToken cancellationToken);

    Task<EventRsvpResponse> SetStatusAsync(
        string eventSlug,
        Guid rsvpId,
        AdminSetEventRsvpStatusRequest request,
        CancellationToken cancellationToken);
}
