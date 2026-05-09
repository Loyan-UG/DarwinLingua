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

    Task<AdminTopicItemResponse?> MergeTopicAsync(Guid topicId, AdminMergeTopicRequest request, CancellationToken cancellationToken);

    Task<AdminLabelsResponse> GetLabelsAsync(CancellationToken cancellationToken);

    Task<AdminLabelItemResponse?> GetLabelAsync(Guid labelId, CancellationToken cancellationToken);

    Task<AdminLabelItemResponse> CreateLabelAsync(AdminSaveLabelRequest request, CancellationToken cancellationToken);

    Task<AdminLabelItemResponse?> UpdateLabelAsync(Guid labelId, AdminSaveLabelRequest request, CancellationToken cancellationToken);

    Task<AdminLabelItemResponse?> RenameLabelAsync(Guid labelId, AdminSaveLabelRequest request, CancellationToken cancellationToken);

    Task<AdminLabelItemResponse?> MergeLabelAsync(Guid labelId, AdminMergeLabelRequest request, CancellationToken cancellationToken);
}

internal sealed class AdminTaxonomyService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IAdminTaxonomyService
{
    public async Task<AdminTopicsResponse> GetTopicsAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<Guid, int> wordCountsByTopicId = await dbContext.WordTopics
            .AsNoTracking()
            .GroupBy(topic => topic.TopicId)
            .Select(group => new { TopicId = group.Key, WordCount = group.Count() })
            .ToDictionaryAsync(item => item.TopicId, item => item.WordCount, cancellationToken)
            .ConfigureAwait(false);

        Topic[] topics = await dbContext.Topics
            .AsNoTracking()
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
            .AsNoTracking()
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

    public async Task<AdminTopicItemResponse?> MergeTopicAsync(Guid topicId, AdminMergeTopicRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (topicId == Guid.Empty || request.TargetTopicId == Guid.Empty)
        {
            return null;
        }

        if (topicId == request.TargetTopicId)
        {
            throw new InvalidOperationException("Select a different target topic to merge into.");
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        Topic? source = await dbContext.Topics
            .Include(topic => topic.Localizations)
            .SingleOrDefaultAsync(item => item.Id == topicId, cancellationToken)
            .ConfigureAwait(false);
        Topic? target = await dbContext.Topics
            .Include(topic => topic.Localizations)
            .SingleOrDefaultAsync(item => item.Id == request.TargetTopicId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null || target is null)
        {
            return null;
        }

        await MergeWordTopicLinksAsync(dbContext, source.Id, target.Id, cancellationToken).ConfigureAwait(false);
        await MergeDialogueTopicLinksAsync(dbContext, source.Id, target.Id, cancellationToken).ConfigureAwait(false);
        await MergeConversationStarterTopicLinksAsync(dbContext, source.Id, target.Id, cancellationToken).ConfigureAwait(false);
        await MergeEventPreparationTopicLinksAsync(dbContext, source.Id, target.Id, cancellationToken).ConfigureAwait(false);

        dbContext.Topics.Remove(source);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        int wordCount = await dbContext.WordTopics.CountAsync(item => item.TopicId == target.Id, cancellationToken).ConfigureAwait(false);
        return MapTopic(target, new Dictionary<Guid, int> { [target.Id] = wordCount });
    }

    public async Task<AdminLabelsResponse> GetLabelsAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        await EnsureLabelDefinitionsAsync(dbContext, cancellationToken).ConfigureAwait(false);

        Dictionary<string, int> wordCountsByLabel = await LoadWordCountsByLabelAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

        LabelDefinition[] labels = await dbContext.LabelDefinitions
            .AsNoTracking()
            .Include(label => label.Localizations)
            .OrderBy(label => label.Kind)
            .ThenBy(label => label.SortOrder)
            .ThenBy(label => label.Key)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminLabelsResponse(labels.Select(label => MapLabel(label, wordCountsByLabel)).ToArray());
    }

    public async Task<AdminLabelItemResponse?> GetLabelAsync(Guid labelId, CancellationToken cancellationToken)
    {
        if (labelId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        await EnsureLabelDefinitionsAsync(dbContext, cancellationToken).ConfigureAwait(false);

        LabelDefinition? label = await dbContext.LabelDefinitions
            .AsNoTracking()
            .Include(item => item.Localizations)
            .SingleOrDefaultAsync(item => item.Id == labelId, cancellationToken)
            .ConfigureAwait(false);

        if (label is null)
        {
            return null;
        }

        int wordCount = await dbContext.WordLabels
            .Where(item => item.Kind == label.Kind && item.Key == label.Key)
            .Select(item => item.WordEntryId)
            .Distinct()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        return MapLabel(label, new Dictionary<string, int> { [CreateLabelLookupKey(label.Kind, label.Key)] = wordCount });
    }

    public async Task<AdminLabelItemResponse> CreateLabelAsync(AdminSaveLabelRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        DateTime now = DateTime.UtcNow;
        WordLabelKind kind = ParseLabelKind(request.Kind);
        LabelDefinition label = new(
            Guid.NewGuid(),
            kind,
            request.Key,
            request.DisplayName,
            request.SortOrder,
            request.IsSystem,
            now);
        ApplyLabelLocalizations(label, request.Localizations, now, requireCompleteCoverage: false);

        dbContext.LabelDefinitions.Add(label);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapLabel(label, new Dictionary<string, int>());
    }

    public async Task<AdminLabelItemResponse?> UpdateLabelAsync(Guid labelId, AdminSaveLabelRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (labelId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        LabelDefinition? label = await dbContext.LabelDefinitions
            .Include(item => item.Localizations)
            .SingleOrDefaultAsync(item => item.Id == labelId, cancellationToken)
            .ConfigureAwait(false);

        if (label is null)
        {
            return null;
        }

        WordLabelKind requestedKind = ParseLabelKind(request.Kind);
        string requestedKey = request.Key.Trim().ToLowerInvariant();
        if (requestedKind != label.Kind || !string.Equals(requestedKey, label.Key, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Label kind and key cannot be changed while labels are attached to words.");
        }

        DateTime now = DateTime.UtcNow;
        label.UpdateMetadata(request.DisplayName, request.SortOrder, request.IsSystem, now);
        ApplyLabelLocalizations(label, request.Localizations, now, requireCompleteCoverage: false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        int wordCount = await dbContext.WordLabels
            .Where(item => item.Kind == label.Kind && item.Key == label.Key)
            .Select(item => item.WordEntryId)
            .Distinct()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        return MapLabel(label, new Dictionary<string, int> { [CreateLabelLookupKey(label.Kind, label.Key)] = wordCount });
    }

    public async Task<AdminLabelItemResponse?> RenameLabelAsync(Guid labelId, AdminSaveLabelRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (labelId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        LabelDefinition? label = await dbContext.LabelDefinitions
            .Include(item => item.Localizations)
            .SingleOrDefaultAsync(item => item.Id == labelId, cancellationToken)
            .ConfigureAwait(false);

        if (label is null)
        {
            return null;
        }

        WordLabelKind newKind = ParseLabelKind(request.Kind);
        string newKey = NormalizeLabelKey(request.Key);
        if ((label.Kind != newKind || !string.Equals(label.Key, newKey, StringComparison.Ordinal)) &&
            await dbContext.LabelDefinitions.AnyAsync(item => item.Kind == newKind && item.Key == newKey, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException("Another label with the selected kind and key already exists. Use merge instead.");
        }

        WordLabel[] attachedLabels = await dbContext.WordLabels
            .Where(item => item.Kind == label.Kind && item.Key == label.Key)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        ReassignWordLabels(attachedLabels, newKind, newKey, await LoadMaxSortOrdersByWordAsync(dbContext, newKind, attachedLabels, cancellationToken).ConfigureAwait(false));

        DateTime now = DateTime.UtcNow;
        label.Rename(newKind, newKey, request.DisplayName, request.SortOrder, request.IsSystem, now);
        ApplyLabelLocalizations(label, request.Localizations, now, requireCompleteCoverage: false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        int wordCount = attachedLabels.Select(item => item.WordEntryId).Distinct().Count();
        return MapLabel(label, new Dictionary<string, int> { [CreateLabelLookupKey(label.Kind, label.Key)] = wordCount });
    }

    public async Task<AdminLabelItemResponse?> MergeLabelAsync(Guid labelId, AdminMergeLabelRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (labelId == Guid.Empty || request.TargetLabelId == Guid.Empty)
        {
            return null;
        }

        if (labelId == request.TargetLabelId)
        {
            throw new InvalidOperationException("Select a different target label to merge into.");
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        LabelDefinition? source = await dbContext.LabelDefinitions
            .Include(item => item.Localizations)
            .SingleOrDefaultAsync(item => item.Id == labelId, cancellationToken)
            .ConfigureAwait(false);
        LabelDefinition? target = await dbContext.LabelDefinitions
            .Include(item => item.Localizations)
            .SingleOrDefaultAsync(item => item.Id == request.TargetLabelId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null || target is null)
        {
            return null;
        }

        WordLabel[] sourceLabels = await dbContext.WordLabels
            .Where(item => item.Kind == source.Kind && item.Key == source.Key)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        HashSet<Guid> targetWordIds = await dbContext.WordLabels
            .Where(item => item.Kind == target.Kind && item.Key == target.Key)
            .Select(item => item.WordEntryId)
            .ToHashSetAsync(cancellationToken)
            .ConfigureAwait(false);

        WordLabel[] duplicateLabels = sourceLabels
            .Where(label => targetWordIds.Contains(label.WordEntryId))
            .ToArray();
        WordLabel[] labelsToMove = sourceLabels
            .Where(label => !targetWordIds.Contains(label.WordEntryId))
            .ToArray();

        if (duplicateLabels.Length > 0)
        {
            dbContext.WordLabels.RemoveRange(duplicateLabels);
        }

        ReassignWordLabels(labelsToMove, target.Kind, target.Key, await LoadMaxSortOrdersByWordAsync(dbContext, target.Kind, labelsToMove, cancellationToken).ConfigureAwait(false));
        dbContext.LabelDefinitions.Remove(source);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        int wordCount = await dbContext.WordLabels
            .AsNoTracking()
            .Where(item => item.Kind == target.Kind && item.Key == target.Key)
            .Select(item => item.WordEntryId)
            .Distinct()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        return MapLabel(target, new Dictionary<string, int> { [CreateLabelLookupKey(target.Kind, target.Key)] = wordCount });
    }

    private static async Task EnsureLabelDefinitionsAsync(DarwinLinguaDbContext dbContext, CancellationToken cancellationToken)
    {
        var attachedLabels = await dbContext.WordLabels
            .GroupBy(label => new { label.Kind, label.Key })
            .Select(group => new
            {
                group.Key.Kind,
                group.Key.Key,
                SortOrder = group.Min(label => label.SortOrder),
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        if (attachedLabels.Length == 0)
        {
            return;
        }

        LabelDefinition[] existingLabels = await dbContext.LabelDefinitions
            .AsNoTracking()
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        HashSet<string> existingKeys = existingLabels
            .Select(label => CreateLabelLookupKey(label.Kind, label.Key))
            .ToHashSet(StringComparer.Ordinal);

        DateTime now = DateTime.UtcNow;
        bool hasNewLabels = false;
        foreach (var attachedLabel in attachedLabels)
        {
            if (existingKeys.Contains(CreateLabelLookupKey(attachedLabel.Kind, attachedLabel.Key)))
            {
                continue;
            }

            dbContext.LabelDefinitions.Add(new LabelDefinition(
                Guid.NewGuid(),
                attachedLabel.Kind,
                attachedLabel.Key,
                HumanizeLabelKey(attachedLabel.Key),
                attachedLabel.SortOrder,
                true,
                now));
            hasNewLabels = true;
        }

        if (hasNewLabels)
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
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

            LanguageCode languageCode = LanguageCode.From(localization.LanguageCode);
            Guid localizationId = topic.Localizations.Any(item => item.LanguageCode == languageCode)
                ? Guid.Empty
                : Guid.NewGuid();

            topic.AddOrUpdateLocalization(
                localizationId,
                languageCode,
                localization.DisplayName,
                now);
        }
    }

    private static async Task MergeWordTopicLinksAsync(DarwinLinguaDbContext dbContext, Guid sourceTopicId, Guid targetTopicId, CancellationToken cancellationToken)
    {
        WordTopic[] sourceLinks = await dbContext.WordTopics.Where(link => link.TopicId == sourceTopicId).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        HashSet<Guid> targetOwnerIds = await dbContext.WordTopics.Where(link => link.TopicId == targetTopicId).Select(link => link.WordEntryId).ToHashSetAsync(cancellationToken).ConfigureAwait(false);
        ReassignOrRemoveTopicLinks(sourceLinks, targetOwnerIds, link => link.WordEntryId, link => link.ReassignTopic(targetTopicId), dbContext.WordTopics);
    }

    private static async Task MergeDialogueTopicLinksAsync(DarwinLinguaDbContext dbContext, Guid sourceTopicId, Guid targetTopicId, CancellationToken cancellationToken)
    {
        DialogueLessonTopic[] sourceLinks = await dbContext.DialogueLessonTopics.Where(link => link.TopicId == sourceTopicId).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        HashSet<Guid> targetOwnerIds = await dbContext.DialogueLessonTopics.Where(link => link.TopicId == targetTopicId).Select(link => link.DialogueLessonId).ToHashSetAsync(cancellationToken).ConfigureAwait(false);
        ReassignOrRemoveTopicLinks(sourceLinks, targetOwnerIds, link => link.DialogueLessonId, link => link.ReassignTopic(targetTopicId), dbContext.DialogueLessonTopics);
    }

    private static async Task MergeConversationStarterTopicLinksAsync(DarwinLinguaDbContext dbContext, Guid sourceTopicId, Guid targetTopicId, CancellationToken cancellationToken)
    {
        ConversationStarterPackTopic[] sourceLinks = await dbContext.ConversationStarterPackTopics.Where(link => link.TopicId == sourceTopicId).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        HashSet<Guid> targetOwnerIds = await dbContext.ConversationStarterPackTopics.Where(link => link.TopicId == targetTopicId).Select(link => link.ConversationStarterPackId).ToHashSetAsync(cancellationToken).ConfigureAwait(false);
        ReassignOrRemoveTopicLinks(sourceLinks, targetOwnerIds, link => link.ConversationStarterPackId, link => link.ReassignTopic(targetTopicId), dbContext.ConversationStarterPackTopics);
    }

    private static async Task MergeEventPreparationTopicLinksAsync(DarwinLinguaDbContext dbContext, Guid sourceTopicId, Guid targetTopicId, CancellationToken cancellationToken)
    {
        EventPreparationPackTopic[] sourceLinks = await dbContext.EventPreparationPackTopics.Where(link => link.TopicId == sourceTopicId).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        HashSet<Guid> targetOwnerIds = await dbContext.EventPreparationPackTopics.Where(link => link.TopicId == targetTopicId).Select(link => link.EventPreparationPackId).ToHashSetAsync(cancellationToken).ConfigureAwait(false);
        ReassignOrRemoveTopicLinks(sourceLinks, targetOwnerIds, link => link.EventPreparationPackId, link => link.ReassignTopic(targetTopicId), dbContext.EventPreparationPackTopics);
    }

    private static void ReassignOrRemoveTopicLinks<TLink>(
        IReadOnlyCollection<TLink> sourceLinks,
        HashSet<Guid> targetOwnerIds,
        Func<TLink, Guid> ownerIdSelector,
        Action<TLink> reassignTopic,
        DbSet<TLink> dbSet)
        where TLink : class
    {
        foreach (TLink link in sourceLinks)
        {
            if (targetOwnerIds.Contains(ownerIdSelector(link)))
            {
                dbSet.Remove(link);
                continue;
            }

            reassignTopic(link);
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

    private static AdminLabelItemResponse MapLabel(LabelDefinition label, IReadOnlyDictionary<string, int> wordCountsByLabel) =>
        new(
            label.Id,
            label.Kind.ToString(),
            label.Key,
            label.DisplayName,
            label.Localizations
                .OrderBy(localization => localization.LanguageCode.Value)
                .Select(localization => new AdminLabelLocalizationResponse(
                    localization.LanguageCode.Value,
                    localization.DisplayName))
                .ToArray(),
            label.SortOrder,
            label.IsSystem,
            wordCountsByLabel.GetValueOrDefault(CreateLabelLookupKey(label.Kind, label.Key)),
            label.UpdatedAtUtc);

    private static void ApplyLabelLocalizations(
        LabelDefinition label,
        IReadOnlyList<AdminLabelLocalizationRequest>? localizations,
        DateTime now,
        bool requireCompleteCoverage)
    {
        if (localizations is null || localizations.Count == 0)
        {
            if (requireCompleteCoverage)
            {
                throw new InvalidOperationException(
                    $"Label localizations are required for every language: {ContentLanguageRequirements.FormatRequiredLocalizationLanguages()}.");
            }

            return;
        }

        if (requireCompleteCoverage)
        {
            IReadOnlyList<string> missing = ContentLanguageRequirements.FindMissingLocalizationLanguages(
                localizations.Select(localization => localization.LanguageCode));
            if (missing.Count > 0)
            {
                throw new InvalidOperationException($"Label localizations are missing languages: {string.Join(", ", missing)}.");
            }
        }

        foreach (AdminLabelLocalizationRequest localization in localizations)
        {
            if (string.IsNullOrWhiteSpace(localization.LanguageCode) || string.IsNullOrWhiteSpace(localization.DisplayName))
            {
                continue;
            }

            LanguageCode languageCode = LanguageCode.From(localization.LanguageCode);
            Guid localizationId = label.Localizations.Any(item => item.LanguageCode == languageCode)
                ? Guid.Empty
                : Guid.NewGuid();

            label.AddOrUpdateLocalization(
                localizationId,
                languageCode,
                localization.DisplayName,
                now);
        }
    }

    private static async Task<Dictionary<string, int>> LoadWordCountsByLabelAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var wordCounts = await dbContext.WordLabels
            .GroupBy(label => new { label.Kind, label.Key })
            .Select(group => new
            {
                group.Key.Kind,
                group.Key.Key,
                WordCount = group.Select(label => label.WordEntryId).Distinct().Count(),
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return wordCounts.ToDictionary(
            item => CreateLabelLookupKey(item.Kind, item.Key),
            item => item.WordCount,
            StringComparer.Ordinal);
    }

    private static WordLabelKind ParseLabelKind(string value)
    {
        if (Enum.TryParse(value, ignoreCase: true, out WordLabelKind kind) &&
            Enum.IsDefined(kind))
        {
            return kind;
        }

        throw new InvalidOperationException($"'{value}' is not a supported label kind.");
    }

    private static string NormalizeLabelKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Label key cannot be empty.");
        }

        return value.Trim().ToLowerInvariant();
    }

    private static async Task<Dictionary<Guid, int>> LoadMaxSortOrdersByWordAsync(
        DarwinLinguaDbContext dbContext,
        WordLabelKind targetKind,
        IReadOnlyCollection<WordLabel> movingLabels,
        CancellationToken cancellationToken)
    {
        if (movingLabels.Count == 0)
        {
            return [];
        }

        Guid[] wordEntryIds = movingLabels.Select(label => label.WordEntryId).Distinct().ToArray();
        Guid[] movingLabelIds = movingLabels.Select(label => label.Id).ToArray();
        var rows = await dbContext.WordLabels
            .AsNoTracking()
            .Where(label =>
                wordEntryIds.Contains(label.WordEntryId) &&
                label.Kind == targetKind &&
                !movingLabelIds.Contains(label.Id))
            .GroupBy(label => label.WordEntryId)
            .Select(group => new { WordEntryId = group.Key, MaxSortOrder = group.Max(label => label.SortOrder) })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.ToDictionary(row => row.WordEntryId, row => row.MaxSortOrder);
    }

    private static void ReassignWordLabels(
        IReadOnlyCollection<WordLabel> labels,
        WordLabelKind targetKind,
        string targetKey,
        Dictionary<Guid, int> maxSortOrdersByWord)
    {
        foreach (WordLabel label in labels)
        {
            int sortOrder = label.Kind == targetKind
                ? label.SortOrder
                : maxSortOrdersByWord.GetValueOrDefault(label.WordEntryId) + 1;
            maxSortOrdersByWord[label.WordEntryId] = Math.Max(maxSortOrdersByWord.GetValueOrDefault(label.WordEntryId), sortOrder);
            label.Reassign(targetKind, targetKey, sortOrder);
        }
    }

    private static string CreateLabelLookupKey(WordLabelKind kind, string key) =>
        string.Concat(kind.ToString(), "::", key);

    private static string HumanizeLabelKey(string key) =>
        string.Join(
            ' ',
            key.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(static part => part.Length == 0 ? part : string.Concat(char.ToUpperInvariant(part[0]), part[1..])));
}
