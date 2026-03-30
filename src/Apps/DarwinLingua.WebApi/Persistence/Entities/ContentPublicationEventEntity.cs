namespace DarwinLingua.WebApi.Persistence.Entities;

/// <summary>
/// Represents one audited publication-lifecycle event for admin operations.
/// </summary>
public sealed class ContentPublicationEventEntity
{
    /// <summary>
    /// Gets or sets the primary key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the client product key.
    /// </summary>
    public string ClientProductKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary batch identifier acted on.
    /// </summary>
    public string PublicationBatchId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event type.
    /// </summary>
    public ContentPublicationEventType EventType { get; set; }

    /// <summary>
    /// Gets or sets the related package identifiers.
    /// </summary>
    public string PackageIds { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the replaced or affected secondary batch identifiers.
    /// </summary>
    public string RelatedBatchIds { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional note summary.
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event timestamp.
    /// </summary>
    public DateTimeOffset OccurredAtUtc { get; set; }
}
