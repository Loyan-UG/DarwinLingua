namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents one package inside an admin-facing catalog batch history record.
/// </summary>
/// <param name="PackageId">The stable package identifier.</param>
/// <param name="PackageType">The package type.</param>
/// <param name="ContentAreaKey">The content area.</param>
/// <param name="SliceKey">The optional slice key.</param>
/// <param name="Version">The package version.</param>
/// <param name="Checksum">The package checksum.</param>
/// <param name="WordCount">The word count.</param>
/// <param name="EntryCount">The entry count.</param>
/// <param name="CreatedAtUtc">The package creation time.</param>
public sealed record AdminCatalogBatchHistoryPackageResponse(
    string PackageId,
    string PackageType,
    string ContentAreaKey,
    string? SliceKey,
    string Version,
    string Checksum,
    int WordCount,
    int EntryCount,
    DateTimeOffset CreatedAtUtc);
