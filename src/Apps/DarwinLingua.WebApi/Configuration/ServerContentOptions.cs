namespace DarwinLingua.WebApi.Configuration;

/// <summary>
/// Holds server-side content-distribution configuration for the mobile manifest API.
/// </summary>
public sealed class ServerContentOptions
{
    /// <summary>
    /// Gets the configuration section name.
    /// </summary>
    public const string SectionName = "ServerContent";

    /// <summary>
    /// Gets or sets the base URL exposed to clients for generated download links.
    /// </summary>
    public string PublicBaseUrl { get; set; } = "http://localhost:5099";

    /// <summary>
    /// Gets or sets the default schema version for generated manifests.
    /// </summary>
    public int DefaultSchemaVersion { get; set; } = 1;

    /// <summary>
    /// Gets the configured client products.
    /// </summary>
    public List<ClientProductOptions> ClientProducts { get; } = [];

    /// <summary>
    /// Gets the configured published packages.
    /// </summary>
    public List<PublishedPackageOptions> Packages { get; } = [];

    /// <summary>
    /// Gets or sets package-storage configuration.
    /// </summary>
    public PackageStorageOptions PackageStorage { get; set; } = new();

    /// <summary>
    /// Validates that at least one active client product is available.
    /// </summary>
    public bool HasAtLeastOneActiveProduct()
        => ClientProducts.Any(product => product.IsActive && !string.IsNullOrWhiteSpace(product.Key));

    /// <summary>
    /// Validates that configured packages reference known client products and areas.
    /// </summary>
    public bool HasValidPackages()
    {
        HashSet<string> activeProducts = ClientProducts
            .Where(product => product.IsActive && !string.IsNullOrWhiteSpace(product.Key))
            .Select(product => product.Key.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (Packages.Count == 0)
        {
            return true;
        }

        foreach (PublishedPackageOptions package in Packages)
        {
            if (string.IsNullOrWhiteSpace(package.PackageId) ||
                string.IsNullOrWhiteSpace(package.ClientProductKey) ||
                string.IsNullOrWhiteSpace(package.ContentAreaKey) ||
                string.IsNullOrWhiteSpace(package.SliceKey) ||
                string.IsNullOrWhiteSpace(package.Version) ||
                string.IsNullOrWhiteSpace(package.RelativeDownloadPath))
            {
                return false;
            }

            if (!activeProducts.Contains(package.ClientProductKey.Trim()))
            {
                return false;
            }
        }

        return true;
    }
}
