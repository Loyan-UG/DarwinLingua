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

    Task<AdminCatalogDraftWordsResponse> GetDraftWordsAsync(string? query, CancellationToken cancellationToken);

    Task<AdminCatalogHistoryViewResponse> GetHistoryAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminCatalogRollbackPreviewResponse> GetRollbackPreviewAsync(CancellationToken cancellationToken);
}

internal sealed class WebsiteAdminQueryService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IWebsiteAdminQueryService
{
    public async Task<AdminCatalogDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordEntrySummary wordSummary = await GetWordEntrySummaryAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

        int topicCount = await dbContext.Topics.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        ContentPackageSummary packageSummary = await GetContentPackageSummaryAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

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
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        WordEntrySummary wordSummary = await GetWordEntrySummaryAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

        int topicCount = await dbContext.Topics.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        int scenarioLessonCount = await dbContext.ScenarioLessons.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        int conversationStarterPackCount = await dbContext.ConversationStarterPacks.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        int eventPreparationPackCount = await dbContext.EventPreparationPacks.AsNoTracking()
            .CountAsync(cancellationToken)
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

        UserReportSummary userReportSummary = await GetUserReportSummaryAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

        int userBlockCount = await dbContext.UserBlocks.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        int moderationDecisionAuditCount = await dbContext.ModerationDecisionAudits.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        ContentPackageSummary packageSummary = await GetContentPackageSummaryAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);

        return new AdminSystemReportResponse(
            DateTime.UtcNow,
            new AdminCatalogSystemReportResponse(
                wordSummary.ActiveCount,
                wordSummary.DraftCount,
                topicCount,
                scenarioLessonCount,
                conversationStarterPackCount,
                eventPreparationPackCount),
            new AdminSocialSystemReportResponse(
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
                partnerRequestSummary.PendingCount),
            new AdminModerationSystemReportResponse(
                userReportSummary.TotalCount,
                userReportSummary.PendingCount,
                userBlockCount,
                moderationDecisionAuditCount),
            new AdminOperationsSystemReportResponse(
                packageSummary.ImportedCount,
                packageSummary.FailedCount,
                packageSummary.LastImportAtUtc));
    }

    public async Task<AdminCatalogImportsResponse> GetImportsAsync(string? statusFilter, CancellationToken cancellationToken)
    {
        string? normalizedStatusFilter = NormalizeFilter(statusFilter);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        IQueryable<ContentOps.Domain.Entities.ContentPackage> query = dbContext.ContentPackages.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(normalizedStatusFilter))
        {
            query = query.Where(package => package.Status.ToString() == normalizedStatusFilter);
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

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        IQueryable<ContentOps.Domain.Entities.ContentPackage> query = dbContext.ContentPackages.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(normalizedStatusFilter))
        {
            query = query.Where(package => package.Status.ToString() == normalizedStatusFilter);
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
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        int draftWordCount = await dbContext.WordEntries.AsNoTracking()
            .CountAsync(word => word.PublicationStatus == PublicationStatus.Draft, cancellationToken)
            .ConfigureAwait(false);

        int importedPackageCount = await dbContext.ContentPackages.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminCatalogRollbackPreviewResponse(
            draftWordCount,
            importedPackageCount,
            "Rollback actions are intentionally gated behind an explicit confirmation step. This screen is the approved modal baseline before destructive operations are implemented.");
    }

    private static string? NormalizeFilter(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
