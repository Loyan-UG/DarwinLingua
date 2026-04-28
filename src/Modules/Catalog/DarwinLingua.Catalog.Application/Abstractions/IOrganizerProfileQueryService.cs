using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IOrganizerProfileQueryService
{
    Task<IReadOnlyList<OrganizerProfileListItemModel>> GetPublishedOrganizerProfilesAsync(CancellationToken cancellationToken);

    Task<OrganizerProfileDetailModel?> GetPublishedOrganizerProfileBySlugAsync(
        string slug,
        CancellationToken cancellationToken);
}
