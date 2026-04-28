using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

public interface IOrganizerProfileOwnerService
{
    Task<OrganizerProfileOwnerResponse> AssignAsync(
        AssignOrganizerProfileOwnerRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OrganizerProfileOwnerResponse>> GetByOwnerEmailAsync(
        string ownerEmail,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OrganizerProfileOwnerResponse>> GetRecentAsync(CancellationToken cancellationToken);
}
