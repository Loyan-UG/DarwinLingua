using DarwinLingua.WebApi.Configuration;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Resolves package payload files and compatibility checks from PostgreSQL-backed metadata.
/// </summary>
public sealed class DatabaseMobileContentPackageDeliveryService(
    ServerContentDbContext dbContext,
    IOptions<ServerContentOptions> options,
    IWebHostEnvironment hostEnvironment) : IMobileContentPackageDeliveryService
{
    /// <inheritdoc />
    public ContentPackageDownloadDescriptor GetPackageById(string? clientProductKey, string packageId, int? clientSchemaVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        ClientProductEntity product = ResolveProduct(clientProductKey);

        PublishedPackageEntity package = QueryPackages(product.Key)
            .FirstOrDefault(existingPackage => existingPackage.PackageId.Equals(packageId.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"No package was found for '{packageId}'.");

        EnsureCompatibility(package, clientSchemaVersion);
        return MapDescriptor(package);
    }

    /// <inheritdoc />
    public ContentPackageDownloadDescriptor GetLatestFullPackage(string? clientProductKey, int? clientSchemaVersion)
    {
        ClientProductEntity product = ResolveProduct(clientProductKey);
        PublishedPackageEntity package = ResolveLatestPackage(product.Key, "all", "full");
        EnsureCompatibility(package, clientSchemaVersion);
        return MapDescriptor(package);
    }

    /// <inheritdoc />
    public ContentPackageDownloadDescriptor GetLatestAreaPackage(string? clientProductKey, string areaKey, int? clientSchemaVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(areaKey);

        ClientProductEntity product = ResolveProduct(clientProductKey);
        PublishedPackageEntity package = ResolveLatestPackage(product.Key, areaKey.Trim(), "full");
        EnsureCompatibility(package, clientSchemaVersion);
        return MapDescriptor(package);
    }

    /// <inheritdoc />
    public ContentPackageDownloadDescriptor GetLatestCefrPackage(string? clientProductKey, string level, int? clientSchemaVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(level);

        ClientProductEntity product = ResolveProduct(clientProductKey);
        string sliceKey = $"cefr:{level.Trim().ToLowerInvariant()}";
        PublishedPackageEntity package = ResolveLatestPackage(product.Key, "catalog", sliceKey);
        EnsureCompatibility(package, clientSchemaVersion);
        return MapDescriptor(package);
    }

    private ClientProductEntity ResolveProduct(string? clientProductKey)
    {
        List<ClientProductEntity> activeProducts = dbContext.ClientProducts
            .AsNoTracking()
            .Where(product => product.IsActive)
            .OrderBy(product => product.Key)
            .ToList();

        if (string.IsNullOrWhiteSpace(clientProductKey))
        {
            if (activeProducts.Count == 1)
            {
                return activeProducts[0];
            }

            throw new InvalidOperationException("A client product key is required when multiple active products are configured.");
        }

        return activeProducts.FirstOrDefault(product =>
                product.Key.Equals(clientProductKey.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"No active client product was found for '{clientProductKey}'.");
    }

    private IEnumerable<PublishedPackageEntity> QueryPackages(string clientProductKey)
    {
        return dbContext.PublishedPackages
            .AsNoTracking()
            .Include(package => package.ContentStream)
            .ThenInclude(stream => stream.ClientProduct)
            .Where(package => package.ContentStream.ClientProduct.Key == clientProductKey)
            .AsEnumerable()
            .OrderByDescending(package => package.CreatedAtUtc)
            .ThenBy(package => package.PackageId, StringComparer.OrdinalIgnoreCase);
    }

    private PublishedPackageEntity ResolveLatestPackage(string clientProductKey, string areaKey, string sliceKey)
    {
        return QueryPackages(clientProductKey)
            .FirstOrDefault(package =>
                package.ContentStream.ContentAreaKey.Equals(areaKey, StringComparison.OrdinalIgnoreCase) &&
                package.ContentStream.SliceKey.Equals(sliceKey, StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"No published package was found for area '{areaKey}' and slice '{sliceKey}'.");
    }

    private void EnsureCompatibility(PublishedPackageEntity package, int? clientSchemaVersion)
    {
        if (!clientSchemaVersion.HasValue)
        {
            return;
        }

        if (clientSchemaVersion.Value < package.MinimumAppSchemaVersion)
        {
            throw new MobileContentSchemaCompatibilityException(
                package.PackageId,
                clientSchemaVersion.Value,
                package.MinimumAppSchemaVersion,
                package.SchemaVersion);
        }
    }

    private ContentPackageDownloadDescriptor MapDescriptor(PublishedPackageEntity package)
    {
        string rootPath = options.Value.PackageStorage.RootPath;
        string basePath = Path.IsPathRooted(rootPath)
            ? rootPath
            : Path.Combine(hostEnvironment.ContentRootPath, rootPath);

        string fullPath = Path.GetFullPath(Path.Combine(basePath, package.RelativeDownloadPath));
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"The package payload file was not found for '{package.PackageId}'.", fullPath);
        }

        return new ContentPackageDownloadDescriptor(
            package.PackageId,
            fullPath,
            "application/json",
            $"{package.PackageId}.json",
            new PublishedContentPackageResponse(
                package.PackageId,
                package.ContentStream.ClientProduct.Key,
                package.ContentStream.ContentAreaKey,
                package.ContentStream.SliceKey,
                package.PackageType,
                package.Version,
                package.SchemaVersion,
                package.MinimumAppSchemaVersion,
                package.Checksum,
                package.EntryCount,
                package.WordCount,
                package.CreatedAtUtc,
                $"{options.Value.PublicBaseUrl.TrimEnd('/')}/api/mobile/content/packages/{package.PackageId}/download"));
    }
}
