namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents the package set generated for one catalog publication run.
/// </summary>
public sealed record CatalogPackagePublicationResult(
    string ClientProductKey,
    string Version,
    IReadOnlyList<string> PackageIds);
