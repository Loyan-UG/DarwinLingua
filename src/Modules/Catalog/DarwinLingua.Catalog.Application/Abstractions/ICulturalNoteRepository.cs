using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface ICulturalNoteRepository
{
    Task<IReadOnlyList<CulturalNoteListItemModel>> GetPublishedCulturalNotesAsync(CulturalNoteListFilterModel filter, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<CulturalNoteDetailModel?> GetPublishedCulturalNoteBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);
}
