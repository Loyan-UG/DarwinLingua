namespace DarwinLingua.WebApi.Configuration;

/// <summary>
/// Configures how published package payloads are stored for Web API delivery.
/// </summary>
public sealed class PackageStorageOptions
{
    /// <summary>
    /// Gets or sets the storage mode.
    /// </summary>
    public string Mode { get; set; } = "FileSystem";

    /// <summary>
    /// Gets or sets the root path for package payload files.
    /// </summary>
    public string RootPath { get; set; } = "assets/ServerContent/PublishedPackages";
}
