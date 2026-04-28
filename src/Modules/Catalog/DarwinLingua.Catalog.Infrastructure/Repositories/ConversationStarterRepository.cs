using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class ConversationStarterRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IConversationStarterRepository
{
    private static readonly LanguageCode EnglishFallbackLanguage = LanguageCode.From("en");

    public async Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetPublishedStarterPacksAsync(
        ConversationStarterListFilterModel filter,
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

        IQueryable<ConversationStarterPack> query = dbContext.ConversationStarterPacks
            .AsNoTracking()
            .AsSplitQuery()
            .Include(pack => pack.Topics)
            .Include(pack => pack.LinkedScenarios)
            .Where(pack => pack.PublicationStatus == PublicationStatus.Active);

        if (!string.IsNullOrWhiteSpace(filter.CefrLevel) &&
            Enum.TryParse(filter.CefrLevel.Trim(), true, out CefrLevel cefrLevel))
        {
            query = query.Where(pack => pack.CefrLevel == cefrLevel);
        }

        if (!string.IsNullOrWhiteSpace(filter.Situation))
        {
            string situation = filter.Situation.Trim().ToLowerInvariant();
            query = query.Where(pack => pack.Situation == situation);
        }

        if (!string.IsNullOrWhiteSpace(filter.Tone))
        {
            string tone = filter.Tone.Trim().ToLowerInvariant();
            query = query.Where(pack => pack.Tone == tone);
        }

        if (!string.IsNullOrWhiteSpace(filter.ConversationGoal))
        {
            string conversationGoal = filter.ConversationGoal.Trim().ToLowerInvariant();
            query = query.Where(pack => pack.ConversationGoal == conversationGoal);
        }

        if (topicId.HasValue)
        {
            query = query.Where(pack => pack.Topics.Any(topic => topic.TopicId == topicId.Value));
        }

        List<ConversationStarterPack> packs = await query
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

    public async Task<IReadOnlyList<ConversationStarterPackListItemModel>> GetPublishedStarterPacksForScenarioAsync(
        string scenarioSlug,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenarioSlug);
        string normalizedScenarioSlug = scenarioSlug.Trim().ToLowerInvariant();

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        List<ConversationStarterPack> packs = await dbContext.ConversationStarterPacks
            .AsNoTracking()
            .AsSplitQuery()
            .Include(pack => pack.Topics)
            .Include(pack => pack.LinkedScenarios)
            .Where(pack => pack.PublicationStatus == PublicationStatus.Active)
            .Where(pack => pack.LinkedScenarios.Any(link => link.ScenarioSlug == normalizedScenarioSlug))
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

    public async Task<ConversationStarterPackDetailModel?> GetPublishedStarterPackBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        string normalizedSlug = slug.Trim().ToLowerInvariant();

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        ConversationStarterPack? pack = await dbContext.ConversationStarterPacks
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.Topics)
            .Include(item => item.LinkedScenarios)
            .Include(item => item.LinkedEventPreparationPacks)
            .Include(item => item.Phrases).ThenInclude(phrase => phrase.Translations)
            .Include(item => item.Phrases).ThenInclude(phrase => phrase.AlternativeBaseTexts)
            .Where(item => item.PublicationStatus == PublicationStatus.Active && item.Slug == normalizedSlug)
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

        LanguageCode primaryLanguage = LanguageCode.From(primaryMeaningLanguageCode);
        LanguageCode? secondaryLanguage = string.IsNullOrWhiteSpace(secondaryMeaningLanguageCode)
            ? null
            : LanguageCode.From(secondaryMeaningLanguageCode);

        return new ConversationStarterPackDetailModel(
            pack.Slug,
            pack.Title,
            pack.Description,
            pack.CefrLevel.ToString(),
            pack.Category,
            pack.Situation,
            pack.Tone,
            pack.ConversationGoal,
            ResolveTopicKeys(pack.Topics, topicKeysById),
            pack.LinkedScenarios
                .OrderBy(link => link.SortOrder)
                .Select(link => link.ScenarioSlug)
                .ToArray(),
            pack.LinkedEventPreparationPacks
                .OrderBy(link => link.SortOrder)
                .Select(link => link.EventPreparationPackSlug)
                .ToArray(),
            pack.Phrases
                .OrderBy(phrase => phrase.SortOrder)
                .Select(phrase => new ConversationStarterPhraseModel(
                    phrase.BaseText,
                    phrase.Function,
                    ResolvePrimaryMeaning(phrase.Translations, primaryLanguage),
                    ResolveSecondaryMeaning(phrase.Translations, secondaryLanguage),
                    phrase.UsageNote,
                    phrase.Register,
                    phrase.AlternativeBaseTexts
                        .OrderBy(alternative => alternative.SortOrder)
                        .Select(alternative => alternative.BaseText)
                        .ToArray(),
                    phrase.CommonMistake))
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
        IEnumerable<ConversationStarterPackTopic> topics,
        IReadOnlyDictionary<Guid, string> topicKeysById) =>
        topics
            .OrderByDescending(topic => topic.IsPrimary)
            .ThenBy(topic => topic.CreatedAtUtc)
            .Select(topic => topicKeysById[topic.TopicId])
            .ToArray();

    private static ConversationStarterPackListItemModel CreateListItem(
        ConversationStarterPack pack,
        IReadOnlyDictionary<Guid, string> topicKeysById) =>
        new(
            pack.Slug,
            pack.Title,
            pack.Description,
            pack.CefrLevel.ToString(),
            pack.Category,
            pack.Situation,
            pack.Tone,
            pack.ConversationGoal,
            ResolveTopicKeys(pack.Topics, topicKeysById),
            pack.LinkedScenarios
                .OrderBy(link => link.SortOrder)
                .Select(link => link.ScenarioSlug)
                .ToArray());

    private static string? ResolvePrimaryMeaning(
        IReadOnlyCollection<ConversationStarterPhraseTranslation> translations,
        LanguageCode primaryLanguage) =>
        translations
            .Where(translation => translation.LanguageCode == primaryLanguage || translation.LanguageCode == EnglishFallbackLanguage)
            .OrderBy(translation => translation.LanguageCode == primaryLanguage ? 0 : 1)
            .Select(translation => translation.Text)
            .FirstOrDefault();

    private static string? ResolveSecondaryMeaning(
        IReadOnlyCollection<ConversationStarterPhraseTranslation> translations,
        LanguageCode? secondaryLanguage) =>
        secondaryLanguage is null
            ? null
            : translations
                .Where(translation => translation.LanguageCode == secondaryLanguage)
                .Select(translation => translation.Text)
                .FirstOrDefault();
}
