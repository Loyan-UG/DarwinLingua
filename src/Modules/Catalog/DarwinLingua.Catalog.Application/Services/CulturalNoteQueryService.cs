using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class CulturalNoteQueryService(ICulturalNoteRepository culturalNoteRepository) : ICulturalNoteQueryService
{
    public Task<IReadOnlyList<CulturalNoteListItemModel>> GetPublishedCulturalNotesAsync(CulturalNoteListFilterModel filter, CancellationToken cancellationToken) =>
        culturalNoteRepository.GetPublishedCulturalNotesAsync(filter, null, cancellationToken);

    public Task<IReadOnlyList<CulturalNoteListItemModel>> GetPublishedCulturalNotesAsync(CulturalNoteListFilterModel filter, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        culturalNoteRepository.GetPublishedCulturalNotesAsync(filter, primaryMeaningLanguageCode, cancellationToken);

    public Task<CulturalNoteDetailModel?> GetPublishedCulturalNoteBySlugAsync(string slug, CancellationToken cancellationToken) =>
        culturalNoteRepository.GetPublishedCulturalNoteBySlugAsync(slug, null, cancellationToken);

    public Task<CulturalNoteDetailModel?> GetPublishedCulturalNoteBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        culturalNoteRepository.GetPublishedCulturalNoteBySlugAsync(slug, primaryMeaningLanguageCode, cancellationToken);
}
