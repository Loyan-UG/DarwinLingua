using System.Security.Cryptography;
using System.Text;
using DarwinLingua.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Services;

public interface IEmailDeliveryLogRepository
{
    Task<WebEmailDeliveryLog> AddQueuedAsync(
        TransactionalEmailMessage message,
        string providerName,
        CancellationToken cancellationToken);

    Task<WebEmailDeliveryLog> AddSuppressedAsync(
        TransactionalEmailMessage message,
        string providerName,
        string reason,
        CancellationToken cancellationToken);

    Task<bool> IsSuppressedAsync(
        string recipientEmail,
        CancellationToken cancellationToken);

    Task MarkSentAsync(
        Guid id,
        string? providerMessageId,
        CancellationToken cancellationToken);

    Task MarkFailedAsync(
        Guid id,
        string? failureCode,
        string? failureMessageSummary,
        CancellationToken cancellationToken);

    Task<bool> MarkProviderEventAsync(
        string providerMessageId,
        string providerEvent,
        DateTimeOffset providerEventAtUtc,
        string? reason,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WebEmailDeliveryLog>> GetRecentAsync(
        int take,
        string? status,
        string? dialogue,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        string? recipientHashPrefix,
        string? providerMessageId,
        string? providerEvent,
        CancellationToken cancellationToken);

    Task<int> DeleteOlderThanAsync(
        DateTimeOffset cutoffUtc,
        CancellationToken cancellationToken);

    Task<EmailDeliverySummary> GetSummarySinceAsync(
        DateTimeOffset sinceUtc,
        CancellationToken cancellationToken);

    Task<EmailDeliveryFailureAlertSnapshot> GetFailureAlertSnapshotAsync(
        DateTimeOffset sinceUtc,
        string excludedScenarioKey,
        CancellationToken cancellationToken);

    Task<EmailSuppressionSummary> GetSuppressionSummaryAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<WebEmailSuppression>> GetSuppressionsAsync(
        string? recipientHashPrefix,
        string? reason,
        int take,
        CancellationToken cancellationToken);

    Task<bool> DeleteSuppressionByHashAsync(
        string recipientEmailHash,
        CancellationToken cancellationToken);
}

public sealed record EmailDeliverySummary(
    int QueuedCount,
    int SentCount,
    int FailedCount,
    int SkippedCount,
    int SuppressedCount,
    DateTimeOffset? LastFailureAtUtc,
    string? LastFailureScenarioKey,
    string? LastFailureCode,
    DateTimeOffset? LastProviderEventAtUtc,
    string? LastProviderEvent);

public sealed record EmailDeliveryFailureAlertSnapshot(
    int FailureCount,
    DateTimeOffset? LastFailureAtUtc,
    string? LastFailureScenarioKey,
    string? LastFailureCode);

public sealed record EmailSuppressionSummary(
    int TotalCount,
    DateTimeOffset? LastCreatedAtUtc,
    string? LastReason);

public sealed class EmailDeliveryLogRepository(WebIdentityDbContext dbContext)
    : IEmailDeliveryLogRepository
{
    public async Task<WebEmailDeliveryLog> AddQueuedAsync(
        TransactionalEmailMessage message,
        string providerName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        WebEmailDeliveryLog log = new()
        {
            Id = Guid.NewGuid(),
            ScenarioKey = message.ScenarioKey,
            RecipientEmailHash = HashEmail(message.RecipientEmail),
            RecipientUserId = message.RecipientUserId,
            TemplateKey = message.TemplateKey,
            Culture = message.Culture,
            Subject = message.Subject,
            ProviderName = providerName,
            Status = WebEmailDeliveryStatus.Queued,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CorrelationId = message.CorrelationId,
        };

        dbContext.EmailDeliveryLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return log;
    }

    public async Task<WebEmailDeliveryLog> AddSuppressedAsync(
        TransactionalEmailMessage message,
        string providerName,
        string reason,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        WebEmailDeliveryLog log = new()
        {
            Id = Guid.NewGuid(),
            ScenarioKey = message.ScenarioKey,
            RecipientEmailHash = HashEmail(message.RecipientEmail),
            RecipientUserId = message.RecipientUserId,
            TemplateKey = message.TemplateKey,
            Culture = message.Culture,
            Subject = message.Subject,
            ProviderName = providerName,
            Status = WebEmailDeliveryStatus.Suppressed,
            FailureCode = Truncate(reason, 128),
            FailureMessageSummary = "Delivery suppressed because this recipient is on the internal email suppression list.",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            LastAttemptAtUtc = DateTimeOffset.UtcNow,
            CorrelationId = message.CorrelationId,
        };

        dbContext.EmailDeliveryLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return log;
    }

    public async Task<bool> IsSuppressedAsync(
        string recipientEmail,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            return false;
        }

        string recipientEmailHash = HashEmail(recipientEmail);
        return await dbContext.EmailSuppressions
            .AsNoTracking()
            .AnyAsync(suppression => suppression.RecipientEmailHash == recipientEmailHash, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task MarkSentAsync(
        Guid id,
        string? providerMessageId,
        CancellationToken cancellationToken)
    {
        WebEmailDeliveryLog? log = await dbContext.EmailDeliveryLogs
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (log is null)
        {
            return;
        }

        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        log.Status = WebEmailDeliveryStatus.Sent;
        log.ProviderMessageId = providerMessageId;
        log.SentAtUtc = nowUtc;
        log.LastAttemptAtUtc = nowUtc;
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task MarkFailedAsync(
        Guid id,
        string? failureCode,
        string? failureMessageSummary,
        CancellationToken cancellationToken)
    {
        WebEmailDeliveryLog? log = await dbContext.EmailDeliveryLogs
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (log is null)
        {
            return;
        }

        log.Status = WebEmailDeliveryStatus.Failed;
        log.FailureCode = Truncate(failureCode, 128);
        log.FailureMessageSummary = Truncate(failureMessageSummary, 512);
        log.RetryCount += 1;
        log.LastAttemptAtUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> MarkProviderEventAsync(
        string providerMessageId,
        string providerEvent,
        DateTimeOffset providerEventAtUtc,
        string? reason,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(providerMessageId) || string.IsNullOrWhiteSpace(providerEvent))
        {
            return false;
        }

        WebEmailDeliveryLog? log = await dbContext.EmailDeliveryLogs
            .SingleOrDefaultAsync(candidate => candidate.ProviderMessageId == providerMessageId, cancellationToken)
            .ConfigureAwait(false);
        if (log is null)
        {
            return false;
        }

        string normalizedEvent = providerEvent.Trim();
        string storedProviderEvent = normalizedEvent.ToLowerInvariant();
        log.ProviderLastEvent = Truncate(storedProviderEvent, 64);
        log.ProviderLastEventAtUtc = providerEventAtUtc.ToUniversalTime();
        log.ProviderLastEventReason = Truncate(reason, 512);
        log.LastAttemptAtUtc = DateTimeOffset.UtcNow;

        if (IsSuccessfulProviderEvent(normalizedEvent))
        {
            log.Status = WebEmailDeliveryStatus.Sent;
            log.SentAtUtc ??= providerEventAtUtc.ToUniversalTime();
            log.FailureCode = null;
            log.FailureMessageSummary = null;
        }
        else if (IsFailedProviderEvent(normalizedEvent))
        {
            log.Status = WebEmailDeliveryStatus.Failed;
            log.FailureCode = Truncate("brevo:" + normalizedEvent, 128);
            log.FailureMessageSummary = Truncate(reason, 512);

            if (IsPermanentSuppressionEvent(normalizedEvent))
            {
                await UpsertSuppressionAsync(
                        log.RecipientEmailHash,
                        "brevo:" + normalizedEvent,
                        log.ProviderName,
                        log.ProviderMessageId,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<IReadOnlyList<WebEmailDeliveryLog>> GetRecentAsync(
        int take,
        string? status,
        string? dialogue,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        string? recipientHashPrefix,
        string? providerMessageId,
        string? providerEvent,
        CancellationToken cancellationToken)
    {
        IQueryable<WebEmailDeliveryLog> query = dbContext.EmailDeliveryLogs.AsNoTracking();

        if (Enum.TryParse(status, ignoreCase: true, out WebEmailDeliveryStatus parsedStatus))
        {
            query = query.Where(log => log.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(dialogue))
        {
            string normalizedScenario = dialogue.Trim();
            query = query.Where(log => log.ScenarioKey == normalizedScenario);
        }

        if (fromUtc is not null)
        {
            DateTimeOffset normalizedFromUtc = fromUtc.Value.ToUniversalTime();
            query = query.Where(log => log.CreatedAtUtc >= normalizedFromUtc);
        }

        if (toUtc is not null)
        {
            DateTimeOffset normalizedToUtc = toUtc.Value.ToUniversalTime();
            query = query.Where(log => log.CreatedAtUtc <= normalizedToUtc);
        }

        if (!string.IsNullOrWhiteSpace(recipientHashPrefix))
        {
            string normalizedHashPrefix = recipientHashPrefix.Trim().ToUpperInvariant();
            if (normalizedHashPrefix.All(static character => Uri.IsHexDigit(character)))
            {
                query = query.Where(log => log.RecipientEmailHash.StartsWith(normalizedHashPrefix));
            }
        }

        if (!string.IsNullOrWhiteSpace(providerMessageId))
        {
            string normalizedProviderMessageId = providerMessageId.Trim();
            query = query.Where(log => log.ProviderMessageId == normalizedProviderMessageId);
        }

        if (!string.IsNullOrWhiteSpace(providerEvent))
        {
            string normalizedProviderEvent = providerEvent.Trim().ToLowerInvariant();
            query = query.Where(log => log.ProviderLastEvent == normalizedProviderEvent);
        }

        int boundedTake = Math.Clamp(take, 1, 200);
        return await query
            .OrderByDescending(log => log.CreatedAtUtc)
            .Take(boundedTake)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<int> DeleteOlderThanAsync(
        DateTimeOffset cutoffUtc,
        CancellationToken cancellationToken)
    {
        DateTimeOffset normalizedCutoffUtc = cutoffUtc.ToUniversalTime();
        return await dbContext.EmailDeliveryLogs
            .Where(log => log.CreatedAtUtc < normalizedCutoffUtc)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<EmailDeliverySummary> GetSummarySinceAsync(
        DateTimeOffset sinceUtc,
        CancellationToken cancellationToken)
    {
        DateTimeOffset normalizedSinceUtc = sinceUtc.ToUniversalTime();
        EmailDeliveryStatusCount[] statusCounts = await dbContext.EmailDeliveryLogs
            .AsNoTracking()
            .Where(log => log.CreatedAtUtc >= normalizedSinceUtc)
            .GroupBy(log => log.Status)
            .Select(group => new EmailDeliveryStatusCount(group.Key, group.Count()))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        WebEmailDeliveryLog? lastFailure = await dbContext.EmailDeliveryLogs
            .AsNoTracking()
            .Where(log =>
                log.Status == WebEmailDeliveryStatus.Failed &&
                log.CreatedAtUtc >= normalizedSinceUtc)
            .OrderByDescending(log => log.LastAttemptAtUtc ?? log.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        WebEmailDeliveryLog? lastProviderEvent = await dbContext.EmailDeliveryLogs
            .AsNoTracking()
            .Where(log =>
                log.ProviderLastEventAtUtc != null &&
                log.CreatedAtUtc >= normalizedSinceUtc)
            .OrderByDescending(log => log.ProviderLastEventAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return new EmailDeliverySummary(
            GetCount(statusCounts, WebEmailDeliveryStatus.Queued),
            GetCount(statusCounts, WebEmailDeliveryStatus.Sent),
            GetCount(statusCounts, WebEmailDeliveryStatus.Failed),
            GetCount(statusCounts, WebEmailDeliveryStatus.Skipped),
            GetCount(statusCounts, WebEmailDeliveryStatus.Suppressed),
            lastFailure?.LastAttemptAtUtc ?? lastFailure?.CreatedAtUtc,
            lastFailure?.ScenarioKey,
            lastFailure?.FailureCode,
            lastProviderEvent?.ProviderLastEventAtUtc,
            lastProviderEvent?.ProviderLastEvent);
    }

    public async Task<EmailDeliveryFailureAlertSnapshot> GetFailureAlertSnapshotAsync(
        DateTimeOffset sinceUtc,
        string excludedScenarioKey,
        CancellationToken cancellationToken)
    {
        DateTimeOffset normalizedSinceUtc = sinceUtc.ToUniversalTime();
        IQueryable<WebEmailDeliveryLog> failures = dbContext.EmailDeliveryLogs
            .AsNoTracking()
            .Where(log =>
                log.Status == WebEmailDeliveryStatus.Failed &&
                log.CreatedAtUtc >= normalizedSinceUtc &&
                log.ScenarioKey != excludedScenarioKey);

        int failureCount = await failures.CountAsync(cancellationToken).ConfigureAwait(false);
        WebEmailDeliveryLog? lastFailure = await failures
            .OrderByDescending(log => log.LastAttemptAtUtc ?? log.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return new EmailDeliveryFailureAlertSnapshot(
            failureCount,
            lastFailure?.LastAttemptAtUtc ?? lastFailure?.CreatedAtUtc,
            lastFailure?.ScenarioKey,
            lastFailure?.FailureCode);
    }

    public async Task<EmailSuppressionSummary> GetSuppressionSummaryAsync(CancellationToken cancellationToken)
    {
        int totalCount = await dbContext.EmailSuppressions
            .AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        WebEmailSuppression? lastSuppression = await dbContext.EmailSuppressions
            .AsNoTracking()
            .OrderByDescending(suppression => suppression.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return new EmailSuppressionSummary(
            totalCount,
            lastSuppression?.CreatedAtUtc,
            lastSuppression?.Reason);
    }

    public async Task<IReadOnlyList<WebEmailSuppression>> GetSuppressionsAsync(
        string? recipientHashPrefix,
        string? reason,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<WebEmailSuppression> query = dbContext.EmailSuppressions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(recipientHashPrefix))
        {
            string normalizedHashPrefix = recipientHashPrefix.Trim().ToUpperInvariant();
            if (normalizedHashPrefix.All(static character => Uri.IsHexDigit(character)))
            {
                query = query.Where(suppression => suppression.RecipientEmailHash.StartsWith(normalizedHashPrefix));
            }
        }

        if (!string.IsNullOrWhiteSpace(reason))
        {
            string normalizedReason = reason.Trim().ToLowerInvariant();
            query = query.Where(suppression => suppression.Reason == normalizedReason);
        }

        int boundedTake = Math.Clamp(take, 1, 200);
        return await query
            .OrderByDescending(suppression => suppression.CreatedAtUtc)
            .Take(boundedTake)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> DeleteSuppressionByHashAsync(
        string recipientEmailHash,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(recipientEmailHash))
        {
            return false;
        }

        string normalizedHash = recipientEmailHash.Trim().ToUpperInvariant();
        int deletedCount = await dbContext.EmailSuppressions
            .Where(suppression => suppression.RecipientEmailHash == normalizedHash)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
        return deletedCount > 0;
    }

    private static int GetCount(
        IReadOnlyList<EmailDeliveryStatusCount> statusCounts,
        WebEmailDeliveryStatus status) =>
        statusCounts.FirstOrDefault(count => count.Status == status)?.Count ?? 0;

    private static string HashEmail(string email)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(email.Trim().ToUpperInvariant()));
        return Convert.ToHexString(bytes);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static bool IsSuccessfulProviderEvent(string providerEvent) =>
        string.Equals(providerEvent, "delivered", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "request", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "sent", StringComparison.OrdinalIgnoreCase);

    private static bool IsFailedProviderEvent(string providerEvent) =>
        string.Equals(providerEvent, "hard_bounce", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "soft_bounce", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "blocked", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "invalid", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "invalid_email", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "error", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "spam", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "complaint", StringComparison.OrdinalIgnoreCase);

    private static bool IsPermanentSuppressionEvent(string providerEvent) =>
        string.Equals(providerEvent, "hard_bounce", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "blocked", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "invalid", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "invalid_email", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "spam", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "complaint", StringComparison.OrdinalIgnoreCase);

    private async Task UpsertSuppressionAsync(
        string recipientEmailHash,
        string reason,
        string providerName,
        string? providerMessageId,
        CancellationToken cancellationToken)
    {
        WebEmailSuppression? suppression = await dbContext.EmailSuppressions
            .SingleOrDefaultAsync(candidate => candidate.RecipientEmailHash == recipientEmailHash, cancellationToken)
            .ConfigureAwait(false);

        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        if (suppression is null)
        {
            dbContext.EmailSuppressions.Add(new WebEmailSuppression
            {
                Id = Guid.NewGuid(),
                RecipientEmailHash = recipientEmailHash,
                Reason = Truncate(reason, 128) ?? "provider-suppression",
                ProviderName = Truncate(providerName, 64) ?? "unknown",
                ProviderMessageId = Truncate(providerMessageId, 256),
                CreatedAtUtc = nowUtc,
                LastSeenAtUtc = nowUtc,
            });
            return;
        }

        suppression.Reason = Truncate(reason, 128) ?? suppression.Reason;
        suppression.ProviderName = Truncate(providerName, 64) ?? suppression.ProviderName;
        suppression.ProviderMessageId = Truncate(providerMessageId, 256);
        suppression.LastSeenAtUtc = nowUtc;
    }

    private sealed record EmailDeliveryStatusCount(WebEmailDeliveryStatus Status, int Count);
}
