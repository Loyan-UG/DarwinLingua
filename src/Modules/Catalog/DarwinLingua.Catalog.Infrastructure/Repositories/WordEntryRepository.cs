using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
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
    public async Task<IReadOnlyList<WordEntry>> GetActiveByTopicKeyAsync(string topicKey, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicKey);

        string normalizedTopicKey = topicKey.Trim().ToLowerInvariant();

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        IQueryable<Guid> matchingTopicIds = dbContext.Topics
            .AsNoTracking()
            .Where(topic => topic.Key == normalizedTopicKey)
            .Select(topic => topic.Id);

        return await CreateAggregateQuery(dbContext)
            .Where(word => word.PublicationStatus == PublicationStatus.Active)
            .Where(word => word.Topics.Any(topic => matchingTopicIds.Contains(topic.TopicId)))
            .OrderBy(word => word.NormalizedLemma)
            .ToListAsync(cancellationToken)
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
            .SingleOrDefaultAsync(word => word.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WordEntry>> GetActiveByCefrAsync(CefrLevel cefrLevel, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        return await CreateAggregateQuery(dbContext)
            .Where(word => word.PublicationStatus == PublicationStatus.Active && word.PrimaryCefrLevel == cefrLevel)
            .OrderBy(word => word.NormalizedLemma)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WordEntry>> SearchActiveByLemmaAsync(string normalizedLemmaQuery, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedLemmaQuery);

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        string prefixPattern = $"{normalizedLemmaQuery}%";
        string containsPattern = $"%{normalizedLemmaQuery}%";

        var rankedMatches = await dbContext.WordEntries
            .AsNoTracking()
            .Where(word => word.PublicationStatus == PublicationStatus.Active)
            .Where(word =>
                EF.Functions.Like(word.NormalizedLemma, prefixPattern) ||
                EF.Functions.Like(word.NormalizedLemma, containsPattern))
            .Select(word => new
            {
                word.Id,
                word.NormalizedLemma,
                IsPrefixMatch = EF.Functions.Like(word.NormalizedLemma, prefixPattern),
            })
            .OrderByDescending(candidate => candidate.IsPrefixMatch)
            .ThenBy(candidate => candidate.NormalizedLemma)
            .Take(SearchResultLimit)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (rankedMatches.Count == 0)
        {
            return [];
        }

        Guid[] orderedWordIds = rankedMatches
            .Select(candidate => candidate.Id)
            .ToArray();
        Dictionary<Guid, int> sortPositions = orderedWordIds
            .Select((wordId, index) => new { wordId, index })
            .ToDictionary(item => item.wordId, item => item.index);

        List<WordEntry> words = await CreateAggregateQuery(dbContext)
            .Where(word => orderedWordIds.Contains(word.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return words
            .OrderBy(word => sortPositions[word.Id])
            .ToArray();
    }

    /// <summary>
    /// Creates the aggregate query shape required by the current browse and detail projections.
    /// </summary>
    /// <param name="dbContext">The active database context.</param>
    /// <returns>The configured aggregate query.</returns>
    private static IQueryable<WordEntry> CreateAggregateQuery(DarwinLinguaDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        return dbContext.WordEntries
            .AsNoTracking()
            .Include(word => word.Senses)
                .ThenInclude(sense => sense.Translations)
            .Include(word => word.Senses)
                .ThenInclude(sense => sense.Examples)
                    .ThenInclude(example => example.Translations)
            .Include(word => word.Topics)
            .Include(word => word.GrammarNotes)
            .Include(word => word.Labels);
    }
}
