namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents one audited publication event for admin visibility.
/// </summary>
public sealed record AdminPublicationAuditEventResponse(
    string ClientProductKey,
    string PublicationBatchId,
    string EventType,
    IReadOnlyList<string> PackageIds,
    IReadOnlyList<string> RelatedBatchIds,
    string Notes,
    DateTimeOffset OccurredAtUtc);
