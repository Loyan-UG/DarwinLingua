namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Describes one resolved package payload ready for HTTP download.
/// </summary>
public sealed record ContentPackageDownloadDescriptor(
    string PackageId,
    string FilePath,
    string ContentType,
    string SuggestedFileName,
    PublishedContentPackageResponse Package);
