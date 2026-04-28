using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class OrganizerProfileQueryService(IOrganizerProfileRepository organizerProfileRepository) : IOrganizerProfileQueryService
{
    public Task<IReadOnlyList<OrganizerProfileListItemModel>> GetPublishedOrganizerProfilesAsync(CancellationToken cancellationToken) =>
        organizerProfileRepository.GetPublishedOrganizerProfilesAsync(cancellationToken);

    public Task<OrganizerProfileDetailModel?> GetPublishedOrganizerProfileBySlugAsync(
        string slug,
        CancellationToken cancellationToken) =>
        organizerProfileRepository.GetPublishedOrganizerProfileBySlugAsync(slug, cancellationToken);
}
