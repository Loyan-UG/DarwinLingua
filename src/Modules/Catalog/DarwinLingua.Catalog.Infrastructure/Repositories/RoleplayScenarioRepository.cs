using System.Text.Json;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class RoleplayScenarioRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IRoleplayScenarioRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly LanguageCode EnglishFallbackLanguage = LanguageCode.From("en");

    public async Task<IReadOnlyList<RoleplayScenarioListItemModel>> GetPublishedRoleplayScenariosAsync(
        RoleplayScenarioListFilterModel filter,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        IQueryable<RoleplayScenario> query = dbContext.RoleplayScenarios
            .AsNoTracking()
            .AsSplitQuery()
            .Include(scenario => scenario.Topics)
            .Where(scenario => scenario.PublicationStatus == PublicationStatus.Active);

        query = ApplyFilters(query, filter);

        List<RoleplayScenario> scenarios = await query
            .OrderBy(scenario => scenario.SortOrder)
            .ThenBy(scenario => scenario.Title)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<Guid, string> topicKeysById = await LoadTopicKeysAsync(dbContext, scenarios.SelectMany(item => item.Topics).Select(item => item.TopicId), cancellationToken)
            .ConfigureAwait(false);

        string? topicKey = NormalizeKey(filter.TopicKey);
        IEnumerable<RoleplayScenario> filtered = scenarios;
        if (topicKey is not null)
        {
            filtered = filtered.Where(scenario => scenario.Topics.Any(topic => topicKeysById.TryGetValue(topic.TopicId, out string? key) && key == topicKey));
        }

        string? examProfile = NormalizeKey(filter.ExamProfile);
        if (examProfile is not null)
        {
            filtered = filtered.Where(scenario => ReadStringArray(scenario.ExamProfilesJson).Contains(examProfile, StringComparer.Ordinal));
        }

        string? skillFocus = NormalizeKey(filter.SkillFocus);
        if (skillFocus is not null)
        {
            filtered = filtered.Where(scenario => ReadStringArray(scenario.SkillFocusJson).Contains(skillFocus, StringComparer.Ordinal));
        }

        return filtered.Select(scenario => new RoleplayScenarioListItemModel(
                scenario.Slug,
                scenario.LinkedDialogueSlug,
                scenario.Title,
                ResolveText(scenario.TitleTranslationsJson, ResolveRequestedLanguage(primaryMeaningLanguageCode)),
                scenario.Description,
                ResolveText(scenario.DescriptionTranslationsJson, ResolveRequestedLanguage(primaryMeaningLanguageCode)),
                scenario.LearnerGoal,
                ResolveText(scenario.LearnerGoalTranslationsJson, ResolveRequestedLanguage(primaryMeaningLanguageCode)),
                scenario.CefrLevel.ToString(),
                scenario.Category,
                scenario.TaskType,
                scenario.InteractionMode,
                scenario.Register,
                scenario.EstimatedPracticeMinutes,
                ResolveTopicKeys(scenario.Topics, topicKeysById),
                ReadStringArray(scenario.ExamProfilesJson),
                ReadStringArray(scenario.SkillFocusJson)))
            .ToArray();
    }

    public async Task<RoleplayScenarioDetailModel?> GetPublishedRoleplayScenarioBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        string normalizedSlug = slug.Trim().ToLowerInvariant();
        LanguageCode primaryLanguage = ResolveRequestedLanguage(primaryMeaningLanguageCode);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        RoleplayScenario? scenario = await dbContext.RoleplayScenarios
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.Topics)
            .Where(item => item.PublicationStatus == PublicationStatus.Active && item.Slug == normalizedSlug)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (scenario is null)
        {
            return null;
        }

        Dictionary<Guid, string> topicKeysById = await LoadTopicKeysAsync(dbContext, scenario.Topics.Select(item => item.TopicId), cancellationToken)
            .ConfigureAwait(false);

        return new RoleplayScenarioDetailModel(
            scenario.Slug,
            scenario.LinkedDialogueSlug,
            scenario.Title,
            ResolveText(scenario.TitleTranslationsJson, primaryLanguage),
            scenario.Description,
            ResolveText(scenario.DescriptionTranslationsJson, primaryLanguage),
            scenario.LearnerGoal,
            ResolveText(scenario.LearnerGoalTranslationsJson, primaryLanguage),
            scenario.CefrLevel.ToString(),
            scenario.Category,
            scenario.TaskType,
            scenario.InteractionMode,
            scenario.Register,
            scenario.EstimatedPracticeMinutes,
            ResolveTopicKeys(scenario.Topics, topicKeysById),
            ReadStringArray(scenario.ExamProfilesJson),
            ReadStringArray(scenario.SkillFocusJson),
            ReadLocalizedArray<RoleplayScenarioRoleModel, StoredRole>(scenario.RolesJson, primaryLanguage, MapRole),
            ReadLocalizedArray<RoleplayScenarioTurnModel, StoredTurn>(scenario.TurnsJson, primaryLanguage, MapTurn),
            ReadAnswerGroups(scenario.AnswerChoicesJson, primaryLanguage),
            ReadLocalizedArray<RoleplayScenarioStaticFeedbackModel, StoredStaticFeedback>(scenario.StaticFeedbackJson, primaryLanguage, MapStaticFeedback),
            ReadLocalizedArray<RoleplayScenarioImageSlotModel, StoredImageSlot>(scenario.ImageSlotsJson, primaryLanguage, MapImageSlot));
    }

    private static IQueryable<RoleplayScenario> ApplyFilters(IQueryable<RoleplayScenario> query, RoleplayScenarioListFilterModel filter)
    {
        if (Enum.TryParse(filter.CefrLevel, true, out CefrLevel cefrLevel))
        {
            query = query.Where(scenario => scenario.CefrLevel == cefrLevel);
        }

        string? category = NormalizeKey(filter.Category);
        if (category is not null)
        {
            query = query.Where(scenario => scenario.Category == category);
        }

        string? taskType = NormalizeKey(filter.TaskType);
        if (taskType is not null)
        {
            query = query.Where(scenario => scenario.TaskType == taskType);
        }

        string? interactionMode = NormalizeKey(filter.InteractionMode);
        if (interactionMode is not null)
        {
            query = query.Where(scenario => scenario.InteractionMode == interactionMode);
        }

        string? register = NormalizeKey(filter.Register);
        if (register is not null)
        {
            query = query.Where(scenario => scenario.Register == register);
        }

        string? search = NormalizeSearch(filter.Query);
        if (search is not null)
        {
            query = query.Where(scenario =>
                EF.Functions.ILike(scenario.Title, $"%{search}%") ||
                EF.Functions.ILike(scenario.Description, $"%{search}%") ||
                EF.Functions.ILike(scenario.LearnerGoal, $"%{search}%") ||
                EF.Functions.ILike(scenario.Slug, $"%{search}%"));
        }

        return query;
    }

    private static async Task<Dictionary<Guid, string>> LoadTopicKeysAsync(DarwinLinguaDbContext dbContext, IEnumerable<Guid> topicIds, CancellationToken cancellationToken)
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

    private static string[] ResolveTopicKeys(IEnumerable<RoleplayScenarioTopic> topics, IReadOnlyDictionary<Guid, string> topicKeysById) =>
        topics
            .OrderByDescending(topic => topic.IsPrimary)
            .ThenBy(topic => topic.CreatedAtUtc)
            .Where(topic => topicKeysById.ContainsKey(topic.TopicId))
            .Select(topic => topicKeysById[topic.TopicId])
            .ToArray();

    private static IReadOnlyList<RoleplayScenarioAnswerChoiceGroupModel> ReadAnswerGroups(string json, LanguageCode primaryLanguage) =>
        ReadStoredArray<StoredAnswerChoiceGroup>(json)
            .OrderBy(group => group.TurnSortOrder)
            .Select(group => new RoleplayScenarioAnswerChoiceGroupModel(
                group.TurnSortOrder,
                (group.Choices ?? [])
                    .Select(choice => new RoleplayScenarioAnswerChoiceModel(
                        choice.Id ?? string.Empty,
                        choice.Text ?? string.Empty,
                        ResolveText(choice.Translations, primaryLanguage),
                        choice.IsCorrect,
                        choice.Feedback ?? string.Empty,
                        ResolveText(choice.FeedbackTranslations, primaryLanguage),
                        choice.ExplanationKey))
                    .ToArray()))
            .ToArray();

    private static IReadOnlyList<TResult> ReadLocalizedArray<TResult, TStored>(
        string json,
        LanguageCode primaryLanguage,
        Func<TStored, LanguageCode, TResult> map) =>
        ReadStoredArray<TStored>(json).Select(item => map(item, primaryLanguage)).ToArray();

    private static RoleplayScenarioRoleModel MapRole(StoredRole role, LanguageCode primaryLanguage) =>
        new(role.RoleKey ?? string.Empty, role.DisplayName ?? string.Empty, ResolveText(role.Translations, primaryLanguage));

    private static RoleplayScenarioTurnModel MapTurn(StoredTurn turn, LanguageCode primaryLanguage) =>
        new(turn.SortOrder, turn.SpeakerRole ?? string.Empty, turn.BaseText ?? string.Empty, ResolveText(turn.Translations, primaryLanguage), turn.Function, turn.ToneNote, turn.ExpectedLearnerAction);

    private static RoleplayScenarioStaticFeedbackModel MapStaticFeedback(StoredStaticFeedback feedback, LanguageCode primaryLanguage) =>
        new(feedback.TurnSortOrder, feedback.FeedbackType ?? string.Empty, feedback.Text ?? string.Empty, ResolveText(feedback.Translations, primaryLanguage));

    private static RoleplayScenarioImageSlotModel MapImageSlot(StoredImageSlot slot, LanguageCode primaryLanguage) =>
        new(slot.SlotKey ?? string.Empty, slot.Placement ?? string.Empty, slot.Purpose ?? string.Empty, slot.AltText ?? string.Empty, ResolveText(slot.AltTextTranslations, primaryLanguage), slot.ImagePrompt ?? string.Empty, slot.AssetPath, slot.IsRequired);

    private static string? ResolveText(string json, LanguageCode primaryLanguage) =>
        ResolveText(ReadStoredArray<StoredTranslation>(json), primaryLanguage);

    private static string? ResolveText(IReadOnlyList<StoredTranslation>? translations, LanguageCode primaryLanguage) =>
        (translations ?? [])
            .Select(translation => new
            {
                Translation = translation,
                Language = TryResolveLanguage(translation.Language)
            })
            .Where(item => item.Language == primaryLanguage)
            .Select(item => item.Translation.Text)
            .FirstOrDefault();

    private static LanguageCode? TryResolveLanguage(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return EnglishFallbackLanguage;
        }

        try
        {
            return LanguageCode.From(languageCode.Trim().ToLowerInvariant());
        }
        catch
        {
            return null;
        }
    }

    private static string[] ReadStringArray(string json) => ReadStoredArray<string>(json);

    private static T[] ReadStoredArray<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<T[]>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static LanguageCode ResolveRequestedLanguage(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return EnglishFallbackLanguage;
        }

        try
        {
            return LanguageCode.From(languageCode.Trim().ToLowerInvariant());
        }
        catch
        {
            return EnglishFallbackLanguage;
        }
    }

    private static string? NormalizeKey(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string? NormalizeSearch(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record StoredTranslation(string? Language, string? Text);
    private sealed record StoredRole(string? RoleKey, string? DisplayName, IReadOnlyList<StoredTranslation>? Translations);
    private sealed record StoredTurn(int SortOrder, string? SpeakerRole, string? BaseText, IReadOnlyList<StoredTranslation>? Translations, string? Function, string? ToneNote, string? ExpectedLearnerAction);
    private sealed record StoredAnswerChoiceGroup(int TurnSortOrder, IReadOnlyList<StoredAnswerChoice>? Choices);
    private sealed record StoredAnswerChoice(string? Id, string? Text, IReadOnlyList<StoredTranslation>? Translations, bool IsCorrect, string? Feedback, IReadOnlyList<StoredTranslation>? FeedbackTranslations, string? ExplanationKey);
    private sealed record StoredStaticFeedback(int TurnSortOrder, string? FeedbackType, string? Text, IReadOnlyList<StoredTranslation>? Translations);
    private sealed record StoredImageSlot(string? SlotKey, string? Placement, string? Purpose, string? AltText, IReadOnlyList<StoredTranslation>? AltTextTranslations, string? ImagePrompt, string? AssetPath, bool IsRequired);
}
