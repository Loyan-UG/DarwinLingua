namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents the package set generated for one catalog staging or publication run.
/// </summary>
public sealed record CatalogPackagePublicationResult(
    string ClientProductKey,
    string Version,
    string PublicationBatchId,
    IReadOnlyList<string> PackageIds);
