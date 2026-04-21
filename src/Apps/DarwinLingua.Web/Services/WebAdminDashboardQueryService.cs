using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Services;

public interface IWebAdminDashboardQueryService
{
    Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken);
}

internal sealed class WebAdminDashboardQueryService(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IWebAdminDashboardQueryService
{
    public async Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        int activeWordCount = await dbContext.WordEntries.AsNoTracking()
            .CountAsync(word => word.PublicationStatus == PublicationStatus.Active, cancellationToken)
            .ConfigureAwait(false);

        int draftWordCount = await dbContext.WordEntries.AsNoTracking()
            .CountAsync(word => word.PublicationStatus == PublicationStatus.Draft, cancellationToken)
            .ConfigureAwait(false);

        int topicCount = await dbContext.Topics.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        int importedPackageCount = await dbContext.ContentPackages.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        int failedPackageCount = await dbContext.ContentPackages.AsNoTracking()
            .CountAsync(package => package.Status.ToString() == "Failed", cancellationToken)
            .ConfigureAwait(false);

        DateTime? lastImportAtUtc = await dbContext.ContentPackages.AsNoTracking()
            .OrderByDescending(package => package.CreatedAtUtc)
            .Select(package => (DateTime?)package.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminDashboardViewModel(
            activeWordCount,
            draftWordCount,
            topicCount,
            importedPackageCount,
            failedPackageCount,
            lastImportAtUtc);
    }
}
