using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IOrganizerProfileRepository
{
    Task<IReadOnlyList<OrganizerProfileListItemModel>> GetPublishedOrganizerProfilesAsync(CancellationToken cancellationToken);

    Task<OrganizerProfileDetailModel?> GetPublishedOrganizerProfileBySlugAsync(
        string slug,
        CancellationToken cancellationToken);
}
