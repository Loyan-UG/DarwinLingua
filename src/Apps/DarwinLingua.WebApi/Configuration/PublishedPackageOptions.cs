namespace DarwinLingua.WebApi.Configuration;

/// <summary>
/// Describes one published package exposed by the Web API.
/// </summary>
public sealed class PublishedPackageOptions
{
    /// <summary>
    /// Gets or sets the stable package identifier.
    /// </summary>
    public string PackageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target client product key.
    /// </summary>
    public string ClientProductKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content area key.
    /// </summary>
    public string ContentAreaKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the slice key such as full or cefr:a1.
    /// </summary>
    public string SliceKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the package type.
    /// </summary>
    public string PackageType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the published version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema version.
    /// </summary>
    public int SchemaVersion { get; set; } = 1;

    /// <summary>
    /// Gets or sets the minimum app schema version supported by the package.
    /// </summary>
    public int MinimumAppSchemaVersion { get; set; } = 1;

    /// <summary>
    /// Gets or sets the package checksum.
    /// </summary>
    public string Checksum { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of entries included in the package.
    /// </summary>
    public int EntryCount { get; set; }

    /// <summary>
    /// Gets or sets the number of words included in the package.
    /// </summary>
    public int WordCount { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp of the published package.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the relative download path exposed to clients.
    /// </summary>
    public string RelativeDownloadPath { get; set; } = string.Empty;
}
