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
    public async Task<IActionResult> Receive(
        [FromQuery] string? secret,
        [FromBody] BrevoTransactionalEmailWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized(secret))
        {
            return Unauthorized();
        }

        string? providerMessageId = webhookEvent.MessageId ?? webhookEvent.MessageIdAlternative;
        if (string.IsNullOrWhiteSpace(providerMessageId) || string.IsNullOrWhiteSpace(webhookEvent.Event))
        {
            logger.LogInformation("Ignored Brevo webhook event without message id or event name.");
            return Accepted();
        }

        DateTimeOffset eventAtUtc = ResolveEventTimestamp(webhookEvent);
        bool updated = await deliveryLogRepository.MarkProviderEventAsync(
                providerMessageId,
                webhookEvent.Event,
                eventAtUtc,
                webhookEvent.Reason ?? webhookEvent.Description ?? webhookEvent.Subject,
                cancellationToken)
            .ConfigureAwait(false);
        if (!updated)
        {
            logger.LogInformation("Brevo webhook event {Event} did not match an internal delivery log for provider message id {ProviderMessageId}.", webhookEvent.Event, providerMessageId);
        }

        return Ok();
    }

    private bool IsAuthorized(string? secret)
    {
        string configuredSecret = emailOptions.Value.BrevoWebhookSecret;
        if (string.IsNullOrWhiteSpace(configuredSecret))
        {
            return true;
        }

        string? bearerToken = ResolveBearerToken();
        if (string.Equals(bearerToken, configuredSecret, StringComparison.Ordinal))
        {
            return true;
        }

        if (Request.Headers.TryGetValue("X-DarwinLingua-Brevo-Webhook-Secret", out var headerSecret) &&
            string.Equals(headerSecret.ToString(), configuredSecret, StringComparison.Ordinal))
        {
            return true;
        }

        return string.Equals(secret, configuredSecret, StringComparison.Ordinal);
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
