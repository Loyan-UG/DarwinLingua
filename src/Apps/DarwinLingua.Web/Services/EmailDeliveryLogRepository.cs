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

    Task MarkSentAsync(
        Guid id,
        string? providerMessageId,
        CancellationToken cancellationToken);

    Task MarkFailedAsync(
        Guid id,
        string? failureCode,
        string? failureMessageSummary,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WebEmailDeliveryLog>> GetRecentAsync(
        int take,
        string? status,
        string? scenario,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        string? recipientHashPrefix,
        CancellationToken cancellationToken);

    Task<int> DeleteOlderThanAsync(
        DateTimeOffset cutoffUtc,
        CancellationToken cancellationToken);
}

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

    public async Task<IReadOnlyList<WebEmailDeliveryLog>> GetRecentAsync(
        int take,
        string? status,
        string? scenario,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        string? recipientHashPrefix,
        CancellationToken cancellationToken)
    {
        IQueryable<WebEmailDeliveryLog> query = dbContext.EmailDeliveryLogs.AsNoTracking();

        if (Enum.TryParse(status, ignoreCase: true, out WebEmailDeliveryStatus parsedStatus))
        {
            query = query.Where(log => log.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(scenario))
        {
            string normalizedScenario = scenario.Trim();
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
}
