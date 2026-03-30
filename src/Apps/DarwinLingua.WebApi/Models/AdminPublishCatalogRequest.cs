namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents the admin request to publish the latest staged catalog package batch.
/// </summary>
public sealed record AdminPublishCatalogRequest(
    string? ClientProductKey,
    string? PublicationBatchId);
