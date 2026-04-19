using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

/// <summary>
/// Reads lexical entry aggregates from the shared SQLite database.
/// </summary>
internal sealed class WordEntryRepository : IWordEntryRepository
{
    private const int SearchResultLimit = 50;
    private readonly IDbContextFactory<DarwinLinguaDbContext> _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="WordEntryRepository"/> class.
    /// </summary>
    public WordEntryRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory)
    {
        ArgumentNullException.ThrowIfNull(dbContextFactory);

        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WordListItemModel>> GetActiveByTopicKeyAsync(
        string topicKey,
        string meaningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicKey);

        string normalizedTopicKey = topicKey.Trim().ToLowerInvariant();
        LanguageCode resolvedMeaningLanguageCode = LanguageCode.From(meaningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        Guid? matchingTopicId = await dbContext.Topics
            .AsNoTracking()
            .Where(topic => topic.Key == normalizedTopicKey)
            .Select(topic => (Guid?)topic.Id)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!matchingTopicId.HasValue)
        {
            return [];
        }

        List<WordBrowseRow> words = await dbContext.WordEntries
            .AsNoTracking()
            .Where(word => word.PublicationStatus == PublicationStatus.Active)
            .Where(word => word.Topics.Any(topic => topic.TopicId == matchingTopicId.Value))
            .OrderBy(word => word.NormalizedLemma)
            .Select(word => new WordBrowseRow(
                word.Id,
                word.PublicId,
                word.Lemma,
                word.Article,
                word.PluralForm,
                word.PartOfSpeech,
                word.PrimaryCefrLevel,
                word.NormalizedLemma))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return await MapToBrowseModelsAsync(dbContext, words, resolvedMeaningLanguageCode, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WordListItemModel>> GetActiveByTopicPageAsync(
        string topicKey,
        string meaningLanguageCode,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicKey);

        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip));
        }

        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        string normalizedTopicKey = topicKey.Trim().ToLowerInvariant();
        LanguageCode resolvedMeaningLanguageCode = LanguageCode.From(meaningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        Guid? matchingTopicId = await dbContext.Topics
            .AsNoTracking()
            .Where(topic => topic.Key == normalizedTopicKey)
            .Select(topic => (Guid?)topic.Id)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!matchingTopicId.HasValue)
        {
            return [];
        }

        List<WordBrowseRow> words = await dbContext.WordEntries
            .AsNoTracking()
            .Where(word => word.PublicationStatus == PublicationStatus.Active)
            .Where(word => word.Topics.Any(topic => topic.TopicId == matchingTopicId.Value))
            .OrderBy(word => word.NormalizedLemma)
            .Skip(skip)
            .Take(take)
            .Select(word => new WordBrowseRow(
                word.Id,
                word.PublicId,
                word.Lemma,
                word.Article,
                word.PluralForm,
                word.PartOfSpeech,
                word.PrimaryCefrLevel,
                word.NormalizedLemma))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return await MapToBrowseModelsAsync(dbContext, words, resolvedMeaningLanguageCode, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<WordEntry?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        if (publicId == Guid.Empty)
        {
            throw new ArgumentException("Public identifier cannot be empty.", nameof(publicId));
        }

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        return await CreateAggregateQuery(dbContext)
            .SingleOrDefaultAsync(
                word => word.PublicId == publicId &&
                        word.PublicationStatus == PublicationStatus.Active,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WordListItemModel>> GetActiveByCefrAsync(
        CefrLevel cefrLevel,
        string meaningLanguageCode,
        CancellationToken cancellationToken)
    {
        LanguageCode resolvedMeaningLanguageCode = LanguageCode.From(meaningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        List<WordBrowseRow> words = await dbContext.WordEntries
            .AsNoTracking()
            .Where(word => word.PublicationStatus == PublicationStatus.Active && word.PrimaryCefrLevel == cefrLevel)
            .OrderBy(word => word.NormalizedLemma)
            .Select(word => new WordBrowseRow(
                word.Id,
                word.PublicId,
                word.Lemma,
                word.Article,
                word.PluralForm,
                word.PartOfSpeech,
                word.PrimaryCefrLevel,
                word.NormalizedLemma))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return await MapToBrowseModelsAsync(dbContext, words, resolvedMeaningLanguageCode, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WordListItemModel>> GetActiveByCefrPageAsync(
        CefrLevel cefrLevel,
        string meaningLanguageCode,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip));
        }

        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        LanguageCode resolvedMeaningLanguageCode = LanguageCode.From(meaningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        List<WordBrowseRow> words = await dbContext.WordEntries
            .AsNoTracking()
            .Where(word => word.PublicationStatus == PublicationStatus.Active && word.PrimaryCefrLevel == cefrLevel)
            .OrderBy(word => word.NormalizedLemma)
            .Skip(skip)
            .Take(take)
            .Select(word => new WordBrowseRow(
                word.Id,
                word.PublicId,
                word.Lemma,
                word.Article,
                word.PluralForm,
                word.PartOfSpeech,
                word.PrimaryCefrLevel,
                word.NormalizedLemma))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return await MapToBrowseModelsAsync(dbContext, words, resolvedMeaningLanguageCode, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WordListItemModel>> SearchActiveByLemmaAsync(
        string normalizedLemmaQuery,
        string meaningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedLemmaQuery);
        LanguageCode resolvedMeaningLanguageCode = LanguageCode.From(meaningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        string prefixPattern = $"{normalizedLemmaQuery}%";
        string containsPattern = $"%{normalizedLemmaQuery}%";
        IQueryable<WordEntry> activeWords = dbContext.WordEntries
            .AsNoTracking()
            .Where(word => word.PublicationStatus == PublicationStatus.Active);

        List<WordBrowseRow> prefixMatches = await activeWords
            .Where(word => EF.Functions.Like(word.NormalizedLemma, prefixPattern))
            .OrderBy(word => word.NormalizedLemma)
            .Select(word => new WordBrowseRow(
                word.Id,
                word.PublicId,
                word.Lemma,
                word.Article,
                word.PluralForm,
                word.PartOfSpeech,
                word.PrimaryCefrLevel,
                word.NormalizedLemma))
            .Take(SearchResultLimit)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (prefixMatches.Count >= SearchResultLimit)
        {
            return await MapToBrowseModelsAsync(dbContext, prefixMatches, resolvedMeaningLanguageCode, cancellationToken)
                .ConfigureAwait(false);
        }

        int remainingCount = SearchResultLimit - prefixMatches.Count;

        List<WordBrowseRow> containsMatches = await activeWords
            .Where(word => !EF.Functions.Like(word.NormalizedLemma, prefixPattern))
            .Where(word => EF.Functions.Like(word.NormalizedLemma, containsPattern))
            .OrderBy(word => word.NormalizedLemma)
            .Select(word => new WordBrowseRow(
                word.Id,
                word.PublicId,
                word.Lemma,
                word.Article,
                word.PluralForm,
                word.PartOfSpeech,
                word.PrimaryCefrLevel,
                word.NormalizedLemma))
            .Take(remainingCount)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return await MapToBrowseModelsAsync(
                dbContext,
                prefixMatches.Concat(containsMatches).ToArray(),
                resolvedMeaningLanguageCode,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Creates the aggregate query shape required by the current detail projection.
    /// </summary>
    /// <param name="dbContext">The active database context.</param>
    /// <returns>The configured aggregate query.</returns>
    private static IQueryable<WordEntry> CreateAggregateQuery(DarwinLinguaDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        return dbContext.WordEntries
            .AsNoTracking()
            .AsSplitQuery()
            .Include(word => word.Senses.Where(sense => sense.PublicationStatus == PublicationStatus.Active))
                .ThenInclude(sense => sense.Translations)
            .Include(word => word.Senses.Where(sense => sense.PublicationStatus == PublicationStatus.Active))
                .ThenInclude(sense => sense.Examples)
                    .ThenInclude(example => example.Translations)
            .Include(word => word.LexicalForms)
            .Include(word => word.Topics)
            .Include(word => word.Collocations)
            .Include(word => word.FamilyMembers)
            .Include(word => word.Relations)
            .Include(word => word.GrammarNotes)
            .Include(word => word.Labels);
    }

    private static async Task<IReadOnlyList<WordListItemModel>> MapToBrowseModelsAsync(
        DarwinLinguaDbContext dbContext,
        IReadOnlyList<WordBrowseRow> rows,
        LanguageCode meaningLanguageCode,
        CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
        {
            return [];
        }

        Dictionary<Guid, string?> meaningsByWordEntryId = await LoadPrimaryMeaningsAsync(
                dbContext,
                rows.Select(row => row.WordEntryId).ToArray(),
                meaningLanguageCode,
                cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(row => new WordListItemModel(
                row.PublicId,
                row.Lemma,
                row.Article,
                row.PluralForm,
                row.PartOfSpeech.ToString(),
                row.CefrLevel.ToString(),
                meaningsByWordEntryId.GetValueOrDefault(row.WordEntryId)))
            .ToArray();
    }

    private static async Task<Dictionary<Guid, string?>> LoadPrimaryMeaningsAsync(
        DarwinLinguaDbContext dbContext,
        Guid[] wordEntryIds,
        LanguageCode meaningLanguageCode,
        CancellationToken cancellationToken)
    {
        List<PrimarySenseRow> primarySenses = await dbContext.WordSenses
            .AsNoTracking()
            .Where(sense => sense.IsPrimarySense)
            .Where(sense => wordEntryIds.Contains(sense.WordEntryId))
            .Select(sense => new PrimarySenseRow(sense.Id, sense.WordEntryId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (primarySenses.Count == 0)
        {
            return [];
        }

        Guid[] primarySenseIds = primarySenses
            .Select(sense => sense.SenseId)
            .ToArray();

        List<PrimaryMeaningTranslationRow> translations = await dbContext.SenseTranslations
            .AsNoTracking()
            .Where(translation => translation.LanguageCode == meaningLanguageCode)
            .Where(translation => primarySenseIds.Contains(translation.WordSenseId))
            .Select(translation => new PrimaryMeaningTranslationRow(
                translation.WordSenseId,
                translation.TranslationText,
                translation.IsPrimary))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false)
            ;

        List<PrimaryMeaningRow> meanings = translations
            .Join(
                primarySenses,
                translation => translation.WordSenseId,
                sense => sense.SenseId,
                (translation, sense) => new PrimaryMeaningRow(
                    sense.WordEntryId,
                    translation.TranslationText,
                    translation.IsPrimary))
            .ToList();

        return meanings
            .GroupBy(row => row.WordEntryId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(row => row.IsPrimary)
                    .ThenBy(row => row.TranslationText, StringComparer.Ordinal)
                    .Select(row => row.TranslationText)
                    .FirstOrDefault());
    }

    private sealed record WordBrowseRow(
        Guid WordEntryId,
        Guid PublicId,
        string Lemma,
        string? Article,
        string? PluralForm,
        PartOfSpeech PartOfSpeech,
        CefrLevel CefrLevel,
        string NormalizedLemma);

    private sealed record PrimarySenseRow(Guid SenseId, Guid WordEntryId);

    private sealed record PrimaryMeaningTranslationRow(Guid WordSenseId, string TranslationText, bool IsPrimary);

    private sealed record PrimaryMeaningRow(Guid WordEntryId, string TranslationText, bool IsPrimary);
}
