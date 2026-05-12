using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.ContentOps.Domain.Enums;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.WebApi.Models;
using Microsoft.EntityFrameworkCore;
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

        Task<List<AdminLearningPortalCountRowResponse>> countsByTypeTask = GetLearningContentCountsByTypeAsync(dbContext, cancellationToken);
        Task<List<AdminLearningPortalCountRowResponse>> countsByCefrTask = GetLearningContentCountsByCefrAsync(dbContext, cancellationToken);
        Task<List<AdminLearningPortalCountRowResponse>> grammarByCategoryTask = CountByAsync(
            dbContext.GrammarTopics.AsNoTracking(),
            topic => topic.GrammarCategory,
            cancellationToken);
        Task<List<AdminLearningPortalCountRowResponse>> expressionsByTypeTask = CountByAsync(
            dbContext.ExpressionEntries.AsNoTracking(),
            expression => expression.ExpressionType,
            cancellationToken);
        Task<List<AdminLearningPortalCountRowResponse>> expressionsByRegisterTask = CountByAsync(
            dbContext.ExpressionEntries.AsNoTracking(),
            expression => expression.Register,
            cancellationToken);
        Task<List<AdminLearningPortalCountRowResponse>> exercisesByTypeTask = CountByAsync(
            dbContext.Exercises.AsNoTracking(),
            exercise => exercise.ExerciseType,
            cancellationToken);
        Task<List<AdminLearningPortalCountRowResponse>> exercisesBySkillTask = CountByAsync(
            dbContext.Exercises.AsNoTracking(),
            exercise => exercise.TargetSkill,
            cancellationToken);
        Task<List<AdminLearningPortalCountRowResponse>> lessonsByCourseTask = CountByAsync(
            dbContext.CourseLessons.AsNoTracking(),
            lesson => lesson.CoursePathSlug,
            cancellationToken);
        Task<List<AdminLearningPortalCountRowResponse>> lessonsByCefrTask = CountValueByAsync(
            dbContext.CourseLessons.AsNoTracking(),
            lesson => lesson.CefrLevel,
            cancellationToken);
        Task<List<AdminLearningPortalCountRowResponse>> lessonsByModuleTask = CountByAsync(
            dbContext.CourseLessons.AsNoTracking(),
            lesson => lesson.ModuleSlug,
            cancellationToken);
        Task<List<AdminLearningPortalCountRowResponse>> examByProfileTask = CountByAsync(
            dbContext.ExamPrepUnits.AsNoTracking(),
            unit => unit.ExamProfileKey,
            cancellationToken);
        Task<List<AdminLearningPortalCountRowResponse>> writingByCategoryTask = CountByAsync(
            dbContext.WritingTemplates.AsNoTracking(),
            template => template.Category,
            cancellationToken);
        Task<List<AdminLearningPortalCountRowResponse>> writingByRegisterTask = CountByAsync(
            dbContext.WritingTemplates.AsNoTracking(),
            template => template.Register,
            cancellationToken);
        Task<List<AdminLearningPortalCountRowResponse>> culturalByCategoryTask = CountByAsync(
            dbContext.CulturalNotes.AsNoTracking(),
            note => note.Category,
            cancellationToken);

        await Task.WhenAll(
            countsByTypeTask,
            countsByCefrTask,
            grammarByCategoryTask,
            expressionsByTypeTask,
            expressionsByRegisterTask,
            exercisesByTypeTask,
            exercisesBySkillTask,
            lessonsByCourseTask,
            lessonsByCefrTask,
            lessonsByModuleTask,
            examByProfileTask,
            writingByCategoryTask,
            writingByRegisterTask,
            culturalByCategoryTask).ConfigureAwait(false);

        LearningPortalQualitySummary qualitySummary = await GetLearningPortalQualitySummaryAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

        return new AdminLearningPortalSystemReportResponse(
            await countsByTypeTask.ConfigureAwait(false),
            await countsByCefrTask.ConfigureAwait(false),
            await grammarByCategoryTask.ConfigureAwait(false),
            await expressionsByTypeTask.ConfigureAwait(false),
            await expressionsByRegisterTask.ConfigureAwait(false),
            await exercisesByTypeTask.ConfigureAwait(false),
            await exercisesBySkillTask.ConfigureAwait(false),
            await lessonsByCourseTask.ConfigureAwait(false),
            await lessonsByCefrTask.ConfigureAwait(false),
            await lessonsByModuleTask.ConfigureAwait(false),
            await examByProfileTask.ConfigureAwait(false),
            await writingByCategoryTask.ConfigureAwait(false),
            await writingByRegisterTask.ConfigureAwait(false),
            await culturalByCategoryTask.ConfigureAwait(false),
            qualitySummary.UnresolvedLinkedWordCount,
            qualitySummary.UnresolvedLinkedContentReferenceCount,
            qualitySummary.MissingTranslationCount,
            qualitySummary.UnpublishedDraftCount,
            qualitySummary.GrammarTopicsMissingExercises,
            qualitySummary.CourseLessonsMissingExerciseSets,
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

    private static async Task<List<AdminLearningPortalCountRowResponse>> GetLearningContentCountsByTypeAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        List<AdminLearningPortalCountRowResponse> rows =
        [
            new("grammar-topic", await dbContext.GrammarTopics.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false)),
            new("expression", await dbContext.ExpressionEntries.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false)),
            new("exercise", await dbContext.Exercises.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false)),
            new("exercise-set", await dbContext.ExerciseSets.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false)),
            new("course", await dbContext.CoursePaths.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false)),
            new("course-module", await dbContext.CourseModules.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false)),
            new("course-lesson", await dbContext.CourseLessons.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false)),
            new("exam-profile", await dbContext.ExamProfiles.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false)),
            new("exam-prep-unit", await dbContext.ExamPrepUnits.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false)),
            new("writing-template", await dbContext.WritingTemplates.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false)),
            new("cultural-note", await dbContext.CulturalNotes.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false)),
        ];

        return rows;
    }

    private static async Task<List<AdminLearningPortalCountRowResponse>> GetLearningContentCountsByCefrAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        List<AdminLearningPortalCountRowResponse> rows = [];
        rows.AddRange(await CountValueByAsync(dbContext.GrammarTopics.AsNoTracking(), topic => topic.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByAsync(dbContext.ExpressionEntries.AsNoTracking(), expression => expression.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByAsync(dbContext.Exercises.AsNoTracking(), exercise => exercise.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByAsync(dbContext.ExerciseSets.AsNoTracking(), set => set.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByAsync(dbContext.CourseLessons.AsNoTracking(), lesson => lesson.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByAsync(dbContext.ExamPrepUnits.AsNoTracking(), unit => unit.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByAsync(dbContext.WritingTemplates.AsNoTracking(), template => template.CefrLevel, cancellationToken).ConfigureAwait(false));
        rows.AddRange(await CountValueByAsync(dbContext.CulturalNotes.AsNoTracking(), note => note.CefrLevel, cancellationToken).ConfigureAwait(false));

        return rows
            .GroupBy(static row => row.Key, StringComparer.Ordinal)
            .Select(static group => new AdminLearningPortalCountRowResponse(group.Key, group.Sum(row => row.Count)))
            .OrderBy(static row => row.Key, StringComparer.Ordinal)
            .ToList();
    }

    private static async Task<LearningPortalQualitySummary> GetLearningPortalQualitySummaryAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        HashSet<string> wordKeys = (await dbContext.WordEntries
                .AsNoTracking()
                .Select(static word => word.NormalizedLemma)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false))
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> grammarSlugs = await GetSlugSetAsync(dbContext.GrammarTopics.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> expressionSlugs = await GetSlugSetAsync(dbContext.ExpressionEntries.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> dialogueSlugs = await GetSlugSetAsync(dbContext.DialogueLessons.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> talkTopicSlugs = await GetSlugSetAsync(dbContext.TalkTopics.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> exerciseSlugs = await GetSlugSetAsync(dbContext.Exercises.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> exerciseSetSlugs = await GetSlugSetAsync(dbContext.ExerciseSets.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> courseLessonSlugs = await GetSlugSetAsync(dbContext.CourseLessons.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> examPrepSlugs = await GetSlugSetAsync(dbContext.ExamPrepUnits.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);
        HashSet<string> writingTemplateSlugs = await GetSlugSetAsync(dbContext.WritingTemplates.Select(static item => item.Slug), cancellationToken).ConfigureAwait(false);

        List<AdminLearningPortalIssueRowResponse> issues = [];

        int unresolvedWordCount = 0;
        int unresolvedContentCount = 0;

        GrammarLinkedWord[] grammarLinkedWords = await dbContext.Set<GrammarLinkedWord>()
            .AsNoTracking()
            .Where(static item => item.WordSlug != null)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        foreach (GrammarLinkedWord link in grammarLinkedWords)
        {
            unresolvedWordCount += AddIssueIfMissing(wordKeys, link.WordSlug!, "Grammar linked word", link.GrammarTopicId.ToString(), issues);
        }

        ExpressionLinkedWord[] expressionLinkedWords = await dbContext.Set<ExpressionLinkedWord>()
            .AsNoTracking()
            .Where(static item => item.WordSlug != null)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        foreach (ExpressionLinkedWord link in expressionLinkedWords)
        {
            unresolvedWordCount += AddIssueIfMissing(wordKeys, link.WordSlug!, "Expression linked word", link.ExpressionEntryId.ToString(), issues);
        }

        unresolvedContentCount += await CountMissingSlugLinksAsync(dbContext.Set<GrammarPrerequisiteLink>(), grammarSlugs, "Grammar prerequisite", issues, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountMissingSlugLinksAsync(dbContext.Set<GrammarRelatedTopicLink>(), grammarSlugs, "Grammar related topic", issues, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountMissingSlugLinksAsync(dbContext.Set<GrammarLinkedDialogue>(), dialogueSlugs, "Grammar linked dialogue", issues, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountMissingSlugLinksAsync(dbContext.Set<GrammarLinkedTalkTopic>(), talkTopicSlugs, "Grammar linked Talk Topic", issues, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountMissingSlugLinksAsync(dbContext.Set<GrammarLinkedExercise>(), exerciseSlugs, "Grammar linked exercise", issues, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountMissingExpressionLinksAsync(dbContext, expressionSlugs, exerciseSlugs, issues, cancellationToken).ConfigureAwait(false);
        unresolvedContentCount += await CountJsonLinkIssuesAsync(dbContext, grammarSlugs, expressionSlugs, dialogueSlugs, talkTopicSlugs, exerciseSlugs, exerciseSetSlugs, courseLessonSlugs, examPrepSlugs, writingTemplateSlugs, issues, cancellationToken).ConfigureAwait(false);

        int missingTranslationCount =
            await dbContext.GrammarSections.AsNoTracking().CountAsync(section => !section.Translations.Any(), cancellationToken).ConfigureAwait(false) +
            await dbContext.GrammarExamples.AsNoTracking().CountAsync(example => !example.Translations.Any(), cancellationToken).ConfigureAwait(false) +
            await dbContext.GrammarCommonMistakes.AsNoTracking().CountAsync(item => !item.Translations.Any(), cancellationToken).ConfigureAwait(false) +
            await dbContext.GrammarRuleSummaries.AsNoTracking().CountAsync(item => !item.Translations.Any(), cancellationToken).ConfigureAwait(false) +
            await dbContext.GrammarExceptionNotes.AsNoTracking().CountAsync(item => !item.Translations.Any(), cancellationToken).ConfigureAwait(false) +
            await dbContext.ExpressionEntries.AsNoTracking().CountAsync(item => !item.Meanings.Any(), cancellationToken).ConfigureAwait(false) +
            await dbContext.ExpressionExamples.AsNoTracking().CountAsync(item => !item.Translations.Any(), cancellationToken).ConfigureAwait(false) +
            await dbContext.ExpressionWarnings.AsNoTracking().CountAsync(item => !item.Translations.Any(), cancellationToken).ConfigureAwait(false);

        int draftCount =
            await dbContext.GrammarTopics.AsNoTracking().CountAsync(item => item.PublicationStatus != PublicationStatus.Active, cancellationToken).ConfigureAwait(false) +
            await dbContext.ExpressionEntries.AsNoTracking().CountAsync(item => item.PublicationStatus != PublicationStatus.Active, cancellationToken).ConfigureAwait(false) +
            await dbContext.Exercises.AsNoTracking().CountAsync(item => item.PublicationStatus != PublicationStatus.Active, cancellationToken).ConfigureAwait(false) +
            await dbContext.ExerciseSets.AsNoTracking().CountAsync(item => item.PublicationStatus != PublicationStatus.Active, cancellationToken).ConfigureAwait(false) +
            await dbContext.CoursePaths.AsNoTracking().CountAsync(item => item.PublicationStatus != PublicationStatus.Active, cancellationToken).ConfigureAwait(false) +
            await dbContext.CourseModules.AsNoTracking().CountAsync(item => item.PublicationStatus != PublicationStatus.Active, cancellationToken).ConfigureAwait(false) +
            await dbContext.CourseLessons.AsNoTracking().CountAsync(item => item.PublicationStatus != PublicationStatus.Active, cancellationToken).ConfigureAwait(false) +
            await dbContext.ExamProfiles.AsNoTracking().CountAsync(item => item.PublicationStatus != PublicationStatus.Active, cancellationToken).ConfigureAwait(false) +
            await dbContext.ExamPrepUnits.AsNoTracking().CountAsync(item => item.PublicationStatus != PublicationStatus.Active, cancellationToken).ConfigureAwait(false) +
            await dbContext.WritingTemplates.AsNoTracking().CountAsync(item => item.PublicationStatus != PublicationStatus.Active, cancellationToken).ConfigureAwait(false) +
            await dbContext.CulturalNotes.AsNoTracking().CountAsync(item => item.PublicationStatus != PublicationStatus.Active, cancellationToken).ConfigureAwait(false);

        int grammarWithoutExercises = await dbContext.GrammarTopics
            .AsNoTracking()
            .CountAsync(topic => !topic.LinkedExercises.Any(), cancellationToken)
            .ConfigureAwait(false);
        CourseLesson[] lessons = await dbContext.CourseLessons.AsNoTracking().ToArrayAsync(cancellationToken).ConfigureAwait(false);
        int lessonsWithoutExerciseSets = lessons.Count(static lesson => ReadStringArray(lesson.LinkedExerciseSetSlugsJson).Count == 0);

        return new LearningPortalQualitySummary(
            unresolvedWordCount,
            unresolvedContentCount,
            missingTranslationCount,
            draftCount,
            grammarWithoutExercises,
            lessonsWithoutExerciseSets,
            issues.Take(30).ToArray());
    }

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

    private static async Task<int> CountMissingExpressionLinksAsync(
        DarwinLinguaDbContext dbContext,
        IReadOnlySet<string> expressionSlugs,
        IReadOnlySet<string> exerciseSlugs,
        List<AdminLearningPortalIssueRowResponse> issues,
        CancellationToken cancellationToken)
    {
        int count = 0;
        RelatedExpressionLink[] relatedExpressions = await dbContext.Set<RelatedExpressionLink>()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        foreach (RelatedExpressionLink link in relatedExpressions)
        {
            count += AddIssueIfMissing(expressionSlugs, link.TargetSlug, "Related expression", link.ExpressionEntryId.ToString(), issues);
        }

        ExpressionLinkedExercise[] linkedExercises = await dbContext.Set<ExpressionLinkedExercise>()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        foreach (ExpressionLinkedExercise link in linkedExercises)
        {
            count += AddIssueIfMissing(exerciseSlugs, link.TargetSlug, "Expression linked exercise", link.ExpressionEntryId.ToString(), issues);
        }

        return count;
    }

    private static async Task<int> CountJsonLinkIssuesAsync(
        DarwinLinguaDbContext dbContext,
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

        WritingTemplate[] writingTemplates = await dbContext.WritingTemplates.AsNoTracking().ToArrayAsync(cancellationToken).ConfigureAwait(false);
        foreach (WritingTemplate template in writingTemplates)
        {
            count += CountJsonTargets(template.LinkedGrammarTopicSlugsJson, grammarSlugs, "Writing linked grammar", template.Slug, issues);
            count += CountJsonTargets(template.LinkedWordSlugsJson, EmptyStringSet, "Writing linked word", template.Slug, issues);
            count += CountJsonTargets(template.LinkedExpressionSlugsJson, expressionSlugs, "Writing linked expression", template.Slug, issues);
            count += CountJsonTargets(template.LinkedExerciseSlugsJson, exerciseSlugs, "Writing linked exercise", template.Slug, issues);
        }

        CulturalNote[] culturalNotes = await dbContext.CulturalNotes.AsNoTracking().ToArrayAsync(cancellationToken).ConfigureAwait(false);
        foreach (CulturalNote note in culturalNotes)
        {
            count += CountJsonTargets(note.LinkedDialogueSlugsJson, dialogueSlugs, "Cultural linked dialogue", note.Slug, issues);
            count += CountJsonTargets(note.LinkedExpressionSlugsJson, expressionSlugs, "Cultural linked expression", note.Slug, issues);
            count += CountJsonTargets(note.LinkedWritingTemplateSlugsJson, writingTemplateSlugs, "Cultural linked writing template", note.Slug, issues);
            count += CountJsonTargets(note.LinkedTalkTopicSlugsJson, talkTopicSlugs, "Cultural linked Talk Topic", note.Slug, issues);
            count += CountJsonTargets(note.LinkedCourseLessonSlugsJson, courseLessonSlugs, "Cultural linked lesson", note.Slug, issues);
        }

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
        IReadOnlyList<AdminLearningPortalIssueRowResponse> SampleIssues);
}
