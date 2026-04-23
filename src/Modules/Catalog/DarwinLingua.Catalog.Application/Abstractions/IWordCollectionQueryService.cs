using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

/// <summary>
/// Provides read-only queries for curated word collections.
/// </summary>
public interface IWordCollectionQueryService
{
    Task<IReadOnlyList<WordCollectionListItemModel>> GetPublishedCollectionsAsync(
        string meaningLanguageCode,
        CancellationToken cancellationToken);

    Task<WordCollectionDetailModel?> GetPublishedCollectionBySlugAsync(
        string slug,
        string meaningLanguageCode,
        CancellationToken cancellationToken);
}
