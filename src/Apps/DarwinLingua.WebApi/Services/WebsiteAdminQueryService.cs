using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.ContentOps.Domain.Enums;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text.Json;

namespace DarwinLingua.WebApi.Services;

public interface IWebsiteAdminQueryService
{
    Task<AdminCatalogDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken);

    Task<AdminSystemReportResponse> GetSystemReportAsync(CancellationToken cancellationToken);

    Task<AdminCatalogImportsResponse> GetImportsAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminCatalogWordsResponse> GetWordsAsync(
        string? query,
        string? statusFilter,
        string? sort,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<AdminCatalogWordDetailResponse?> GetWordAsync(Guid publicId, CancellationToken cancellationToken);

    Task<AdminCatalogDraftWordsResponse> GetDraftWordsAsync(string? query, CancellationToken cancellationToken);

    Task<AdminCatalogHistoryViewResponse> GetHistoryAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminCatalogRollbackPreviewResponse> GetRollbackPreviewAsync(CancellationToken cancellationToken);
}

public sealed class WebsiteAdminQueryService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IWebsiteAdminQueryService
{
    private static readonly IReadOnlySet<string> EmptyStringSet = new HashSet<string>(StringComparer.Ordinal);

    public async Task<AdminCatalogDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken)
    {
        Task<WordEntrySummary> wordSummaryTask = QueryAsync(GetWordEntrySummaryAsync, cancellationToken);
        Task<int> topicCountTask = QueryAsync(
            static async (dbContext, token) => await dbContext.Topics
                .AsNoTracking()
                .CountAsync(token)
                .ConfigureAwait(false),
            cancellationToken);
        Task<ContentPackageSummary> packageSummaryTask = QueryAsync(GetContentPackageSummaryAsync, cancellationToken);

        await Task.WhenAll(wordSummaryTask, topicCountTask, packageSummaryTask).ConfigureAwait(false);

        WordEntrySummary wordSummary = await wordSummaryTask.ConfigureAwait(false);
        int topicCount = await topicCountTask.ConfigureAwait(false);
        ContentPackageSummary packageSummary = await packageSummaryTask.ConfigureAwait(false);

        return new AdminCatalogDashboardResponse(
            wordSummary.ActiveCount,
            wordSummary.DraftCount,
            topicCount,
            packageSummary.ImportedCount,
            packageSummary.FailedCount,
            packageSummary.LastImportAtUtc);
    }

    public async Task<AdminSystemReportResponse> GetSystemReportAsync(CancellationToken cancellationToken)
    {
        Task<AdminCatalogSystemReportResponse> catalogTask = GetCatalogSystemReportAsync(cancellationToken);
        Task<AdminSocialSystemReportResponse> socialTask = GetSocialSystemReportAsync(cancellationToken);
        Task<AdminModerationSystemReportResponse> moderationTask = GetModerationSystemReportAsync(cancellationToken);
        Task<AdminOperationsSystemReportResponse> operationsTask = GetOperationsSystemReportAsync(cancellationToken);
        Task<AdminLearningPortalSystemReportResponse> learningPortalTask = GetLearningPortalSystemReportAsync(cancellationToken);

        await Task.WhenAll(catalogTask, socialTask, moderationTask, operationsTask, learningPortalTask).ConfigureAwait(false);

        return new AdminSystemReportResponse(
            DateTime.UtcNow,
            await catalogTask.ConfigureAwait(false),
            await socialTask.ConfigureAwait(false),
            await moderationTask.ConfigureAwait(false),
            await operationsTask.ConfigureAwait(false),
            await learningPortalTask.ConfigureAwait(false));
    }

    public async Task<AdminCatalogImportsResponse> GetImportsAsync(string? statusFilter, CancellationToken cancellationToken)
    {
        string? normalizedStatusFilter = NormalizeFilter(statusFilter);
        if (!TryParseContentPackageStatusFilter(normalizedStatusFilter, out ContentPackageStatus? parsedStatusFilter))
        {
            return new AdminCatalogImportsResponse(normalizedStatusFilter, []);
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        IQueryable<ContentOps.Domain.Entities.ContentPackage> query = dbContext.ContentPackages.AsNoTracking();

        if (parsedStatusFilter is not null)
        {
            query = query.Where(package => package.Status == parsedStatusFilter);
        }

        AdminCatalogImportItemResponse[] packages = await query
            .OrderByDescending(package => package.CreatedAtUtc)
            .Take(40)
            .Select(package => new AdminCatalogImportItemResponse(
                package.PackageId,
                package.PackageVersion,
                package.PackageName,
                package.SourceType.ToString(),
                package.Status.ToString(),
                package.TotalEntries,
                package.InsertedEntries,
                package.InvalidEntries,
                package.WarningCount,
                package.CreatedAtUtc))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminCatalogImportsResponse(normalizedStatusFilter, packages);
    }

    public async Task<AdminCatalogWordsResponse> GetWordsAsync(
        string? queryText,
        string? statusFilter,
        string? sort,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        string? normalizedQuery = NormalizeFilter(queryText);
        string? normalizedStatusFilter = NormalizeFilter(statusFilter);
        string normalizedSort = NormalizeWordSort(sort);
        int normalizedSkip = Math.Max(0, skip);
        int normalizedTake = Math.Clamp(take, 10, 100);

        if (!TryParsePublicationStatusFilter(normalizedStatusFilter, out PublicationStatus? parsedStatusFilter))
        {
            return new AdminCatalogWordsResponse(
                normalizedQuery,
                normalizedStatusFilter,
                normalizedSort,
                normalizedSkip,
                normalizedTake,
                0,
                []);
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        IQueryable<Catalog.Domain.Entities.WordEntry> query = dbContext.WordEntries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            string lowered = normalizedQuery.ToLowerInvariant();
            query = query.Where(word => word.NormalizedLemma.Contains(lowered));
        }

        if (parsedStatusFilter is not null)
        {
            query = query.Where(word => word.PublicationStatus == parsedStatusFilter);
        }

        int totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        query = normalizedSort switch
        {
            "updated" => query.OrderByDescending(word => word.UpdatedAtUtc).ThenBy(word => word.NormalizedLemma),
            "cefr" => query.OrderBy(word => word.PrimaryCefrLevel).ThenBy(word => word.NormalizedLemma),
            "status" => query.OrderBy(word => word.PublicationStatus).ThenBy(word => word.NormalizedLemma),
            _ => query.OrderBy(word => word.NormalizedLemma)
        };

        AdminCatalogWordItemResponse[] words = await query
            .Skip(normalizedSkip)
            .Take(normalizedTake)
            .Select(word => new AdminCatalogWordItemResponse(
                word.PublicId,
                word.Lemma,
                word.Article,
                word.PartOfSpeech.ToString(),
                word.PrimaryCefrLevel.ToString(),
                word.PublicationStatus.ToString(),
                word.ContentSourceType.ToString(),
                word.Senses.Count,
                word.Topics.Count,
                word.UpdatedAtUtc))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminCatalogWordsResponse(
            normalizedQuery,
            normalizedStatusFilter,
            normalizedSort,
            normalizedSkip,
            normalizedTake,
            totalCount,
            words);
    }

    public async Task<AdminCatalogWordDetailResponse?> GetWordAsync(Guid publicId, CancellationToken cancellationToken)
    {
        if (publicId == Guid.Empty)
        {
            return null;
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        Catalog.Domain.Entities.WordEntry? word = await dbContext.WordEntries
            .AsNoTracking()
            .AsSplitQuery()
            .Include(entry => entry.LexicalForms)
            .Include(entry => entry.Senses)
                .ThenInclude(sense => sense.Translations)
            .Include(entry => entry.Senses)
                .ThenInclude(sense => sense.Examples)
                    .ThenInclude(example => example.Translations)
            .Include(entry => entry.Topics)
            .Include(entry => entry.Labels)
            .Include(entry => entry.GrammarNotes)
            .Include(entry => entry.Collocations)
            .SingleOrDefaultAsync(entry => entry.PublicId == publicId, cancellationToken)
            .ConfigureAwait(false);

        if (word is null)
        {
            return null;
        }

        Guid[] topicIds = word.Topics.Select(topic => topic.TopicId).Distinct().ToArray();
        Dictionary<Guid, string> topicKeysById = topicIds.Length == 0
            ? []
            : await dbContext.Topics
                .AsNoTracking()
                .Where(topic => topicIds.Contains(topic.Id))
                .ToDictionaryAsync(topic => topic.Id, topic => topic.Key, cancellationToken)
                .ConfigureAwait(false);

        WordLabelKind[] labelKinds = word.Labels.Select(label => label.Kind).Distinct().ToArray();
        LabelDefinition[] labelDefinitions = labelKinds.Length == 0
            ? []
            : await dbContext.LabelDefinitions
            .AsNoTracking()
            .Where(label => labelKinds.Contains(label.Kind))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<string, string> labelDisplayNamesByKey = labelDefinitions.ToDictionary(
            label => CreateLabelLookupKey(label.Kind, label.Key),
            label => label.DisplayName,
            StringComparer.Ordinal);

        return new AdminCatalogWordDetailResponse(
            word.PublicId,
            word.Lemma,
            word.NormalizedLemma,
            word.LanguageCode.Value,
            word.Article,
            word.PluralForm,
            word.InfinitiveForm,
            word.PronunciationIpa,
            word.SyllableBreak,
            word.PartOfSpeech.ToString(),
            word.PrimaryCefrLevel.ToString(),
            word.PublicationStatus.ToString(),
            word.ContentSourceType.ToString(),
            word.SourceReference,
            word.CreatedAtUtc,
            word.UpdatedAtUtc,
            word.LexicalForms
                .OrderBy(form => form.SortOrder)
                .Select(form => new AdminCatalogWordLexicalFormResponse(
                    form.PartOfSpeech.ToString(),
                    form.Article,
                    form.PluralForm,
                    form.InfinitiveForm,
                    form.IsPrimary,
                    form.SortOrder))
                .ToArray(),
            word.Senses
                .OrderBy(sense => sense.SenseOrder)
                .Select(sense => new AdminCatalogWordSenseResponse(
                    sense.Id,
                    sense.SenseOrder,
                    sense.IsPrimarySense,
                    sense.PublicationStatus.ToString(),
                    sense.ShortDefinitionDe,
                    sense.ShortGloss,
                    sense.Translations
                        .OrderByDescending(translation => translation.IsPrimary)
                        .ThenBy(translation => translation.LanguageCode.Value)
                        .Select(translation => new AdminCatalogWordTranslationResponse(
                            translation.Id,
                            translation.LanguageCode.Value,
                            translation.TranslationText,
                            translation.IsPrimary))
                        .ToArray(),
                    sense.Examples
                        .OrderBy(example => example.SentenceOrder)
                        .Select(example => new AdminCatalogWordExampleResponse(
                            example.Id,
                            example.SentenceOrder,
                            example.GermanText,
                            example.IsPrimaryExample,
                            example.Translations
                                .OrderBy(translation => translation.LanguageCode.Value)
                                .Select(translation => new AdminCatalogWordExampleTranslationResponse(
                                    translation.Id,
                                    translation.LanguageCode.Value,
                                    translation.TranslationText))
                                .ToArray()))
                        .ToArray()))
                .ToArray(),
            word.Topics
                .OrderByDescending(topic => topic.IsPrimaryTopic)
                .ThenBy(topic => topicKeysById.GetValueOrDefault(topic.TopicId, topic.TopicId.ToString("D")))
                .Select(topic => new AdminCatalogWordTopicResponse(
                    topic.TopicId,
                    topicKeysById.GetValueOrDefault(topic.TopicId, topic.TopicId.ToString("D")),
                    topic.IsPrimaryTopic))
                .ToArray(),
            word.Labels
                .OrderBy(label => label.Kind)
                .ThenBy(label => label.SortOrder)
                .Select(label => new AdminCatalogWordLabelResponse(
                    label.Kind.ToString(),
                    label.Key,
                    labelDisplayNamesByKey.GetValueOrDefault(CreateLabelLookupKey(label.Kind, label.Key), HumanizeLabelKey(label.Key)),
                    label.SortOrder))
                .ToArray(),
            word.GrammarNotes
                .OrderBy(note => note.SortOrder)
                .Select(note => new AdminCatalogWordTextItemResponse(note.Text, note.SortOrder))
                .ToArray(),
            word.Collocations
                .OrderBy(collocation => collocation.SortOrder)
                .Select(collocation => new AdminCatalogWordCollocationResponse(
                    collocation.Text,
                    collocation.Meaning,
                    collocation.SortOrder))
                .ToArray());
    }

    private static string CreateLabelLookupKey(WordLabelKind kind, string key) =>
        string.Concat(kind.ToString(), "::", key);

    private static string HumanizeLabelKey(string key) =>
        string.Join(
            ' ',
            key.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(static part => part.Length == 0 ? part : string.Concat(char.ToUpperInvariant(part[0]), part[1..])));

    public async Task<AdminCatalogDraftWordsResponse> GetDraftWordsAsync(string? queryText, CancellationToken cancellationToken)
    {
        string? normalizedQuery = NormalizeFilter(queryText);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        IQueryable<Catalog.Domain.Entities.WordEntry> query = dbContext.WordEntries
            .AsNoTracking()
            .Where(word => word.PublicationStatus == PublicationStatus.Draft);

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            string lowered = normalizedQuery.ToLowerInvariant();
            query = query.Where(word => word.NormalizedLemma.Contains(lowered));
        }

        AdminCatalogDraftWordItemResponse[] words = await query
            .OrderBy(word => word.NormalizedLemma)
            .Take(50)
            .Select(word => new AdminCatalogDraftWordItemResponse(
                word.PublicId,
                word.Lemma,
                word.PartOfSpeech.ToString(),
                word.PrimaryCefrLevel.ToString(),
                word.PublicationStatus.ToString()))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminCatalogDraftWordsResponse(normalizedQuery, words);
    }

    public async Task<AdminCatalogHistoryViewResponse> GetHistoryAsync(string? statusFilter, CancellationToken cancellationToken)
    {
        string? normalizedStatusFilter = NormalizeFilter(statusFilter);
        if (!TryParseContentPackageStatusFilter(normalizedStatusFilter, out ContentPackageStatus? parsedStatusFilter))
        {
            return new AdminCatalogHistoryViewResponse(normalizedStatusFilter, []);
        }

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        IQueryable<ContentOps.Domain.Entities.ContentPackage> query = dbContext.ContentPackages.AsNoTracking();

        if (parsedStatusFilter is not null)
        {
            query = query.Where(package => package.Status == parsedStatusFilter);
        }

        AdminCatalogHistoryItemResponse[] items = await query
            .OrderByDescending(package => package.CreatedAtUtc)
            .Take(30)
            .Select(package => new AdminCatalogHistoryItemResponse(
                package.PackageId,
                package.PackageVersion,
                package.Status.ToString(),
                package.TotalEntries,
                package.InsertedEntries,
                package.InvalidEntries,
                package.CreatedAtUtc))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminCatalogHistoryViewResponse(normalizedStatusFilter, items);
    }

    public async Task<AdminCatalogRollbackPreviewResponse> GetRollbackPreviewAsync(CancellationToken cancellationToken)
    {
        Task<int> draftWordCountTask = QueryAsync(
            static async (dbContext, token) => await dbContext.WordEntries
                .AsNoTracking()
                .CountAsync(word => word.PublicationStatus == PublicationStatus.Draft, token)
                .ConfigureAwait(false),
            cancellationToken);
        Task<int> importedPackageCountTask = QueryAsync(
            static async (dbContext, token) => await dbContext.ContentPackages
                .AsNoTracking()
                .CountAsync(token)
                .ConfigureAwait(false),
            cancellationToken);

        await Task.WhenAll(draftWordCountTask, importedPackageCountTask).ConfigureAwait(false);

        return new AdminCatalogRollbackPreviewResponse(
            await draftWordCountTask.ConfigureAwait(false),
            await importedPackageCountTask.ConfigureAwait(false),
            "Rollback actions are intentionally gated behind an explicit confirmation step. This screen is the approved modal baseline before destructive operations are implemented.");
    }

    private static string? NormalizeFilter(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeWordSort(string? value)
    {
        string? normalized = NormalizeFilter(value)?.ToLowerInvariant();
        return normalized is "updated" or "cefr" or "status" ? normalized : "lemma";
    }

    private static bool TryParseContentPackageStatusFilter(
        string? statusFilter,
        out ContentPackageStatus? parsedStatusFilter)
    {
        if (string.IsNullOrWhiteSpace(statusFilter))
        {
            parsedStatusFilter = null;
            return true;
        }

        if (Enum.TryParse(statusFilter, ignoreCase: true, out ContentPackageStatus status) &&
            Enum.IsDefined(status))
        {
            parsedStatusFilter = status;
            return true;
        }

        parsedStatusFilter = null;
        return false;
    }

    private static bool TryParsePublicationStatusFilter(
        string? statusFilter,
        out PublicationStatus? parsedStatusFilter)
    {
        if (string.IsNullOrWhiteSpace(statusFilter))
        {
            parsedStatusFilter = null;
            return true;
        }

        if (Enum.TryParse(statusFilter, ignoreCase: true, out PublicationStatus status) &&
            Enum.IsDefined(status))
        {
            parsedStatusFilter = status;
            return true;
        }

        parsedStatusFilter = null;
        return false;
    }

    private async Task<T> QueryAsync<T>(
        Func<DarwinLinguaDbContext, CancellationToken, Task<T>> query,
        CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        return await query(dbContext, cancellationToken).ConfigureAwait(false);
    }

    private async Task<AdminCatalogSystemReportResponse> GetCatalogSystemReportAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordEntrySummary wordSummary = await GetWordEntrySummaryAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

        int topicCount = await dbContext.Topics.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        int dialogueLessonCount = await dbContext.DialogueLessons.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        int conversationStarterPackCount = await dbContext.ConversationStarterPacks.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        int eventPreparationPackCount = await dbContext.EventPreparationPacks.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminCatalogSystemReportResponse(
            wordSummary.ActiveCount,
            wordSummary.DraftCount,
            topicCount,
            dialogueLessonCount,
            conversationStarterPackCount,
            eventPreparationPackCount);
    }

    private async Task<AdminSocialSystemReportResponse> GetSocialSystemReportAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        OrganizerProfileSummary organizerProfileSummary = await GetOrganizerProfileSummaryAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

        ConversationEventSummary conversationEventSummary = await GetConversationEventSummaryAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

        int eventRsvpCount = await dbContext.EventRsvps.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        OrganizerClaimRequestSummary organizerClaimRequestSummary = await GetOrganizerClaimRequestSummaryAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

        int organizerProfileOwnerCount = await dbContext.OrganizerProfileOwners.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        LearnerConversationProfileSummary learnerConversationProfileSummary = await GetLearnerConversationProfileSummaryAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

        PartnerRequestSummary partnerRequestSummary = await GetPartnerRequestSummaryAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

        return new AdminSocialSystemReportResponse(
            organizerProfileSummary.TotalCount,
            conversationEventSummary.TotalCount,
            conversationEventSummary.OnlineCount,
            eventRsvpCount,
            organizerClaimRequestSummary.TotalCount,
            organizerClaimRequestSummary.PendingCount,
            organizerProfileOwnerCount,
            learnerConversationProfileSummary.TotalCount,
            learnerConversationProfileSummary.PublicCount,
            partnerRequestSummary.TotalCount,
            partnerRequestSummary.PendingCount);
    }

    private async Task<AdminModerationSystemReportResponse> GetModerationSystemReportAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        UserReportSummary userReportSummary = await GetUserReportSummaryAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

        int userBlockCount = await dbContext.UserBlocks.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        int moderationDecisionAuditCount = await dbContext.ModerationDecisionAudits.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminModerationSystemReportResponse(
            userReportSummary.TotalCount,
            userReportSummary.PendingCount,
            userBlockCount,
            moderationDecisionAuditCount);
    }

    private async Task<AdminOperationsSystemReportResponse> GetOperationsSystemReportAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        ContentPackageSummary packageSummary = await GetContentPackageSummaryAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

        return new AdminOperationsSystemReportResponse(
            packageSummary.ImportedCount,
            packageSummary.FailedCount,
            packageSummary.LastImportAtUtc);
    }

    private async Task<AdminLearningPortalSystemReportResponse> GetLearningPortalSystemReportAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyDictionary<string, bool> tableAvailability = await GetLearningPortalTableAvailabilityAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

        List<AdminLearningPortalCountRowResponse> countsByType = await GetLearningContentCountsByTypeAsync(dbContext, tableAvailability, cancellationToken)
            .ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> countsByCefr = await GetLearningContentCountsByCefrAsync(dbContext, tableAvailability, cancellationToken)
            .ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> grammarByCategory = await CountByIfTableExistsAsync(
            tableAvailability,
            "GrammarTopics",
            dbContext.GrammarTopics.AsNoTracking(),
            topic => topic.GrammarCategory,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> expressionsByType = await CountByIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking(),
            expression => expression.ExpressionType,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> expressionsByRegister = await CountByIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking(),
            expression => expression.Register,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> expressionsByMeaningTransparency = await CountByIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking(),
            expression => expression.MeaningTransparency ?? "missing",
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> expressionsBySafetyRating = await CountByIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking(),
            expression => expression.SafetyRating,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> expressionsBySensitiveContentKind = await CountByIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking(),
            expression => expression.SensitiveContentKind,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> expressionsByUsagePolicy = await CountByIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking(),
            expression => expression.UsagePolicy,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> exercisesByType = await CountByIfTableExistsAsync(
            tableAvailability,
            "Exercises",
            dbContext.Exercises.AsNoTracking(),
            exercise => exercise.ExerciseType,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> exercisesBySkill = await CountByIfTableExistsAsync(
            tableAvailability,
            "Exercises",
            dbContext.Exercises.AsNoTracking(),
            exercise => exercise.TargetSkill,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> lessonsByCourse = await CountByIfTableExistsAsync(
            tableAvailability,
            "CourseLessons",
            dbContext.CourseLessons.AsNoTracking(),
            lesson => lesson.CoursePathSlug,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> lessonsByCefr = await CountValueByIfTableExistsAsync(
            tableAvailability,
            "CourseLessons",
            dbContext.CourseLessons.AsNoTracking(),
            lesson => lesson.CefrLevel,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> lessonsByModule = await CountByIfTableExistsAsync(
            tableAvailability,
            "CourseLessons",
            dbContext.CourseLessons.AsNoTracking(),
            lesson => lesson.ModuleSlug,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> examByProfile = await CountByIfTableExistsAsync(
            tableAvailability,
            "ExamPrepUnits",
            dbContext.ExamPrepUnits.AsNoTracking(),
            unit => unit.ExamProfileKey,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> writingByCategory = await CountByIfTableExistsAsync(
            tableAvailability,
            "WritingTemplates",
            dbContext.WritingTemplates.AsNoTracking(),
            template => template.Category,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> writingByRegister = await CountByIfTableExistsAsync(
            tableAvailability,
            "WritingTemplates",
            dbContext.WritingTemplates.AsNoTracking(),
            template => template.Register,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> culturalByCategory = await CountByIfTableExistsAsync(
            tableAvailability,
            "CulturalNotes",
            dbContext.CulturalNotes.AsNoTracking(),
            note => note.Category,
            cancellationToken).ConfigureAwait(false);

        LearningPortalQualitySummary qualitySummary = await GetLearningPortalQualitySummaryAsync(dbContext, tableAvailability, cancellationToken)
            .ConfigureAwait(false);

        return new AdminLearningPortalSystemReportResponse(
            countsByType,
            countsByCefr,
            grammarByCategory,
            expressionsByType,
            expressionsByRegister,
            expressionsByMeaningTransparency,
            expressionsBySafetyRating,
            expressionsBySensitiveContentKind,
            expressionsByUsagePolicy,
            exercisesByType,
            exercisesBySkill,
            lessonsByCourse,
            lessonsByCefr,
            lessonsByModule,
            examByProfile,
            writingByCategory,
            writingByRegister,
            culturalByCategory,
            qualitySummary.UnresolvedLinkedWordCount,
            qualitySummary.UnresolvedLinkedContentReferenceCount,
            qualitySummary.MissingTranslationCount,
            qualitySummary.UnpublishedDraftCount,
            qualitySummary.GrammarTopicsMissingExercises,
            qualitySummary.CourseLessonsMissingExerciseSets,
            qualitySummary.ExpressionEntriesMissingEligibilityMetadata,
            qualitySummary.ExpressionEntriesWithOrdinaryLiteralLeakage,
            qualitySummary.ExpressionEntriesMissingTeachingReason,
            qualitySummary.ExpressionEntriesWithFewerThanTwoExamples,
            qualitySummary.ExpressionEntriesMissingWarningsForRiskyContent,
            qualitySummary.ExpressionEntriesRequiringAdultAccess,
            qualitySummary.ExpressionEntriesRequiringSensitiveOptIn,
            qualitySummary.ExpressionEntriesRequiringVerifiedAdult,
            qualitySummary.ExpressionEntriesBlockedOrExplicitAdult,
            qualitySummary.ExpressionEntriesMissingSensitiveUsagePolicy,
            qualitySummary.ExpressionEntriesOldRiskyMissingSensitiveMetadata,
            qualitySummary.SampleIssues);
    }

    private static async Task<WordEntrySummary> GetWordEntrySummaryAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken) =>
        await dbContext.WordEntries
            .AsNoTracking()
            .GroupBy(static _ => 1)
            .Select(static group => new WordEntrySummary(
                group.Count(word => word.PublicationStatus == PublicationStatus.Active),
                group.Count(word => word.PublicationStatus == PublicationStatus.Draft)))
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false) ?? new WordEntrySummary(0, 0);

    private static async Task<List<AdminLearningPortalCountRowResponse>> CountByAsync<T>(
        IQueryable<T> query,
        Expression<Func<T, string>> keySelector,
        CancellationToken cancellationToken)
    {
        var rows = await query
            .GroupBy(keySelector)
            .Select(static group => new
            {
                group.Key,
                Count = group.Count(),
            })
            .OrderBy(static row => row.Key)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(static row => new AdminLearningPortalCountRowResponse(row.Key, row.Count))
            .ToList();
    }

    private static Task<List<AdminLearningPortalCountRowResponse>> CountByIfTableExistsAsync<T>(
        IReadOnlyDictionary<string, bool> tableAvailability,
        string tableName,
        IQueryable<T> query,
        Expression<Func<T, string>> keySelector,
        CancellationToken cancellationToken) =>
        HasTable(tableAvailability, tableName)
            ? CountByAsync(query, keySelector, cancellationToken)
            : Task.FromResult(new List<AdminLearningPortalCountRowResponse>());

    private static async Task<List<AdminLearningPortalCountRowResponse>> CountValueByAsync<T, TKey>(
        IQueryable<T> query,
        Expression<Func<T, TKey>> keySelector,
        CancellationToken cancellationToken)
    {
        var rows = await query
            .GroupBy(keySelector)
            .Select(static group => new
            {
                group.Key,
                Count = group.Count(),
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(static row => new AdminLearningPortalCountRowResponse(row.Key?.ToString() ?? string.Empty, row.Count))
            .OrderBy(static row => row.Key, StringComparer.Ordinal)
            .ToList();
    }

    private static Task<List<AdminLearningPortalCountRowResponse>> CountValueByIfTableExistsAsync<T, TKey>(
        IReadOnlyDictionary<string, bool> tableAvailability,
        string tableName,
        IQueryable<T> query,
        Expression<Func<T, TKey>> keySelector,
        CancellationToken cancellationToken) =>
        HasTable(tableAvailability, tableName)
            ? CountValueByAsync(query, keySelector, cancellationToken)
            : Task.FromResult(new List<AdminLearningPortalCountRowResponse>());

    private static async Task<List<AdminLearningPortalCountRowResponse>> GetLearningContentCountsByTypeAsync(
        DarwinLinguaDbContext dbContext,
        IReadOnlyDictionary<string, bool> tableAvailability,
        CancellationToken cancellationToken)
    {
        List<AdminLearningPortalCountRowResponse> rows =
        [
            new("grammar-topic", await CountIfTableExistsAsync(tableAvailability, "GrammarTopics", dbContext.GrammarTopics.AsNoTracking(), cancellationToken).ConfigureAwait(false)),
            new("expression", await CountIfTableExistsAsync(tableAvailability, "ExpressionEntries", dbContext.ExpressionEntries.AsNoTracking(), cancellationToken).ConfigureAwait(false)),
            new("exercise", await CountIfTableExistsAsync(tableAvailability, "Exercises", dbContext.Exercises.AsNoTracking(), cancellationToken).ConfigureAwait(false)),
            new("exercise-set", await CountIfTableExistsAsync(tableAvailability, "ExerciseSets", dbContext.ExerciseSets.AsNoTracking(), cancellationToken).ConfigureAwait(false)),
            new("course", await CountIfTableExistsAsync(tableAvailability, "CoursePaths", dbContext.CoursePaths.AsNoTracking(), cancellationToken).ConfigureAwait(false)),
            new("course-module", await CountIfTableExistsAsync(tableAvailability, "CourseModules", dbContext.CourseModules.AsNoTracking(), cancellationToken).ConfigureAwait(false)),
            new("course-lesson", await CountIfTableExistsAsync(tableAvailability, "CourseLessons", dbContext.CourseLessons.AsNoTracking(), cancellationToken).ConfigureAwait(false)),
            new("exam-profile", await CountIfTableExistsAsync(tableAvailability, "ExamProfiles", dbContext.ExamProfiles.AsNoTracking(), cancellationToken).ConfigureAwait(false)),
            new("exam-prep-unit", await CountIfTableExistsAsync(tableAvailability, "ExamPrepUnits", dbContext.ExamPrepUnits.AsNoTracking(), cancellationToken).ConfigureAwait(false)),
            new("writing-template", await CountIfTableExistsAsync(tableAvailability, "WritingTemplates", dbContext.WritingTemplates.AsNoTracking(), cancellationToken).ConfigureAwait(false)),
            new("cultural-note", await CountIfTableExistsAsync(tableAvailability, "CulturalNotes", dbContext.CulturalNotes.AsNoTracking(), cancellationToken).ConfigureAwait(false)),
            new("roleplay", await CountIfTableExistsAsync(tableAvailability, "RoleplayScenarios", dbContext.RoleplayScenarios.AsNoTracking(), cancellationToken).ConfigureAwait(false)),
        ];

        return rows;
    }

    private static Task<int> CountIfTableExistsAsync<T>(
        IReadOnlyDictionary<string, bool> tableAvailability,
        string tableName,
        IQueryable<T> query,
        CancellationToken cancellationToken) =>
        HasTable(tableAvailability, tableName)
            ? query.CountAsync(cancellationToken)
            : Task.FromResult(0);

    private static async Task<List<AdminLearningPortalCountRowResponse>> GetLearningContentCountsByCefrAsync(
        DarwinLinguaDbContext dbContext,
        IReadOnlyDictionary<string, bool> tableAvailability,
        CancellationToken cancellationToken)
    {
        List<AdminLearningPortalCountRowResponse> rows = [];
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "GrammarTopics", dbContext.GrammarTopics.AsNoTracking(), topic => topic.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "ExpressionEntries", dbContext.ExpressionEntries.AsNoTracking(), expression => expression.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "Exercises", dbContext.Exercises.AsNoTracking(), exercise => exercise.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "ExerciseSets", dbContext.ExerciseSets.AsNoTracking(), set => set.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "CourseLessons", dbContext.CourseLessons.AsNoTracking(), lesson => lesson.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "ExamPrepUnits", dbContext.ExamPrepUnits.AsNoTracking(), unit => unit.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "WritingTemplates", dbContext.WritingTemplates.AsNoTracking(), template => template.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "CulturalNotes", dbContext.CulturalNotes.AsNoTracking(), note => note.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "RoleplayScenarios", dbContext.RoleplayScenarios.AsNoTracking(), scenario => scenario.CefrLevel, cancellationToken).ConfigureAwait(false));

        return rows
            .GroupBy(static row => row.Key, StringComparer.Ordinal)
            .Select(static group => new AdminLearningPortalCountRowResponse(group.Key, group.Sum(row => row.Count)))
            .OrderBy(static row => row.Key, StringComparer.Ordinal)
            .ToList();
    }

    private static async Task<LearningPortalQualitySummary> GetLearningPortalQualitySummaryAsync(
        DarwinLinguaDbContext dbContext,
        IReadOnlyDictionary<string, bool> tableAvailability,
        CancellationToken cancellationToken)
    {
        HashSet<string> wordKeys = (await dbContext.WordEntries
                .AsNoTracking()
                .Select(static word => word.NormalizedLemma)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false))
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> grammarSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "GrammarTopics", dbContext.GrammarTopics.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> expressionSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "ExpressionEntries", dbContext.ExpressionEntries.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> dialogueSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "DialogueLessons", dbContext.DialogueLessons.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> talkTopicSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "TalkTopics", dbContext.TalkTopics.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> exerciseSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "Exercises", dbContext.Exercises.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> exerciseSetSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "ExerciseSets", dbContext.ExerciseSets.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> courseLessonSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "CourseLessons", dbContext.CourseLessons.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> examPrepSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "ExamPrepUnits", dbContext.ExamPrepUnits.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> writingTemplateSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "WritingTemplates", dbContext.WritingTemplates.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);

        List<AdminLearningPortalIssueRowResponse> issues = [];

        int unresolvedWordCount = 0;
        int unresolvedContentCount = 0;

        if (HasTable(tableAvailability, "GrammarLinkedWords"))
        {
            GrammarLinkedWord[] grammarLinkedWords = await dbContext.Set<GrammarLinkedWord>()
                .AsNoTracking()
                .Where(static item => item.WordSlug != null)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (GrammarLinkedWord link in grammarLinkedWords)
            {
                unresolvedWordCount += AddIssueIfMissing(wordKeys, link.WordSlug!, "Grammar linked word", link.GrammarTopicId.ToString(), issues);
            }
        }

        if (HasTable(tableAvailability, "ExpressionLinkedWords"))
        {
            ExpressionLinkedWord[] expressionLinkedWords = await dbContext.Set<ExpressionLinkedWord>()
                .AsNoTracking()
                .Where(static item => item.WordSlug != null)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (ExpressionLinkedWord link in expressionLinkedWords)
            {
                unresolvedWordCount += AddIssueIfMissing(wordKeys, link.WordSlug!, "Expression linked word", link.ExpressionEntryId.ToString(), issues);
            }
        }

        unresolvedContentCount += await CountMissingSlugLinksIfTableExistsAsync(tableAvailability, "GrammarPrerequisiteLinks", dbContext.Set<GrammarPrerequisiteLink>(), grammarSlugs, "Grammar prerequisite", issues, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountMissingSlugLinksIfTableExistsAsync(tableAvailability, "GrammarRelatedTopicLinks", dbContext.Set<GrammarRelatedTopicLink>(), grammarSlugs, "Grammar related topic", issues, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountMissingSlugLinksIfTableExistsAsync(tableAvailability, "GrammarLinkedDialogues", dbContext.Set<GrammarLinkedDialogue>(), dialogueSlugs, "Grammar linked dialogue", issues, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountMissingSlugLinksIfTableExistsAsync(tableAvailability, "GrammarLinkedTalkTopics", dbContext.Set<GrammarLinkedTalkTopic>(), talkTopicSlugs, "Grammar linked Talk Topic", issues, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountMissingSlugLinksIfTableExistsAsync(tableAvailability, "GrammarLinkedExercises", dbContext.Set<GrammarLinkedExercise>(), exerciseSlugs, "Grammar linked exercise", issues, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountMissingExpressionLinksAsync(dbContext, tableAvailability, expressionSlugs, exerciseSlugs, issues, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountJsonLinkIssuesAsync(dbContext, tableAvailability, grammarSlugs, expressionSlugs, dialogueSlugs, talkTopicSlugs, exerciseSlugs, exerciseSetSlugs, courseLessonSlugs, examPrepSlugs, writingTemplateSlugs, issues, cancellationToken).ConfigureAwait(false);

        int missingTranslationCount =
            await CountIfTableExistsAsync(tableAvailability, "GrammarSections", dbContext.GrammarSections.AsNoTracking().Where(section => !section.Translations.Any()), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "GrammarExamples", dbContext.GrammarExamples.AsNoTracking().Where(example => !example.Translations.Any()), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "GrammarCommonMistakes", dbContext.GrammarCommonMistakes.AsNoTracking().Where(item => !item.Translations.Any()), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "GrammarRuleSummaries", dbContext.GrammarRuleSummaries.AsNoTracking().Where(item => !item.Translations.Any()), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "GrammarExceptionNotes", dbContext.GrammarExceptionNotes.AsNoTracking().Where(item => !item.Translations.Any()), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "ExpressionEntries", dbContext.ExpressionEntries.AsNoTracking().Where(item => !item.Meanings.Any()), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "ExpressionExamples", dbContext.ExpressionExamples.AsNoTracking().Where(item => !item.Translations.Any()), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "ExpressionWarnings", dbContext.ExpressionWarnings.AsNoTracking().Where(item => !item.Translations.Any()), cancellationToken).ConfigureAwait(false);

        int draftCount =
            await CountIfTableExistsAsync(tableAvailability, "GrammarTopics", dbContext.GrammarTopics.AsNoTracking().Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "ExpressionEntries", dbContext.ExpressionEntries.AsNoTracking().Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "Exercises", dbContext.Exercises.AsNoTracking().Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "ExerciseSets", dbContext.ExerciseSets.AsNoTracking().Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "CoursePaths", dbContext.CoursePaths.AsNoTracking().Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "CourseModules", dbContext.CourseModules.AsNoTracking().Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "CourseLessons", dbContext.CourseLessons.AsNoTracking().Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "ExamProfiles", dbContext.ExamProfiles.AsNoTracking().Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "ExamPrepUnits", dbContext.ExamPrepUnits.AsNoTracking().Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "WritingTemplates", dbContext.WritingTemplates.AsNoTracking().Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "CulturalNotes", dbContext.CulturalNotes.AsNoTracking().Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false);

        int grammarWithoutExercises = HasTable(tableAvailability, "GrammarTopics") && HasTable(tableAvailability, "GrammarLinkedExercises")
            ? await dbContext.GrammarTopics
                .AsNoTracking()
                .CountAsync(topic => !topic.LinkedExercises.Any(), cancellationToken)
                .ConfigureAwait(false)
            : 0;
        CourseLesson[] lessons = HasTable(tableAvailability, "CourseLessons")
            ? await dbContext.CourseLessons.AsNoTracking().ToArrayAsync(cancellationToken).ConfigureAwait(false)
            : [];
        int lessonsWithoutExerciseSets = lessons.Count(static lesson => ReadStringArray(lesson.LinkedExerciseSetSlugsJson).Count == 0);
        int expressionsMissingEligibilityMetadata = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking().Where(item => item.MeaningTransparency == null || item.MeaningTransparency == string.Empty),
            cancellationToken).ConfigureAwait(false);
        int ordinaryLiteralLeakage = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking().Where(item => item.PublicationStatus == PublicationStatus.Active && item.MeaningTransparency == "ordinary-literal"),
            cancellationToken).ConfigureAwait(false);
        int expressionsMissingTeachingReason = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking().Where(item => item.MeaningTransparency != null && item.MeaningTransparency != string.Empty && (item.TeachingReason == null || item.TeachingReason == string.Empty)),
            cancellationToken).ConfigureAwait(false);
        int expressionsWithFewerThanTwoExamples = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking().Where(item => item.PublicationStatus == PublicationStatus.Active && item.MeaningTransparency != null && item.Examples.Count < 2),
            cancellationToken).ConfigureAwait(false);
        int expressionsMissingWarningsForRiskyContent = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking().Where(item =>
                (item.IsRisky ||
                    item.Register == "slang" ||
                    item.Register == "rude" ||
                    item.Register == "friends-only" ||
                    item.ExpressionType == "slang" ||
                    item.ExpressionType == "warning-phrase" ||
                    item.SafetyRating == "mild-rude" ||
                    item.SafetyRating == "strong-rude" ||
                    item.SafetyRating == "sexual-educational" ||
                    item.SafetyRating == "romantic-social" ||
                    item.SafetyRating == "explicit-adult" ||
                    item.SafetyRating == "blocked-illegal" ||
                    item.SafetyRating == "discriminatory-slur" ||
                    item.SafetyRating == "politically-sensitive" ||
                    item.SensitiveContentKind != "none" ||
                    item.UsagePolicy == "use-with-care" ||
                    item.UsagePolicy == "understand-only" ||
                    item.UsagePolicy == "do-not-use" ||
                    item.UsagePolicy == "blocked") &&
                !item.Warnings.Any()),
            cancellationToken).ConfigureAwait(false);
        int expressionsRequiringAdultAccess = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking().Where(item => item.RequiresAdultAccess || item.SafetyRating == "explicit-adult"),
            cancellationToken).ConfigureAwait(false);
        int expressionsRequiringSensitiveOptIn = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking().Where(item => item.RequiresSensitiveOptIn),
            cancellationToken).ConfigureAwait(false);
        int expressionsRequiringVerifiedAdult = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking().Where(item => item.RequiresVerifiedAdult),
            cancellationToken).ConfigureAwait(false);
        int expressionsBlockedOrExplicitAdult = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking().Where(item =>
                item.SafetyRating == "explicit-adult" ||
                item.SafetyRating == "blocked-illegal" ||
                item.SensitiveContentKind == "blocked" ||
                item.UsagePolicy == "blocked"),
            cancellationToken).ConfigureAwait(false);
        int expressionsMissingSensitiveUsagePolicy = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking().Where(item =>
                (item.RequiresSensitiveOptIn ||
                    item.SafetyRating != "general" ||
                    item.SensitiveContentKind != "none" ||
                    item.MinimumAge > 0) &&
                (item.UsagePolicy == null || item.UsagePolicy == string.Empty || item.UsagePolicy == "safe-to-use")),
            cancellationToken).ConfigureAwait(false);
        int expressionsOldRiskyMissingSensitiveMetadata = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            dbContext.ExpressionEntries.AsNoTracking().Where(item =>
                (item.IsRisky ||
                    item.SafetyRating == "mild-rude" ||
                    item.SafetyRating == "strong-rude" ||
                    item.Register == "rude" ||
                    item.Register == "slang" ||
                    item.ExpressionType == "slang" ||
                    item.ExpressionType == "warning-phrase") &&
                item.SensitiveContentKind == "none" &&
                !item.RequiresSensitiveOptIn),
            cancellationToken).ConfigureAwait(false);

        AddQualityIssue(expressionsMissingEligibilityMetadata, "Expression eligibility", "all", "Missing meaningTransparency metadata", issues);
        AddQualityIssue(ordinaryLiteralLeakage, "Expression eligibility", "published", "Published ordinary-literal expression leakage", issues);
        AddQualityIssue(expressionsMissingTeachingReason, "Expression eligibility", "all", "Missing teachingReason", issues);
        AddQualityIssue(expressionsWithFewerThanTwoExamples, "Expression examples", "published", "Fewer than two examples", issues);
        AddQualityIssue(expressionsMissingWarningsForRiskyContent, "Expression safety", "all", "Missing warning for risky/sensitive expression", issues);
        AddQualityIssue(expressionsRequiringVerifiedAdult, "Expression safety", "all", "Requires verified adult access but no verified-adult system exists", issues);
        AddQualityIssue(expressionsBlockedOrExplicitAdult, "Expression safety", "all", "Blocked or explicit-adult entries present", issues);
        AddQualityIssue(expressionsMissingSensitiveUsagePolicy, "Expression safety", "all", "Sensitive entry missing usagePolicy", issues);
        AddQualityIssue(expressionsOldRiskyMissingSensitiveMetadata, "Expression safety", "all", "Risky entry missing sensitive metadata", issues);

        return new LearningPortalQualitySummary(
            unresolvedWordCount,
            unresolvedContentCount,
            missingTranslationCount,
            draftCount,
            grammarWithoutExercises,
            lessonsWithoutExerciseSets,
            expressionsMissingEligibilityMetadata,
            ordinaryLiteralLeakage,
            expressionsMissingTeachingReason,
            expressionsWithFewerThanTwoExamples,
            expressionsMissingWarningsForRiskyContent,
            expressionsRequiringAdultAccess,
            expressionsRequiringSensitiveOptIn,
            expressionsRequiringVerifiedAdult,
            expressionsBlockedOrExplicitAdult,
            expressionsMissingSensitiveUsagePolicy,
            expressionsOldRiskyMissingSensitiveMetadata,
            issues.Take(30).ToArray());
    }

    private static void AddQualityIssue(
        int count,
        string area,
        string owner,
        string issue,
        List<AdminLearningPortalIssueRowResponse> issues)
    {
        if (count <= 0 || issues.Count >= 30)
        {
            return;
        }

        issues.Add(new AdminLearningPortalIssueRowResponse(area, owner, issue, count.ToString(System.Globalization.CultureInfo.InvariantCulture)));
    }

    private static async Task<HashSet<string>> GetSlugSetIfTableExistsAsync(
        IReadOnlyDictionary<string, bool> tableAvailability,
        string tableName,
        IQueryable<string> query,
        CancellationToken cancellationToken) =>
        HasTable(tableAvailability, tableName)
            ? await GetSlugSetAsync(query, cancellationToken).ConfigureAwait(false)
            : new HashSet<string>(StringComparer.Ordinal);

    private static async Task<HashSet<string>> GetSlugSetAsync(
        IQueryable<string> query,
        CancellationToken cancellationToken) =>
        (await query
                .AsNoTracking()
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false))
            .ToHashSet(StringComparer.Ordinal);

    private static int AddIssueIfMissing(
        IReadOnlySet<string> validTargets,
        string target,
        string area,
        string owner,
        List<AdminLearningPortalIssueRowResponse> issues)
    {
        string normalizedTarget = target.Trim().ToLowerInvariant();
        if (validTargets.Contains(normalizedTarget))
        {
            return 0;
        }

        if (issues.Count < 30)
        {
            issues.Add(new AdminLearningPortalIssueRowResponse(area, owner, "Unresolved reference", normalizedTarget));
        }

        return 1;
    }

    private static async Task<int> CountMissingSlugLinksAsync<TLink>(
        IQueryable<TLink> query,
        IReadOnlySet<string> validTargets,
        string area,
        List<AdminLearningPortalIssueRowResponse> issues,
        CancellationToken cancellationToken)
        where TLink : GrammarSlugLink
    {
        TLink[] links = await query.AsNoTracking().ToArrayAsync(cancellationToken).ConfigureAwait(false);
        int count = 0;
        foreach (TLink link in links)
        {
            count += AddIssueIfMissing(validTargets, link.TargetSlug, area, link.GrammarTopicId.ToString(), issues);
        }

        return count;
    }

    private static Task<int> CountMissingSlugLinksIfTableExistsAsync<TLink>(
        IReadOnlyDictionary<string, bool> tableAvailability,
        string tableName,
        IQueryable<TLink> query,
        IReadOnlySet<string> validTargets,
        string area,
        List<AdminLearningPortalIssueRowResponse> issues,
        CancellationToken cancellationToken)
        where TLink : GrammarSlugLink =>
        HasTable(tableAvailability, tableName)
            ? CountMissingSlugLinksAsync(query, validTargets, area, issues, cancellationToken)
            : Task.FromResult(0);

    private static async Task<int> CountMissingExpressionLinksAsync(
        DarwinLinguaDbContext dbContext,
        IReadOnlyDictionary<string, bool> tableAvailability,
        IReadOnlySet<string> expressionSlugs,
        IReadOnlySet<string> exerciseSlugs,
        List<AdminLearningPortalIssueRowResponse> issues,
        CancellationToken cancellationToken)
    {
        int count = 0;
        if (HasTable(tableAvailability, "RelatedExpressionLinks"))
        {
            RelatedExpressionLink[] relatedExpressions = await dbContext.Set<RelatedExpressionLink>()
                .AsNoTracking()
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (RelatedExpressionLink link in relatedExpressions)
            {
                count += AddIssueIfMissing(expressionSlugs, link.TargetSlug, "Related expression", link.ExpressionEntryId.ToString(), issues);
            }
        }

        if (HasTable(tableAvailability, "ExpressionLinkedExercises"))
        {
            ExpressionLinkedExercise[] linkedExercises = await dbContext.Set<ExpressionLinkedExercise>()
                .AsNoTracking()
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (ExpressionLinkedExercise link in linkedExercises)
            {
                count += AddIssueIfMissing(exerciseSlugs, link.TargetSlug, "Expression linked exercise", link.ExpressionEntryId.ToString(), issues);
            }
        }

        return count;
    }

    private static async Task<int> CountJsonLinkIssuesAsync(
        DarwinLinguaDbContext dbContext,
        IReadOnlyDictionary<string, bool> tableAvailability,
        IReadOnlySet<string> grammarSlugs,
        IReadOnlySet<string> expressionSlugs,
        IReadOnlySet<string> dialogueSlugs,
        IReadOnlySet<string> talkTopicSlugs,
        IReadOnlySet<string> exerciseSlugs,
        IReadOnlySet<string> exerciseSetSlugs,
        IReadOnlySet<string> courseLessonSlugs,
        IReadOnlySet<string> examPrepSlugs,
        IReadOnlySet<string> writingTemplateSlugs,
        List<AdminLearningPortalIssueRowResponse> issues,
        CancellationToken cancellationToken)
    {
        int count = 0;

        if (HasTable(tableAvailability, "CourseLessons"))
        {
            CourseLesson[] lessons = await dbContext.CourseLessons.AsNoTracking().ToArrayAsync(cancellationToken).ConfigureAwait(false);
            foreach (CourseLesson lesson in lessons)
            {
                count += CountJsonTargets(lesson.LinkedGrammarTopicSlugsJson, grammarSlugs, "Lesson linked grammar", lesson.Slug, issues);
                count += CountJsonTargets(lesson.LinkedWordSlugsJson, EmptyStringSet, "Lesson linked word", lesson.Slug, issues);
                count += CountJsonTargets(lesson.LinkedExpressionSlugsJson, expressionSlugs, "Lesson linked expression", lesson.Slug, issues);
                count += CountJsonTargets(lesson.LinkedDialogueSlugsJson, dialogueSlugs, "Lesson linked dialogue", lesson.Slug, issues);
                count += CountJsonTargets(lesson.LinkedTalkTopicSlugsJson, talkTopicSlugs, "Lesson linked Talk Topic", lesson.Slug, issues);
                count += CountJsonTargets(lesson.LinkedExerciseSetSlugsJson, exerciseSetSlugs, "Lesson linked exercise set", lesson.Slug, issues);
                count += CountJsonTargets(lesson.LinkedExamPrepSlugsJson, examPrepSlugs, "Lesson linked exam prep", lesson.Slug, issues);
                if (!string.IsNullOrWhiteSpace(lesson.NextLessonSlug))
                {
                    count += AddIssueIfMissing(courseLessonSlugs, lesson.NextLessonSlug, "Lesson next link", lesson.Slug, issues);
                }

                count += CountJsonTargets(lesson.PrerequisiteLessonSlugsJson, courseLessonSlugs, "Lesson prerequisite", lesson.Slug, issues);
            }
        }

        if (HasTable(tableAvailability, "WritingTemplates"))
        {
            WritingTemplate[] writingTemplates = await dbContext.WritingTemplates.AsNoTracking().ToArrayAsync(cancellationToken).ConfigureAwait(false);
            foreach (WritingTemplate template in writingTemplates)
            {
                count += CountJsonTargets(template.LinkedGrammarTopicSlugsJson, grammarSlugs, "Writing linked grammar", template.Slug, issues);
                count += CountJsonTargets(template.LinkedWordSlugsJson, EmptyStringSet, "Writing linked word", template.Slug, issues);
                count += CountJsonTargets(template.LinkedExpressionSlugsJson, expressionSlugs, "Writing linked expression", template.Slug, issues);
                count += CountJsonTargets(template.LinkedExerciseSlugsJson, exerciseSlugs, "Writing linked exercise", template.Slug, issues);
            }
        }

        if (HasTable(tableAvailability, "CulturalNotes"))
        {
            CulturalNote[] culturalNotes = await dbContext.CulturalNotes.AsNoTracking().ToArrayAsync(cancellationToken).ConfigureAwait(false);
            foreach (CulturalNote note in culturalNotes)
            {
                count += CountJsonTargets(note.LinkedDialogueSlugsJson, dialogueSlugs, "Cultural linked dialogue", note.Slug, issues);
                count += CountJsonTargets(note.LinkedExpressionSlugsJson, expressionSlugs, "Cultural linked expression", note.Slug, issues);
                count += CountJsonTargets(note.LinkedWritingTemplateSlugsJson, writingTemplateSlugs, "Cultural linked writing template", note.Slug, issues);
                count += CountJsonTargets(note.LinkedTalkTopicSlugsJson, talkTopicSlugs, "Cultural linked Talk Topic", note.Slug, issues);
                count += CountJsonTargets(note.LinkedCourseLessonSlugsJson, courseLessonSlugs, "Cultural linked lesson", note.Slug, issues);
            }
        }

        if (HasTable(tableAvailability, "ExamPrepUnits"))
        {
            ExamPrepUnit[] examPrepUnits = await dbContext.ExamPrepUnits.AsNoTracking().ToArrayAsync(cancellationToken).ConfigureAwait(false);
            foreach (ExamPrepUnit unit in examPrepUnits)
            {
                count += CountJsonTargets(unit.LinkedDialogueSlugsJson, dialogueSlugs, "Exam linked dialogue", unit.Slug, issues);
                count += CountJsonTargets(unit.LinkedTalkTopicSlugsJson, talkTopicSlugs, "Exam linked Talk Topic", unit.Slug, issues);
                count += CountJsonTargets(unit.LinkedGrammarTopicSlugsJson, grammarSlugs, "Exam linked grammar", unit.Slug, issues);
                count += CountJsonTargets(unit.LinkedExpressionSlugsJson, expressionSlugs, "Exam linked expression", unit.Slug, issues);
                count += CountJsonTargets(unit.LinkedWritingTemplateSlugsJson, writingTemplateSlugs, "Exam linked writing template", unit.Slug, issues);
                count += CountJsonTargets(unit.LinkedExerciseSlugsJson, exerciseSlugs, "Exam linked exercise", unit.Slug, issues);
                count += CountJsonTargets(unit.LinkedCourseLessonSlugsJson, courseLessonSlugs, "Exam linked lesson", unit.Slug, issues);
            }
        }

        return count;
    }

    private static int CountJsonTargets(
        string json,
        IReadOnlySet<string> validTargets,
        string area,
        string owner,
        List<AdminLearningPortalIssueRowResponse> issues)
    {
        if (validTargets.Count == 0 && area.Contains("word", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        int count = 0;
        foreach (string target in ReadStringArray(json))
        {
            count += AddIssueIfMissing(validTargets, target, area, owner, issues);
        }

        return count;
    }

    private static IReadOnlyList<string> ReadStringArray(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static async Task<ContentPackageSummary> GetContentPackageSummaryAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken) =>
        await dbContext.ContentPackages
            .AsNoTracking()
            .GroupBy(static _ => 1)
            .Select(static group => new ContentPackageSummary(
                group.Count(),
                group.Count(package => package.Status == ContentPackageStatus.Failed),
                group.Max(package => (DateTime?)package.CreatedAtUtc)))
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false) ?? new ContentPackageSummary(0, 0, null);

    private static async Task<OrganizerProfileSummary> GetOrganizerProfileSummaryAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken) =>
        new(await dbContext.OrganizerProfiles
            .AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false));

    private static async Task<ConversationEventSummary> GetConversationEventSummaryAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken) =>
        await dbContext.ConversationEvents
            .AsNoTracking()
            .GroupBy(static _ => 1)
            .Select(static group => new ConversationEventSummary(
                group.Count(),
                group.Count(conversationEvent => conversationEvent.IsOnline)))
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false) ?? new ConversationEventSummary(0, 0);

    private static async Task<OrganizerClaimRequestSummary> GetOrganizerClaimRequestSummaryAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken) =>
        await dbContext.OrganizerClaimRequests
            .AsNoTracking()
            .GroupBy(static _ => 1)
            .Select(static group => new OrganizerClaimRequestSummary(
                group.Count(),
                group.Count(request => request.Status == OrganizerClaimRequestStatuses.Submitted || request.Status == OrganizerClaimRequestStatuses.Reviewing)))
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false) ?? new OrganizerClaimRequestSummary(0, 0);

    private static async Task<LearnerConversationProfileSummary> GetLearnerConversationProfileSummaryAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken) =>
        await dbContext.LearnerConversationProfiles
            .AsNoTracking()
            .GroupBy(static _ => 1)
            .Select(static group => new LearnerConversationProfileSummary(
                group.Count(),
                group.Count(profile => profile.Visibility == "public")))
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false) ?? new LearnerConversationProfileSummary(0, 0);

    private static async Task<PartnerRequestSummary> GetPartnerRequestSummaryAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken) =>
        await dbContext.PartnerRequests
            .AsNoTracking()
            .GroupBy(static _ => 1)
            .Select(static group => new PartnerRequestSummary(
                group.Count(),
                group.Count(request => request.Status == PartnerRequestStatuses.Pending)))
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false) ?? new PartnerRequestSummary(0, 0);

    private static async Task<UserReportSummary> GetUserReportSummaryAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken) =>
        await dbContext.UserReports
            .AsNoTracking()
            .GroupBy(static _ => 1)
            .Select(static group => new UserReportSummary(
                group.Count(),
                group.Count(report => report.Status == UserReportStatuses.Pending)))
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false) ?? new UserReportSummary(0, 0);

    private static async Task<IReadOnlyDictionary<string, bool>> GetLearningPortalTableAvailabilityAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        string[] tableNames =
        [
            "GrammarTopics",
            "GrammarSections",
            "GrammarExamples",
            "GrammarCommonMistakes",
            "GrammarRuleSummaries",
            "GrammarExceptionNotes",
            "GrammarLinkedWords",
            "GrammarPrerequisiteLinks",
            "GrammarRelatedTopicLinks",
            "GrammarLinkedDialogues",
            "GrammarLinkedTalkTopics",
            "GrammarLinkedExercises",
            "ExpressionEntries",
            "ExpressionMeanings",
            "ExpressionExamples",
            "ExpressionWarnings",
            "ExpressionLinkedWords",
            "ExpressionLinkedExercises",
            "RelatedExpressionLinks",
            "DialogueLessons",
            "TalkTopics",
            "Exercises",
            "ExerciseSets",
            "CoursePaths",
            "CourseModules",
            "CourseLessons",
            "ExamProfiles",
            "ExamPrepUnits",
            "WritingTemplates",
            "CulturalNotes",
        ];

        Dictionary<string, bool> result = new(StringComparer.Ordinal);
        foreach (string tableName in tableNames)
        {
            result[tableName] = await TableExistsAsync(dbContext, tableName, cancellationToken)
                .ConfigureAwait(false);
        }

        return result;
    }

    private static bool HasTable(IReadOnlyDictionary<string, bool> tableAvailability, string tableName) =>
        tableAvailability.TryGetValue(tableName, out bool exists) && exists;

    private static async Task<bool> TableExistsAsync(
        DarwinLinguaDbContext dbContext,
        string tableName,
        CancellationToken cancellationToken)
    {
        if (!dbContext.Database.IsRelational())
        {
            return true;
        }

        string? providerName = dbContext.Database.ProviderName;
        await dbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        DbConnection connection = dbContext.Database.GetDbConnection();
        await using DbCommand command = connection.CreateCommand();
        DbParameter parameter = command.CreateParameter();
        parameter.ParameterName = providerName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true ? "tableName" : "@tableName";
        parameter.Value = providerName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true
            ? $"\"{tableName}\""
            : tableName;
        command.Parameters.Add(parameter);

        if (providerName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
        {
            command.CommandText = "SELECT to_regclass(@tableName) IS NOT NULL";
        }
        else if (providerName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
        {
            command.CommandText = "SELECT EXISTS(SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = @tableName)";
        }
        else
        {
            return true;
        }

        object? value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return value switch
        {
            bool boolean => boolean,
            long number => number > 0,
            int number => number > 0,
            _ => false,
        };
    }

    private sealed record WordEntrySummary(int ActiveCount, int DraftCount);

    private sealed record ContentPackageSummary(int ImportedCount, int FailedCount, DateTime? LastImportAtUtc);

    private sealed record OrganizerProfileSummary(int TotalCount);

    private sealed record ConversationEventSummary(int TotalCount, int OnlineCount);

    private sealed record OrganizerClaimRequestSummary(int TotalCount, int PendingCount);

    private sealed record LearnerConversationProfileSummary(int TotalCount, int PublicCount);

    private sealed record PartnerRequestSummary(int TotalCount, int PendingCount);

    private sealed record UserReportSummary(int TotalCount, int PendingCount);

    private sealed record LearningPortalQualitySummary(
        int UnresolvedLinkedWordCount,
        int UnresolvedLinkedContentReferenceCount,
        int MissingTranslationCount,
        int UnpublishedDraftCount,
        int GrammarTopicsMissingExercises,
        int CourseLessonsMissingExerciseSets,
        int ExpressionEntriesMissingEligibilityMetadata,
        int ExpressionEntriesWithOrdinaryLiteralLeakage,
        int ExpressionEntriesMissingTeachingReason,
        int ExpressionEntriesWithFewerThanTwoExamples,
        int ExpressionEntriesMissingWarningsForRiskyContent,
        int ExpressionEntriesRequiringAdultAccess,
        int ExpressionEntriesRequiringSensitiveOptIn,
        int ExpressionEntriesRequiringVerifiedAdult,
        int ExpressionEntriesBlockedOrExplicitAdult,
        int ExpressionEntriesMissingSensitiveUsagePolicy,
        int ExpressionEntriesOldRiskyMissingSensitiveMetadata,
        IReadOnlyList<AdminLearningPortalIssueRowResponse> SampleIssues);
}
