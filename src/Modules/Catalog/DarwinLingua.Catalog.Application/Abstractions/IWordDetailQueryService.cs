using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

/// <summary>
/// Provides detailed lexical queries for the selected word screen.
/// </summary>
public interface IWordDetailQueryService
{
    /// <summary>
    /// Loads the detail view model for a lexical entry.
    /// </summary>
    Task<WordDetailModel?> GetWordDetailsAsync(
        Guid publicId,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads the detail view model for a lexical entry by its public URL slug.
    /// </summary>
    Task<WordDetailModel?> GetWordDetailsBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode,
        CancellationToken cancellationToken);
}
