using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class WordCollectionQueryService(IWordCollectionRepository wordCollectionRepository) : IWordCollectionQueryService
{
    public Task<IReadOnlyList<WordCollectionListItemModel>> GetPublishedCollectionsAsync(
        string meaningLanguageCode,
        CancellationToken cancellationToken) =>
        wordCollectionRepository.GetPublishedCollectionsAsync(meaningLanguageCode, cancellationToken);

    public Task<WordCollectionDetailModel?> GetPublishedCollectionBySlugAsync(
        string slug,
        string meaningLanguageCode,
        CancellationToken cancellationToken) =>
        wordCollectionRepository.GetPublishedCollectionBySlugAsync(slug, meaningLanguageCode, cancellationToken);
}
