using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class CulturalNoteQueryService(ICulturalNoteRepository culturalNoteRepository) : ICulturalNoteQueryService
{
    public Task<IReadOnlyList<CulturalNoteListItemModel>> GetPublishedCulturalNotesAsync(CulturalNoteListFilterModel filter, CancellationToken cancellationToken) =>
        culturalNoteRepository.GetPublishedCulturalNotesAsync(filter, cancellationToken);

    public Task<CulturalNoteDetailModel?> GetPublishedCulturalNoteBySlugAsync(string slug, CancellationToken cancellationToken) =>
        culturalNoteRepository.GetPublishedCulturalNoteBySlugAsync(slug, cancellationToken);
}
