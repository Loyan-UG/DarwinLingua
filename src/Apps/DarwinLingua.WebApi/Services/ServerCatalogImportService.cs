using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Executes one shared-catalog import and then republishes mobile package payloads.
/// </summary>
public sealed class ServerCatalogImportService(
    IContentImportService contentImportService,
    ICatalogPackagePublisher catalogPackagePublisher,
    ServerContentDbContext serverContentDbContext) : IServerCatalogImportService
{
    /// <inheritdoc />
    public async Task<AdminImportCatalogResponse> ImportAndPublishAsync(
        AdminImportCatalogRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FilePath);

        string clientProductKey = await ResolveClientProductKeyAsync(request.ClientProductKey, cancellationToken).ConfigureAwait(false);
        ImportContentPackageResult importResult = await contentImportService
            .ImportAsync(new ImportContentPackageRequest(request.FilePath), cancellationToken)
            .ConfigureAwait(false);

        CatalogPackagePublicationResult? publicationResult = null;
        if (importResult.IsSuccess)
        {
            publicationResult = await catalogPackagePublisher
                .PublishAsync(clientProductKey, cancellationToken)
                .ConfigureAwait(false);
        }

        ContentImportReceiptEntity receipt = new()
        {
            Id = Guid.NewGuid(),
            ClientProductKey = clientProductKey,
            SourceFilePath = Path.GetFullPath(request.FilePath),
            SourceFileName = Path.GetFileName(request.FilePath),
            ImportedPackageId = importResult.PackageId,
            ImportedPackageName = importResult.PackageName,
            ImportStatus = importResult.Status,
            TotalEntries = importResult.TotalEntries,
            ImportedEntries = importResult.ImportedEntries,
            SkippedDuplicateEntries = importResult.SkippedDuplicateEntries,
            InvalidEntries = importResult.InvalidEntries,
            WarningCount = importResult.WarningCount,
            IssueSummary = string.Join(" | ", importResult.Issues.Select(issue => issue.Message).Take(20)),
            PublishedPackageCount = publicationResult?.PackageIds.Count ?? 0,
            PublishedPackageIds = publicationResult is null ? string.Empty : string.Join(",", publicationResult.PackageIds),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
        };

        serverContentDbContext.ContentImportReceipts.Add(receipt);
        await serverContentDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AdminImportCatalogResponse(
            importResult.IsSuccess,
            clientProductKey,
            importResult.PackageId,
            importResult.PackageName,
            importResult.Status,
            importResult.TotalEntries,
            importResult.ImportedEntries,
            importResult.SkippedDuplicateEntries,
            importResult.InvalidEntries,
            importResult.WarningCount,
            publicationResult?.PackageIds ?? [],
            importResult.ImportedLemmas,
            importResult.Issues.Select(issue => issue.Message).ToArray());
    }

    private async Task<string> ResolveClientProductKeyAsync(string? clientProductKey, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(clientProductKey))
        {
            bool exists = await serverContentDbContext.ClientProducts
                .AnyAsync(product => product.Key == clientProductKey.Trim() && product.IsActive, cancellationToken)
                .ConfigureAwait(false);

            if (!exists)
            {
                throw new KeyNotFoundException($"No active client product was found for '{clientProductKey}'.");
            }

            return clientProductKey.Trim();
        }

        List<string> activeProductKeys = await serverContentDbContext.ClientProducts
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
