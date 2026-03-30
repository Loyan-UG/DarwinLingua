using DarwinLingua.WebApi.Configuration;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Builds mobile content manifests from PostgreSQL-backed published-package data.
/// </summary>
public sealed class DatabaseMobileContentManifestService(
    ServerContentDbContext dbContext,
    IOptions<ServerContentOptions> options) : IMobileContentManifestService
{
    /// <inheritdoc />
    public MobileContentManifestResponse GetGlobalManifest(string? clientProductKey)
    {
        ClientProductEntity product = ResolveProduct(clientProductKey);
        List<PublishedPackageEntity> packages = ResolvePackages(product.Key);
        return BuildManifest(product, "all", "all", packages);
    }

    /// <inheritdoc />
    public IReadOnlyList<MobileContentAreaSummaryResponse> GetAreas(string? clientProductKey)
    {
        ClientProductEntity product = ResolveProduct(clientProductKey);

        return dbContext.PublishedPackages
            .AsNoTracking()
            .Include(package => package.ContentStream)
            .Where(package => package.ContentStream.ClientProduct.Key == product.Key)
            .GroupBy(package => package.ContentStream.ContentAreaKey)
            .Select(group => new MobileContentAreaSummaryResponse(
                group.Key,
                group.Count(),
                group.Sum(package => package.WordCount),
                group.Select(package => package.ContentStream.SliceKey)
                    .Distinct()
                    .OrderBy(sliceKey => sliceKey)
                    .ToArray()))
            .OrderBy(area => area.ContentAreaKey)
            .ToArray();
    }

    /// <inheritdoc />
    public MobileContentManifestResponse GetAreaManifest(string? clientProductKey, string areaKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(areaKey);

        ClientProductEntity product = ResolveProduct(clientProductKey);
        List<PublishedPackageEntity> packages = ResolvePackages(product.Key, areaKey: areaKey);
        return BuildManifest(product, areaKey.Trim(), "all", packages);
    }

    /// <inheritdoc />
    public MobileContentManifestResponse GetCefrManifest(string? clientProductKey, string level)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(level);

        ClientProductEntity product = ResolveProduct(clientProductKey);
        string normalizedSliceKey = $"cefr:{level.Trim().ToLowerInvariant()}";
        List<PublishedPackageEntity> packages = ResolvePackages(product.Key, areaKey: "catalog", sliceKey: normalizedSliceKey);
        return BuildManifest(product, "catalog", normalizedSliceKey, packages);
    }

    /// <inheritdoc />
    public PublishedContentPackageResponse GetPackage(string? clientProductKey, string packageId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        ClientProductEntity product = ResolveProduct(clientProductKey);

        PublishedPackageEntity package = dbContext.PublishedPackages
            .AsNoTracking()
            .Include(existingPackage => existingPackage.ContentStream)
            .ThenInclude(stream => stream.ClientProduct)
            .FirstOrDefault(existingPackage =>
                existingPackage.PackageId == packageId.Trim() &&
                existingPackage.ContentStream.ClientProduct.Key == product.Key)
            ?? throw new KeyNotFoundException($"No package was found for '{packageId}'.");

        return MapPackage(package);
    }

    private MobileContentManifestResponse BuildManifest(
        ClientProductEntity product,
        string areaKey,
        string sliceKey,
        IReadOnlyList<PublishedPackageEntity> packages)
    {
        return new MobileContentManifestResponse(
            product.Key,
            product.LearningLanguageCode,
            areaKey,
            sliceKey,
            options.Value.DefaultSchemaVersion,
            DateTimeOffset.UtcNow,
            packages.Count,
            packages.Sum(package => package.WordCount),
            packages.Select(MapPackage).ToArray());
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

    private List<PublishedPackageEntity> ResolvePackages(
        string clientProductKey,
        string? areaKey = null,
        string? sliceKey = null)
    {
        IQueryable<PublishedPackageEntity> query = dbContext.PublishedPackages
            .AsNoTracking()
            .Include(package => package.ContentStream)
            .ThenInclude(stream => stream.ClientProduct)
            .Where(package => package.ContentStream.ClientProduct.Key == clientProductKey);

        if (!string.IsNullOrWhiteSpace(areaKey))
        {
            query = query.Where(package => package.ContentStream.ContentAreaKey == areaKey.Trim());
        }

        if (!string.IsNullOrWhiteSpace(sliceKey))
        {
            query = query.Where(package => package.ContentStream.SliceKey == sliceKey.Trim());
        }

        return query
            .AsEnumerable()
            .GroupBy(package => package.ContentStreamId)
            .Select(group => group
                .OrderByDescending(package => package.CreatedAtUtc)
                .ThenBy(package => package.PackageId, StringComparer.OrdinalIgnoreCase)
                .First())
            .OrderByDescending(package => package.CreatedAtUtc)
            .ThenBy(package => package.PackageId, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private PublishedContentPackageResponse MapPackage(PublishedPackageEntity package)
    {
        string baseUrl = options.Value.PublicBaseUrl.TrimEnd('/');

        return new PublishedContentPackageResponse(
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
            $"{baseUrl}/api/mobile/content/packages/{package.PackageId}/download");
    }
}
