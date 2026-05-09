using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.ContentOps.Domain.Enums;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;

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

internal sealed class WebsiteAdminQueryService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IWebsiteAdminQueryService
{
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

        await Task.WhenAll(catalogTask, socialTask, moderationTask, operationsTask).ConfigureAwait(false);

        return new AdminSystemReportResponse(
            DateTime.UtcNow,
            await catalogTask.ConfigureAwait(false),
            await socialTask.ConfigureAwait(false),
            await moderationTask.ConfigureAwait(false),
            await operationsTask.ConfigureAwait(false));
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

    private sealed record WordEntrySummary(int ActiveCount, int DraftCount);

    private sealed record ContentPackageSummary(int ImportedCount, int FailedCount, DateTime? LastImportAtUtc);

    private sealed record OrganizerProfileSummary(int TotalCount);

    private sealed record ConversationEventSummary(int TotalCount, int OnlineCount);

    private sealed record OrganizerClaimRequestSummary(int TotalCount, int PendingCount);

    private sealed record LearnerConversationProfileSummary(int TotalCount, int PublicCount);

    private sealed record PartnerRequestSummary(int TotalCount, int PendingCount);

    private sealed record UserReportSummary(int TotalCount, int PendingCount);
}
