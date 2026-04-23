using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

/// <summary>
/// Reads curated word collection projections from persistence.
/// </summary>
public interface IWordCollectionRepository
{
    Task<IReadOnlyList<WordCollectionListItemModel>> GetPublishedCollectionsAsync(
        string meaningLanguageCode,
        CancellationToken cancellationToken);

    Task<WordCollectionDetailModel?> GetPublishedCollectionBySlugAsync(
        string slug,
        string meaningLanguageCode,
        CancellationToken cancellationToken);
}
