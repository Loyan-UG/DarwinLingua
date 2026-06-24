using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IOrganizerProfileRepository
{
    Task<IReadOnlyList<OrganizerProfileListItemModel>> GetPublishedOrganizerProfilesAsync(
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);

    Task<OrganizerProfileDetailModel?> GetPublishedOrganizerProfileBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);
}
