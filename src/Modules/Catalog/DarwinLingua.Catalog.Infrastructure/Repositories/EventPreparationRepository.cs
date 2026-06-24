using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class EventPreparationRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IEventPreparationRepository
{
    public async Task<IReadOnlyList<EventPreparationPackListItemModel>> GetPublishedEventPreparationPacksAsync(
        EventPreparationListFilterModel filter,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        Guid? topicId = null;
        if (!string.IsNullOrWhiteSpace(filter.TopicKey))
        {
            string normalizedTopicKey = filter.TopicKey.Trim().ToLowerInvariant();
            topicId = await dbContext.Topics
                .AsNoTracking()
                .Where(topic => topic.Key == normalizedTopicKey)
                .Select(topic => (Guid?)topic.Id)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (topicId is null)
            {
                return [];
            }
        }

        string normalizedTargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode);

        IQueryable<EventPreparationPack> query = dbContext.EventPreparationPacks
            .AsNoTracking()
            .AsSplitQuery()
            .Include(pack => pack.Topics)
            .Include(pack => pack.LinkedDialogues)
            .Include(pack => pack.LinkedConversationStarterPacks)
            .Where(pack => pack.PublicationStatus == PublicationStatus.Active)
            .Where(pack => pack.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode);

        if (!string.IsNullOrWhiteSpace(filter.CefrLevel) &&
            Enum.TryParse(filter.CefrLevel.Trim(), true, out CefrLevel cefrLevel))
        {
            query = query.Where(pack => pack.CefrLevel == cefrLevel);
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            string category = filter.Category.Trim().ToLowerInvariant();
            query = query.Where(pack => pack.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(filter.EventType))
        {
            string eventType = filter.EventType.Trim().ToLowerInvariant();
            query = query.Where(pack => pack.EventType == eventType);
        }

        if (topicId.HasValue)
        {
            query = query.Where(pack => pack.Topics.Any(topic => topic.TopicId == topicId.Value));
        }

        List<EventPreparationPack> packs = await query
            .OrderBy(pack => pack.SortOrder)
            .ThenBy(pack => pack.Title)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<Guid, string> topicKeysById = await LoadTopicKeysAsync(
            dbContext,
            packs.SelectMany(pack => pack.Topics).Select(topic => topic.TopicId),
            cancellationToken).ConfigureAwait(false);

        return packs
            .Select(pack => CreateListItem(pack, topicKeysById))
            .ToArray();
    }

    public async Task<IReadOnlyList<EventPreparationPackListItemModel>> GetPublishedEventPreparationPacksForDialogueAsync(
        string dialogueSlug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dialogueSlug);
        string normalizedDialogueSlug = dialogueSlug.Trim().ToLowerInvariant();
        string normalizedTargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        List<EventPreparationPack> packs = await dbContext.EventPreparationPacks
            .AsNoTracking()
            .AsSplitQuery()
            .Include(pack => pack.Topics)
            .Include(pack => pack.LinkedDialogues)
            .Include(pack => pack.LinkedConversationStarterPacks)
            .Where(pack => pack.PublicationStatus == PublicationStatus.Active)
            .Where(pack => pack.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode)
            .Where(pack => pack.LinkedDialogues.Any(link => link.DialogueSlug == normalizedDialogueSlug))
            .OrderBy(pack => pack.SortOrder)
            .ThenBy(pack => pack.Title)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<Guid, string> topicKeysById = await LoadTopicKeysAsync(
            dbContext,
            packs.SelectMany(pack => pack.Topics).Select(topic => topic.TopicId),
            cancellationToken).ConfigureAwait(false);

        return packs
            .Select(pack => CreateListItem(pack, topicKeysById))
            .ToArray();
    }

    public async Task<EventPreparationPackDetailModel?> GetPublishedEventPreparationPackBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        string normalizedSlug = slug.Trim().ToLowerInvariant();
        string normalizedTargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        EventPreparationPack? pack = await dbContext.EventPreparationPacks
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.Topics)
            .Include(item => item.LinkedDialogues)
            .Include(item => item.LinkedConversationStarterPacks)
            .Include(item => item.LinkedVocabulary)
            .Include(item => item.Prompts)
            .Where(item => item.PublicationStatus == PublicationStatus.Active && item.Slug == normalizedSlug)
            .Where(item => item.TargetLearningLanguageCode == normalizedTargetLearningLanguageCode)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (pack is null)
        {
            return null;
        }

        Dictionary<Guid, string> topicKeysById = await LoadTopicKeysAsync(
            dbContext,
            pack.Topics.Select(topic => topic.TopicId),
            cancellationToken).ConfigureAwait(false);

        return new EventPreparationPackDetailModel(
            pack.Slug,
            pack.Title,
            pack.Description,
            pack.CefrLevel.ToString(),
            pack.Category,
            pack.EventType,
            ResolveTopicKeys(pack.Topics, topicKeysById),
            ResolveLinkedDialogues(pack),
            ResolveLinkedStarterPacks(pack),
            pack.LinkedVocabulary
                .OrderBy(reference => reference.SortOrder)
                .Select(reference => new EventPreparationVocabularyReferenceModel(
                    reference.Word,
                    reference.PartOfSpeech?.ToString(),
                    reference.CefrLevel?.ToString()))
                .ToArray(),
            pack.Prompts
                .OrderBy(prompt => prompt.PromptType)
                .ThenBy(prompt => prompt.SortOrder)
                .Select(prompt => new EventPreparationPromptModel(prompt.PromptType, prompt.Text))
                .ToArray());
    }

    private static async Task<Dictionary<Guid, string>> LoadTopicKeysAsync(
        DarwinLinguaDbContext dbContext,
        IEnumerable<Guid> topicIds,
        CancellationToken cancellationToken)
    {
        Guid[] ids = topicIds.Distinct().ToArray();

        if (ids.Length == 0)
        {
            return [];
        }

        return await dbContext.Topics
            .AsNoTracking()
            .Where(topic => ids.Contains(topic.Id))
            .ToDictionaryAsync(topic => topic.Id, topic => topic.Key, cancellationToken)
            .ConfigureAwait(false);
    }

    private static string[] ResolveTopicKeys(
        IEnumerable<EventPreparationPackTopic> topics,
        IReadOnlyDictionary<Guid, string> topicKeysById) =>
        topics
            .OrderByDescending(topic => topic.IsPrimary)
            .ThenBy(topic => topic.CreatedAtUtc)
            .Select(topic => topicKeysById[topic.TopicId])
            .ToArray();

    private static string[] ResolveLinkedDialogues(EventPreparationPack pack) =>
        pack.LinkedDialogues
            .OrderBy(link => link.SortOrder)
            .Select(link => link.DialogueSlug)
            .ToArray();

    private static string[] ResolveLinkedStarterPacks(EventPreparationPack pack) =>
        pack.LinkedConversationStarterPacks
            .OrderBy(link => link.SortOrder)
            .Select(link => link.ConversationStarterPackSlug)
            .ToArray();

    private static EventPreparationPackListItemModel CreateListItem(
        EventPreparationPack pack,
        IReadOnlyDictionary<Guid, string> topicKeysById) =>
        new(
            pack.Slug,
            pack.Title,
            pack.Description,
            pack.CefrLevel.ToString(),
            pack.Category,
            pack.EventType,
            ResolveTopicKeys(pack.Topics, topicKeysById),
            ResolveLinkedDialogues(pack),
            ResolveLinkedStarterPacks(pack));
}
