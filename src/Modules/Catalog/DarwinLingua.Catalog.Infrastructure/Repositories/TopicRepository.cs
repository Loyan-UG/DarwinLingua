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
                topic.SortOrder,
                topic.Localizations
                    .Where(localization =>
                        localization.LanguageCode == preferredLanguageCode ||
                        localization.LanguageCode == fallbackLanguageCode)
                    .Select(localization => new TopicLocalizationProjection(localization.LanguageCode, localization.DisplayName))
                    .ToArray()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return topics.ToDictionary(
            topic => topic.Id,
            topic => topic.Localizations.FirstOrDefault(localization => localization.LanguageCode == preferredLanguageCode)?.DisplayName
                ?? topic.Localizations.FirstOrDefault(localization => localization.LanguageCode == fallbackLanguageCode)?.DisplayName
                ?? topic.Key);
    }

    private sealed record TopicDisplayNameProjection(
        Guid Id,
        string Key,
        int SortOrder,
        IReadOnlyList<TopicLocalizationProjection> Localizations);

    private sealed record TopicLocalizationProjection(LanguageCode LanguageCode, string DisplayName);
}
