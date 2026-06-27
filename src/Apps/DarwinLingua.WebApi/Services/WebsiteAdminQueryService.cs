using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.ContentOps.Domain.Enums;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.WebApi.Services;

public interface IWebsiteAdminQueryService
{
    Task<AdminCatalogDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken);

    Task<AdminSystemReportResponse> GetSystemReportAsync(
        string? targetLearningLanguageCode,
        CancellationToken cancellationToken);

    Task<AdminCatalogImportsResponse> GetImportsAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminLearningPortalIssuesResponse> GetLearningPortalIssuesAsync(
        string? areaFilter,
        string? queryText,
        string? targetLearningLanguageCode,
        int take,
        CancellationToken cancellationToken);

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
    private static readonly IReadOnlyList<string> RequiredLearnerMeaningLanguages = ContentLanguageRequirements.RequiredMeaningLanguageCodes;
    private static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly IReadOnlySet<string> SupportedCourseActivityTargetTypes = new HashSet<string>(
        [
            "none",
            "course-lesson",
            "grammar-topic",
            "expression",
            "dialogue",
            "talk-topic",
            "exercise-set",
            "exercise",
            "roleplay",
            "writing-template",
            "country-guidance",
            "exam-prep-unit",
        ],
        StringComparer.Ordinal);

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

    public async Task<AdminSystemReportResponse> GetSystemReportAsync(
        string? targetLearningLanguageCode,
        CancellationToken cancellationToken)
    {
        string normalizedTargetLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode);
        Task<AdminCatalogSystemReportResponse> catalogTask = GetCatalogSystemReportAsync(cancellationToken);
        Task<AdminSocialSystemReportResponse> socialTask = GetSocialSystemReportAsync(cancellationToken);
        Task<AdminModerationSystemReportResponse> moderationTask = GetModerationSystemReportAsync(cancellationToken);
        Task<AdminOperationsSystemReportResponse> operationsTask = GetOperationsSystemReportAsync(cancellationToken);
        Task<AdminLearningPortalSystemReportResponse> learningPortalTask = GetLearningPortalSystemReportAsync(
            normalizedTargetLanguageCode,
            cancellationToken);

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

    public async Task<AdminLearningPortalIssuesResponse> GetLearningPortalIssuesAsync(
        string? areaFilter,
        string? queryText,
        string? targetLearningLanguageCode,
        int take,
        CancellationToken cancellationToken)
    {
        string normalizedTargetLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode);
        string? normalizedAreaFilter = NormalizeFilter(areaFilter);
        string? normalizedQuery = NormalizeFilter(queryText);
        int normalizedTake = Math.Clamp(take <= 0 ? 250 : take, 1, 1000);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyDictionary<string, bool> tableAvailability = await GetLearningPortalTableAvailabilityAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);
        LearningPortalQualitySummary qualitySummary = await GetLearningPortalQualitySummaryAsync(
                dbContext,
                tableAvailability,
                normalizedTargetLanguageCode,
                int.MaxValue,
                cancellationToken)
            .ConfigureAwait(false);

        AdminLearningPortalIssueRowResponse[] allIssues = qualitySummary.SampleIssues
            .OrderBy(static issue => issue.Area, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static issue => issue.Owner, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static issue => issue.Issue, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static issue => issue.Target ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        IReadOnlyList<string> areas = allIssues
            .Select(static issue => issue.Area)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static area => area, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        IEnumerable<AdminLearningPortalIssueRowResponse> filteredIssues = allIssues;
        if (!string.IsNullOrWhiteSpace(normalizedAreaFilter))
        {
            filteredIssues = filteredIssues.Where(issue => string.Equals(issue.Area, normalizedAreaFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            filteredIssues = filteredIssues.Where(issue =>
                issue.Area.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                issue.Owner.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                issue.Issue.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                (issue.Target?.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        AdminLearningPortalIssueRowResponse[] filteredIssueArray = filteredIssues.ToArray();

        return new AdminLearningPortalIssuesResponse(
            normalizedTargetLanguageCode,
            GetDefaultCountryContextCode(normalizedTargetLanguageCode),
            normalizedAreaFilter,
            normalizedQuery,
            normalizedTake,
            filteredIssueArray.Length,
            allIssues.Length,
            areas,
            filteredIssueArray.Take(normalizedTake).ToArray());
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

    private async Task<AdminLearningPortalSystemReportResponse> GetLearningPortalSystemReportAsync(
        string targetLearningLanguageCode,
        CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyDictionary<string, bool> tableAvailability = await GetLearningPortalTableAvailabilityAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

        IQueryable<GrammarTopic> grammarTopics = TargetScoped(dbContext.GrammarTopics.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<ExpressionEntry> expressions = TargetScoped(dbContext.ExpressionEntries.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<Exercise> exercises = TargetScoped(dbContext.Exercises.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<ExerciseSet> exerciseSets = TargetScoped(dbContext.ExerciseSets.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<CoursePath> coursePaths = TargetScoped(dbContext.CoursePaths.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<CourseModule> courseModules = TargetScoped(dbContext.CourseModules.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<CourseLesson> courseLessons = TargetScoped(dbContext.CourseLessons.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<ExamProfile> examProfiles = TargetScoped(dbContext.ExamProfiles.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<ExamPrepUnit> examPrepUnits = TargetScoped(dbContext.ExamPrepUnits.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<WritingTemplate> writingTemplates = TargetScoped(dbContext.WritingTemplates.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<CountryGuidanceNote> countryGuidanceNotes = TargetScoped(dbContext.CountryGuidanceNotes.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<RoleplayScenario> roleplayScenarios = TargetScoped(dbContext.RoleplayScenarios.AsNoTracking(), targetLearningLanguageCode);

        List<AdminLearningPortalCountRowResponse> countsByType = await GetLearningContentCountsByTypeAsync(
                tableAvailability,
                grammarTopics,
                expressions,
                exercises,
                exerciseSets,
                coursePaths,
                courseModules,
                courseLessons,
                examProfiles,
                examPrepUnits,
                writingTemplates,
                countryGuidanceNotes,
                roleplayScenarios,
                cancellationToken)
            .ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> countsByCefr = await GetLearningContentCountsByCefrAsync(
                tableAvailability,
                grammarTopics,
                expressions,
                exercises,
                exerciseSets,
                courseLessons,
                examPrepUnits,
                writingTemplates,
                countryGuidanceNotes,
                roleplayScenarios,
                cancellationToken)
            .ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> grammarByCategory = await CountByIfTableExistsAsync(
            tableAvailability,
            "GrammarTopics",
            grammarTopics,
            topic => topic.GrammarCategory,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> expressionsByType = await CountByIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            expressions,
            expression => expression.ExpressionType,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> expressionsByRegister = await CountByIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            expressions,
            expression => expression.Register,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> expressionsByMeaningTransparency = await CountByIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            expressions,
            expression => expression.MeaningTransparency ?? "missing",
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> expressionsBySafetyRating = await CountByIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            expressions,
            expression => expression.SafetyRating,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> expressionsBySensitiveContentKind = await CountByIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            expressions,
            expression => expression.SensitiveContentKind,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> expressionsByUsagePolicy = await CountByIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            expressions,
            expression => expression.UsagePolicy,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> exercisesByType = await CountByIfTableExistsAsync(
            tableAvailability,
            "Exercises",
            exercises,
            exercise => exercise.ExerciseType,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> exercisesBySkill = await CountByIfTableExistsAsync(
            tableAvailability,
            "Exercises",
            exercises,
            exercise => exercise.TargetSkill,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> lessonsByCourse = await CountByIfTableExistsAsync(
            tableAvailability,
            "CourseLessons",
            courseLessons,
            lesson => lesson.CoursePathSlug,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> lessonsByCefr = await CountValueByIfTableExistsAsync(
            tableAvailability,
            "CourseLessons",
            courseLessons,
            lesson => lesson.CefrLevel,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> lessonsByModule = await CountByIfTableExistsAsync(
            tableAvailability,
            "CourseLessons",
            courseLessons,
            lesson => lesson.ModuleSlug,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> examByProfile = await CountByIfTableExistsAsync(
            tableAvailability,
            "ExamPrepUnits",
            examPrepUnits,
            unit => unit.ExamProfileKey,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> writingByCategory = await CountByIfTableExistsAsync(
            tableAvailability,
            "WritingTemplates",
            writingTemplates,
            template => template.Category,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> writingByRegister = await CountByIfTableExistsAsync(
            tableAvailability,
            "WritingTemplates",
            writingTemplates,
            template => template.Register,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> culturalByCategory = await CountByIfTableExistsAsync(
            tableAvailability,
            "CountryGuidanceNotes",
            countryGuidanceNotes,
            note => note.Category,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> countsByTargetLanguage = await GetLearningContentCountsByTargetLanguageAsync(
            dbContext,
            tableAvailability,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> countryGuidanceByCountryContext = await CountByIfTableExistsAsync(
            tableAvailability,
            "CountryGuidanceNotes",
            countryGuidanceNotes,
            note => note.CountryContextCode,
            cancellationToken).ConfigureAwait(false);
        List<AdminLearningPortalCountRowResponse> targetLanguageActivationGate = BuildTargetLanguageActivationGateRows(
            targetLearningLanguageCode,
            countsByType,
            countsByTargetLanguage,
            countryGuidanceByCountryContext);

        LearningPortalQualitySummary qualitySummary = await GetLearningPortalQualitySummaryAsync(
                dbContext,
                tableAvailability,
                targetLearningLanguageCode,
                30,
                cancellationToken)
            .ConfigureAwait(false);

        return new AdminLearningPortalSystemReportResponse(
            targetLearningLanguageCode,
            GetDefaultCountryContextCode(targetLearningLanguageCode),
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
            countsByTargetLanguage,
            countryGuidanceByCountryContext,
            targetLanguageActivationGate,
            qualitySummary.MissingTranslationsByHelperLanguage,
            qualitySummary.MissingTranslationsByModule,
            qualitySummary.DuplicateSlugsByType,
            qualitySummary.DuplicateSlugCount,
            qualitySummary.UnresolvedLinkedWordCount,
            qualitySummary.UnresolvedLinkedContentReferenceCount,
            qualitySummary.MissingTranslationCount,
            qualitySummary.UnpublishedDraftCount,
            qualitySummary.GrammarTopicsMissingExercises,
            qualitySummary.CourseLessonsMissingExerciseSets,
            qualitySummary.CoursePathsMissingTranslations,
            qualitySummary.CourseModulesMissingTranslations,
            qualitySummary.CourseLessonsMissingTranslations,
            qualitySummary.PublishedCourseLessonsWithoutActivityBlocks,
            qualitySummary.CourseLessonsWithMalformedActivityBlocksJson,
            qualitySummary.CourseActivityBlocksWithUnsupportedTargetType,
            qualitySummary.CourseActivityBlocksWithUnresolvedTargetSlug,
            qualitySummary.ExercisesMissingTranslations,
            qualitySummary.ExerciseSetsMissingTranslations,
            qualitySummary.ExercisesUnpublishedDrafts,
            qualitySummary.ExerciseSetsUnpublishedDrafts,
            qualitySummary.ExerciseSetsWithoutItems,
            qualitySummary.ExerciseSetsWithUnresolvedExerciseSlugs,
            qualitySummary.ExerciseSetsWithUnresolvedOwnerReferences,
            qualitySummary.ExercisesWithMalformedPrompt,
            qualitySummary.ExercisesWithMalformedAnswerKey,
            qualitySummary.ExercisesMissingExplanations,
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
            qualitySummary.ExamPrepProfilesMissingTranslations,
            qualitySummary.ExamPrepUnitsMissingTranslations,
            qualitySummary.ExamPrepUnpublishedDrafts,
            qualitySummary.ExamPrepUnitsWithMalformedStrategyOrChecklist,
            qualitySummary.ExamPrepUnitsWithoutActiveProfile,
            qualitySummary.WritingTemplatesMissingTranslations,
            qualitySummary.WritingTemplatesUnpublishedDrafts,
            qualitySummary.WritingTemplatesWithMalformedVariables,
            qualitySummary.RoleplayScenariosMissingTranslations,
            qualitySummary.RoleplayScenariosUnpublishedDrafts,
            qualitySummary.RoleplayScenariosMissingRequiredImageAssets,
            qualitySummary.RoleplayScenariosWithoutAnswerChoices,
            qualitySummary.RoleplayScenariosWithoutStaticFeedback,
            qualitySummary.RoleplayScenariosWithInvalidPlayableSequence,
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

    private static IQueryable<T> TargetScoped<T>(IQueryable<T> query, string targetLearningLanguageCode)
        where T : class =>
        query.Where(item => EF.Property<string>(item, "TargetLearningLanguageCode") == targetLearningLanguageCode);

    private static string? GetDefaultCountryContextCode(string targetLearningLanguageCode) =>
        CountryContextCatalog.Active
            .Where(context => context.TargetLearningLanguageCodes.Contains(targetLearningLanguageCode, StringComparer.OrdinalIgnoreCase))
            .OrderBy(static context => context.SortOrder)
            .Select(static context => context.Code)
            .FirstOrDefault();

    private static async Task<List<AdminLearningPortalCountRowResponse>> GetLearningContentCountsByTypeAsync(
        IReadOnlyDictionary<string, bool> tableAvailability,
        IQueryable<GrammarTopic> grammarTopics,
        IQueryable<ExpressionEntry> expressions,
        IQueryable<Exercise> exercises,
        IQueryable<ExerciseSet> exerciseSets,
        IQueryable<CoursePath> coursePaths,
        IQueryable<CourseModule> courseModules,
        IQueryable<CourseLesson> courseLessons,
        IQueryable<ExamProfile> examProfiles,
        IQueryable<ExamPrepUnit> examPrepUnits,
        IQueryable<WritingTemplate> writingTemplates,
        IQueryable<CountryGuidanceNote> countryGuidanceNotes,
        IQueryable<RoleplayScenario> roleplayScenarios,
        CancellationToken cancellationToken)
    {
        List<AdminLearningPortalCountRowResponse> rows =
        [
            new("grammar-topic", await CountIfTableExistsAsync(tableAvailability, "GrammarTopics", grammarTopics, cancellationToken).ConfigureAwait(false)),
            new("expression", await CountIfTableExistsAsync(tableAvailability, "ExpressionEntries", expressions, cancellationToken).ConfigureAwait(false)),
            new("exercise", await CountIfTableExistsAsync(tableAvailability, "Exercises", exercises, cancellationToken).ConfigureAwait(false)),
            new("exercise-set", await CountIfTableExistsAsync(tableAvailability, "ExerciseSets", exerciseSets, cancellationToken).ConfigureAwait(false)),
            new("course", await CountIfTableExistsAsync(tableAvailability, "CoursePaths", coursePaths, cancellationToken).ConfigureAwait(false)),
            new("course-module", await CountIfTableExistsAsync(tableAvailability, "CourseModules", courseModules, cancellationToken).ConfigureAwait(false)),
            new("course-lesson", await CountIfTableExistsAsync(tableAvailability, "CourseLessons", courseLessons, cancellationToken).ConfigureAwait(false)),
            new("exam-profile", await CountIfTableExistsAsync(tableAvailability, "ExamProfiles", examProfiles, cancellationToken).ConfigureAwait(false)),
            new("exam-prep-unit", await CountIfTableExistsAsync(tableAvailability, "ExamPrepUnits", examPrepUnits, cancellationToken).ConfigureAwait(false)),
            new("writing-template", await CountIfTableExistsAsync(tableAvailability, "WritingTemplates", writingTemplates, cancellationToken).ConfigureAwait(false)),
            new("country-guidance", await CountIfTableExistsAsync(tableAvailability, "CountryGuidanceNotes", countryGuidanceNotes, cancellationToken).ConfigureAwait(false)),
            new("roleplay", await CountIfTableExistsAsync(tableAvailability, "RoleplayScenarios", roleplayScenarios, cancellationToken).ConfigureAwait(false)),
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
        IReadOnlyDictionary<string, bool> tableAvailability,
        IQueryable<GrammarTopic> grammarTopics,
        IQueryable<ExpressionEntry> expressions,
        IQueryable<Exercise> exercises,
        IQueryable<ExerciseSet> exerciseSets,
        IQueryable<CourseLesson> courseLessons,
        IQueryable<ExamPrepUnit> examPrepUnits,
        IQueryable<WritingTemplate> writingTemplates,
        IQueryable<CountryGuidanceNote> countryGuidanceNotes,
        IQueryable<RoleplayScenario> roleplayScenarios,
        CancellationToken cancellationToken)
    {
        List<AdminLearningPortalCountRowResponse> rows = [];
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "GrammarTopics", grammarTopics, topic => topic.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "ExpressionEntries", expressions, expression => expression.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "Exercises", exercises, exercise => exercise.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "ExerciseSets", exerciseSets, set => set.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "CourseLessons", courseLessons, lesson => lesson.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "ExamPrepUnits", examPrepUnits, unit => unit.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "WritingTemplates", writingTemplates, template => template.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "CountryGuidanceNotes", countryGuidanceNotes, note => note.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByIfTableExistsAsync(tableAvailability, "RoleplayScenarios", roleplayScenarios, scenario => scenario.CefrLevel, cancellationToken).ConfigureAwait(false));

        return rows
            .GroupBy(static row => row.Key, StringComparer.Ordinal)
            .Select(static group => new AdminLearningPortalCountRowResponse(group.Key, group.Sum(row => row.Count)))
            .OrderBy(static row => row.Key, StringComparer.Ordinal)
            .ToList();
    }

    private static async Task<List<AdminLearningPortalCountRowResponse>> GetLearningContentCountsByTargetLanguageAsync(
        DarwinLinguaDbContext dbContext,
        IReadOnlyDictionary<string, bool> tableAvailability,
        CancellationToken cancellationToken)
    {
        List<AdminLearningPortalCountRowResponse> rows = [];
        await AddTargetLanguageCountRowsAsync(rows, tableAvailability, "GrammarTopics", "grammar-topic", dbContext.GrammarTopics.AsNoTracking(), cancellationToken).ConfigureAwait(false);
        await AddTargetLanguageCountRowsAsync(rows, tableAvailability, "ExpressionEntries", "expression", dbContext.ExpressionEntries.AsNoTracking(), cancellationToken).ConfigureAwait(false);
        await AddTargetLanguageCountRowsAsync(rows, tableAvailability, "DialogueLessons", "dialogue", dbContext.DialogueLessons.AsNoTracking(), cancellationToken).ConfigureAwait(false);
        await AddTargetLanguageCountRowsAsync(rows, tableAvailability, "TalkTopics", "talk-topic", dbContext.TalkTopics.AsNoTracking(), cancellationToken).ConfigureAwait(false);
        await AddTargetLanguageCountRowsAsync(rows, tableAvailability, "RoleplayScenarios", "roleplay", dbContext.RoleplayScenarios.AsNoTracking(), cancellationToken).ConfigureAwait(false);
        await AddTargetLanguageCountRowsAsync(rows, tableAvailability, "Exercises", "exercise", dbContext.Exercises.AsNoTracking(), cancellationToken).ConfigureAwait(false);
        await AddTargetLanguageCountRowsAsync(rows, tableAvailability, "ExerciseSets", "exercise-set", dbContext.ExerciseSets.AsNoTracking(), cancellationToken).ConfigureAwait(false);
        await AddTargetLanguageCountRowsAsync(rows, tableAvailability, "CoursePaths", "course", dbContext.CoursePaths.AsNoTracking(), cancellationToken).ConfigureAwait(false);
        await AddTargetLanguageCountRowsAsync(rows, tableAvailability, "CourseModules", "course-module", dbContext.CourseModules.AsNoTracking(), cancellationToken).ConfigureAwait(false);
        await AddTargetLanguageCountRowsAsync(rows, tableAvailability, "CourseLessons", "course-lesson", dbContext.CourseLessons.AsNoTracking(), cancellationToken).ConfigureAwait(false);
        await AddTargetLanguageCountRowsAsync(rows, tableAvailability, "ExamProfiles", "exam-profile", dbContext.ExamProfiles.AsNoTracking(), cancellationToken).ConfigureAwait(false);
        await AddTargetLanguageCountRowsAsync(rows, tableAvailability, "ExamPrepUnits", "exam-prep-unit", dbContext.ExamPrepUnits.AsNoTracking(), cancellationToken).ConfigureAwait(false);
        await AddTargetLanguageCountRowsAsync(rows, tableAvailability, "WritingTemplates", "writing-template", dbContext.WritingTemplates.AsNoTracking(), cancellationToken).ConfigureAwait(false);
        await AddTargetLanguageCountRowsAsync(rows, tableAvailability, "CountryGuidanceNotes", "country-guidance", dbContext.CountryGuidanceNotes.AsNoTracking(), cancellationToken).ConfigureAwait(false);

        return rows
            .OrderBy(static row => row.Key, StringComparer.Ordinal)
            .ToList();
    }

    private static async Task AddTargetLanguageCountRowsAsync<T>(
        List<AdminLearningPortalCountRowResponse> rows,
        IReadOnlyDictionary<string, bool> tableAvailability,
        string tableName,
        string moduleKey,
        IQueryable<T> query,
        CancellationToken cancellationToken)
        where T : class
    {
        if (!HasTable(tableAvailability, tableName))
        {
            return;
        }

        var targetRows = await query
            .GroupBy(static item => EF.Property<string>(item, "TargetLearningLanguageCode"))
            .Select(static group => new
            {
                TargetLearningLanguageCode = group.Key,
                Count = group.Count(),
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        rows.AddRange(targetRows.Select(row => new AdminLearningPortalCountRowResponse(
            $"{moduleKey}:{row.TargetLearningLanguageCode}",
            row.Count)));
    }

    private static List<AdminLearningPortalCountRowResponse> BuildTargetLanguageActivationGateRows(
        string targetLearningLanguageCode,
        IReadOnlyList<AdminLearningPortalCountRowResponse> scopedCountsByType,
        IReadOnlyList<AdminLearningPortalCountRowResponse> countsByTargetLanguage,
        IReadOnlyList<AdminLearningPortalCountRowResponse> countryGuidanceByCountryContext)
    {
        TargetLearningLanguageDefinition? selectedLanguage = TargetLearningLanguageCatalog.All
            .FirstOrDefault(language => string.Equals(language.Code, targetLearningLanguageCode, StringComparison.OrdinalIgnoreCase));
        int selectedCountryContextCount = CountryContextCatalog.Active.Count(context =>
            context.TargetLearningLanguageCodes.Contains(targetLearningLanguageCode, StringComparer.OrdinalIgnoreCase));
        int selectedLevelDefinitionCount = selectedLanguage is null
            ? 0
            : LearningLevelSystemCatalog.GetLevelDefinitionsForTargetLanguage(
                selectedLanguage.Code,
                selectedLanguage.DefaultLevelSystemCode).Count;

        List<AdminLearningPortalCountRowResponse> rows =
        [
            new("active-target-languages", TargetLearningLanguageCatalog.Active.Count),
            new("pilot-target-languages", TargetLearningLanguageCatalog.Pilot.Count),
            new("planned-target-languages", TargetLearningLanguageCatalog.All.Count(language => language.Status == TargetLearningLanguageStatus.Planned)),
            new($"selected-target-active:{targetLearningLanguageCode}", selectedLanguage?.IsActive == true ? 1 : 0),
            new($"selected-target-pilot:{targetLearningLanguageCode}", selectedLanguage?.IsPilot == true ? 1 : 0),
            new($"selected-level-definitions:{targetLearningLanguageCode}", selectedLevelDefinitionCount),
            new($"selected-active-country-contexts:{targetLearningLanguageCode}", selectedCountryContextCount),
            new($"selected-content-items:{targetLearningLanguageCode}", scopedCountsByType.Sum(static row => row.Count)),
            new($"selected-country-guidance-streams:{targetLearningLanguageCode}", countryGuidanceByCountryContext.Count),
        ];

        foreach (TargetLearningLanguageDefinition language in TargetLearningLanguageCatalog.All.OrderBy(static language => language.SortOrder))
        {
            int contentItemCount = countsByTargetLanguage
                .Where(row => row.Key.EndsWith($":{language.Code}", StringComparison.OrdinalIgnoreCase))
                .Sum(static row => row.Count);
            int activeCountryContexts = CountryContextCatalog.Active.Count(context =>
                context.TargetLearningLanguageCodes.Contains(language.Code, StringComparer.OrdinalIgnoreCase));
            int plannedCountryContexts = CountryContextCatalog.All.Count(context =>
                !context.IsActive &&
                context.TargetLearningLanguageCodes.Contains(language.Code, StringComparer.OrdinalIgnoreCase));
            int levelDefinitionCount = LearningLevelSystemCatalog.GetLevelDefinitionsForTargetLanguage(
                language.Code,
                language.DefaultLevelSystemCode).Count;

            rows.Add(new($"target-active:{language.Code}", language.IsActive ? 1 : 0));
            rows.Add(new($"target-pilot:{language.Code}", language.IsPilot ? 1 : 0));
            rows.Add(new($"target-planned:{language.Code}", language.Status == TargetLearningLanguageStatus.Planned ? 1 : 0));
            rows.Add(new($"target-level-definitions:{language.Code}", levelDefinitionCount));
            rows.Add(new($"target-active-country-contexts:{language.Code}", activeCountryContexts));
            rows.Add(new($"target-planned-country-contexts:{language.Code}", plannedCountryContexts));
            rows.Add(new($"target-content-items:{language.Code}", contentItemCount));
        }

        return rows;
    }

    private static async Task<LearningPortalQualitySummary> GetLearningPortalQualitySummaryAsync(
        DarwinLinguaDbContext dbContext,
        IReadOnlyDictionary<string, bool> tableAvailability,
        string targetLearningLanguageCode,
        int issueLimit,
        CancellationToken cancellationToken)
    {
        LanguageCode targetLanguage = LanguageCode.From(targetLearningLanguageCode);
        IQueryable<GrammarTopic> grammarTopics = TargetScoped(dbContext.GrammarTopics.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<ExpressionEntry> expressions = TargetScoped(dbContext.ExpressionEntries.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<DialogueLesson> dialogues = TargetScoped(dbContext.DialogueLessons.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<TalkTopic> talkTopics = TargetScoped(dbContext.TalkTopics.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<Exercise> exercises = TargetScoped(dbContext.Exercises.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<ExerciseSet> exerciseSets = TargetScoped(dbContext.ExerciseSets.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<CoursePath> coursePaths = TargetScoped(dbContext.CoursePaths.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<CourseModule> courseModules = TargetScoped(dbContext.CourseModules.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<CourseLesson> courseLessons = TargetScoped(dbContext.CourseLessons.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<ExamProfile> examProfiles = TargetScoped(dbContext.ExamProfiles.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<ExamPrepUnit> examPrepUnits = TargetScoped(dbContext.ExamPrepUnits.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<RoleplayScenario> roleplays = TargetScoped(dbContext.RoleplayScenarios.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<WritingTemplate> writingTemplates = TargetScoped(dbContext.WritingTemplates.AsNoTracking(), targetLearningLanguageCode);
        IQueryable<CountryGuidanceNote> countryGuidanceNotes = TargetScoped(dbContext.CountryGuidanceNotes.AsNoTracking(), targetLearningLanguageCode);

        HashSet<string> wordKeys = (await dbContext.WordEntries
                .AsNoTracking()
                .Where(word => word.LanguageCode == targetLanguage)
                .Select(static word => word.NormalizedLemma)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false))
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> grammarSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "GrammarTopics", grammarTopics.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> expressionSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "ExpressionEntries", expressions.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> dialogueSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "DialogueLessons", dialogues.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> talkTopicSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "TalkTopics", talkTopics.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> exerciseSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "Exercises", exercises.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> exerciseSetSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "ExerciseSets", exerciseSets.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> courseLessonSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "CourseLessons", courseLessons.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> examPrepSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "ExamPrepUnits", examPrepUnits.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> roleplaySlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "RoleplayScenarios", roleplays.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> writingTemplateSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "WritingTemplates", writingTemplates.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> countryGuidanceNoteSlugs = await GetSlugSetIfTableExistsAsync(tableAvailability, "CountryGuidanceNotes", countryGuidanceNotes.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        Guid[] grammarTopicIds = HasTable(tableAvailability, "GrammarTopics")
            ? await grammarTopics.Select(static item => item.Id).ToArrayAsync(cancellationToken).ConfigureAwait(false)
            : [];
        Guid[] expressionEntryIds = HasTable(tableAvailability, "ExpressionEntries")
            ? await expressions.Select(static item => item.Id).ToArrayAsync(cancellationToken).ConfigureAwait(false)
            : [];

        List<AdminLearningPortalIssueRowResponse> issues = [];

        int unresolvedWordCount = 0;
        int unresolvedContentCount = 0;

        if (HasTable(tableAvailability, "GrammarLinkedWords"))
        {
            GrammarLinkedWord[] grammarLinkedWords = await dbContext.Set<GrammarLinkedWord>()
                .AsNoTracking()
                .Where(item => grammarTopicIds.Contains(item.GrammarTopicId) && item.WordSlug != null)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (GrammarLinkedWord link in grammarLinkedWords)
            {
                unresolvedWordCount += AddIssueIfMissing(wordKeys, link.WordSlug!, "Grammar linked word", link.GrammarTopicId.ToString(), issues, issueLimit);
            }
        }

        if (HasTable(tableAvailability, "ExpressionLinkedWords"))
        {
            ExpressionLinkedWord[] expressionLinkedWords = await dbContext.Set<ExpressionLinkedWord>()
                .AsNoTracking()
                .Where(item => expressionEntryIds.Contains(item.ExpressionEntryId) && item.WordSlug != null)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (ExpressionLinkedWord link in expressionLinkedWords)
            {
                unresolvedWordCount += AddIssueIfMissing(wordKeys, link.WordSlug!, "Expression linked word", link.ExpressionEntryId.ToString(), issues, issueLimit);
            }
        }

        unresolvedContentCount += await CountMissingSlugLinksIfTableExistsAsync(tableAvailability, "GrammarPrerequisiteLinks", dbContext.Set<GrammarPrerequisiteLink>().Where(link => grammarTopicIds.Contains(link.GrammarTopicId)), grammarSlugs, "Grammar prerequisite", issues, issueLimit, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountMissingSlugLinksIfTableExistsAsync(tableAvailability, "GrammarRelatedTopicLinks", dbContext.Set<GrammarRelatedTopicLink>().Where(link => grammarTopicIds.Contains(link.GrammarTopicId)), grammarSlugs, "Grammar related topic", issues, issueLimit, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountMissingSlugLinksIfTableExistsAsync(tableAvailability, "GrammarLinkedDialogues", dbContext.Set<GrammarLinkedDialogue>().Where(link => grammarTopicIds.Contains(link.GrammarTopicId)), dialogueSlugs, "Grammar linked dialogue", issues, issueLimit, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountMissingSlugLinksIfTableExistsAsync(tableAvailability, "GrammarLinkedTalkTopics", dbContext.Set<GrammarLinkedTalkTopic>().Where(link => grammarTopicIds.Contains(link.GrammarTopicId)), talkTopicSlugs, "Grammar linked Talk Topic", issues, issueLimit, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountMissingSlugLinksIfTableExistsAsync(tableAvailability, "GrammarLinkedExercises", dbContext.Set<GrammarLinkedExercise>().Where(link => grammarTopicIds.Contains(link.GrammarTopicId)), exerciseSlugs, "Grammar linked exercise", issues, issueLimit, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountMissingExpressionLinksAsync(dbContext, tableAvailability, expressionEntryIds, expressionSlugs, exerciseSlugs, issues, issueLimit, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountJsonLinkIssuesAsync(dbContext, tableAvailability, targetLearningLanguageCode, grammarSlugs, expressionSlugs, dialogueSlugs, talkTopicSlugs, exerciseSlugs, exerciseSetSlugs, courseLessonSlugs, examPrepSlugs, roleplaySlugs, writingTemplateSlugs, issues, issueLimit, cancellationToken).ConfigureAwait(false);

        int missingTranslationCount =
            await CountIfTableExistsAsync(tableAvailability, "GrammarSections", dbContext.GrammarSections.AsNoTracking().Where(section => grammarTopicIds.Contains(section.GrammarTopicId) && !section.Translations.Any()), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "GrammarExamples", dbContext.GrammarExamples.AsNoTracking().Where(example => grammarTopicIds.Contains(example.GrammarTopicId) && !example.Translations.Any()), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "GrammarCommonMistakes", dbContext.GrammarCommonMistakes.AsNoTracking().Where(item => grammarTopicIds.Contains(item.GrammarTopicId) && !item.Translations.Any()), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "GrammarRuleSummaries", dbContext.GrammarRuleSummaries.AsNoTracking().Where(item => grammarTopicIds.Contains(item.GrammarTopicId) && !item.Translations.Any()), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "GrammarExceptionNotes", dbContext.GrammarExceptionNotes.AsNoTracking().Where(item => grammarTopicIds.Contains(item.GrammarTopicId) && !item.Translations.Any()), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "ExpressionEntries", expressions.Where(item => !item.Meanings.Any()), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "ExpressionExamples", dbContext.ExpressionExamples.AsNoTracking().Where(item => expressionEntryIds.Contains(item.ExpressionEntryId) && !item.Translations.Any()), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "ExpressionWarnings", dbContext.ExpressionWarnings.AsNoTracking().Where(item => expressionEntryIds.Contains(item.ExpressionEntryId) && !item.Translations.Any()), cancellationToken).ConfigureAwait(false);

        int draftCount =
            await CountIfTableExistsAsync(tableAvailability, "GrammarTopics", grammarTopics.Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "ExpressionEntries", expressions.Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "Exercises", exercises.Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "ExerciseSets", exerciseSets.Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "CoursePaths", coursePaths.Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "CourseModules", courseModules.Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "CourseLessons", courseLessons.Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "ExamProfiles", examProfiles.Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "ExamPrepUnits", examPrepUnits.Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "WritingTemplates", writingTemplates.Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false) +
            await CountIfTableExistsAsync(tableAvailability, "CountryGuidanceNotes", countryGuidanceNotes.Where(item => item.PublicationStatus != PublicationStatus.Active), cancellationToken).ConfigureAwait(false);

        int grammarWithoutExercises = HasTable(tableAvailability, "GrammarTopics") && HasTable(tableAvailability, "GrammarLinkedExercises")
            ? await dbContext.GrammarTopics
                .AsNoTracking()
                .Where(topic => topic.TargetLearningLanguageCode == targetLearningLanguageCode)
                .CountAsync(topic => !topic.LinkedExercises.Any(), cancellationToken)
                .ConfigureAwait(false)
            : 0;
        CourseLesson[] lessons = HasTable(tableAvailability, "CourseLessons")
            ? await courseLessons.ToArrayAsync(cancellationToken).ConfigureAwait(false)
            : [];
        int lessonsWithoutExerciseSets = lessons.Count(static lesson => ReadStringArray(lesson.LinkedExerciseSetSlugsJson).Count == 0);
        CourseActivityQualityCounts courseActivityQuality = CountCourseActivityQuality(
            lessons,
            grammarSlugs,
            expressionSlugs,
            dialogueSlugs,
            talkTopicSlugs,
            exerciseSetSlugs,
            exerciseSlugs,
            courseLessonSlugs,
            roleplaySlugs,
            writingTemplateSlugs,
            countryGuidanceNoteSlugs,
            examPrepSlugs,
            issues,
            issueLimit);
        CourseQualityCounts courseQuality = CountCourseQuality(
            HasTable(tableAvailability, "CoursePaths")
                ? await coursePaths.ToArrayAsync(cancellationToken).ConfigureAwait(false)
                : [],
            HasTable(tableAvailability, "CourseModules")
                ? await courseModules.ToArrayAsync(cancellationToken).ConfigureAwait(false)
                : [],
            lessons);
        ExerciseQualityCounts exerciseQuality = HasTable(tableAvailability, "Exercises")
            ? CountExerciseQuality(
                await exercises.ToArrayAsync(cancellationToken).ConfigureAwait(false),
                HasTable(tableAvailability, "ExerciseSets")
                    ? await exerciseSets.Include(static set => set.Items).ToArrayAsync(cancellationToken).ConfigureAwait(false)
                    : [],
                exerciseSlugs,
                wordKeys,
                grammarSlugs,
                expressionSlugs,
                dialogueSlugs,
                talkTopicSlugs,
                courseLessonSlugs,
                examPrepSlugs,
                issues,
                issueLimit)
            : new ExerciseQualityCounts(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        int expressionsMissingEligibilityMetadata = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            expressions.Where(item => item.MeaningTransparency == null || item.MeaningTransparency == string.Empty),
            cancellationToken).ConfigureAwait(false);
        int ordinaryLiteralLeakage = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            expressions.Where(item => item.PublicationStatus == PublicationStatus.Active && item.MeaningTransparency == "ordinary-literal"),
            cancellationToken).ConfigureAwait(false);
        int expressionsMissingTeachingReason = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            expressions.Where(item => item.MeaningTransparency != null && item.MeaningTransparency != string.Empty && (item.TeachingReason == null || item.TeachingReason == string.Empty)),
            cancellationToken).ConfigureAwait(false);
        int expressionsWithFewerThanTwoExamples = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            expressions.Where(item => item.PublicationStatus == PublicationStatus.Active && item.MeaningTransparency != null && item.Examples.Count < 2),
            cancellationToken).ConfigureAwait(false);
        int expressionsMissingWarningsForRiskyContent = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            expressions.Where(item =>
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
            expressions.Where(item => item.RequiresAdultAccess || item.SafetyRating == "explicit-adult"),
            cancellationToken).ConfigureAwait(false);
        int expressionsRequiringSensitiveOptIn = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            expressions.Where(item => item.RequiresSensitiveOptIn),
            cancellationToken).ConfigureAwait(false);
        int expressionsRequiringVerifiedAdult = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            expressions.Where(item => item.RequiresVerifiedAdult),
            cancellationToken).ConfigureAwait(false);
        int expressionsBlockedOrExplicitAdult = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            expressions.Where(item =>
                item.SafetyRating == "explicit-adult" ||
                item.SafetyRating == "blocked-illegal" ||
                item.SensitiveContentKind == "blocked" ||
                item.UsagePolicy == "blocked"),
            cancellationToken).ConfigureAwait(false);
        int expressionsMissingSensitiveUsagePolicy = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            expressions.Where(item =>
                (item.RequiresSensitiveOptIn ||
                    item.SafetyRating != "general" ||
                    item.SensitiveContentKind != "none" ||
                    item.MinimumAge > 0) &&
                (item.UsagePolicy == null || item.UsagePolicy == string.Empty || item.UsagePolicy == "safe-to-use")),
            cancellationToken).ConfigureAwait(false);
        int expressionsOldRiskyMissingSensitiveMetadata = await CountIfTableExistsAsync(
            tableAvailability,
            "ExpressionEntries",
            expressions.Where(item =>
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

        RoleplayQualityCounts roleplayQuality = HasTable(tableAvailability, "RoleplayScenarios")
            ? CountRoleplayQuality(await roleplays.ToArrayAsync(cancellationToken).ConfigureAwait(false))
            : new RoleplayQualityCounts(0, 0, 0, 0, 0, 0);
        ExamPrepQualityCounts examPrepQuality = HasTable(tableAvailability, "ExamProfiles") && HasTable(tableAvailability, "ExamPrepUnits")
            ? CountExamPrepQuality(
                await examProfiles.ToArrayAsync(cancellationToken).ConfigureAwait(false),
                await examPrepUnits.ToArrayAsync(cancellationToken).ConfigureAwait(false))
            : new ExamPrepQualityCounts(0, 0, 0, 0, 0);
        WritingTemplateQualityCounts writingTemplateQuality = HasTable(tableAvailability, "WritingTemplates")
            ? CountWritingTemplateQuality(await writingTemplates.ToArrayAsync(cancellationToken).ConfigureAwait(false))
            : new WritingTemplateQualityCounts(0, 0, 0);
        DuplicateSlugQuality duplicateSlugQuality = CountDuplicateSlugQuality(
            await GetSlugDiagnosticRowsAsync(dbContext, tableAvailability, targetLearningLanguageCode, cancellationToken).ConfigureAwait(false),
            issues,
            issueLimit);
        TranslationCoverageCounts translationCoverage = CountMissingTranslationCoverage(
            BuildTranslationCoverageItems(
                HasTable(tableAvailability, "CoursePaths") ? await coursePaths.ToArrayAsync(cancellationToken).ConfigureAwait(false) : [],
                HasTable(tableAvailability, "CourseModules") ? await courseModules.ToArrayAsync(cancellationToken).ConfigureAwait(false) : [],
                lessons,
                HasTable(tableAvailability, "Exercises") ? await exercises.ToArrayAsync(cancellationToken).ConfigureAwait(false) : [],
                HasTable(tableAvailability, "ExerciseSets") ? await exerciseSets.ToArrayAsync(cancellationToken).ConfigureAwait(false) : [],
                HasTable(tableAvailability, "ExamProfiles") ? await examProfiles.ToArrayAsync(cancellationToken).ConfigureAwait(false) : [],
                HasTable(tableAvailability, "ExamPrepUnits") ? await examPrepUnits.ToArrayAsync(cancellationToken).ConfigureAwait(false) : [],
                HasTable(tableAvailability, "WritingTemplates") ? await writingTemplates.ToArrayAsync(cancellationToken).ConfigureAwait(false) : [],
                HasTable(tableAvailability, "CountryGuidanceNotes") ? await countryGuidanceNotes.ToArrayAsync(cancellationToken).ConfigureAwait(false) : [],
                HasTable(tableAvailability, "RoleplayScenarios") ? await roleplays.ToArrayAsync(cancellationToken).ConfigureAwait(false) : []),
            issues,
            issueLimit);

        AddQualityIssue(expressionsMissingEligibilityMetadata, "Expression eligibility", "all", "Missing meaningTransparency metadata", issues, issueLimit);
        AddQualityIssue(ordinaryLiteralLeakage, "Expression eligibility", "published", "Published ordinary-literal expression leakage", issues, issueLimit);
        AddQualityIssue(expressionsMissingTeachingReason, "Expression eligibility", "all", "Missing teachingReason", issues, issueLimit);
        AddQualityIssue(expressionsWithFewerThanTwoExamples, "Expression examples", "published", "Fewer than two examples", issues, issueLimit);
        AddQualityIssue(expressionsMissingWarningsForRiskyContent, "Expression safety", "all", "Missing warning for risky/sensitive expression", issues, issueLimit);
        AddQualityIssue(expressionsRequiringVerifiedAdult, "Expression safety", "all", "Requires verified adult access but no verified-adult system exists", issues, issueLimit);
        AddQualityIssue(expressionsBlockedOrExplicitAdult, "Expression safety", "all", "Blocked or explicit-adult entries present", issues, issueLimit);
        AddQualityIssue(expressionsMissingSensitiveUsagePolicy, "Expression safety", "all", "Sensitive entry missing usagePolicy", issues, issueLimit);
        AddQualityIssue(expressionsOldRiskyMissingSensitiveMetadata, "Expression safety", "all", "Risky entry missing sensitive metadata", issues, issueLimit);
        AddQualityIssue(examPrepQuality.ProfilesMissingTranslations, "ExamPrep quality", "all", "Exam profiles missing required learner-language translations", issues, issueLimit);
        AddQualityIssue(examPrepQuality.UnitsMissingTranslations, "ExamPrep quality", "all", "Exam prep units missing required learner-language translations", issues, issueLimit);
        AddQualityIssue(examPrepQuality.UnpublishedDrafts, "ExamPrep quality", "all", "Unpublished draft exam prep content", issues, issueLimit);
        AddQualityIssue(examPrepQuality.UnitsWithMalformedStrategyOrChecklist, "ExamPrep quality", "all", "Malformed strategy/checklist JSON", issues, issueLimit);
        AddQualityIssue(examPrepQuality.UnitsWithoutActiveProfile, "ExamPrep quality", "all", "Exam prep unit references no active exam profile", issues, issueLimit);
        AddQualityIssue(writingTemplateQuality.MissingTranslations, "WritingTemplate quality", "all", "Missing required learner-language translations", issues, issueLimit);
        AddQualityIssue(writingTemplateQuality.UnpublishedDrafts, "WritingTemplate quality", "all", "Unpublished draft writing templates", issues, issueLimit);
        AddQualityIssue(writingTemplateQuality.WithMalformedVariables, "WritingTemplate quality", "all", "Malformed variables or template placeholder JSON", issues, issueLimit);
        AddQualityIssue(courseQuality.CoursePathsMissingTranslations, "CoursePath quality", "all", "Missing required learner-language translations", issues, issueLimit);
        AddQualityIssue(courseQuality.CourseModulesMissingTranslations, "CourseModule quality", "all", "Missing required learner-language translations", issues, issueLimit);
        AddQualityIssue(courseQuality.CourseLessonsMissingTranslations, "CourseLesson quality", "all", "Missing required learner-language translations", issues, issueLimit);
        AddQualityIssue(courseActivityQuality.PublishedLessonsWithoutActivityBlocks, "CourseLesson activity", "published", "Published course lessons without activity blocks", issues, issueLimit);
        AddQualityIssue(courseActivityQuality.MalformedActivityBlocksJson, "CourseLesson activity", "all", "Malformed activityBlocks JSON", issues, issueLimit);
        AddQualityIssue(courseActivityQuality.ActivityBlocksWithUnsupportedTargetType, "CourseLesson activity", "all", "Activity block has unsupported targetType", issues, issueLimit);
        AddQualityIssue(courseActivityQuality.ActivityBlocksWithUnresolvedTargetSlug, "CourseLesson activity", "all", "Activity block target slug does not resolve", issues, issueLimit);
        AddQualityIssue(exerciseQuality.ExercisesMissingTranslations, "Exercise quality", "all", "Missing required learner-language translations", issues, issueLimit);
        AddQualityIssue(exerciseQuality.ExerciseSetsMissingTranslations, "ExerciseSet quality", "all", "Missing required learner-language translations", issues, issueLimit);
        AddQualityIssue(exerciseQuality.ExercisesUnpublishedDrafts, "Exercise quality", "all", "Unpublished draft exercises", issues, issueLimit);
        AddQualityIssue(exerciseQuality.ExerciseSetsUnpublishedDrafts, "ExerciseSet quality", "all", "Unpublished draft exercise sets", issues, issueLimit);
        AddQualityIssue(exerciseQuality.ExerciseSetsWithoutItems, "ExerciseSet quality", "published", "Exercise set has no items", issues, issueLimit);
        AddQualityIssue(exerciseQuality.ExerciseSetsWithUnresolvedExerciseSlugs, "ExerciseSet quality", "all", "Exercise set references missing exercise slug", issues, issueLimit);
        AddQualityIssue(exerciseQuality.ExerciseSetsWithUnresolvedOwnerReferences, "ExerciseSet quality", "all", "Exercise set owner target is unresolved", issues, issueLimit);
        AddQualityIssue(exerciseQuality.ExercisesWithMalformedPrompt, "Exercise quality", "all", "Malformed prompt JSON", issues, issueLimit);
        AddQualityIssue(exerciseQuality.ExercisesWithMalformedAnswerKey, "Exercise quality", "all", "Malformed answer key JSON", issues, issueLimit);
        AddQualityIssue(exerciseQuality.ExercisesMissingExplanations, "Exercise quality", "all", "Missing correct/incorrect explanation", issues, issueLimit);
        AddQualityIssue(roleplayQuality.MissingTranslations, "RoleplayScenario quality", "all", "Missing required learner-language translations", issues, issueLimit);
        AddQualityIssue(roleplayQuality.UnpublishedDrafts, "RoleplayScenario quality", "all", "Unpublished draft roleplay scenarios", issues, issueLimit);
        AddQualityIssue(roleplayQuality.MissingRequiredImageAssets, "RoleplayScenario quality", "all", "Required image slot missing assetPath", issues, issueLimit);
        AddQualityIssue(roleplayQuality.WithoutAnswerChoices, "RoleplayScenario quality", "published", "No deterministic answer choices", issues, issueLimit);
        AddQualityIssue(roleplayQuality.WithoutStaticFeedback, "RoleplayScenario quality", "published", "No static feedback", issues, issueLimit);
        AddQualityIssue(roleplayQuality.InvalidPlayableSequence, "RoleplayScenario quality", "published", "Invalid playable sequence", issues, issueLimit);
        AddQualityIssue(duplicateSlugQuality.DuplicateSlugCount, "Slug namespace", "all", "Duplicate slug found inside target-language namespace or across module namespaces", issues, issueLimit);

        return new LearningPortalQualitySummary(
            unresolvedWordCount,
            unresolvedContentCount,
            missingTranslationCount,
            translationCoverage.ByHelperLanguage,
            translationCoverage.ByModule,
            duplicateSlugQuality.DuplicateSlugCount,
            duplicateSlugQuality.ByType,
            draftCount,
            grammarWithoutExercises,
            lessonsWithoutExerciseSets,
            courseQuality.CoursePathsMissingTranslations,
            courseQuality.CourseModulesMissingTranslations,
            courseQuality.CourseLessonsMissingTranslations,
            courseActivityQuality.PublishedLessonsWithoutActivityBlocks,
            courseActivityQuality.MalformedActivityBlocksJson,
            courseActivityQuality.ActivityBlocksWithUnsupportedTargetType,
            courseActivityQuality.ActivityBlocksWithUnresolvedTargetSlug,
            exerciseQuality.ExercisesMissingTranslations,
            exerciseQuality.ExerciseSetsMissingTranslations,
            exerciseQuality.ExercisesUnpublishedDrafts,
            exerciseQuality.ExerciseSetsUnpublishedDrafts,
            exerciseQuality.ExerciseSetsWithoutItems,
            exerciseQuality.ExerciseSetsWithUnresolvedExerciseSlugs,
            exerciseQuality.ExerciseSetsWithUnresolvedOwnerReferences,
            exerciseQuality.ExercisesWithMalformedPrompt,
            exerciseQuality.ExercisesWithMalformedAnswerKey,
            exerciseQuality.ExercisesMissingExplanations,
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
            examPrepQuality.ProfilesMissingTranslations,
            examPrepQuality.UnitsMissingTranslations,
            examPrepQuality.UnpublishedDrafts,
            examPrepQuality.UnitsWithMalformedStrategyOrChecklist,
            examPrepQuality.UnitsWithoutActiveProfile,
            writingTemplateQuality.MissingTranslations,
            writingTemplateQuality.UnpublishedDrafts,
            writingTemplateQuality.WithMalformedVariables,
            roleplayQuality.MissingTranslations,
            roleplayQuality.UnpublishedDrafts,
            roleplayQuality.MissingRequiredImageAssets,
            roleplayQuality.WithoutAnswerChoices,
            roleplayQuality.WithoutStaticFeedback,
            roleplayQuality.InvalidPlayableSequence,
            issues.Take(issueLimit).ToArray());
    }

    private static IReadOnlyList<TranslationCoverageItem> BuildTranslationCoverageItems(
        IReadOnlyList<CoursePath> coursePaths,
        IReadOnlyList<CourseModule> courseModules,
        IReadOnlyList<CourseLesson> courseLessons,
        IReadOnlyList<Exercise> exercises,
        IReadOnlyList<ExerciseSet> exerciseSets,
        IReadOnlyList<ExamProfile> examProfiles,
        IReadOnlyList<ExamPrepUnit> examPrepUnits,
        IReadOnlyList<WritingTemplate> writingTemplates,
        IReadOnlyList<CountryGuidanceNote> countryGuidanceNotes,
        IReadOnlyList<RoleplayScenario> roleplayScenarios)
    {
        List<TranslationCoverageItem> items = [];

        foreach (CoursePath path in coursePaths)
        {
            AddStandardTranslationItems(items, "course", path.Slug, ("title", path.TitleTranslationsJson), ("description", path.DescriptionTranslationsJson));
        }

        foreach (CourseModule module in courseModules)
        {
            AddStandardTranslationItems(items, "course-module", module.Slug, ("title", module.TitleTranslationsJson), ("description", module.DescriptionTranslationsJson));
        }

        foreach (CourseLesson lesson in courseLessons)
        {
            AddStandardTranslationItems(items, "course-lesson", lesson.Slug, ("title", lesson.TitleTranslationsJson), ("shortDescription", lesson.ShortDescriptionTranslationsJson), ("narrative", lesson.NarrativeTranslationsJson));
            items.Add(new TranslationCoverageItem("course-lesson", lesson.Slug, "learningGoals", lesson.LearningGoalsTranslationsJson, true));
            if (!string.IsNullOrWhiteSpace(lesson.ReviewSummary))
            {
                items.Add(new TranslationCoverageItem("course-lesson", lesson.Slug, "reviewSummary", lesson.ReviewSummaryTranslationsJson, false));
            }

            if (!string.IsNullOrWhiteSpace(lesson.HomeworkTask))
            {
                items.Add(new TranslationCoverageItem("course-lesson", lesson.Slug, "homeworkTask", lesson.HomeworkTaskTranslationsJson, false));
            }
        }

        foreach (Exercise exercise in exercises)
        {
            AddStandardTranslationItems(
                items,
                "exercise",
                exercise.Slug,
                ("title", exercise.TitleTranslationsJson),
                ("instruction", exercise.InstructionTranslationsJson),
                ("correctExplanation", exercise.CorrectExplanationTranslationsJson),
                ("incorrectExplanation", exercise.IncorrectExplanationTranslationsJson));
            if (!string.IsNullOrWhiteSpace(exercise.Hint))
            {
                items.Add(new TranslationCoverageItem("exercise", exercise.Slug, "hint", exercise.HintTranslationsJson, false));
            }

            if (!string.IsNullOrWhiteSpace(exercise.CommonMistakeNote))
            {
                items.Add(new TranslationCoverageItem("exercise", exercise.Slug, "commonMistakeNote", exercise.CommonMistakeNoteTranslationsJson, false));
            }
        }

        foreach (ExerciseSet exerciseSet in exerciseSets)
        {
            AddStandardTranslationItems(items, "exercise-set", exerciseSet.Slug, ("title", exerciseSet.TitleTranslationsJson), ("description", exerciseSet.DescriptionTranslationsJson));
        }

        foreach (ExamProfile profile in examProfiles)
        {
            AddStandardTranslationItems(items, "exam-profile", profile.Key, ("displayName", profile.DisplayNameTranslationsJson), ("description", profile.DescriptionTranslationsJson));
        }

        foreach (ExamPrepUnit unit in examPrepUnits)
        {
            AddStandardTranslationItems(items, "exam-prep-unit", unit.Slug, ("title", unit.TitleTranslationsJson), ("shortDescription", unit.ShortDescriptionTranslationsJson), ("explanation", unit.ExplanationTranslationsJson));
            items.Add(new TranslationCoverageItem("exam-prep-unit", unit.Slug, "strategyNotes", unit.StrategyNotesTranslationsJson, true));
            items.Add(new TranslationCoverageItem("exam-prep-unit", unit.Slug, "checklist", unit.ChecklistTranslationsJson, true));
        }

        foreach (WritingTemplate template in writingTemplates)
        {
            AddStandardTranslationItems(
                items,
                "writing-template",
                template.Slug,
                ("title", template.TitleTranslationsJson),
                ("shortDescription", template.ShortDescriptionTranslationsJson),
                ("situation", template.SituationTranslationsJson),
                ("explanation", template.ExplanationTranslationsJson),
                ("templateText", template.TemplateTextTranslationsJson),
                ("sampleFilledVersion", template.SampleFilledVersionTranslationsJson));
        }

        foreach (CountryGuidanceNote note in countryGuidanceNotes)
        {
            AddStandardTranslationItems(
                items,
                "country-guidance",
                $"{note.TargetLearningLanguageCode}|{note.CountryContextCode}|{note.Slug}",
                ("title", note.TitleTranslationsJson),
                ("shortDescription", note.ShortDescriptionTranslationsJson),
                ("context", note.ContextTranslationsJson));
            items.Add(new TranslationCoverageItem("country-guidance", note.Slug, "sections", note.SectionsTranslationsJson, true));
            items.Add(new TranslationCoverageItem("country-guidance", note.Slug, "examples", note.ExamplesTranslationsJson, true));
            items.Add(new TranslationCoverageItem("country-guidance", note.Slug, "doNotes", note.DoNotesTranslationsJson, true));
            items.Add(new TranslationCoverageItem("country-guidance", note.Slug, "dontNotes", note.DontNotesTranslationsJson, true));
            if (!string.IsNullOrWhiteSpace(note.SensitivityWarning))
            {
                items.Add(new TranslationCoverageItem("country-guidance", note.Slug, "sensitivityWarning", note.SensitivityWarningTranslationsJson, false));
            }
        }

        foreach (RoleplayScenario scenario in roleplayScenarios)
        {
            AddStandardTranslationItems(
                items,
                "roleplay",
                scenario.Slug,
                ("title", scenario.TitleTranslationsJson),
                ("description", scenario.DescriptionTranslationsJson),
                ("learnerGoal", scenario.LearnerGoalTranslationsJson));
        }

        return items;
    }

    private static void AddStandardTranslationItems(
        List<TranslationCoverageItem> items,
        string module,
        string owner,
        params (string Field, string Json)[] fields)
    {
        foreach ((string field, string json) in fields)
        {
            items.Add(new TranslationCoverageItem(module, owner, field, json, false));
        }
    }

    private static TranslationCoverageCounts CountMissingTranslationCoverage(
        IReadOnlyList<TranslationCoverageItem> items,
        List<AdminLearningPortalIssueRowResponse> issues,
        int issueLimit)
    {
        Dictionary<string, int> byHelperLanguage = new(StringComparer.Ordinal);
        Dictionary<string, int> byModule = new(StringComparer.Ordinal);

        foreach (TranslationCoverageItem item in items)
        {
            IReadOnlySet<string> presentLanguages = ReadTranslationLanguages(item.Json, item.IsTextList);
            foreach (string requiredLanguage in RequiredLearnerMeaningLanguages)
            {
                if (presentLanguages.Contains(requiredLanguage))
                {
                    continue;
                }

                byHelperLanguage[requiredLanguage] = byHelperLanguage.GetValueOrDefault(requiredLanguage) + 1;
                byModule[item.Module] = byModule.GetValueOrDefault(item.Module) + 1;
                AddDetailedQualityIssue(
                    "Helper translation",
                    $"{item.Module}:{item.Owner}",
                    "Missing required helper-language translation",
                    $"{requiredLanguage}:{item.Field}",
                    issues,
                    issueLimit);
            }
        }

        return new TranslationCoverageCounts(
            ToCountRows(byHelperLanguage),
            ToCountRows(byModule));
    }

    private static IReadOnlySet<string> ReadTranslationLanguages(string json, bool isTextList)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return EmptyStringSet;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return EmptyStringSet;
            }

            HashSet<string> languages = new(StringComparer.Ordinal);
            foreach (JsonElement translation in document.RootElement.EnumerateArray())
            {
                string? language = ReadStringProperty(translation, "language");
                if (string.IsNullOrWhiteSpace(language))
                {
                    continue;
                }

                bool hasValue = isTextList
                    ? translation.TryGetProperty("texts", out JsonElement texts) &&
                        texts.ValueKind == JsonValueKind.Array &&
                        texts.EnumerateArray().Any(static item => item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                    : !string.IsNullOrWhiteSpace(ReadStringProperty(translation, "text"));
                if (hasValue)
                {
                    languages.Add(language.Trim().ToLowerInvariant());
                }
            }

            return languages;
        }
        catch (JsonException)
        {
            return EmptyStringSet;
        }
    }

    private static async Task<IReadOnlyList<SlugDiagnosticRow>> GetSlugDiagnosticRowsAsync(
        DarwinLinguaDbContext dbContext,
        IReadOnlyDictionary<string, bool> tableAvailability,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken)
    {
        List<SlugDiagnosticRow> rows = [];
        await AddSlugRowsIfTableExistsAsync(rows, tableAvailability, "GrammarTopics", "grammar-topic", TargetScoped(dbContext.GrammarTopics.AsNoTracking(), targetLearningLanguageCode), cancellationToken).ConfigureAwait(false);
        await AddSlugRowsIfTableExistsAsync(rows, tableAvailability, "ExpressionEntries", "expression", TargetScoped(dbContext.ExpressionEntries.AsNoTracking(), targetLearningLanguageCode), cancellationToken).ConfigureAwait(false);
        await AddSlugRowsIfTableExistsAsync(rows, tableAvailability, "DialogueLessons", "dialogue", TargetScoped(dbContext.DialogueLessons.AsNoTracking(), targetLearningLanguageCode), cancellationToken).ConfigureAwait(false);
        await AddSlugRowsIfTableExistsAsync(rows, tableAvailability, "TalkTopics", "talk-topic", TargetScoped(dbContext.TalkTopics.AsNoTracking(), targetLearningLanguageCode), cancellationToken).ConfigureAwait(false);
        await AddSlugRowsIfTableExistsAsync(rows, tableAvailability, "RoleplayScenarios", "roleplay", TargetScoped(dbContext.RoleplayScenarios.AsNoTracking(), targetLearningLanguageCode), cancellationToken).ConfigureAwait(false);
        await AddSlugRowsIfTableExistsAsync(rows, tableAvailability, "Exercises", "exercise", TargetScoped(dbContext.Exercises.AsNoTracking(), targetLearningLanguageCode), cancellationToken).ConfigureAwait(false);
        await AddSlugRowsIfTableExistsAsync(rows, tableAvailability, "ExerciseSets", "exercise-set", TargetScoped(dbContext.ExerciseSets.AsNoTracking(), targetLearningLanguageCode), cancellationToken).ConfigureAwait(false);
        await AddSlugRowsIfTableExistsAsync(rows, tableAvailability, "CoursePaths", "course", TargetScoped(dbContext.CoursePaths.AsNoTracking(), targetLearningLanguageCode), cancellationToken).ConfigureAwait(false);
        await AddSlugRowsIfTableExistsAsync(rows, tableAvailability, "CourseModules", "course-module", TargetScoped(dbContext.CourseModules.AsNoTracking(), targetLearningLanguageCode), cancellationToken).ConfigureAwait(false);
        await AddSlugRowsIfTableExistsAsync(rows, tableAvailability, "CourseLessons", "course-lesson", TargetScoped(dbContext.CourseLessons.AsNoTracking(), targetLearningLanguageCode), cancellationToken).ConfigureAwait(false);
        await AddSlugRowsIfTableExistsAsync(rows, tableAvailability, "ExamProfiles", "exam-profile", TargetScoped(dbContext.ExamProfiles.AsNoTracking(), targetLearningLanguageCode), cancellationToken, "Key").ConfigureAwait(false);
        await AddSlugRowsIfTableExistsAsync(rows, tableAvailability, "ExamPrepUnits", "exam-prep-unit", TargetScoped(dbContext.ExamPrepUnits.AsNoTracking(), targetLearningLanguageCode), cancellationToken).ConfigureAwait(false);
        await AddSlugRowsIfTableExistsAsync(rows, tableAvailability, "WritingTemplates", "writing-template", TargetScoped(dbContext.WritingTemplates.AsNoTracking(), targetLearningLanguageCode), cancellationToken).ConfigureAwait(false);
        await AddSlugRowsIfTableExistsAsync(rows, tableAvailability, "CountryGuidanceNotes", "country-guidance", TargetScoped(dbContext.CountryGuidanceNotes.AsNoTracking(), targetLearningLanguageCode), cancellationToken).ConfigureAwait(false);

        return rows;
    }

    private static async Task AddSlugRowsIfTableExistsAsync<T>(
        List<SlugDiagnosticRow> rows,
        IReadOnlyDictionary<string, bool> tableAvailability,
        string tableName,
        string module,
        IQueryable<T> query,
        CancellationToken cancellationToken,
        string slugPropertyName = "Slug")
        where T : class
    {
        if (!HasTable(tableAvailability, tableName))
        {
            return;
        }

        var data = await query
            .Select(item => new
            {
                TargetLearningLanguageCode = EF.Property<string>(item, "TargetLearningLanguageCode"),
                Slug = EF.Property<string>(item, slugPropertyName),
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        rows.AddRange(data.Select(row => new SlugDiagnosticRow(module, row.TargetLearningLanguageCode, row.Slug)));
    }

    private static DuplicateSlugQuality CountDuplicateSlugQuality(
        IReadOnlyList<SlugDiagnosticRow> slugRows,
        List<AdminLearningPortalIssueRowResponse> issues,
        int issueLimit)
    {
        Dictionary<string, int> counts = new(StringComparer.Ordinal);
        foreach (var group in slugRows
            .GroupBy(static row => $"{row.Module}:{row.TargetLearningLanguageCode}:{row.Slug}", StringComparer.Ordinal)
            .Where(static group => group.Count() > 1))
        {
            counts[$"within-module:{group.Key}"] = group.Count();
            AddDetailedQualityIssue("Slug namespace", group.Key, "Duplicate slug inside one target-language module namespace", group.Key, issues, issueLimit);
        }

        foreach (var group in slugRows
            .GroupBy(static row => $"{row.TargetLearningLanguageCode}:{row.Slug}", StringComparer.Ordinal)
            .Where(static group => group.Select(static row => row.Module).Distinct(StringComparer.Ordinal).Count() > 1))
        {
            int moduleCount = group.Select(static row => row.Module).Distinct(StringComparer.Ordinal).Count();
            counts[$"cross-module:{group.Key}"] = moduleCount;
            AddDetailedQualityIssue("Slug namespace", group.Key, "Same slug appears in multiple module namespaces", string.Join(",", group.Select(static row => row.Module).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)), issues, issueLimit);
        }

        return new DuplicateSlugQuality(counts.Values.Sum(), ToCountRows(counts));
    }

    private static IReadOnlyList<AdminLearningPortalCountRowResponse> ToCountRows(IReadOnlyDictionary<string, int> counts) =>
        counts
            .Select(static item => new AdminLearningPortalCountRowResponse(item.Key, item.Value))
            .OrderBy(static row => row.Key, StringComparer.Ordinal)
            .ToArray();

    private static RoleplayQualityCounts CountRoleplayQuality(IReadOnlyList<RoleplayScenario> scenarios)
    {
        int missingTranslations = 0;
        int unpublishedDrafts = 0;
        int missingRequiredImageAssets = 0;
        int withoutAnswerChoices = 0;
        int withoutStaticFeedback = 0;
        int invalidPlayableSequence = 0;

        foreach (RoleplayScenario scenario in scenarios)
        {
            if (scenario.PublicationStatus != PublicationStatus.Active)
            {
                unpublishedDrafts++;
            }

            if (RoleplayScenarioHasMissingTranslations(scenario))
            {
                missingTranslations++;
            }

            if (CountMissingRequiredRoleplayImageAssets(scenario.ImageSlotsJson) > 0)
            {
                missingRequiredImageAssets++;
            }

            if (scenario.PublicationStatus != PublicationStatus.Active)
            {
                continue;
            }

            if (!RoleplayHasAnswerChoiceGroup(scenario.AnswerChoicesJson))
            {
                withoutAnswerChoices++;
            }

            if (!JsonArrayHasItems(scenario.StaticFeedbackJson))
            {
                withoutStaticFeedback++;
            }

            if (!RoleplayHasValidPlayableSequence(scenario.TurnsJson, scenario.AnswerChoicesJson))
            {
                invalidPlayableSequence++;
            }
        }

        return new RoleplayQualityCounts(
            missingTranslations,
            unpublishedDrafts,
            missingRequiredImageAssets,
            withoutAnswerChoices,
            withoutStaticFeedback,
            invalidPlayableSequence);
    }

    private static ExamPrepQualityCounts CountExamPrepQuality(
        IReadOnlyList<ExamProfile> profiles,
        IReadOnlyList<ExamPrepUnit> units)
    {
        HashSet<string> activeProfiles = profiles
            .Where(static profile => profile.PublicationStatus == PublicationStatus.Active)
            .Select(static profile => profile.Key)
            .ToHashSet(StringComparer.Ordinal);

        int profilesMissingTranslations = profiles.Count(static profile =>
            !TranslationArrayHasRequiredLanguages(profile.DisplayNameTranslationsJson) ||
            !TranslationArrayHasRequiredLanguages(profile.DescriptionTranslationsJson));

        int unitsMissingTranslations = units.Count(static unit =>
            !TranslationArrayHasRequiredLanguages(unit.TitleTranslationsJson) ||
            !TranslationArrayHasRequiredLanguages(unit.ShortDescriptionTranslationsJson) ||
            !TranslationArrayHasRequiredLanguages(unit.ExplanationTranslationsJson) ||
            !TextListTranslationArrayHasRequiredLanguages(unit.StrategyNotesTranslationsJson) ||
            !TextListTranslationArrayHasRequiredLanguages(unit.ChecklistTranslationsJson));

        int unpublishedDrafts =
            profiles.Count(static profile => profile.PublicationStatus != PublicationStatus.Active) +
            units.Count(static unit => unit.PublicationStatus != PublicationStatus.Active);

        int malformedStrategyOrChecklist = units.Count(static unit =>
            !JsonArrayIsReadable(unit.StrategyNotesJson) ||
            !JsonArrayIsReadable(unit.ChecklistJson) ||
            !JsonArrayIsReadable(unit.StrategyNotesTranslationsJson) ||
            !JsonArrayIsReadable(unit.ChecklistTranslationsJson));

        int unitsWithoutActiveProfile = units.Count(unit => !activeProfiles.Contains(unit.ExamProfileKey));

        return new ExamPrepQualityCounts(
            profilesMissingTranslations,
            unitsMissingTranslations,
            unpublishedDrafts,
            malformedStrategyOrChecklist,
            unitsWithoutActiveProfile);
    }

    private static WritingTemplateQualityCounts CountWritingTemplateQuality(IReadOnlyList<WritingTemplate> templates)
    {
        int missingTranslations = templates.Count(static template =>
            !TranslationArrayHasRequiredLanguages(template.TitleTranslationsJson) ||
            !TranslationArrayHasRequiredLanguages(template.ShortDescriptionTranslationsJson) ||
            !TranslationArrayHasRequiredLanguages(template.SituationTranslationsJson) ||
            !TranslationArrayHasRequiredLanguages(template.ExplanationTranslationsJson) ||
            !TranslationArrayHasRequiredLanguages(template.TemplateTextTranslationsJson) ||
            !TranslationArrayHasRequiredLanguages(template.SampleFilledVersionTranslationsJson));

        int unpublishedDrafts = templates.Count(static template => template.PublicationStatus != PublicationStatus.Active);
        int malformedVariables = templates.Count(static template => !WritingTemplateVariablesAreConsistent(template));

        return new WritingTemplateQualityCounts(missingTranslations, unpublishedDrafts, malformedVariables);
    }

    private static bool WritingTemplateVariablesAreConsistent(WritingTemplate template)
    {
        string[] declared = ReadStringArray(template.VariablesJson).ToArray();
        HashSet<string> declaredSet = declared.ToHashSet(StringComparer.Ordinal);
        if (declared.Length != declaredSet.Count)
        {
            return false;
        }

        foreach (string variable in declared)
        {
            if (!Regex.IsMatch(variable, "^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.CultureInvariant) ||
                !template.TemplateText.Contains($"{{{{{variable}}}}}", StringComparison.Ordinal))
            {
                return false;
            }
        }

        foreach (Match match in Regex.Matches(template.TemplateText, "\\{\\{(?<name>[a-z0-9]+(?:-[a-z0-9]+)*)\\}\\}", RegexOptions.CultureInvariant))
        {
            if (!declaredSet.Contains(match.Groups["name"].Value))
            {
                return false;
            }
        }

        return true;
    }

    private static CourseQualityCounts CountCourseQuality(
        IReadOnlyList<CoursePath> paths,
        IReadOnlyList<CourseModule> modules,
        IReadOnlyList<CourseLesson> lessons)
    {
        int pathsMissingTranslations = paths.Count(static path =>
            !TranslationArrayHasRequiredLanguages(path.TitleTranslationsJson) ||
            !TranslationArrayHasRequiredLanguages(path.DescriptionTranslationsJson));

        int modulesMissingTranslations = modules.Count(static module =>
            !TranslationArrayHasRequiredLanguages(module.TitleTranslationsJson) ||
            !TranslationArrayHasRequiredLanguages(module.DescriptionTranslationsJson));

        int lessonsMissingTranslations = lessons.Count(static lesson =>
            !TranslationArrayHasRequiredLanguages(lesson.TitleTranslationsJson) ||
            !TranslationArrayHasRequiredLanguages(lesson.ShortDescriptionTranslationsJson) ||
            !TranslationArrayHasRequiredLanguages(lesson.NarrativeTranslationsJson) ||
            !TextListTranslationArrayHasRequiredLanguages(lesson.LearningGoalsTranslationsJson) ||
            (lesson.ReviewSummary is not null && !TranslationArrayHasRequiredLanguages(lesson.ReviewSummaryTranslationsJson)) ||
            (lesson.HomeworkTask is not null && !TranslationArrayHasRequiredLanguages(lesson.HomeworkTaskTranslationsJson)));

        return new CourseQualityCounts(pathsMissingTranslations, modulesMissingTranslations, lessonsMissingTranslations);
    }

    private static CourseActivityQualityCounts CountCourseActivityQuality(
        IReadOnlyList<CourseLesson> lessons,
        IReadOnlySet<string> grammarSlugs,
        IReadOnlySet<string> expressionSlugs,
        IReadOnlySet<string> dialogueSlugs,
        IReadOnlySet<string> talkTopicSlugs,
        IReadOnlySet<string> exerciseSetSlugs,
        IReadOnlySet<string> exerciseSlugs,
        IReadOnlySet<string> courseLessonSlugs,
        IReadOnlySet<string> roleplaySlugs,
        IReadOnlySet<string> writingTemplateSlugs,
        IReadOnlySet<string> countryGuidanceNoteSlugs,
        IReadOnlySet<string> examPrepSlugs,
        List<AdminLearningPortalIssueRowResponse> issues,
        int issueLimit)
    {
        int publishedLessonsWithoutActivityBlocks = 0;
        int malformedActivityBlocksJson = 0;
        int unsupportedTargetTypes = 0;
        int unresolvedTargetSlugs = 0;

        foreach (CourseLesson lesson in lessons)
        {
            CourseActivityQualityRow[]? activities;
            try
            {
                activities = string.IsNullOrWhiteSpace(lesson.ActivityBlocksJson)
                    ? []
                    : JsonSerializer.Deserialize<CourseActivityQualityRow[]>(lesson.ActivityBlocksJson, WebJsonOptions) ?? [];
            }
            catch (JsonException)
            {
                malformedActivityBlocksJson++;
                AddDetailedQualityIssue("CourseLesson activity", lesson.Slug, "Malformed activityBlocks JSON", null, issues, issueLimit);
                continue;
            }

            if (lesson.PublicationStatus == PublicationStatus.Active && activities.Length == 0)
            {
                publishedLessonsWithoutActivityBlocks++;
                AddDetailedQualityIssue("CourseLesson activity", lesson.Slug, "Published course lesson has no activity blocks", null, issues, issueLimit);
            }

            foreach (CourseActivityQualityRow activity in activities)
            {
                string targetType = (activity.TargetType ?? string.Empty).Trim().ToLowerInvariant();
                string targetSlug = (activity.TargetSlug ?? string.Empty).Trim().ToLowerInvariant();
                string target = string.IsNullOrWhiteSpace(targetSlug) ? $"{targetType}:" : $"{targetType}:{targetSlug}";

                if (!SupportedCourseActivityTargetTypes.Contains(targetType))
                {
                    unsupportedTargetTypes++;
                    AddDetailedQualityIssue("CourseLesson activity", lesson.Slug, "Unsupported activity targetType", target, issues, issueLimit);
                    continue;
                }

                if (targetType == "none")
                {
                    continue;
                }

                IReadOnlySet<string> validTargets = targetType switch
                {
                    "course-lesson" => courseLessonSlugs,
                    "grammar-topic" => grammarSlugs,
                    "expression" => expressionSlugs,
                    "dialogue" => dialogueSlugs,
                    "talk-topic" => talkTopicSlugs,
                    "exercise-set" => exerciseSetSlugs,
                    "exercise" => exerciseSlugs,
                    "roleplay" => roleplaySlugs,
                    "writing-template" => writingTemplateSlugs,
                    "country-guidance" => countryGuidanceNoteSlugs,
                    "exam-prep-unit" => examPrepSlugs,
                    _ => EmptyStringSet,
                };

                if (string.IsNullOrWhiteSpace(targetSlug) || !validTargets.Contains(targetSlug))
                {
                    unresolvedTargetSlugs++;
                    AddDetailedQualityIssue("CourseLesson activity", lesson.Slug, "Unresolved activity target slug", target, issues, issueLimit);
                }
            }
        }

        return new CourseActivityQualityCounts(
            publishedLessonsWithoutActivityBlocks,
            malformedActivityBlocksJson,
            unsupportedTargetTypes,
            unresolvedTargetSlugs);
    }

    private static bool RoleplayScenarioHasMissingTranslations(RoleplayScenario scenario) =>
        !TranslationArrayHasRequiredLanguages(scenario.TitleTranslationsJson) ||
        !TranslationArrayHasRequiredLanguages(scenario.DescriptionTranslationsJson) ||
        !TranslationArrayHasRequiredLanguages(scenario.LearnerGoalTranslationsJson) ||
        JsonObjectArrayHasMissingTranslations(scenario.RolesJson, "translations") ||
        JsonObjectArrayHasMissingTranslations(scenario.TurnsJson, "translations") ||
        AnswerChoiceJsonHasMissingTranslations(scenario.AnswerChoicesJson) ||
        JsonObjectArrayHasMissingTranslations(scenario.StaticFeedbackJson, "translations") ||
        JsonObjectArrayHasMissingTranslations(scenario.ImageSlotsJson, "altTextTranslations");

    private static ExerciseQualityCounts CountExerciseQuality(
        IReadOnlyList<Exercise> exercises,
        IReadOnlyList<ExerciseSet> exerciseSets,
        IReadOnlySet<string> knownExerciseSlugs,
        IReadOnlySet<string> knownWordSlugs,
        IReadOnlySet<string> knownGrammarSlugs,
        IReadOnlySet<string> knownExpressionSlugs,
        IReadOnlySet<string> knownDialogueSlugs,
        IReadOnlySet<string> knownTalkTopicSlugs,
        IReadOnlySet<string> knownCourseLessonSlugs,
        IReadOnlySet<string> knownExamPrepSlugs,
        List<AdminLearningPortalIssueRowResponse> issues,
        int issueLimit)
    {
        int exercisesMissingTranslations = 0;
        int exerciseSetsMissingTranslations = 0;
        int exercisesUnpublishedDrafts = 0;
        int exerciseSetsUnpublishedDrafts = 0;
        int exerciseSetsWithoutItems = 0;
        int exerciseSetsWithUnresolvedExerciseSlugs = 0;
        int exerciseSetsWithUnresolvedOwnerReferences = 0;
        int exercisesWithMalformedPrompt = 0;
        int exercisesWithMalformedAnswerKey = 0;
        int exercisesMissingExplanations = 0;

        foreach (Exercise exercise in exercises)
        {
            if (exercise.PublicationStatus != PublicationStatus.Active)
            {
                exercisesUnpublishedDrafts++;
            }

            if (!TranslationArrayHasRequiredLanguages(exercise.TitleTranslationsJson) ||
                !TranslationArrayHasRequiredLanguages(exercise.InstructionTranslationsJson) ||
                !TranslationArrayHasRequiredLanguages(exercise.CorrectExplanationTranslationsJson) ||
                !TranslationArrayHasRequiredLanguages(exercise.IncorrectExplanationTranslationsJson) ||
                (exercise.Hint is not null && !TranslationArrayHasRequiredLanguages(exercise.HintTranslationsJson)) ||
                (exercise.CommonMistakeNote is not null && !TranslationArrayHasRequiredLanguages(exercise.CommonMistakeNoteTranslationsJson)))
            {
                exercisesMissingTranslations++;
            }

            if (!JsonObjectHasItems(exercise.PromptJson))
            {
                exercisesWithMalformedPrompt++;
            }

            if (!JsonObjectHasItems(exercise.AnswerKeyJson))
            {
                exercisesWithMalformedAnswerKey++;
            }

            if (string.IsNullOrWhiteSpace(exercise.CorrectExplanation) ||
                string.IsNullOrWhiteSpace(exercise.IncorrectExplanation))
            {
                exercisesMissingExplanations++;
            }
        }

        foreach (ExerciseSet set in exerciseSets)
        {
            if (set.PublicationStatus != PublicationStatus.Active)
            {
                exerciseSetsUnpublishedDrafts++;
            }

            if (!TranslationArrayHasRequiredLanguages(set.TitleTranslationsJson) ||
                !TranslationArrayHasRequiredLanguages(set.DescriptionTranslationsJson))
            {
                exerciseSetsMissingTranslations++;
            }

            if (set.Items.Count == 0)
            {
                exerciseSetsWithoutItems++;
            }

            if (set.Items.Any(item => !knownExerciseSlugs.Contains(item.ExerciseSlug)))
            {
                exerciseSetsWithUnresolvedExerciseSlugs++;
            }

            if (!string.IsNullOrWhiteSpace(set.OwnerSlug))
            {
                IReadOnlySet<string>? validOwnerTargets = GetExerciseOwnerTargets(
                    set.OwnerType,
                    knownWordSlugs,
                    knownGrammarSlugs,
                    knownExpressionSlugs,
                    knownDialogueSlugs,
                    knownTalkTopicSlugs,
                    knownCourseLessonSlugs,
                    knownExamPrepSlugs);

                string normalizedOwnerSlug = set.OwnerSlug.Trim().ToLowerInvariant();
                if (validOwnerTargets is not null && !validOwnerTargets.Contains(normalizedOwnerSlug))
                {
                    exerciseSetsWithUnresolvedOwnerReferences++;
                    AddDetailedQualityIssue(
                        "ExerciseSet owner",
                        set.Slug,
                        "Unresolved owner target",
                        $"{set.OwnerType}:{normalizedOwnerSlug}",
                        issues,
                        issueLimit);
                }
            }
        }

        return new ExerciseQualityCounts(
            exercisesMissingTranslations,
            exerciseSetsMissingTranslations,
            exercisesUnpublishedDrafts,
            exerciseSetsUnpublishedDrafts,
            exerciseSetsWithoutItems,
            exerciseSetsWithUnresolvedExerciseSlugs,
            exerciseSetsWithUnresolvedOwnerReferences,
            exercisesWithMalformedPrompt,
            exercisesWithMalformedAnswerKey,
            exercisesMissingExplanations);
    }

    private static IReadOnlySet<string>? GetExerciseOwnerTargets(
        string ownerType,
        IReadOnlySet<string> knownWordSlugs,
        IReadOnlySet<string> knownGrammarSlugs,
        IReadOnlySet<string> knownExpressionSlugs,
        IReadOnlySet<string> knownDialogueSlugs,
        IReadOnlySet<string> knownTalkTopicSlugs,
        IReadOnlySet<string> knownCourseLessonSlugs,
        IReadOnlySet<string> knownExamPrepSlugs) =>
        ownerType.Trim().ToLowerInvariant() switch
        {
            "word" => knownWordSlugs,
            "grammar-topic" => knownGrammarSlugs,
            "expression" => knownExpressionSlugs,
            "dialogue" => knownDialogueSlugs,
            "talk-topic" => knownTalkTopicSlugs,
            "course-lesson" => knownCourseLessonSlugs,
            "exam-prep-unit" => knownExamPrepSlugs,
            _ => null,
        };

    private static bool JsonArrayHasItems(string json)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Array && document.RootElement.GetArrayLength() > 0;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool JsonArrayIsReadable(string json)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool JsonObjectHasItems(string json)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.EnumerateObject().Any();
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TranslationArrayHasRequiredLanguages(string json)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            return TranslationArrayHasRequiredLanguages(document.RootElement);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TranslationArrayHasRequiredLanguages(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        HashSet<string> languages = new(StringComparer.Ordinal);
        foreach (JsonElement translation in element.EnumerateArray())
        {
            string? language = ReadStringProperty(translation, "language");
            string? text = ReadStringProperty(translation, "text");
            if (!string.IsNullOrWhiteSpace(language) && !string.IsNullOrWhiteSpace(text))
            {
                languages.Add(language.Trim().ToLowerInvariant());
            }
        }

        return RequiredLearnerMeaningLanguages.All(languages.Contains);
    }

    private static bool TextListTranslationArrayHasRequiredLanguages(string json)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            HashSet<string> languages = new(StringComparer.Ordinal);
            foreach (JsonElement translation in document.RootElement.EnumerateArray())
            {
                string? language = ReadStringProperty(translation, "language");
                if (string.IsNullOrWhiteSpace(language) ||
                    !translation.TryGetProperty("texts", out JsonElement texts) ||
                    texts.ValueKind != JsonValueKind.Array ||
                    texts.GetArrayLength() == 0 ||
                    texts.EnumerateArray().Any(static item => item.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(item.GetString())))
                {
                    continue;
                }

                languages.Add(language.Trim().ToLowerInvariant());
            }

            return RequiredLearnerMeaningLanguages.All(languages.Contains);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool JsonObjectArrayHasMissingTranslations(string json, string translationProperty)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return true;
            }

            foreach (JsonElement item in document.RootElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object &&
                    (!item.TryGetProperty(translationProperty, out JsonElement translations) ||
                     !TranslationArrayHasRequiredLanguages(translations)))
                {
                    return true;
                }
            }
        }
        catch (JsonException)
        {
            return true;
        }

        return false;
    }

    private static bool AnswerChoiceJsonHasMissingTranslations(string json)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return true;
            }

            foreach (JsonElement group in document.RootElement.EnumerateArray())
            {
                if (!group.TryGetProperty("choices", out JsonElement choices) ||
                    choices.ValueKind != JsonValueKind.Array)
                {
                    return true;
                }

                foreach (JsonElement choice in choices.EnumerateArray())
                {
                    if (!choice.TryGetProperty("translations", out JsonElement translations) ||
                        !TranslationArrayHasRequiredLanguages(translations) ||
                        !choice.TryGetProperty("feedbackTranslations", out JsonElement feedbackTranslations) ||
                        !TranslationArrayHasRequiredLanguages(feedbackTranslations))
                    {
                        return true;
                    }
                }
            }
        }
        catch (JsonException)
        {
            return true;
        }

        return false;
    }

    private static int CountMissingRequiredRoleplayImageAssets(string json)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return 0;
            }

            int count = 0;
            foreach (JsonElement slot in document.RootElement.EnumerateArray())
            {
                if (ReadBooleanProperty(slot, "isRequired") &&
                    string.IsNullOrWhiteSpace(ReadStringProperty(slot, "assetPath")))
                {
                    count++;
                }
            }

            return count;
        }
        catch (JsonException)
        {
            return 0;
        }
    }

    private static bool RoleplayHasAnswerChoiceGroup(string json)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            return document.RootElement
                .EnumerateArray()
                .Any(group =>
                    group.TryGetProperty("choices", out JsonElement choices) &&
                    choices.ValueKind == JsonValueKind.Array &&
                    choices.GetArrayLength() >= 2 &&
                    choices.EnumerateArray().Any(choice => ReadBooleanProperty(choice, "isCorrect")));
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool RoleplayHasValidPlayableSequence(string turnsJson, string answerChoicesJson)
    {
        try
        {
            using JsonDocument turnsDocument = JsonDocument.Parse(turnsJson);
            if (turnsDocument.RootElement.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            RoleplayTurnQualityRow[] turns = turnsDocument.RootElement
                .EnumerateArray()
                .Select(static item => new RoleplayTurnQualityRow(
                    ReadIntProperty(item, "sortOrder"),
                    ReadStringProperty(item, "speakerRole") ?? string.Empty,
                    ReadStringProperty(item, "expectedLearnerAction")))
                .OrderBy(static item => item.SortOrder)
                .ToArray();

            if (turns.Length < 2)
            {
                return false;
            }

            bool hasPromptedLearnerTurn = turns
                .Zip(turns.Skip(1), static (current, next) => new { Current = current, Next = next })
                .Any(static pair =>
                    !string.Equals(pair.Current.SpeakerRole, "learner", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(pair.Next.SpeakerRole, "learner", StringComparison.OrdinalIgnoreCase));

            if (!hasPromptedLearnerTurn)
            {
                return false;
            }

            HashSet<int> answerChoiceTurnSortOrders = ReadRoleplayAnswerChoiceTurnSortOrders(answerChoicesJson);
            return turns
                .Where(static turn => string.Equals(turn.SpeakerRole, "learner", StringComparison.OrdinalIgnoreCase))
                .All(turn =>
                    !string.IsNullOrWhiteSpace(turn.ExpectedLearnerAction) ||
                    answerChoiceTurnSortOrders.Contains(turn.SortOrder));
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static HashSet<int> ReadRoleplayAnswerChoiceTurnSortOrders(string json)
    {
        HashSet<int> values = [];
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return values;
            }

            foreach (JsonElement group in document.RootElement.EnumerateArray())
            {
                values.Add(ReadIntProperty(group, "turnSortOrder"));
            }
        }
        catch (JsonException)
        {
            return values;
        }

        return values;
    }

    private static string? ReadStringProperty(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(propertyName, out JsonElement value) ||
            value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return value.GetString();
    }

    private static bool ReadBooleanProperty(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(propertyName, out JsonElement value) &&
        value.ValueKind == JsonValueKind.True;

    private static int ReadIntProperty(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(propertyName, out JsonElement value) &&
        value.ValueKind == JsonValueKind.Number &&
        value.TryGetInt32(out int result)
            ? result
            : 0;

    private static void AddQualityIssue(
        int count,
        string area,
        string owner,
        string issue,
        List<AdminLearningPortalIssueRowResponse> issues,
        int issueLimit)
    {
        if (count <= 0 || issues.Count >= issueLimit)
        {
            return;
        }

        issues.Add(new AdminLearningPortalIssueRowResponse(area, owner, issue, count.ToString(System.Globalization.CultureInfo.InvariantCulture)));
    }

    private static void AddDetailedQualityIssue(
        string area,
        string owner,
        string issue,
        string? target,
        List<AdminLearningPortalIssueRowResponse> issues,
        int issueLimit)
    {
        if (issues.Count >= issueLimit)
        {
            return;
        }

        issues.Add(new AdminLearningPortalIssueRowResponse(area, owner, issue, target));
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
        List<AdminLearningPortalIssueRowResponse> issues,
        int issueLimit)
    {
        string normalizedTarget = target.Trim().ToLowerInvariant();
        if (validTargets.Contains(normalizedTarget))
        {
            return 0;
        }

        if (issues.Count < issueLimit)
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
        int issueLimit,
        CancellationToken cancellationToken)
        where TLink : GrammarSlugLink
    {
        TLink[] links = await query.AsNoTracking().ToArrayAsync(cancellationToken).ConfigureAwait(false);
        int count = 0;
        foreach (TLink link in links)
        {
            count += AddIssueIfMissing(validTargets, link.TargetSlug, area, link.GrammarTopicId.ToString(), issues, issueLimit);
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
        int issueLimit,
        CancellationToken cancellationToken)
        where TLink : GrammarSlugLink =>
        HasTable(tableAvailability, tableName)
            ? CountMissingSlugLinksAsync(query, validTargets, area, issues, issueLimit, cancellationToken)
            : Task.FromResult(0);

    private static async Task<int> CountMissingExpressionLinksAsync(
        DarwinLinguaDbContext dbContext,
        IReadOnlyDictionary<string, bool> tableAvailability,
        IReadOnlyCollection<Guid> expressionEntryIds,
        IReadOnlySet<string> expressionSlugs,
        IReadOnlySet<string> exerciseSlugs,
        List<AdminLearningPortalIssueRowResponse> issues,
        int issueLimit,
        CancellationToken cancellationToken)
    {
        int count = 0;
        if (HasTable(tableAvailability, "RelatedExpressionLinks"))
        {
            RelatedExpressionLink[] relatedExpressions = await dbContext.Set<RelatedExpressionLink>()
                .AsNoTracking()
                .Where(link => expressionEntryIds.Contains(link.ExpressionEntryId))
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (RelatedExpressionLink link in relatedExpressions)
            {
                count += AddIssueIfMissing(expressionSlugs, link.TargetSlug, "Related expression", link.ExpressionEntryId.ToString(), issues, issueLimit);
            }
        }

        if (HasTable(tableAvailability, "ExpressionLinkedExercises"))
        {
            ExpressionLinkedExercise[] linkedExercises = await dbContext.Set<ExpressionLinkedExercise>()
                .AsNoTracking()
                .Where(link => expressionEntryIds.Contains(link.ExpressionEntryId))
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (ExpressionLinkedExercise link in linkedExercises)
            {
                count += AddIssueIfMissing(exerciseSlugs, link.TargetSlug, "Expression linked exercise", link.ExpressionEntryId.ToString(), issues, issueLimit);
            }
        }

        return count;
    }

    private static async Task<int> CountJsonLinkIssuesAsync(
        DarwinLinguaDbContext dbContext,
        IReadOnlyDictionary<string, bool> tableAvailability,
        string targetLearningLanguageCode,
        IReadOnlySet<string> grammarSlugs,
        IReadOnlySet<string> expressionSlugs,
        IReadOnlySet<string> dialogueSlugs,
        IReadOnlySet<string> talkTopicSlugs,
        IReadOnlySet<string> exerciseSlugs,
        IReadOnlySet<string> exerciseSetSlugs,
        IReadOnlySet<string> courseLessonSlugs,
        IReadOnlySet<string> examPrepSlugs,
        IReadOnlySet<string> roleplaySlugs,
        IReadOnlySet<string> writingTemplateSlugs,
        List<AdminLearningPortalIssueRowResponse> issues,
        int issueLimit,
        CancellationToken cancellationToken)
    {
        int count = 0;

        if (HasTable(tableAvailability, "CourseLessons"))
        {
            CourseLesson[] lessons = await TargetScoped(dbContext.CourseLessons.AsNoTracking(), targetLearningLanguageCode)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (CourseLesson lesson in lessons)
            {
                count += CountJsonTargets(lesson.LinkedGrammarTopicSlugsJson, grammarSlugs, "Lesson linked grammar", lesson.Slug, issues, issueLimit);
                count += CountJsonTargets(lesson.LinkedWordSlugsJson, EmptyStringSet, "Lesson linked word", lesson.Slug, issues, issueLimit);
                count += CountJsonTargets(lesson.LinkedExpressionSlugsJson, expressionSlugs, "Lesson linked expression", lesson.Slug, issues, issueLimit);
                count += CountJsonTargets(lesson.LinkedDialogueSlugsJson, dialogueSlugs, "Lesson linked dialogue", lesson.Slug, issues, issueLimit);
                count += CountJsonTargets(lesson.LinkedTalkTopicSlugsJson, talkTopicSlugs, "Lesson linked Talk Topic", lesson.Slug, issues, issueLimit);
                count += CountJsonTargets(lesson.LinkedExerciseSetSlugsJson, exerciseSetSlugs, "Lesson linked exercise set", lesson.Slug, issues, issueLimit);
                count += CountJsonTargets(lesson.LinkedExamPrepSlugsJson, examPrepSlugs, "Lesson linked exam prep", lesson.Slug, issues, issueLimit);
                if (!string.IsNullOrWhiteSpace(lesson.NextLessonSlug))
                {
                    count += AddIssueIfMissing(courseLessonSlugs, lesson.NextLessonSlug, "Lesson next link", lesson.Slug, issues, issueLimit);
                }

                count += CountJsonTargets(lesson.PrerequisiteLessonSlugsJson, courseLessonSlugs, "Lesson prerequisite", lesson.Slug, issues, issueLimit);
            }
        }

        if (HasTable(tableAvailability, "WritingTemplates"))
        {
            WritingTemplate[] writingTemplates = await TargetScoped(dbContext.WritingTemplates.AsNoTracking(), targetLearningLanguageCode)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (WritingTemplate template in writingTemplates)
            {
                count += CountJsonTargets(template.LinkedGrammarTopicSlugsJson, grammarSlugs, "Writing linked grammar", template.Slug, issues, issueLimit);
                count += CountJsonTargets(template.LinkedWordSlugsJson, EmptyStringSet, "Writing linked word", template.Slug, issues, issueLimit);
                count += CountJsonTargets(template.LinkedExpressionSlugsJson, expressionSlugs, "Writing linked expression", template.Slug, issues, issueLimit);
                count += CountJsonTargets(template.LinkedExerciseSlugsJson, exerciseSlugs, "Writing linked exercise", template.Slug, issues, issueLimit);
                count += CountJsonTargets(template.LinkedCourseLessonSlugsJson, courseLessonSlugs, "Writing linked course lesson", template.Slug, issues, issueLimit);
            }
        }

        if (HasTable(tableAvailability, "CountryGuidanceNotes"))
        {
            CountryGuidanceNote[] countryGuidanceNotes = await TargetScoped(dbContext.CountryGuidanceNotes.AsNoTracking(), targetLearningLanguageCode)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (CountryGuidanceNote note in countryGuidanceNotes)
            {
                count += CountJsonTargets(note.LinkedDialogueSlugsJson, dialogueSlugs, "Country Guidance linked dialogue", note.Slug, issues, issueLimit);
                count += CountJsonTargets(note.LinkedExpressionSlugsJson, expressionSlugs, "Country Guidance linked expression", note.Slug, issues, issueLimit);
                count += CountJsonTargets(note.LinkedWritingTemplateSlugsJson, writingTemplateSlugs, "Country Guidance linked writing template", note.Slug, issues, issueLimit);
                count += CountJsonTargets(note.LinkedTalkTopicSlugsJson, talkTopicSlugs, "Country Guidance linked Talk Topic", note.Slug, issues, issueLimit);
                count += CountJsonTargets(note.LinkedCourseLessonSlugsJson, courseLessonSlugs, "Country Guidance linked lesson", note.Slug, issues, issueLimit);
            }
        }

        if (HasTable(tableAvailability, "ExamPrepUnits"))
        {
            ExamPrepUnit[] examPrepUnits = await TargetScoped(dbContext.ExamPrepUnits.AsNoTracking(), targetLearningLanguageCode)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (ExamPrepUnit unit in examPrepUnits)
            {
                count += CountJsonTargets(unit.LinkedDialogueSlugsJson, dialogueSlugs, "Exam linked dialogue", unit.Slug, issues, issueLimit);
                count += CountJsonTargets(unit.LinkedTalkTopicSlugsJson, talkTopicSlugs, "Exam linked Talk Topic", unit.Slug, issues, issueLimit);
                count += CountJsonTargets(unit.LinkedGrammarTopicSlugsJson, grammarSlugs, "Exam linked grammar", unit.Slug, issues, issueLimit);
                count += CountJsonTargets(unit.LinkedExpressionSlugsJson, expressionSlugs, "Exam linked expression", unit.Slug, issues, issueLimit);
                count += CountJsonTargets(unit.LinkedWritingTemplateSlugsJson, writingTemplateSlugs, "Exam linked writing template", unit.Slug, issues, issueLimit);
                count += CountJsonTargets(unit.LinkedExerciseSlugsJson, exerciseSlugs, "Exam linked exercise", unit.Slug, issues, issueLimit);
                count += CountJsonTargets(unit.LinkedRoleplaySlugsJson, roleplaySlugs, "Exam linked roleplay", unit.Slug, issues, issueLimit);
                count += CountJsonTargets(unit.LinkedCourseLessonSlugsJson, courseLessonSlugs, "Exam linked lesson", unit.Slug, issues, issueLimit);
            }
        }

        return count;
    }

    private static int CountJsonTargets(
        string json,
        IReadOnlySet<string> validTargets,
        string area,
        string owner,
        List<AdminLearningPortalIssueRowResponse> issues,
        int issueLimit)
    {
        if (validTargets.Count == 0 && area.Contains("word", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        int count = 0;
        foreach (string target in ReadStringArray(json))
        {
            count += AddIssueIfMissing(validTargets, target, area, owner, issues, issueLimit);
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
            "CountryGuidanceNotes",
            "RoleplayScenarios",
            "RoleplayScenarioTopics",
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
        parameter.ParameterName = "tableName";
        parameter.Value = $"\"{tableName}\"";
        command.Parameters.Add(parameter);

        if (providerName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
        {
            command.CommandText = "SELECT to_regclass(@tableName) IS NOT NULL";
        }
        else
        {
            throw new InvalidOperationException("DarwinLingua.WebApi requires the PostgreSQL Npgsql provider for admin reporting.");
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
        IReadOnlyList<AdminLearningPortalCountRowResponse> MissingTranslationsByHelperLanguage,
        IReadOnlyList<AdminLearningPortalCountRowResponse> MissingTranslationsByModule,
        int DuplicateSlugCount,
        IReadOnlyList<AdminLearningPortalCountRowResponse> DuplicateSlugsByType,
        int UnpublishedDraftCount,
        int GrammarTopicsMissingExercises,
        int CourseLessonsMissingExerciseSets,
        int CoursePathsMissingTranslations,
        int CourseModulesMissingTranslations,
        int CourseLessonsMissingTranslations,
        int PublishedCourseLessonsWithoutActivityBlocks,
        int CourseLessonsWithMalformedActivityBlocksJson,
        int CourseActivityBlocksWithUnsupportedTargetType,
        int CourseActivityBlocksWithUnresolvedTargetSlug,
        int ExercisesMissingTranslations,
        int ExerciseSetsMissingTranslations,
        int ExercisesUnpublishedDrafts,
        int ExerciseSetsUnpublishedDrafts,
        int ExerciseSetsWithoutItems,
        int ExerciseSetsWithUnresolvedExerciseSlugs,
        int ExerciseSetsWithUnresolvedOwnerReferences,
        int ExercisesWithMalformedPrompt,
        int ExercisesWithMalformedAnswerKey,
        int ExercisesMissingExplanations,
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
        int ExamPrepProfilesMissingTranslations,
        int ExamPrepUnitsMissingTranslations,
        int ExamPrepUnpublishedDrafts,
        int ExamPrepUnitsWithMalformedStrategyOrChecklist,
        int ExamPrepUnitsWithoutActiveProfile,
        int WritingTemplatesMissingTranslations,
        int WritingTemplatesUnpublishedDrafts,
        int WritingTemplatesWithMalformedVariables,
        int RoleplayScenariosMissingTranslations,
        int RoleplayScenariosUnpublishedDrafts,
        int RoleplayScenariosMissingRequiredImageAssets,
        int RoleplayScenariosWithoutAnswerChoices,
        int RoleplayScenariosWithoutStaticFeedback,
        int RoleplayScenariosWithInvalidPlayableSequence,
        IReadOnlyList<AdminLearningPortalIssueRowResponse> SampleIssues);

    private sealed record RoleplayQualityCounts(
        int MissingTranslations,
        int UnpublishedDrafts,
        int MissingRequiredImageAssets,
        int WithoutAnswerChoices,
        int WithoutStaticFeedback,
        int InvalidPlayableSequence);

    private sealed record TranslationCoverageItem(
        string Module,
        string Owner,
        string Field,
        string Json,
        bool IsTextList);

    private sealed record TranslationCoverageCounts(
        IReadOnlyList<AdminLearningPortalCountRowResponse> ByHelperLanguage,
        IReadOnlyList<AdminLearningPortalCountRowResponse> ByModule);

    private sealed record SlugDiagnosticRow(
        string Module,
        string TargetLearningLanguageCode,
        string Slug);

    private sealed record DuplicateSlugQuality(
        int DuplicateSlugCount,
        IReadOnlyList<AdminLearningPortalCountRowResponse> ByType);

    private sealed record CourseQualityCounts(
        int CoursePathsMissingTranslations,
        int CourseModulesMissingTranslations,
        int CourseLessonsMissingTranslations);

    private sealed record CourseActivityQualityCounts(
        int PublishedLessonsWithoutActivityBlocks,
        int MalformedActivityBlocksJson,
        int ActivityBlocksWithUnsupportedTargetType,
        int ActivityBlocksWithUnresolvedTargetSlug);

    private sealed record CourseActivityQualityRow(
        string? TargetType,
        string? TargetSlug);

    private sealed record ExerciseQualityCounts(
        int ExercisesMissingTranslations,
        int ExerciseSetsMissingTranslations,
        int ExercisesUnpublishedDrafts,
        int ExerciseSetsUnpublishedDrafts,
        int ExerciseSetsWithoutItems,
        int ExerciseSetsWithUnresolvedExerciseSlugs,
        int ExerciseSetsWithUnresolvedOwnerReferences,
        int ExercisesWithMalformedPrompt,
        int ExercisesWithMalformedAnswerKey,
        int ExercisesMissingExplanations);

    private sealed record ExamPrepQualityCounts(
        int ProfilesMissingTranslations,
        int UnitsMissingTranslations,
        int UnpublishedDrafts,
        int UnitsWithMalformedStrategyOrChecklist,
        int UnitsWithoutActiveProfile);

    private sealed record WritingTemplateQualityCounts(
        int MissingTranslations,
        int UnpublishedDrafts,
        int WithMalformedVariables);

    private sealed record RoleplayTurnQualityRow(
        int SortOrder,
        string SpeakerRole,
        string? ExpectedLearnerAction);
}
