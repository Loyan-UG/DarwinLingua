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

        List<WordBrowseProjection> words = await dbContext.WordEntries
            .AsNoTracking()
            .Where(word => word.PublicationStatus == PublicationStatus.Active)
            .Where(word => word.Topics.Any(topic => topic.TopicId == matchingTopicId.Value))
            .OrderBy(word => word.NormalizedLemma)
            .Select(word => new WordBrowseProjection(
                word.PublicId,
                word.Lemma,
                word.Article,
                word.PluralForm,
                word.PartOfSpeech,
                word.PrimaryCefrLevel,
                word.NormalizedLemma,
                word.Senses
                    .Where(sense => sense.IsPrimarySense)
                    .SelectMany(sense => sense.Translations
                        .Where(translation => translation.LanguageCode == resolvedMeaningLanguageCode)
                        .OrderByDescending(translation => translation.IsPrimary)
                        .ThenBy(translation => translation.TranslationText)
                        .Select(translation => translation.TranslationText)
                        .Take(1))
                    .FirstOrDefault(),
                false))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return words
            .Select(MapToBrowseModel)
            .ToArray();
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

        List<WordBrowseProjection> words = await dbContext.WordEntries
            .AsNoTracking()
            .Where(word => word.PublicationStatus == PublicationStatus.Active)
            .Where(word => word.Topics.Any(topic => topic.TopicId == matchingTopicId.Value))
            .OrderBy(word => word.NormalizedLemma)
            .Skip(skip)
            .Take(take)
            .Select(word => new WordBrowseProjection(
                word.PublicId,
                word.Lemma,
                word.Article,
                word.PluralForm,
                word.PartOfSpeech,
                word.PrimaryCefrLevel,
                word.NormalizedLemma,
                word.Senses
                    .Where(sense => sense.IsPrimarySense)
                    .SelectMany(sense => sense.Translations
                        .Where(translation => translation.LanguageCode == resolvedMeaningLanguageCode)
                        .OrderByDescending(translation => translation.IsPrimary)
                        .ThenBy(translation => translation.TranslationText)
                        .Select(translation => translation.TranslationText)
                        .Take(1))
                    .FirstOrDefault(),
                false))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return words
            .Select(MapToBrowseModel)
            .ToArray();
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
            .SingleOrDefaultAsync(word => word.PublicId == publicId, cancellationToken)
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

        List<WordBrowseProjection> words = await dbContext.WordEntries
            .AsNoTracking()
            .Where(word => word.PublicationStatus == PublicationStatus.Active && word.PrimaryCefrLevel == cefrLevel)
            .OrderBy(word => word.NormalizedLemma)
            .Select(word => new WordBrowseProjection(
                word.PublicId,
                word.Lemma,
                word.Article,
                word.PluralForm,
                word.PartOfSpeech,
                word.PrimaryCefrLevel,
                word.NormalizedLemma,
                word.Senses
                    .Where(sense => sense.IsPrimarySense)
                    .SelectMany(sense => sense.Translations
                        .Where(translation => translation.LanguageCode == resolvedMeaningLanguageCode)
                        .OrderByDescending(translation => translation.IsPrimary)
                        .ThenBy(translation => translation.TranslationText)
                        .Select(translation => translation.TranslationText)
                        .Take(1))
                    .FirstOrDefault(),
                false))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return words
            .Select(MapToBrowseModel)
            .ToArray();
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

        List<WordBrowseProjection> words = await dbContext.WordEntries
            .AsNoTracking()
            .Where(word => word.PublicationStatus == PublicationStatus.Active && word.PrimaryCefrLevel == cefrLevel)
            .OrderBy(word => word.NormalizedLemma)
            .Skip(skip)
            .Take(take)
            .Select(word => new WordBrowseProjection(
                word.PublicId,
                word.Lemma,
                word.Article,
                word.PluralForm,
                word.PartOfSpeech,
                word.PrimaryCefrLevel,
                word.NormalizedLemma,
                word.Senses
                    .Where(sense => sense.IsPrimarySense)
                    .SelectMany(sense => sense.Translations
                        .Where(translation => translation.LanguageCode == resolvedMeaningLanguageCode)
                        .OrderByDescending(translation => translation.IsPrimary)
                        .ThenBy(translation => translation.TranslationText)
                        .Select(translation => translation.TranslationText)
                        .Take(1))
                    .FirstOrDefault(),
                false))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return words
            .Select(MapToBrowseModel)
            .ToArray();
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

        List<WordBrowseProjection> prefixMatches = await activeWords
            .Where(word => EF.Functions.Like(word.NormalizedLemma, prefixPattern))
            .OrderBy(word => word.NormalizedLemma)
            .Select(word => CreateBrowseProjection(word, resolvedMeaningLanguageCode, true))
            .Take(SearchResultLimit)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (prefixMatches.Count >= SearchResultLimit)
        {
            return prefixMatches
                .Select(MapToBrowseModel)
                .ToArray();
        }

        int remainingCount = SearchResultLimit - prefixMatches.Count;

        List<WordBrowseProjection> containsMatches = await activeWords
            .Where(word => !EF.Functions.Like(word.NormalizedLemma, prefixPattern))
            .Where(word => EF.Functions.Like(word.NormalizedLemma, containsPattern))
            .OrderBy(word => word.NormalizedLemma)
            .Select(word => CreateBrowseProjection(word, resolvedMeaningLanguageCode, false))
            .Take(remainingCount)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return prefixMatches
            .Concat(containsMatches)
            .Select(MapToBrowseModel)
            .ToArray();
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
            .Include(word => word.Senses)
                .ThenInclude(sense => sense.Translations)
            .Include(word => word.Senses)
                .ThenInclude(sense => sense.Examples)
                    .ThenInclude(example => example.Translations)
            .Include(word => word.Topics)
            .Include(word => word.Collocations)
            .Include(word => word.FamilyMembers)
            .Include(word => word.Relations)
            .Include(word => word.GrammarNotes)
            .Include(word => word.Labels);
    }

    private sealed record WordBrowseProjection(
        Guid PublicId,
        string Lemma,
        string? Article,
        string? PluralForm,
        PartOfSpeech PartOfSpeech,
        CefrLevel CefrLevel,
        string NormalizedLemma,
        string? PrimaryMeaning,
        bool IsPrefixMatch);

    private static WordBrowseProjection CreateBrowseProjection(
        WordEntry word,
        LanguageCode resolvedMeaningLanguageCode,
        bool isPrefixMatch)
    {
        return new WordBrowseProjection(
            word.PublicId,
            word.Lemma,
            word.Article,
            word.PluralForm,
            word.PartOfSpeech,
            word.PrimaryCefrLevel,
            word.NormalizedLemma,
            word.Senses
                .Where(sense => sense.IsPrimarySense)
                .SelectMany(sense => sense.Translations
                    .Where(translation => translation.LanguageCode == resolvedMeaningLanguageCode)
                    .OrderByDescending(translation => translation.IsPrimary)
                    .ThenBy(translation => translation.TranslationText)
                    .Select(translation => translation.TranslationText)
                    .Take(1))
                .FirstOrDefault(),
            isPrefixMatch);
    }

    private static WordListItemModel MapToBrowseModel(WordBrowseProjection word)
    {
        return new WordListItemModel(
            word.PublicId,
            word.Lemma,
            word.Article,
            word.PluralForm,
            word.PartOfSpeech.ToString(),
            word.CefrLevel.ToString(),
            word.PrimaryMeaning);
    }
}
