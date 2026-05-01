using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

public interface IAdminTaxonomyService
{
    Task<AdminTopicsResponse> GetTopicsAsync(CancellationToken cancellationToken);

    Task<AdminTopicItemResponse?> GetTopicAsync(Guid topicId, CancellationToken cancellationToken);

    Task<AdminTopicItemResponse> CreateTopicAsync(AdminSaveTopicRequest request, CancellationToken cancellationToken);

    Task<AdminTopicItemResponse?> UpdateTopicAsync(Guid topicId, AdminSaveTopicRequest request, CancellationToken cancellationToken);

    Task<AdminLabelsResponse> GetLabelsAsync(CancellationToken cancellationToken);
}

internal sealed class AdminTaxonomyService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IAdminTaxonomyService
{
    public async Task<AdminTopicsResponse> GetTopicsAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<Guid, int> wordCountsByTopicId = await dbContext.WordTopics
            .GroupBy(topic => topic.TopicId)
            .Select(group => new { TopicId = group.Key, WordCount = group.Count() })
            .ToDictionaryAsync(item => item.TopicId, item => item.WordCount, cancellationToken)
            .ConfigureAwait(false);

        Topic[] topics = await dbContext.Topics
            .Include(topic => topic.Localizations)
            .OrderBy(topic => topic.SortOrder)
            .ThenBy(topic => topic.Key)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminTopicsResponse(topics.Select(topic => MapTopic(topic, wordCountsByTopicId)).ToArray());
    }

    public async Task<AdminTopicItemResponse?> GetTopicAsync(Guid topicId, CancellationToken cancellationToken)
    {
        if (topicId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        Topic? topic = await dbContext.Topics
            .Include(item => item.Localizations)
            .SingleOrDefaultAsync(item => item.Id == topicId, cancellationToken)
            .ConfigureAwait(false);

        if (topic is null)
        {
            return null;
        }

        int wordCount = await dbContext.WordTopics.CountAsync(item => item.TopicId == topicId, cancellationToken).ConfigureAwait(false);
        return MapTopic(topic, new Dictionary<Guid, int> { [topic.Id] = wordCount });
    }

    public async Task<AdminTopicItemResponse> CreateTopicAsync(AdminSaveTopicRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        DateTime now = DateTime.UtcNow;
        Topic topic = new(Guid.NewGuid(), request.Key, request.SortOrder, request.IsSystem, now);
        ApplyLocalizations(topic, request.Localizations, now);

        dbContext.Topics.Add(topic);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapTopic(topic, new Dictionary<Guid, int>());
    }

    public async Task<AdminTopicItemResponse?> UpdateTopicAsync(Guid topicId, AdminSaveTopicRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (topicId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        Topic? topic = await dbContext.Topics
            .Include(item => item.Localizations)
            .SingleOrDefaultAsync(item => item.Id == topicId, cancellationToken)
            .ConfigureAwait(false);

        if (topic is null)
        {
            return null;
        }

        DateTime now = DateTime.UtcNow;
        topic.UpdateMetadata(request.Key, request.SortOrder, request.IsSystem, now);
        ApplyLocalizations(topic, request.Localizations, now);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        int wordCount = await dbContext.WordTopics.CountAsync(item => item.TopicId == topicId, cancellationToken).ConfigureAwait(false);
        return MapTopic(topic, new Dictionary<Guid, int> { [topic.Id] = wordCount });
    }

    public async Task<AdminLabelsResponse> GetLabelsAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        AdminLabelItemResponse[] labels = await dbContext.WordLabels
            .GroupBy(label => new { label.Kind, label.Key })
            .Select(group => new AdminLabelItemResponse(
                group.Key.Kind.ToString(),
                group.Key.Key,
                group.Select(label => label.WordEntryId).Distinct().Count(),
                group.Min(label => label.SortOrder)))
            .OrderBy(label => label.Kind)
            .ThenByDescending(label => label.WordCount)
            .ThenBy(label => label.Key)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminLabelsResponse(labels);
    }

    private static void ApplyLocalizations(Topic topic, IReadOnlyList<AdminTopicLocalizationRequest>? localizations, DateTime now)
    {
        if (localizations is null)
        {
            return;
        }

        foreach (AdminTopicLocalizationRequest localization in localizations)
        {
            if (string.IsNullOrWhiteSpace(localization.LanguageCode) || string.IsNullOrWhiteSpace(localization.DisplayName))
            {
                continue;
            }

            topic.AddOrUpdateLocalization(
                Guid.NewGuid(),
                LanguageCode.From(localization.LanguageCode),
                localization.DisplayName,
                now);
        }
    }

    private static AdminTopicItemResponse MapTopic(Topic topic, IReadOnlyDictionary<Guid, int> wordCountsByTopicId) =>
        new(
            topic.Id,
            topic.Key,
            topic.SortOrder,
            topic.IsSystem,
            wordCountsByTopicId.GetValueOrDefault(topic.Id),
            topic.UpdatedAtUtc,
            topic.Localizations
                .OrderBy(localization => localization.LanguageCode.Value)
                .Select(localization => new AdminTopicLocalizationResponse(
                    localization.LanguageCode.Value,
                    localization.DisplayName))
                .ToArray());
}
