using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class OrganizerProfileQueryService(IOrganizerProfileRepository organizerProfileRepository) : IOrganizerProfileQueryService
{
    public Task<IReadOnlyList<OrganizerProfileListItemModel>> GetPublishedOrganizerProfilesAsync(
        string targetLearningLanguageCode,
        CancellationToken cancellationToken) =>
        organizerProfileRepository.GetPublishedOrganizerProfilesAsync(targetLearningLanguageCode, cancellationToken);

    public Task<OrganizerProfileDetailModel?> GetPublishedOrganizerProfileBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken) =>
        organizerProfileRepository.GetPublishedOrganizerProfileBySlugAsync(slug, targetLearningLanguageCode, cancellationToken);
}
