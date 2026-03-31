using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Application.Abstractions;

/// <summary>
/// Provides read access to lexical entry aggregates needed by Phase 1 catalog queries.
/// </summary>
public interface IWordEntryRepository
{
    /// <summary>
    /// Loads the active lexical entries linked to the specified topic key.
    /// </summary>
    Task<IReadOnlyList<WordListItemModel>> GetActiveByTopicKeyAsync(
        string topicKey,
        string meaningLanguageCode,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads a lexical entry aggregate by its public identifier.
    /// </summary>
    Task<WordEntry?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the active lexical entries for the specified CEFR level.
    /// </summary>
    Task<IReadOnlyList<WordListItemModel>> GetActiveByCefrAsync(
        CefrLevel cefrLevel,
        string meaningLanguageCode,
        CancellationToken cancellationToken);

    /// <summary>
    /// Searches active lexical entries by normalized lemma text.
    /// </summary>
    Task<IReadOnlyList<WordListItemModel>> SearchActiveByLemmaAsync(
        string normalizedLemmaQuery,
        string meaningLanguageCode,
        CancellationToken cancellationToken);
}
