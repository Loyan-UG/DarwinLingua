using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Controllers;

[ApiController]
[AllowAnonymous]
[Route("webhooks/brevo/transactional-email")]
public sealed class BrevoTransactionalWebhookController(
    IEmailDeliveryLogRepository deliveryLogRepository,
    IOptions<TransactionalEmailOptions> emailOptions,
    ILogger<BrevoTransactionalWebhookController> logger) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(64 * 1024)]
    public async Task<IActionResult> Receive(
        [FromQuery] string? secret,
        [FromBody] BrevoTransactionalEmailWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized(secret))
        {
            return Unauthorized();
        }

        string? providerMessageId = NormalizeLength(webhookEvent.MessageId ?? webhookEvent.MessageIdAlternative, 256);
        string? providerEvent = NormalizeLength(webhookEvent.Event, 64);
        if (string.IsNullOrWhiteSpace(providerMessageId) ||
            string.IsNullOrWhiteSpace(providerEvent) ||
            !IsAllowedProviderEvent(providerEvent))
        {
            logger.LogInformation("Ignored Brevo webhook event without a supported message id or event name.");
            return Accepted();
        }

        DateTimeOffset eventAtUtc = ResolveEventTimestamp(webhookEvent);
        bool updated = await deliveryLogRepository.MarkProviderEventAsync(
                providerMessageId,
                providerEvent,
                eventAtUtc,
                NormalizeLength(webhookEvent.Reason ?? webhookEvent.Description ?? webhookEvent.Subject, 512),
                cancellationToken)
            .ConfigureAwait(false);
        if (!updated)
        {
            logger.LogInformation("Brevo webhook event {Event} did not match an internal delivery log for provider message id {ProviderMessageId}.", providerEvent, providerMessageId);
        }

        return Ok();
    }

    private bool IsAuthorized(string? secret)
    {
        string configuredSecret = emailOptions.Value.BrevoWebhookSecret;
        if (string.IsNullOrWhiteSpace(configuredSecret))
        {
            logger.LogWarning("Rejected Brevo webhook because TransactionalEmail:BrevoWebhookSecret is not configured.");
            return false;
        }

        string? bearerToken = ResolveBearerToken();
        if (SecretEquals(bearerToken, configuredSecret))
        {
            return true;
        }

        if (Request.Headers.TryGetValue("X-DarwinLingua-Brevo-Webhook-Secret", out var headerSecret) &&
            SecretEquals(headerSecret.ToString(), configuredSecret))
        {
            return true;
        }

        return emailOptions.Value.BrevoAllowQuerySecretFallback &&
            SecretEquals(secret, configuredSecret);
    }

    private static bool SecretEquals(string? suppliedSecret, string configuredSecret)
    {
        if (string.IsNullOrWhiteSpace(suppliedSecret))
        {
            return false;
        }

        byte[] suppliedBytes = Encoding.UTF8.GetBytes(suppliedSecret.Trim());
        byte[] configuredBytes = Encoding.UTF8.GetBytes(configuredSecret.Trim());
        return suppliedBytes.Length == configuredBytes.Length &&
            CryptographicOperations.FixedTimeEquals(suppliedBytes, configuredBytes);
    }

    private static DateTimeOffset ResolveEventTimestamp(BrevoTransactionalEmailWebhookEvent webhookEvent)
    {
        if (webhookEvent.EventTimestamp is not null)
        {
            return DateTimeOffset.FromUnixTimeSeconds(webhookEvent.EventTimestamp.Value).ToUniversalTime();
        }

        if (webhookEvent.EpochMilliseconds is not null)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(webhookEvent.EpochMilliseconds.Value).ToUniversalTime();
        }

        if (webhookEvent.Date is not null &&
            DateTimeOffset.TryParse(webhookEvent.Date, out DateTimeOffset parsedDate))
        {
            return parsedDate.ToUniversalTime();
        }

        if (webhookEvent.Timestamp is not null)
        {
            return DateTimeOffset.FromUnixTimeSeconds(webhookEvent.Timestamp.Value).ToUniversalTime();
        }

        return DateTimeOffset.UtcNow;
    }

    private string? ResolveBearerToken()
    {
        string authorization = Request.Headers.Authorization.ToString();
        const string bearerPrefix = "Bearer ";
        return authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? authorization[bearerPrefix.Length..].Trim()
            : null;
    }

    private static string? NormalizeLength(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static bool IsAllowedProviderEvent(string providerEvent) =>
        string.Equals(providerEvent, "request", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "sent", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "delivered", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "soft_bounce", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "hard_bounce", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "blocked", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "invalid", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "invalid_email", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "error", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "spam", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "opened", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "unique_opened", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "click", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "unsubscribed", StringComparison.OrdinalIgnoreCase);
}

public sealed record BrevoTransactionalEmailWebhookEvent(
    [property: JsonPropertyName("event")] string? Event,
    [property: JsonPropertyName("message-id")] string? MessageId,
    [property: JsonPropertyName("messageId")] string? MessageIdAlternative,
    [property: JsonPropertyName("date")] string? Date,
    [property: JsonPropertyName("ts")] long? Timestamp,
    [property: JsonPropertyName("ts_event")] long? EventTimestamp,
    [property: JsonPropertyName("ts_epoch")] long? EpochMilliseconds,
    [property: JsonPropertyName("reason")] string? Reason,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("subject")] string? Subject);
