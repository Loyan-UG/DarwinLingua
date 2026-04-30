using System.Text.Json;
using DarwinLingua.Identity;
using DarwinLingua.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public sealed class StripeBillingWebhookHandler(
    WebIdentityDbContext dbContext,
    IUserEntitlementService userEntitlementService,
    IOptions<BillingOptions> options,
    ILogger<StripeBillingWebhookHandler> logger) : IStripeBillingWebhookHandler
{
    private const string ProviderName = "Stripe";
    private const string ProcessedStatus = "Processed";
    private const string FailedStatus = "Failed";

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        using JsonDocument document = JsonDocument.Parse(payload);
        JsonElement root = document.RootElement;
        string eventId = GetString(root, "id") ?? throw new InvalidOperationException("Stripe event id is missing.");
        string eventType = GetString(root, "type") ?? throw new InvalidOperationException("Stripe event type is missing.");

        WebBillingEvent? billingEvent = await dbContext.WebBillingEvents
            .SingleOrDefaultAsync(candidate =>
                candidate.ProviderName == ProviderName &&
                candidate.ProviderEventId == eventId,
                cancellationToken)
            .ConfigureAwait(false);

        if (billingEvent?.Status == ProcessedStatus)
        {
            return;
        }

        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        if (billingEvent is null)
        {
            billingEvent = new WebBillingEvent
            {
                Id = Guid.NewGuid(),
                ProviderName = ProviderName,
                ProviderEventId = eventId,
                EventType = eventType,
                Status = "Received",
                CreatedAtUtc = nowUtc,
            };
            dbContext.WebBillingEvents.Add(billingEvent);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        try
        {
            JsonElement stripeObject = root.GetProperty("data").GetProperty("object");
            await ApplyEventAsync(eventType, eventId, stripeObject, billingEvent, cancellationToken).ConfigureAwait(false);

            billingEvent.Status = ProcessedStatus;
            billingEvent.ProcessedAtUtc = DateTimeOffset.UtcNow;
            billingEvent.ErrorSummary = null;
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            billingEvent.Status = FailedStatus;
            billingEvent.ErrorSummary = Summarize(exception.Message);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger.LogWarning(exception, "Stripe billing event {EventId} failed.", eventId);
            throw;
        }
    }

    private async Task ApplyEventAsync(
        string eventType,
        string eventId,
        JsonElement stripeObject,
        WebBillingEvent billingEvent,
        CancellationToken cancellationToken)
    {
        switch (eventType)
        {
            case "checkout.session.completed":
                await ApplyCheckoutCompletedAsync(eventId, stripeObject, billingEvent, cancellationToken).ConfigureAwait(false);
                break;
            case "customer.subscription.created":
            case "customer.subscription.updated":
            case "customer.subscription.deleted":
                await ApplySubscriptionEventAsync(eventType, eventId, stripeObject, billingEvent, cancellationToken).ConfigureAwait(false);
                break;
            default:
                logger.LogInformation("Ignoring Stripe billing event type {EventType}.", eventType);
                break;
        }
    }

    private async Task ApplyCheckoutCompletedAsync(
        string eventId,
        JsonElement session,
        WebBillingEvent billingEvent,
        CancellationToken cancellationToken)
    {
        string? userId = GetMetadataValue(session, "darwin_user_id") ?? GetString(session, "client_reference_id");
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("Stripe checkout session did not include a Darwin Lingua user id.");
        }

        string? customerId = GetString(session, "customer");
        string? subscriptionId = GetString(session, "subscription");
        billingEvent.UserId = userId;
        billingEvent.ProviderCustomerId = customerId;
        billingEvent.ProviderSubscriptionId = subscriptionId;

        await UpsertBillingProfileAsync(
            userId,
            customerId,
            subscriptionId,
            "checkout.session.completed",
            null,
            cancellationToken)
            .ConfigureAwait(false);

        await userEntitlementService.SetTierAsync(
            userId,
            DarwinLinguaEntitlementTiers.Premium,
            expiresAtUtc: null,
            updatedBy: $"stripe:{eventId}",
            cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task ApplySubscriptionEventAsync(
        string eventType,
        string eventId,
        JsonElement subscription,
        WebBillingEvent billingEvent,
        CancellationToken cancellationToken)
    {
        string? subscriptionId = GetString(subscription, "id");
        string? customerId = GetString(subscription, "customer");
        string? userId = GetMetadataValue(subscription, "darwin_user_id");

        if (string.IsNullOrWhiteSpace(userId))
        {
            WebBillingProfile? existingProfile = await dbContext.WebBillingProfiles
                .AsNoTracking()
                .OrderByDescending(profile => profile.UpdatedAtUtc)
                .FirstOrDefaultAsync(profile =>
                    (!string.IsNullOrWhiteSpace(subscriptionId) && profile.ProviderSubscriptionId == subscriptionId) ||
                    (!string.IsNullOrWhiteSpace(customerId) && profile.ProviderCustomerId == customerId),
                    cancellationToken)
                .ConfigureAwait(false);
            userId = existingProfile?.UserId;
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("Stripe subscription event could not be mapped to a Darwin Lingua user.");
        }

        string status = GetString(subscription, "status") ?? string.Empty;
        DateTimeOffset? currentPeriodEndsAtUtc = GetUnixTime(subscription, "current_period_end");

        billingEvent.UserId = userId;
        billingEvent.ProviderCustomerId = customerId;
        billingEvent.ProviderSubscriptionId = subscriptionId;

        await UpsertBillingProfileAsync(
            userId,
            customerId,
            subscriptionId,
            status,
            currentPeriodEndsAtUtc,
            cancellationToken)
            .ConfigureAwait(false);

        if (eventType == "customer.subscription.deleted" || IsNonEntitledSubscriptionStatus(status))
        {
            await userEntitlementService.SetTierAsync(
                userId,
                DarwinLinguaEntitlementTiers.Free,
                expiresAtUtc: null,
                updatedBy: $"stripe:{eventId}",
                cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        if (IsEntitledSubscriptionStatus(status))
        {
            await userEntitlementService.SetTierAsync(
                userId,
                DarwinLinguaEntitlementTiers.Premium,
                currentPeriodEndsAtUtc,
                updatedBy: $"stripe:{eventId}",
                cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task UpsertBillingProfileAsync(
        string userId,
        string? customerId,
        string? subscriptionId,
        string status,
        DateTimeOffset? currentPeriodEndsAtUtc,
        CancellationToken cancellationToken)
    {
        WebBillingProfile? profile = await dbContext.WebBillingProfiles
            .SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        if (profile is null)
        {
            profile = new WebBillingProfile
            {
                UserId = userId,
                ProviderName = ProviderName,
                PlanKey = options.Value.PremiumPlanKey,
                CreatedAtUtc = nowUtc,
            };
            dbContext.WebBillingProfiles.Add(profile);
        }

        profile.ProviderCustomerId = string.IsNullOrWhiteSpace(customerId) ? profile.ProviderCustomerId : customerId;
        profile.ProviderSubscriptionId = string.IsNullOrWhiteSpace(subscriptionId) ? profile.ProviderSubscriptionId : subscriptionId;
        profile.Status = status;
        profile.CurrentPeriodEndsAtUtc = currentPeriodEndsAtUtc;
        profile.UpdatedAtUtc = nowUtc;
    }

    private static bool IsEntitledSubscriptionStatus(string status) =>
        string.Equals(status, "active", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "trialing", StringComparison.OrdinalIgnoreCase);

    private static bool IsNonEntitledSubscriptionStatus(string status) =>
        string.Equals(status, "canceled", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "incomplete_expired", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "unpaid", StringComparison.OrdinalIgnoreCase);

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement value) ||
            value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static DateTimeOffset? GetUnixTime(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement value) ||
            value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return value.TryGetInt64(out long unixTime) ? DateTimeOffset.FromUnixTimeSeconds(unixTime) : null;
    }

    private static string? GetMetadataValue(JsonElement element, string key)
    {
        if (!element.TryGetProperty("metadata", out JsonElement metadata) ||
            metadata.ValueKind != JsonValueKind.Object ||
            !metadata.TryGetProperty(key, out JsonElement value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static string Summarize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string collapsed = value.ReplaceLineEndings(" ").Trim();
        return collapsed.Length <= 512 ? collapsed : collapsed[..512];
    }
}
