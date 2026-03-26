namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents the input request for importing a content package from a file.
/// </summary>
public sealed record ImportContentPackageRequest(string FilePath);
