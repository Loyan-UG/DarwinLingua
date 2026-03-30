namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Describes one staged or published package inside an admin batch view.
/// </summary>
public sealed record AdminDraftCatalogBatchPackageResponse(
    string PackageId,
    string PackageType,
    string ContentAreaKey,
    string SliceKey,
    string Version,
    string Checksum,
    int WordCount,
    int EntryCount,
    DateTimeOffset CreatedAtUtc);
