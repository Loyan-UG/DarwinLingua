using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Builds admin-facing publication history and retention summaries for catalog batches.
/// </summary>
public sealed class CatalogPublicationHistoryService(ServerContentDbContext dbContext) : ICatalogPublicationHistoryService
{
    public async Task<IReadOnlyList<AdminCatalogBatchHistoryResponse>> GetHistoryAsync(
        string? clientProductKey,
        CancellationToken cancellationToken)
    {
        string? normalizedClientProductKey = NormalizeOptionalClientProductKey(clientProductKey);
        await EnsureClientProductExistsAsync(normalizedClientProductKey, cancellationToken).ConfigureAwait(false);

        List<PublishedPackageEntity> packages = await LoadPackagesAsync(cancellationToken).ConfigureAwait(false);

        return FilterPackages(packages, normalizedClientProductKey)
            .GroupBy(package => new
            {
                ClientProductKey = package.ContentStream.ClientProduct.Key,
                package.PublicationBatchId,
                package.PublicationStatus,
            })
            .Select(group => BuildBatchResponse(group.Key.ClientProductKey, group.Key.PublicationBatchId, group.Key.PublicationStatus, group))
            .OrderByDescending(batch => batch.CreatedAtUtc)
            .ThenBy(batch => batch.ClientProductKey, StringComparer.OrdinalIgnoreCase)
            .ThenBy(batch => batch.PublicationBatchId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<AdminCatalogBatchHistorySummaryResponse> GetSummaryAsync(
        string? clientProductKey,
        CancellationToken cancellationToken)
    {
        string? normalizedClientProductKey = NormalizeOptionalClientProductKey(clientProductKey);
        await EnsureClientProductExistsAsync(normalizedClientProductKey, cancellationToken).ConfigureAwait(false);

        List<PublishedPackageEntity> packages = await LoadPackagesAsync(cancellationToken).ConfigureAwait(false);

        AdminCatalogBatchHistoryResponse[] batches = FilterPackages(packages, normalizedClientProductKey)
            .GroupBy(package => new
            {
                ClientProductKey = package.ContentStream.ClientProduct.Key,
                package.PublicationBatchId,
                package.PublicationStatus,
            })
            .Select(group => BuildBatchResponse(group.Key.ClientProductKey, group.Key.PublicationBatchId, group.Key.PublicationStatus, group))
            .ToArray();

        AdminCatalogBatchHistoryResponse? latestPublishedBatch = batches
            .Where(batch => string.Equals(batch.PublicationStatus, nameof(PackagePublicationStatus.Published), StringComparison.Ordinal))
            .OrderByDescending(batch => batch.PublishedAtUtc ?? DateTimeOffset.MinValue)
            .ThenByDescending(batch => batch.CreatedAtUtc)
            .FirstOrDefault();

        return new AdminCatalogBatchHistorySummaryResponse(
            normalizedClientProductKey ?? string.Empty,
            batches.Length,
            batches.Count(batch => string.Equals(batch.PublicationStatus, nameof(PackagePublicationStatus.Draft), StringComparison.Ordinal)),
            batches.Count(batch => string.Equals(batch.PublicationStatus, nameof(PackagePublicationStatus.Published), StringComparison.Ordinal)),
            batches.Count(batch => string.Equals(batch.PublicationStatus, nameof(PackagePublicationStatus.Superseded), StringComparison.Ordinal)),
            batches.Count(batch => batch.CanDelete),
            latestPublishedBatch?.PublicationBatchId,
            latestPublishedBatch?.PublishedAtUtc);
    }

    private async Task<List<PublishedPackageEntity>> LoadPackagesAsync(CancellationToken cancellationToken) =>
        await dbContext.PublishedPackages
            .Include(package => package.ContentStream)
            .ThenInclude(stream => stream.ClientProduct)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    private static IEnumerable<PublishedPackageEntity> FilterPackages(
        IEnumerable<PublishedPackageEntity> packages,
        string? clientProductKey) =>
        packages.Where(package => clientProductKey is null ||
                                  string.Equals(package.ContentStream.ClientProduct.Key, clientProductKey, StringComparison.OrdinalIgnoreCase));

    private async Task EnsureClientProductExistsAsync(string? clientProductKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(clientProductKey))
        {
            return;
        }

        bool exists = await dbContext.ClientProducts
            .AnyAsync(product => product.Key == clientProductKey && product.IsActive, cancellationToken)
            .ConfigureAwait(false);

        if (!exists)
        {
            throw new KeyNotFoundException($"No active client product was found for '{clientProductKey}'.");
        }
    }

    private static string? NormalizeOptionalClientProductKey(string? clientProductKey) =>
        string.IsNullOrWhiteSpace(clientProductKey) ? null : clientProductKey.Trim();

    private static AdminCatalogBatchHistoryResponse BuildBatchResponse(
        string clientProductKey,
        string publicationBatchId,
        PackagePublicationStatus publicationStatus,
        IEnumerable<PublishedPackageEntity> packages)
    {
        List<PublishedPackageEntity> orderedPackages = packages
            .OrderBy(package => package.PackageType, StringComparer.OrdinalIgnoreCase)
            .ThenBy(package => package.PackageId, StringComparer.OrdinalIgnoreCase)
            .ToList();

        PublishedPackageEntity firstPackage = orderedPackages[0];

        return new AdminCatalogBatchHistoryResponse(
            clientProductKey,
            publicationBatchId,
            publicationStatus.ToString(),
            firstPackage.Version,
            orderedPackages.Count,
            orderedPackages.Sum(package => package.WordCount),
            orderedPackages.Sum(package => package.EntryCount),
            publicationStatus == PackagePublicationStatus.Superseded,
            orderedPackages.Min(package => package.CreatedAtUtc),
            orderedPackages.Max(package => package.PublishedAtUtc),
            orderedPackages.Max(package => package.SupersededAtUtc),
            orderedPackages
                .Select(package => new AdminCatalogBatchHistoryPackageResponse(
                    package.PackageId,
                    package.PackageType,
                    package.ContentStream.ContentAreaKey,
                    package.ContentStream.SliceKey,
                    package.Version,
                    package.Checksum,
                    package.WordCount,
                    package.EntryCount,
                    package.CreatedAtUtc))
                .ToArray());
    }
}
