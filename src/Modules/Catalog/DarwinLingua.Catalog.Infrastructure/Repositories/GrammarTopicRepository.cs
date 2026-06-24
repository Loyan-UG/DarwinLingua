using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class GrammarTopicRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IGrammarTopicRepository
{
    private static readonly LanguageCode EnglishFallbackLanguage = LanguageCode.From("en");

    public async Task<IReadOnlyList<GrammarTopicListItemModel>> GetPublishedGrammarTopicsAsync(
        GrammarTopicListFilterModel filter,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);
        string targetLanguageCode = NormalizeRequiredLanguageCode(targetLearningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        IQueryable<GrammarTopic> query = dbContext.GrammarTopics
            .AsNoTracking()
            .Include(topic => topic.Topics)
            .Where(topic => topic.PublicationStatus == PublicationStatus.Active && topic.TargetLearningLanguageCode == targetLanguageCode);

        if (Enum.TryParse(filter.CefrLevel, true, out CefrLevel cefrLevel))
        {
            query = query.Where(topic => topic.CefrLevel == cefrLevel);
        }

        string? category = NormalizeOptionalKey(filter.GrammarCategory);
        if (category is not null)
        {
            query = query.Where(topic => topic.GrammarCategory == category);
        }

        string? search = NormalizeOptionalSearch(filter.Query);
        if (search is not null)
        {
            query = query.Where(topic =>
                EF.Functions.ILike(topic.Title, $"%{search}%") ||
                EF.Functions.ILike(topic.ShortDescription, $"%{search}%") ||
                EF.Functions.ILike(topic.Slug, $"%{search}%"));
        }

        List<GrammarTopic> topics = await query
            .OrderBy(topic => topic.SortOrder)
            .ThenBy(topic => topic.Title)
            .AsSplitQuery()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<Guid, string> topicKeysById = await LoadTopicKeysAsync(dbContext, topics.SelectMany(item => item.Topics).Select(item => item.TopicId), cancellationToken)
            .ConfigureAwait(false);

        string? topicKey = NormalizeOptionalKey(filter.TopicKey);
        IEnumerable<GrammarTopic> filteredTopics = topics;
        if (topicKey is not null)
        {
            filteredTopics = filteredTopics.Where(topic =>
                topic.Topics.Any(link =>
                    topicKeysById.TryGetValue(link.TopicId, out string? key) &&
                    string.Equals(key, topicKey, StringComparison.Ordinal)));
        }

        return filteredTopics
            .Select(topic => new GrammarTopicListItemModel(
                topic.Slug,
                topic.Title,
                topic.ShortDescription,
                topic.CefrLevel.ToString(),
                topic.GrammarCategory,
                GetTopicKeys(topic.Topics, topicKeysById)))
            .ToArray();
    }

    public async Task<GrammarTopicDetailModel?> GetPublishedGrammarTopicBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        string normalizedSlug = slug.Trim().ToLowerInvariant();
        string targetLanguageCode = NormalizeRequiredLanguageCode(targetLearningLanguageCode);
        LanguageCode primaryLanguage = LanguageCode.From(primaryMeaningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        GrammarTopic? topic = await dbContext.GrammarTopics
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.Topics)
            .Include(item => item.Sections).ThenInclude(section => section.Translations)
            .Include(item => item.Examples).ThenInclude(example => example.Translations)
            .Include(item => item.RuleSummaries).ThenInclude(rule => rule.Translations)
            .Include(item => item.CommonMistakes).ThenInclude(mistake => mistake.Translations)
            .Include(item => item.ExceptionNotes).ThenInclude(note => note.Translations)
            .Include(item => item.LinkedWords)
            .Include(item => item.LinkedDialogues)
            .Include(item => item.LinkedTalkTopics)
            .Include(item => item.LinkedExercises)
            .Include(item => item.Prerequisites)
            .Include(item => item.RelatedTopics)
            .Where(item => item.PublicationStatus == PublicationStatus.Active && item.TargetLearningLanguageCode == targetLanguageCode && item.Slug == normalizedSlug)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (topic is null)
        {
            return null;
        }

        Dictionary<Guid, string> topicKeysById = await LoadTopicKeysAsync(dbContext, topic.Topics.Select(link => link.TopicId), cancellationToken)
            .ConfigureAwait(false);

        return new GrammarTopicDetailModel(
            topic.Slug,
            ResolveLocalizedText(topic.TitleLocalizedJson, primaryLanguage, topic.Title),
            ResolveLocalizedText(topic.ShortDescriptionLocalizedJson, primaryLanguage, topic.ShortDescription),
            topic.ContentRevision,
            topic.CefrLevel.ToString(),
            topic.GrammarCategory,
            GetTopicKeys(topic.Topics, topicKeysById),
            topic.Sections.OrderBy(item => item.SortOrder).Select(item => MapSection(item, primaryLanguage)).ToArray(),
            topic.Examples.OrderBy(item => item.SortOrder).Select(item => MapExample(item, primaryLanguage)).ToArray(),
            topic.RuleSummaries.OrderBy(item => item.SortOrder).Select(item => MapTextItem(item.Text, item.Translations, primaryLanguage)).ToArray(),
            topic.CommonMistakes.OrderBy(item => item.SortOrder).Select(item => MapMistake(item, primaryLanguage)).ToArray(),
            topic.ExceptionNotes.OrderBy(item => item.SortOrder).Select(item => MapTextItem(item.Text, item.Translations, primaryLanguage)).ToArray(),
            topic.LinkedWords.OrderBy(item => item.SortOrder).Select(item => new GrammarLinkedWordModel(item.Lemma, item.WordSlug)).ToArray(),
            topic.LinkedDialogues.OrderBy(item => item.SortOrder).Select(item => item.TargetSlug).ToArray(),
            topic.LinkedTalkTopics.OrderBy(item => item.SortOrder).Select(item => item.TargetSlug).ToArray(),
            topic.LinkedExercises.OrderBy(item => item.SortOrder).Select(item => item.TargetSlug).ToArray(),
            topic.Prerequisites.OrderBy(item => item.SortOrder).Select(item => item.TargetSlug).ToArray(),
            topic.RelatedTopics.OrderBy(item => item.SortOrder).Select(item => item.TargetSlug).ToArray(),
            ParseImageSlots(topic.ImageSlotsJson));
    }

    private static GrammarSectionModel MapSection(GrammarSection section, LanguageCode primaryLanguage)
    {
        GrammarSectionTranslation? translation = ResolveTranslation(section.Translations, primaryLanguage);
        IReadOnlyList<GrammarContentBlockModel> blocks = ResolveRichBlocks(section.LocalizedBlocksJson, primaryLanguage);
        return new GrammarSectionModel(
            section.SectionKey,
            translation?.Heading ?? section.Heading,
            translation?.Text ?? section.Explanation,
            blocks,
            primaryLanguage.Value,
            blocks.Count > 0
                ? !HasLocalizedBlocksForLanguage(section.LocalizedBlocksJson, primaryLanguage)
                : translation is null || translation.LanguageCode != primaryLanguage);
    }

    private static string ResolveLocalizedText(string? localizedJson, LanguageCode primaryLanguage, string baseText)
    {
        Dictionary<string, string> values = DeserializeStringDictionary(localizedJson);
        if (values.TryGetValue(primaryLanguage.Value, out string? primary) && !string.IsNullOrWhiteSpace(primary))
        {
            return primary;
        }

        if (values.TryGetValue(EnglishFallbackLanguage.Value, out string? english) && !string.IsNullOrWhiteSpace(english))
        {
            return english;
        }

        return string.IsNullOrWhiteSpace(baseText)
            ? values.Values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty
            : baseText;
    }

    private static IReadOnlyList<GrammarContentBlockModel> ResolveRichBlocks(string? localizedBlocksJson, LanguageCode primaryLanguage)
    {
        if (string.IsNullOrWhiteSpace(localizedBlocksJson))
        {
            return [];
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(localizedBlocksJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return [];
            }

            JsonElement? blocks = TryGetProperty(document.RootElement, primaryLanguage.Value)
                ?? TryGetProperty(document.RootElement, EnglishFallbackLanguage.Value)
                ?? document.RootElement.EnumerateObject().FirstOrDefault().Value;

            return blocks.HasValue && blocks.Value.ValueKind == JsonValueKind.Array
                ? blocks.Value.EnumerateArray().Select(ParseBlock).Where(block => block is not null).Cast<GrammarContentBlockModel>().ToArray()
                : [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static bool HasLocalizedBlocksForLanguage(string? localizedBlocksJson, LanguageCode language)
    {
        if (string.IsNullOrWhiteSpace(localizedBlocksJson))
        {
            return false;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(localizedBlocksJson);
            return document.RootElement.ValueKind == JsonValueKind.Object &&
                TryGetProperty(document.RootElement, language.Value).HasValue;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static GrammarContentBlockModel? ParseBlock(JsonElement block)
    {
        if (block.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        string? type = ReadString(block, "type");
        if (string.IsNullOrWhiteSpace(type))
        {
            return null;
        }

        return new GrammarContentBlockModel(
            type,
            ReadString(block, "text"),
            ReadString(block, "style"),
            ReadString(block, "caption"),
            ReadStringArray(block, "columns"),
            ReadTableRows(block, "rows"),
            ReadStringArray(block, "items"),
            ReadString(block, "wrong"),
            ReadString(block, "correct"),
            ReadString(block, "assetKey"),
            ReadString(block, "imageSlotKey"));
    }

    private static IReadOnlyList<GrammarImageSlotModel> ParseImageSlots(string? imageSlotsJson)
    {
        if (string.IsNullOrWhiteSpace(imageSlotsJson))
        {
            return [];
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(imageSlotsJson);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            return document.RootElement.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.Object)
                .Select(item => new GrammarImageSlotModel(
                    ReadString(item, "imageSlotKey") ?? string.Empty,
                    ReadString(item, "assetKey"),
                    ReadString(item, "imageFileName"),
                    ReadString(item, "altText")))
                .Where(item => !string.IsNullOrWhiteSpace(item.ImageSlotKey) || !string.IsNullOrWhiteSpace(item.AssetKey))
                .ToArray();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static Dictionary<string, string> DeserializeStringDictionary(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static JsonElement? TryGetProperty(JsonElement element, string propertyName)
    {
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value;
            }
        }

        return null;
    }

    private static string? ReadString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static string[] ReadStringArray(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.Array
            ? property.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.String).Select(item => item.GetString() ?? string.Empty).ToArray()
            : [];

    private static IReadOnlyList<IReadOnlyList<string>> ReadTableRows(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.Array
            ? property.EnumerateArray()
                .Where(row => row.ValueKind == JsonValueKind.Array)
                .Select(row => (IReadOnlyList<string>)row.EnumerateArray()
                    .Where(cell => cell.ValueKind == JsonValueKind.String)
                    .Select(cell => cell.GetString() ?? string.Empty)
                    .ToArray())
                .ToArray()
            : [];

    private static GrammarExampleModel MapExample(GrammarExample example, LanguageCode primaryLanguage)
    {
        GrammarExampleTranslation? translation = ResolveTranslation(example.Translations, primaryLanguage);
        return new GrammarExampleModel(
            example.GermanText,
            example.Note,
            translation?.Text,
            primaryLanguage.Value,
            translation is null || translation.LanguageCode != primaryLanguage);
    }

    private static GrammarCommonMistakeModel MapMistake(GrammarCommonMistake mistake, LanguageCode primaryLanguage)
    {
        GrammarCommonMistakeTranslation? translation = ResolveTranslation(mistake.Translations, primaryLanguage);
        return new GrammarCommonMistakeModel(
            mistake.WrongText,
            mistake.CorrectedText,
            translation?.Text ?? mistake.Explanation,
            primaryLanguage.Value,
            translation is null || translation.LanguageCode != primaryLanguage);
    }

    private static GrammarTextItemModel MapTextItem<TTranslation>(
        string baseText,
        IReadOnlyCollection<TTranslation> translations,
        LanguageCode primaryLanguage)
        where TTranslation : GrammarTranslationBase
    {
        TTranslation? translation = ResolveTranslation(translations, primaryLanguage);
        return new GrammarTextItemModel(
            translation?.Text ?? baseText,
            primaryLanguage.Value,
            translation is null || translation.LanguageCode != primaryLanguage);
    }

    private static TTranslation? ResolveTranslation<TTranslation>(
        IReadOnlyCollection<TTranslation> translations,
        LanguageCode primaryLanguage)
        where TTranslation : GrammarTranslationBase =>
        translations
            .Where(translation => translation.LanguageCode == primaryLanguage || translation.LanguageCode == EnglishFallbackLanguage)
            .OrderBy(translation => translation.LanguageCode == primaryLanguage ? 0 : 1)
            .FirstOrDefault();

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

    private static string[] GetTopicKeys(IEnumerable<GrammarTopicTopic> links, IReadOnlyDictionary<Guid, string> topicKeysById) =>
        links
            .OrderByDescending(link => link.IsPrimary)
            .ThenBy(link => link.CreatedAtUtc)
            .Where(link => topicKeysById.ContainsKey(link.TopicId))
            .Select(link => topicKeysById[link.TopicId])
            .ToArray();

    private static string? NormalizeOptionalKey(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string? NormalizeOptionalSearch(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeRequiredLanguageCode(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? ContentLanguageRequirements.DefaultTargetLearningLanguageCode
            : value.Trim().ToLowerInvariant();
}
