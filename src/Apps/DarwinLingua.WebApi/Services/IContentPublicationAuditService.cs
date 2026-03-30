using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence.Entities;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Writes and reads admin-facing publication audit events.
/// </summary>
public interface IContentPublicationAuditService
{
    /// <summary>
    /// Records one publication-lifecycle audit event.
    /// </summary>
    Task RecordAsync(
        string clientProductKey,
        string publicationBatchId,
        ContentPublicationEventType eventType,
        IEnumerable<string> packageIds,
        IEnumerable<string> relatedBatchIds,
        string? notes,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets recent publication-lifecycle audit events.
    /// </summary>
    Task<IReadOnlyList<AdminPublicationAuditEventResponse>> GetRecentEventsAsync(
        string? clientProductKey,
        CancellationToken cancellationToken);
}
