using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

public interface IPartnerMatchingService
{
    Task<IReadOnlyList<PartnerMatchProfileResponse>> SearchAsync(
        string ownerEmail,
        PartnerMatchSearchRequest request,
        CancellationToken cancellationToken);

    Task<PartnerRequestResponse> SubmitRequestAsync(
        string requesterEmail,
        SubmitPartnerRequestRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PartnerRequestResponse>> GetRequestsAsync(
        string ownerEmail,
        CancellationToken cancellationToken);

    Task<PartnerRequestResponse> UpdateRequestStateAsync(
        string ownerEmail,
        Guid requestId,
        PartnerRequestStateUpdateRequest request,
        CancellationToken cancellationToken);
}
