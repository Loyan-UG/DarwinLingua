using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.ContentOps.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.ContentOps.Application.Abstractions;

/// <summary>
/// Provides persistence and lookup operations required by the content import workflow.
/// </summary>
public interface IContentImportRepository
{
    /// <summary>
    /// Loads the active topic reference rows keyed by their normalized topic keys.
    /// </summary>
    Task<IReadOnlyDictionary<string, Topic>> GetActiveTopicsByKeyAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Loads the active meaning-language codes supported by the platform.
    /// </summary>
    Task<IReadOnlySet<LanguageCode>> GetActiveMeaningLanguagesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether a content package with the same package identifier already exists.
    /// </summary>
    Task<bool> PackageExistsAsync(string packageId, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether a lexical entry already exists using the Phase 1 duplicate identity rule.
    /// </summary>
    Task<bool> WordExistsAsync(
        string normalizedLemma,
        PartOfSpeech partOfSpeech,
        CefrLevel cefrLevel,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists the completed package audit rows and any imported lexical aggregates in one operation.
    /// </summary>
    Task PersistImportAsync(
        ContentPackage contentPackage,
        IReadOnlyList<WordEntry> importedWords,
        CancellationToken cancellationToken);
}
