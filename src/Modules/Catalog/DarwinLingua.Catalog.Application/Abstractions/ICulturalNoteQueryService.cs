using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface ICulturalNoteQueryService
{
    Task<IReadOnlyList<CulturalNoteListItemModel>> GetPublishedCulturalNotesAsync(CulturalNoteListFilterModel filter, CancellationToken cancellationToken);

    Task<CulturalNoteDetailModel?> GetPublishedCulturalNoteBySlugAsync(string slug, CancellationToken cancellationToken);
}
