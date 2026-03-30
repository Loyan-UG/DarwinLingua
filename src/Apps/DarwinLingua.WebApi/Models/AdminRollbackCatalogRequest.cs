namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents the admin request to reactivate one superseded catalog package batch.
/// </summary>
public sealed record AdminRollbackCatalogRequest(
    string? ClientProductKey,
    string PublicationBatchId);
