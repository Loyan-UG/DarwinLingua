namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents an admin request to stage mobile catalog packages from the current shared catalog.
/// </summary>
public sealed record AdminStageCatalogRequest(string? ClientProductKey);
