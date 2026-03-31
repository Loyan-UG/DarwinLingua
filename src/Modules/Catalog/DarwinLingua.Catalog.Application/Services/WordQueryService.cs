using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.SharedKernel.Lexicon;
using System.Text.RegularExpressions;

namespace DarwinLingua.Catalog.Application.Services;

/// <summary>
/// Implements browse-oriented lexical queries for the catalog module.
/// </summary>
internal sealed partial class WordQueryService : IWordQueryService
{
    private readonly IWordEntryRepository _wordEntryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="WordQueryService"/> class.
    /// </summary>
    public WordQueryService(IWordEntryRepository wordEntryRepository)
    {
        ArgumentNullException.ThrowIfNull(wordEntryRepository);

        _wordEntryRepository = wordEntryRepository;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WordListItemModel>> GetWordsByTopicAsync(
        string topicKey,
        string meaningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicKey);
        return await _wordEntryRepository
            .GetActiveByTopicKeyAsync(topicKey, meaningLanguageCode, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WordListItemModel>> GetWordsByCefrAsync(
        string cefrLevel,
        string meaningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cefrLevel);

        CefrLevel resolvedCefrLevel = Enum.Parse<CefrLevel>(cefrLevel.Trim(), ignoreCase: true);

        return await _wordEntryRepository
            .GetActiveByCefrAsync(resolvedCefrLevel, meaningLanguageCode, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WordListItemModel>> SearchWordsAsync(
        string query,
        string meaningLanguageCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        string normalizedQuery = NormalizeSearchQuery(query);

        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return [];
        }

        return await _wordEntryRepository
            .SearchActiveByLemmaAsync(normalizedQuery, meaningLanguageCode, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Normalizes the search query using the same whitespace and casing rules as lexical lemmas.
    /// </summary>
    private static string NormalizeSearchQuery(string query)
    {
        return CollapseWhitespace().Replace(query.Trim(), " ").ToLowerInvariant();
    }

    [GeneratedRegex("\\s+", RegexOptions.Compiled)]
    private static partial Regex CollapseWhitespace();
}
