using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IOrganizerProfileQueryService
{
    Task<IReadOnlyList<OrganizerProfileListItemModel>> GetPublishedOrganizerProfilesAsync(
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);

    Task<OrganizerProfileDetailModel?> GetPublishedOrganizerProfileBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);
}
