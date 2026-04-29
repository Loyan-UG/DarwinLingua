using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

public interface IOrganizerClaimRequestService
{
    Task<OrganizerClaimRequestResponse> SubmitAsync(
        string organizerProfileSlug,
        SubmitOrganizerClaimRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OrganizerClaimRequestResponse>> GetRecentAsync(CancellationToken cancellationToken);

    Task<OrganizerClaimRequestResponse> SetStatusAsync(
        Guid claimRequestId,
        OrganizerClaimDecisionRequest request,
        CancellationToken cancellationToken);
}
