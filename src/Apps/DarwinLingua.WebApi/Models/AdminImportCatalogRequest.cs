namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents the local-development request to import one content package into the shared catalog.
/// </summary>
public sealed record AdminImportCatalogRequest(
    string FilePath,
    string? ClientProductKey);
