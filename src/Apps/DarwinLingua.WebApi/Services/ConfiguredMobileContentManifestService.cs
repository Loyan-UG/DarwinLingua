using DarwinLingua.WebApi.Configuration;
using DarwinLingua.WebApi.Models;
using Microsoft.Extensions.Options;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Builds mobile content manifests from configuration-backed package metadata.
/// </summary>
public sealed class ConfiguredMobileContentManifestService : IMobileContentManifestService
{
    private readonly ServerContentOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfiguredMobileContentManifestService"/> class.
    /// </summary>
    /// <param name="options">The bound server-content options.</param>
    public ConfiguredMobileContentManifestService(IOptions<ServerContentOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        this.options = options.Value;
    }

    /// <inheritdoc />
    public MobileContentManifestResponse GetGlobalManifest(string? clientProductKey)
    {
        ClientProductOptions product = ResolveProduct(clientProductKey);
        IReadOnlyList<PublishedContentPackageResponse> packages = ResolvePackages(product.Key);
        return BuildManifest(product, "all", "all", packages);
    }

    /// <inheritdoc />
    public IReadOnlyList<MobileContentAreaSummaryResponse> GetAreas(string? clientProductKey)
    {
        ClientProductOptions product = ResolveProduct(clientProductKey);

        return options.Packages
            .Where(package => package.ClientProductKey.Equals(product.Key, StringComparison.OrdinalIgnoreCase))
            .GroupBy(package => package.ContentAreaKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => new MobileContentAreaSummaryResponse(
                group.Key,
                group.Count(),
                group.Sum(package => package.WordCount),
                group.Select(package => package.SliceKey)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(sliceKey => sliceKey, StringComparer.OrdinalIgnoreCase)
                    .ToArray()))
            .OrderBy(area => area.ContentAreaKey, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <inheritdoc />
    public MobileContentManifestResponse GetAreaManifest(string? clientProductKey, string areaKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(areaKey);

        ClientProductOptions product = ResolveProduct(clientProductKey);
        IReadOnlyList<PublishedContentPackageResponse> packages = ResolvePackages(product.Key, areaKey: areaKey);
        return BuildManifest(product, areaKey.Trim(), "all", packages);
    }

    /// <inheritdoc />
    public MobileContentManifestResponse GetCefrManifest(string? clientProductKey, string level)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(level);

        ClientProductOptions product = ResolveProduct(clientProductKey);
        string normalizedSliceKey = $"cefr:{level.Trim().ToLowerInvariant()}";
        IReadOnlyList<PublishedContentPackageResponse> packages = ResolvePackages(
            product.Key,
            areaKey: "catalog",
            sliceKey: normalizedSliceKey);

        return BuildManifest(product, "catalog", normalizedSliceKey, packages);
    }

    /// <inheritdoc />
    public PublishedContentPackageResponse GetPackage(string? clientProductKey, string packageId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        ClientProductOptions product = ResolveProduct(clientProductKey);

        PublishedPackageOptions package = options.Packages.FirstOrDefault(package =>
                package.ClientProductKey.Equals(product.Key, StringComparison.OrdinalIgnoreCase) &&
                package.PackageId.Equals(packageId.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"No package was found for '{packageId}'.");

        return MapPackage(package);
    }

    private MobileContentManifestResponse BuildManifest(
        ClientProductOptions product,
        string areaKey,
        string sliceKey,
        IReadOnlyList<PublishedContentPackageResponse> packages)
    {
        return new MobileContentManifestResponse(
            product.Key,
            product.LearningLanguageCode,
            areaKey,
            sliceKey,
            options.DefaultSchemaVersion,
            DateTimeOffset.UtcNow,
            packages.Count,
            packages.Sum(package => package.WordCount),
            packages);
    }

    private IReadOnlyList<PublishedContentPackageResponse> ResolvePackages(
        string clientProductKey,
        string? areaKey = null,
        string? sliceKey = null)
    {
        IEnumerable<PublishedPackageOptions> query = options.Packages.Where(package =>
            package.ClientProductKey.Equals(clientProductKey, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(areaKey))
        {
            query = query.Where(package => package.ContentAreaKey.Equals(areaKey.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(sliceKey))
        {
            query = query.Where(package => package.SliceKey.Equals(sliceKey.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        return query
            .OrderByDescending(package => package.CreatedAtUtc)
            .ThenBy(package => package.PackageId, StringComparer.OrdinalIgnoreCase)
            .Select(MapPackage)
            .ToArray();
    }

    private ClientProductOptions ResolveProduct(string? clientProductKey)
    {
        IReadOnlyList<ClientProductOptions> activeProducts = options.ClientProducts
            .Where(product => product.IsActive)
            .ToArray();

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

    private PublishedContentPackageResponse MapPackage(PublishedPackageOptions package)
    {
        string baseUrl = options.PublicBaseUrl.TrimEnd('/');
        string relativePath = package.RelativeDownloadPath.TrimStart('/');

        return new PublishedContentPackageResponse(
            package.PackageId,
            package.ClientProductKey,
            package.ContentAreaKey,
            package.SliceKey,
            package.PackageType,
            package.Version,
            package.SchemaVersion,
            package.MinimumAppSchemaVersion,
            package.Checksum,
            package.EntryCount,
            package.WordCount,
            package.CreatedAtUtc,
            $"{baseUrl}/{relativePath}");
    }
}
