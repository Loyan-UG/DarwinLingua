using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Configuration;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Removes superseded package batches and their payload files from server storage.
/// </summary>
public sealed class CatalogPackageCleanupService(
    ServerContentDbContext dbContext,
    IWebHostEnvironment hostEnvironment,
    IOptions<ServerContentOptions> options,
    IContentPublicationAuditService auditService) : ICatalogPackageCleanupService
{
    public async Task<AdminDeleteCatalogBatchResponse> DeleteSupersededBatchAsync(
        string publicationBatchId,
        string? clientProductKey,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(publicationBatchId);

        string normalizedBatchId = publicationBatchId.Trim();
        string? normalizedClientProductKey = string.IsNullOrWhiteSpace(clientProductKey) ? null : clientProductKey.Trim();

        List<PublishedPackageEntity> packages = await dbContext.PublishedPackages
            .Include(package => package.ContentStream)
            .ThenInclude(stream => stream.ClientProduct)
            .Where(package => package.PublicationBatchId == normalizedBatchId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (normalizedClientProductKey is not null)
        {
            packages = packages
                .Where(package => string.Equals(package.ContentStream.ClientProduct.Key, normalizedClientProductKey, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (packages.Count == 0)
        {
            return new AdminDeleteCatalogBatchResponse(
                false,
                normalizedClientProductKey ?? string.Empty,
                normalizedBatchId,
                0,
                [],
                [$"No package batch '{normalizedBatchId}' was found."]);
        }

        PackagePublicationStatus status = packages[0].PublicationStatus;
        string resolvedClientProductKey = packages[0].ContentStream.ClientProduct.Key;

        if (status != PackagePublicationStatus.Superseded)
        {
            return new AdminDeleteCatalogBatchResponse(
                false,
                resolvedClientProductKey,
                normalizedBatchId,
                0,
                [],
                [$"Only superseded package batches can be deleted. Current status is '{status}' for '{normalizedBatchId}'."]);
        }

        List<string> deletedPackageIds = [];
        Guid[] deletedEntityIds = packages.Select(package => package.Id).ToArray();
        foreach (PublishedPackageEntity package in packages.OrderBy(package => package.PackageId, StringComparer.OrdinalIgnoreCase))
        {
            deletedPackageIds.Add(package.PackageId);
            TryDeletePayload(package.RelativeDownloadPath);
        }

        await dbContext.PublishedPackages
            .Where(package => deletedEntityIds.Contains(package.Id))
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
        await auditService.RecordAsync(
                resolvedClientProductKey,
                normalizedBatchId,
                ContentPublicationEventType.Cleanup,
                deletedPackageIds,
                [],
                "Deleted superseded package batch and payload files.",
                cancellationToken)
            .ConfigureAwait(false);

        return new AdminDeleteCatalogBatchResponse(
            true,
            resolvedClientProductKey,
            normalizedBatchId,
            deletedPackageIds.Count,
            deletedPackageIds,
            []);
    }

    private void TryDeletePayload(string relativeDownloadPath)
    {
        if (string.IsNullOrWhiteSpace(relativeDownloadPath))
        {
            return;
        }

        string normalizedRelativePath = relativeDownloadPath.Replace('/', Path.DirectorySeparatorChar);
        string configuredRootPath = options.Value.PackageStorage.RootPath;
        string packageRootPath = Path.IsPathRooted(configuredRootPath)
            ? configuredRootPath
            : Path.Combine(hostEnvironment.ContentRootPath, configuredRootPath);
        string fullPath = Path.Combine(packageRootPath, normalizedRelativePath);

        if (!File.Exists(fullPath))
        {
            return;
        }

        try
        {
            File.Delete(fullPath);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
