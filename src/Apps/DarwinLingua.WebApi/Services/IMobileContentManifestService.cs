using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Provides manifest and package data for mobile content-update clients.
/// </summary>
public interface IMobileContentManifestService
{
    /// <summary>
    /// Returns the global manifest for the selected client product.
    /// </summary>
    MobileContentManifestResponse GetGlobalManifest(string? clientProductKey);

    /// <summary>
    /// Returns area summaries for the selected client product.
    /// </summary>
    IReadOnlyList<MobileContentAreaSummaryResponse> GetAreas(string? clientProductKey);

    /// <summary>
    /// Returns one area-scoped manifest for the selected client product.
    /// </summary>
    MobileContentManifestResponse GetAreaManifest(string? clientProductKey, string areaKey);

    /// <summary>
    /// Returns one CEFR-scoped manifest for the catalog area.
    /// </summary>
    MobileContentManifestResponse GetCefrManifest(string? clientProductKey, string level);

    /// <summary>
    /// Returns one package by identifier.
    /// </summary>
    PublishedContentPackageResponse GetPackage(string? clientProductKey, string packageId);
}
