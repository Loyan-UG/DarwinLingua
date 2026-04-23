using DarwinDeutsch.Maui.Services.Browse.Models;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinDeutsch.Maui.Services.Browse;

/// <summary>
/// Provides cached browse state for curated word collections.
/// </summary>
public interface IWordCollectionBrowseStateService
{
    Task<IReadOnlyList<WordCollectionListItemModel>> GetCollectionsAsync(CancellationToken cancellationToken);

    Task<WordCollectionDetailModel?> GetCollectionAsync(string slug, CancellationToken cancellationToken);

    Task<Guid?> GetStartingWordPublicIdAsync(string slug, CancellationToken cancellationToken);

    Task<CefrBrowseNavigationState> GetNavigationStateAsync(
        string slug,
        Guid currentWordPublicId,
        CancellationToken cancellationToken);

    Task PrefetchCollectionAsync(string slug, CancellationToken cancellationToken);

    void ResetCache();
}
