using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Resolves published package payloads for download endpoints.
/// </summary>
public interface IMobileContentPackageDeliveryService
{
    /// <summary>
    /// Resolves a package by identifier for download.
    /// </summary>
    ContentPackageDownloadDescriptor GetPackageById(string? clientProductKey, string packageId, int? clientSchemaVersion);

    /// <summary>
    /// Resolves the latest full package for the selected client product.
    /// </summary>
    ContentPackageDownloadDescriptor GetLatestFullPackage(string? clientProductKey, int? clientSchemaVersion);

    /// <summary>
    /// Resolves the latest package for one content area.
    /// </summary>
    ContentPackageDownloadDescriptor GetLatestAreaPackage(string? clientProductKey, string areaKey, int? clientSchemaVersion);

    /// <summary>
    /// Resolves the latest CEFR package for the catalog area.
    /// </summary>
    ContentPackageDownloadDescriptor GetLatestCefrPackage(string? clientProductKey, string level, int? clientSchemaVersion);
}
