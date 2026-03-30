namespace DarwinLingua.WebApi.Persistence.Entities;

/// <summary>
/// Represents one client product that can consume shared-content packages.
/// </summary>
public sealed class ClientProductEntity
{
    /// <summary>
    /// Gets or sets the primary key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the stable product key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the learning language code.
    /// </summary>
    public string LearningLanguageCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default UI language code.
    /// </summary>
    public string DefaultUiLanguageCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the product is active.
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
    /// Gets the content streams for the product.
    /// </summary>
    public List<ContentStreamEntity> ContentStreams { get; } = [];
}
