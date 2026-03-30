using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Provides admin-facing visibility into draft and published package batches.
/// </summary>
public sealed class CatalogPackageDraftQueryService(ServerContentDbContext dbContext) : ICatalogPackageDraftQueryService
{
    public async Task<IReadOnlyList<AdminDraftCatalogBatchResponse>> GetBatchesAsync(
        string? clientProductKey,
        CancellationToken cancellationToken)
    {
        string? normalizedClientProductKey = NormalizeOptionalClientProductKey(clientProductKey);
        await EnsureClientProductExistsAsync(normalizedClientProductKey, cancellationToken).ConfigureAwait(false);

        List<PublishedPackageEntity> packages = await dbContext.PublishedPackages
            .Include(package => package.ContentStream)
            .ThenInclude(stream => stream.ClientProduct)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        IEnumerable<PublishedPackageEntity> filteredPackages = packages
            .Where(package => normalizedClientProductKey is null ||
                              string.Equals(package.ContentStream.ClientProduct.Key, normalizedClientProductKey, StringComparison.OrdinalIgnoreCase));

        return filteredPackages
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

    public async Task<AdminDraftCatalogBatchResponse> GetBatchAsync(
        string publicationBatchId,
        string? clientProductKey,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(publicationBatchId);

        string normalizedBatchId = publicationBatchId.Trim();
        string? normalizedClientProductKey = NormalizeOptionalClientProductKey(clientProductKey);
        await EnsureClientProductExistsAsync(normalizedClientProductKey, cancellationToken).ConfigureAwait(false);

        List<PublishedPackageEntity> packages = await dbContext.PublishedPackages
            .Include(package => package.ContentStream)
            .ThenInclude(stream => stream.ClientProduct)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<PublishedPackageEntity> batchPackages = packages
            .Where(package =>
                string.Equals(package.PublicationBatchId, normalizedBatchId, StringComparison.OrdinalIgnoreCase) &&
                (normalizedClientProductKey is null ||
                 string.Equals(package.ContentStream.ClientProduct.Key, normalizedClientProductKey, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (batchPackages.Count == 0)
        {
            throw new KeyNotFoundException($"No package batch '{normalizedBatchId}' was found.");
        }

        PublishedPackageEntity firstPackage = batchPackages[0];
        return BuildBatchResponse(
            firstPackage.ContentStream.ClientProduct.Key,
            firstPackage.PublicationBatchId,
            firstPackage.PublicationStatus,
            batchPackages);
    }

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

    private static AdminDraftCatalogBatchResponse BuildBatchResponse(
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

        return new AdminDraftCatalogBatchResponse(
            clientProductKey,
            publicationBatchId,
            publicationStatus.ToString(),
            firstPackage.Version,
            orderedPackages.Count,
            orderedPackages.Sum(package => package.WordCount),
            orderedPackages.Sum(package => package.EntryCount),
            orderedPackages.Min(package => package.CreatedAtUtc),
            orderedPackages.Max(package => package.PublishedAtUtc),
            orderedPackages.Max(package => package.SupersededAtUtc),
            orderedPackages
                .Select(package => new AdminDraftCatalogBatchPackageResponse(
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
