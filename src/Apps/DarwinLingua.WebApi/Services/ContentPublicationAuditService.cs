using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Persists and serves publication audit events.
/// </summary>
public sealed class ContentPublicationAuditService(ServerContentDbContext dbContext) : IContentPublicationAuditService
{
    public async Task RecordAsync(
        string clientProductKey,
        string publicationBatchId,
        ContentPublicationEventType eventType,
        IEnumerable<string> packageIds,
        IEnumerable<string> relatedBatchIds,
        string? notes,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientProductKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(publicationBatchId);

        ContentPublicationEventEntity entity = new()
        {
            Id = Guid.NewGuid(),
            ClientProductKey = clientProductKey.Trim(),
            PublicationBatchId = publicationBatchId.Trim(),
            EventType = eventType,
            PackageIds = SerializeValues(packageIds),
            RelatedBatchIds = SerializeValues(relatedBatchIds),
            Notes = string.IsNullOrWhiteSpace(notes) ? string.Empty : notes.Trim(),
            OccurredAtUtc = DateTimeOffset.UtcNow,
        };

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AdminPublicationAuditEventResponse>> GetRecentEventsAsync(
        string? clientProductKey,
        CancellationToken cancellationToken)
    {
        string? normalizedClientProductKey = string.IsNullOrWhiteSpace(clientProductKey) ? null : clientProductKey.Trim();

        if (normalizedClientProductKey is not null)
        {
            bool exists = await dbContext.ClientProducts
                .AnyAsync(product => product.Key == normalizedClientProductKey && product.IsActive, cancellationToken)
                .ConfigureAwait(false);

            if (!exists)
            {
                throw new KeyNotFoundException($"No active client product was found for '{normalizedClientProductKey}'.");
            }
        }

        List<ContentPublicationEventEntity> events = await dbContext.ContentPublicationEvents
            .Where(entry => normalizedClientProductKey == null || entry.ClientProductKey == normalizedClientProductKey)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return events
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .ThenByDescending(entry => entry.PublicationBatchId, StringComparer.OrdinalIgnoreCase)
            .Take(50)
            .Select(entry => new AdminPublicationAuditEventResponse(
                entry.ClientProductKey,
                entry.PublicationBatchId,
                entry.EventType.ToString(),
                DeserializeValues(entry.PackageIds),
                DeserializeValues(entry.RelatedBatchIds),
                entry.Notes,
                entry.OccurredAtUtc))
            .ToArray();
    }

    private static string SerializeValues(IEnumerable<string> values) =>
        string.Join('|', values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()));

    private static IReadOnlyList<string> DeserializeValues(string rawValue) =>
        string.IsNullOrWhiteSpace(rawValue)
            ? []
            : rawValue.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
