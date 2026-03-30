using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Re-activates one superseded package batch and supersedes the currently published batch.
/// </summary>
public sealed class CatalogPackageRollbackService(
    ServerContentDbContext dbContext,
    IContentPublicationAuditService auditService) : ICatalogPackageRollbackService
{
    public async Task<AdminRollbackCatalogResponse> RollbackAsync(
        AdminRollbackCatalogRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PublicationBatchId);

        string clientProductKey = await ResolveClientProductKeyAsync(request.ClientProductKey, cancellationToken).ConfigureAwait(false);
        string publicationBatchId = request.PublicationBatchId.Trim();

        List<PublishedPackageEntity> targetBatch = await dbContext.PublishedPackages
            .Include(package => package.ContentStream)
            .ThenInclude(stream => stream.ClientProduct)
            .Where(package =>
                package.ContentStream.ClientProduct.Key == clientProductKey &&
                package.PublicationBatchId == publicationBatchId)
            .OrderBy(package => package.PackageId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (targetBatch.Count == 0)
        {
            return new AdminRollbackCatalogResponse(
                false,
                clientProductKey,
                publicationBatchId,
                string.Empty,
                [],
                [],
                [$"No package batch '{publicationBatchId}' exists for this client product."]);
        }

        PackagePublicationStatus currentStatus = targetBatch[0].PublicationStatus;
        if (currentStatus != PackagePublicationStatus.Superseded)
        {
            return new AdminRollbackCatalogResponse(
                false,
                clientProductKey,
                publicationBatchId,
                string.Empty,
                [],
                [],
                [$"Only superseded package batches can be rolled back. Current status is '{currentStatus}' for '{publicationBatchId}'."]);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        List<PublishedPackageEntity> currentlyPublished = await dbContext.PublishedPackages
            .Include(package => package.ContentStream)
            .ThenInclude(stream => stream.ClientProduct)
            .Where(package =>
                package.ContentStream.ClientProduct.Key == clientProductKey &&
                package.PublicationStatus == PackagePublicationStatus.Published)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<string> supersededPackageIds = [];
        foreach (PublishedPackageEntity package in currentlyPublished)
        {
            package.PublicationStatus = PackagePublicationStatus.Superseded;
            package.SupersededAtUtc = now;
            package.UpdatedAtUtc = now;
            supersededPackageIds.Add(package.PackageId);
        }

        foreach (PublishedPackageEntity package in targetBatch)
        {
            package.PublicationStatus = PackagePublicationStatus.Published;
            package.PublishedAtUtc = now;
            package.SupersededAtUtc = null;
            package.UpdatedAtUtc = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await auditService.RecordAsync(
                clientProductKey,
                publicationBatchId,
                ContentPublicationEventType.Rollback,
                targetBatch.Select(package => package.PackageId),
                currentlyPublished.Select(package => package.PublicationBatchId).Distinct(StringComparer.OrdinalIgnoreCase),
                $"Rolled back batch version {targetBatch[0].Version}.",
                cancellationToken)
            .ConfigureAwait(false);

        return new AdminRollbackCatalogResponse(
            true,
            clientProductKey,
            publicationBatchId,
            targetBatch[0].Version,
            targetBatch.Select(package => package.PackageId).ToArray(),
            supersededPackageIds,
            []);
    }

    private async Task<string> ResolveClientProductKeyAsync(string? clientProductKey, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(clientProductKey))
        {
            bool exists = await dbContext.ClientProducts
                .AnyAsync(product => product.Key == clientProductKey.Trim() && product.IsActive, cancellationToken)
                .ConfigureAwait(false);

            if (!exists)
            {
                throw new KeyNotFoundException($"No active client product was found for '{clientProductKey}'.");
            }

            return clientProductKey.Trim();
        }

        List<string> activeProductKeys = await dbContext.ClientProducts
            .Where(product => product.IsActive)
            .OrderBy(product => product.Key)
            .Select(product => product.Key)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (activeProductKeys.Count == 1)
        {
            return activeProductKeys[0];
        }

        throw new InvalidOperationException("A client product key is required when multiple active products are configured.");
    }
}
