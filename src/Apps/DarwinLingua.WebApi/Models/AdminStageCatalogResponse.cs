namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents the result of staging mobile catalog packages from the current shared catalog.
/// </summary>
public sealed record AdminStageCatalogResponse(
    bool IsSuccess,
    string ClientProductKey,
    string PublicationBatchId,
    string Version,
    IReadOnlyList<string> PackageIds,
    IReadOnlyList<string> Issues);
