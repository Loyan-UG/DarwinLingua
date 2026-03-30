namespace DarwinLingua.WebApi.Persistence.Entities;

/// <summary>
/// Represents one published package stored in PostgreSQL for mobile distribution.
/// </summary>
public sealed class PublishedPackageEntity
{
    /// <summary>
    /// Gets or sets the primary key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the stable package identifier.
    /// </summary>
    public string PackageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owning content-stream identifier.
    /// </summary>
    public Guid ContentStreamId { get; set; }

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
    public int SchemaVersion { get; set; }

    /// <summary>
    /// Gets or sets the minimum app schema version.
    /// </summary>
    public int MinimumAppSchemaVersion { get; set; }

    /// <summary>
    /// Gets or sets the checksum.
    /// </summary>
    public string Checksum { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entry count.
    /// </summary>
    public int EntryCount { get; set; }

    /// <summary>
    /// Gets or sets the word count.
    /// </summary>
    public int WordCount { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the relative download path.
    /// </summary>
    public string RelativeDownloadPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owning stream.
    /// </summary>
    public ContentStreamEntity ContentStream { get; set; } = null!;
}
