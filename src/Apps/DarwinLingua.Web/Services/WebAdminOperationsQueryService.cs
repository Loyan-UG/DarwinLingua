using DarwinLingua.ContentOps.Domain.Enums;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Services;

public interface IWebAdminOperationsQueryService
{
    Task<AdminImportsPageViewModel> GetImportsAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminDraftWordsPageViewModel> GetDraftWordsAsync(string? query, CancellationToken cancellationToken);

    Task<AdminHistoryPageViewModel> GetHistoryAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminRollbackPageViewModel> GetRollbackPreviewAsync(CancellationToken cancellationToken);
}

internal sealed class WebAdminOperationsQueryService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IWebAdminOperationsQueryService
{
    public async Task<AdminImportsPageViewModel> GetImportsAsync(string? statusFilter, CancellationToken cancellationToken)
    {
        string? normalizedStatusFilter = NormalizeFilter(statusFilter);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        IQueryable<ContentOps.Domain.Entities.ContentPackage> query = dbContext.ContentPackages.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(normalizedStatusFilter))
        {
            query = query.Where(package => package.Status.ToString() == normalizedStatusFilter);
        }

        AdminContentPackageListItemViewModel[] packages = await query
            .OrderByDescending(package => package.CreatedAtUtc)
            .Take(40)
            .Select(package => new AdminContentPackageListItemViewModel(
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

        return new AdminImportsPageViewModel(normalizedStatusFilter, packages);
    }

    public async Task<AdminDraftWordsPageViewModel> GetDraftWordsAsync(string? queryText, CancellationToken cancellationToken)
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

        AdminDraftWordListItemViewModel[] words = await query
            .OrderBy(word => word.NormalizedLemma)
            .Take(50)
            .Select(word => new AdminDraftWordListItemViewModel(
                word.PublicId,
                word.Lemma,
                word.PartOfSpeech.ToString(),
                word.PrimaryCefrLevel.ToString(),
                word.PublicationStatus.ToString()))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminDraftWordsPageViewModel(normalizedQuery, words);
    }

    public async Task<AdminHistoryPageViewModel> GetHistoryAsync(string? statusFilter, CancellationToken cancellationToken)
    {
        string? normalizedStatusFilter = NormalizeFilter(statusFilter);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        IQueryable<ContentOps.Domain.Entities.ContentPackage> query = dbContext.ContentPackages.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(normalizedStatusFilter))
        {
            query = query.Where(package => package.Status.ToString() == normalizedStatusFilter);
        }

        AdminHistoryItemViewModel[] items = await query
            .OrderByDescending(package => package.CreatedAtUtc)
            .Take(30)
            .Select(package => new AdminHistoryItemViewModel(
                package.PackageId,
                package.PackageVersion,
                package.Status.ToString(),
                package.TotalEntries,
                package.InsertedEntries,
                package.InvalidEntries,
                package.CreatedAtUtc))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminHistoryPageViewModel(normalizedStatusFilter, items);
    }

    public async Task<AdminRollbackPageViewModel> GetRollbackPreviewAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        int draftWordCount = await dbContext.WordEntries.AsNoTracking()
            .CountAsync(word => word.PublicationStatus == PublicationStatus.Draft, cancellationToken)
            .ConfigureAwait(false);

        int importedPackageCount = await dbContext.ContentPackages.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminRollbackPageViewModel(
            draftWordCount,
            importedPackageCount,
            "Rollback actions are intentionally gated behind an explicit confirmation step. This screen is the approved modal baseline before destructive operations are implemented.");
    }

    private static string? NormalizeFilter(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
