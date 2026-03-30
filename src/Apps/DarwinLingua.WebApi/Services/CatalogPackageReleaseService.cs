using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Promotes one staged package batch to published status and supersedes older published batches.
/// </summary>
public sealed class CatalogPackageReleaseService(
    ServerContentDbContext dbContext,
    IContentPublicationAuditService auditService) : ICatalogPackageReleaseService
{
    public async Task<AdminPublishCatalogResponse> PublishAsync(
        AdminPublishCatalogRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string clientProductKey = await ResolveClientProductKeyAsync(request.ClientProductKey, cancellationToken).ConfigureAwait(false);

        List<PublishedPackageEntity> candidateDrafts = await dbContext.PublishedPackages
            .Include(package => package.ContentStream)
            .ThenInclude(stream => stream.ClientProduct)
            .Where(package =>
                package.ContentStream.ClientProduct.Key == clientProductKey &&
                package.PublicationStatus == PackagePublicationStatus.Draft)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        candidateDrafts = candidateDrafts
            .OrderByDescending(package => package.CreatedAtUtc)
            .ThenBy(package => package.PackageId, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (candidateDrafts.Count == 0)
        {
            return new AdminPublishCatalogResponse(
                false,
                clientProductKey,
                string.Empty,
                string.Empty,
                [],
                [],
                ["No staged draft package batch exists for this client product."]);
        }

        string publicationBatchId = string.IsNullOrWhiteSpace(request.PublicationBatchId)
            ? candidateDrafts.Select(package => package.PublicationBatchId).First()
            : request.PublicationBatchId.Trim();

        List<PublishedPackageEntity> draftBatch = candidateDrafts
            .Where(package => package.PublicationBatchId.Equals(publicationBatchId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(package => package.PackageId, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (draftBatch.Count == 0)
        {
            return new AdminPublishCatalogResponse(
                false,
                clientProductKey,
                publicationBatchId,
                string.Empty,
                [],
                [],
                [$"No staged draft package batch '{publicationBatchId}' exists for this client product."]);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        List<string> supersededPackageIds = [];

        List<PublishedPackageEntity> previouslyPublished = await dbContext.PublishedPackages
            .Include(package => package.ContentStream)
            .ThenInclude(stream => stream.ClientProduct)
            .Where(package =>
                package.ContentStream.ClientProduct.Key == clientProductKey &&
                package.PublicationStatus == PackagePublicationStatus.Published)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        previouslyPublished = previouslyPublished
            .Where(package => !package.PublicationBatchId.Equals(publicationBatchId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (PublishedPackageEntity package in previouslyPublished)
        {
            package.PublicationStatus = PackagePublicationStatus.Superseded;
            package.SupersededAtUtc = now;
            package.UpdatedAtUtc = now;
            supersededPackageIds.Add(package.PackageId);
        }

        foreach (PublishedPackageEntity package in draftBatch)
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
                ContentPublicationEventType.Publish,
                draftBatch.Select(package => package.PackageId),
                previouslyPublished.Select(package => package.PublicationBatchId).Distinct(StringComparer.OrdinalIgnoreCase),
                $"Published batch version {draftBatch[0].Version}.",
                cancellationToken)
            .ConfigureAwait(false);

        return new AdminPublishCatalogResponse(
            true,
            clientProductKey,
            publicationBatchId,
            draftBatch[0].Version,
            draftBatch.Select(package => package.PackageId).ToArray(),
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
