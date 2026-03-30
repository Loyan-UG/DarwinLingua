namespace DarwinLingua.WebApi.Persistence.Entities;

/// <summary>
/// Represents one publishable content stream for a product and slice.
/// </summary>
public sealed class ContentStreamEntity
{
    /// <summary>
    /// Gets or sets the primary key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the owning client-product identifier.
    /// </summary>
    public Guid ClientProductId { get; set; }

    /// <summary>
    /// Gets or sets the content-area key.
    /// </summary>
    public string ContentAreaKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the slice key.
    /// </summary>
    public string SliceKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the learning language code.
    /// </summary>
    public string LearningLanguageCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema version.
    /// </summary>
    public int SchemaVersion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the stream is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the owning product.
    /// </summary>
    public ClientProductEntity ClientProduct { get; set; } = null!;

    /// <summary>
    /// Gets the published packages attached to the stream.
    /// </summary>
    public List<PublishedPackageEntity> PublishedPackages { get; } = [];
}
