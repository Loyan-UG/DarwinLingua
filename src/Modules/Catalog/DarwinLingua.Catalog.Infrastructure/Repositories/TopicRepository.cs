using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

/// <summary>
/// Reads topic aggregates from the shared SQLite database.
/// </summary>
internal sealed class TopicRepository : ITopicRepository
{
    private readonly IDbContextFactory<DarwinLinguaDbContext> _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TopicRepository"/> class.
    /// </summary>
    public TopicRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory)
    {
        ArgumentNullException.ThrowIfNull(dbContextFactory);

        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Topic>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        return await dbContext.Topics
            .AsNoTracking()
            .Include(topic => topic.Localizations)
            .OrderBy(topic => topic.SortOrder)
            .ThenBy(topic => topic.Key)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesByIdsAsync(
        IReadOnlyCollection<Guid> topicIds,
        LanguageCode preferredLanguageCode,
        LanguageCode fallbackLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(topicIds);

        if (topicIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        List<TopicDisplayNameProjection> topics = await dbContext.Topics
            .AsNoTracking()
            .Where(topic => topicIds.Contains(topic.Id))
            .OrderBy(topic => topic.SortOrder)
            .ThenBy(topic => topic.Key)
            .Select(topic => new TopicDisplayNameProjection(
                topic.Id,
                topic.Key,
                topic.Localizations
                    .Where(localization => localization.LanguageCode == preferredLanguageCode)
                    .Select(localization => localization.DisplayName)
                    .FirstOrDefault(),
                topic.Localizations
                    .Where(localization => localization.LanguageCode == fallbackLanguageCode)
                    .Select(localization => localization.DisplayName)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return topics.ToDictionary(
            topic => topic.Id,
            topic => topic.PreferredDisplayName
                ?? topic.FallbackDisplayName
                ?? topic.Key);
    }

    private sealed record TopicDisplayNameProjection(
        Guid Id,
        string Key,
        string? PreferredDisplayName,
        string? FallbackDisplayName);
}
