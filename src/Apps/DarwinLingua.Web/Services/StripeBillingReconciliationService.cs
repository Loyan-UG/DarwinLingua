using System.Net.Http.Headers;
using System.Text.Json;
using DarwinLingua.Identity;
using DarwinLingua.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public sealed class StripeBillingReconciliationService(
    IHttpClientFactory httpClientFactory,
    WebIdentityDbContext dbContext,
    IUserEntitlementService userEntitlementService,
    UserManager<DarwinLinguaIdentityUser> userManager,
    IBillingNotificationEmailService billingNotificationEmailService,
    IOptions<BillingOptions> options,
    ILogger<StripeBillingReconciliationService> logger) : IStripeBillingReconciliationService
{
    private const string ProviderName = "Stripe";

    public async Task<StripeBillingReconciliationResult> ReconcileSubscriptionAsync(
        string subscriptionId,
        string updatedBy,
        CancellationToken cancellationToken)
    {
        string normalizedSubscriptionId = NormalizeStripeIdentifier(subscriptionId, "sub_", 128);
        string normalizedUpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "admin" : updatedBy.Trim();
        BillingOptions billingOptions = options.Value;

        if (!billingOptions.EnableStripe)
        {
            throw new InvalidOperationException("Stripe billing is not enabled.");
        }

        using HttpRequestMessage request = new(
            HttpMethod.Get,
            $"/v1/subscriptions/{Uri.EscapeDataString(normalizedSubscriptionId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", billingOptions.StripeSecretKey);

        HttpClient client = httpClientFactory.CreateClient("StripeBilling");
        using HttpResponseMessage response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Stripe subscription reconciliation failed for {SubscriptionId} with status {StatusCode} ({ReasonPhrase}).",
                normalizedSubscriptionId,
                (int)response.StatusCode,
                response.ReasonPhrase ?? "no reason phrase");
            throw new InvalidOperationException("Stripe subscription reconciliation failed.");
        }

        using JsonDocument document = JsonDocument.Parse(responseBody);
        JsonElement subscription = document.RootElement;
        string returnedSubscriptionId = GetString(subscription, "id") ?? normalizedSubscriptionId;
        string? customerId = GetString(subscription, "customer");
        string? userId = GetMetadataValue(subscription, "darwin_user_id");

        if (string.IsNullOrWhiteSpace(userId))
        {
            WebBillingProfile? existingProfile = await dbContext.WebBillingProfiles
                .AsNoTracking()
                .OrderByDescending(profile => profile.UpdatedAtUtc)
                .FirstOrDefaultAsync(profile =>
                    profile.ProviderSubscriptionId == returnedSubscriptionId ||
                    (!string.IsNullOrWhiteSpace(customerId) && profile.ProviderCustomerId == customerId),
                    cancellationToken)
                .ConfigureAwait(false);
            userId = existingProfile?.UserId;
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("Stripe subscription could not be mapped to a Darwin Lingua user.");
        }

        string status = GetString(subscription, "status") ?? string.Empty;
        DateTimeOffset? currentPeriodEndsAtUtc = GetUnixTime(subscription, "current_period_end");
        string entitlementTier = ResolveEntitlementTier(status);

        await UpsertBillingProfileAsync(
                userId,
                customerId,
                returnedSubscriptionId,
                status,
                currentPeriodEndsAtUtc,
                cancellationToken)
            .ConfigureAwait(false);

        await userEntitlementService.SetTierAsync(
                userId,
                entitlementTier,
                string.Equals(entitlementTier, DarwinLinguaEntitlementTiers.Premium, StringComparison.Ordinal)
                    ? currentPeriodEndsAtUtc
                    : null,
                $"stripe-reconcile:{normalizedUpdatedBy}",
                cancellationToken)
            .ConfigureAwait(false);

        await SendReconciliationEmailsAsync(
                userId,
                returnedSubscriptionId,
                status,
                currentPeriodEndsAtUtc,
                entitlementTier,
                normalizedUpdatedBy,
                cancellationToken)
            .ConfigureAwait(false);

        return new StripeBillingReconciliationResult(
            userId,
            returnedSubscriptionId,
            customerId,
            status,
            currentPeriodEndsAtUtc,
            entitlementTier);
    }

    private async Task SendReconciliationEmailsAsync(
        string userId,
        string subscriptionId,
        string billingStatus,
        DateTimeOffset? currentPeriodEndsAtUtc,
        string entitlementTier,
        string adminActor,
        CancellationToken cancellationToken)
    {
        DarwinLinguaIdentityUser? user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(user?.Email))
        {
            if (string.Equals(entitlementTier, DarwinLinguaEntitlementTiers.Premium, StringComparison.Ordinal))
            {
                await billingNotificationEmailService
                    .SendPremiumActivatedAsync(
                        user.Email,
                        user.Id,
                        billingStatus,
                        subscriptionId,
                        currentPeriodEndsAtUtc,
                        null,
                        $"stripe-reconcile:{subscriptionId}",
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                await billingNotificationEmailService
                    .SendPremiumEndedAsync(
                        user.Email,
                        user.Id,
                        billingStatus,
                        subscriptionId,
                        null,
                        $"stripe-reconcile:{subscriptionId}",
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        await billingNotificationEmailService
            .SendAdminReconciliationCompletedAsync(
                subscriptionId,
                userId,
                billingStatus,
                entitlementTier,
                adminActor,
                null,
                $"stripe-reconcile:{subscriptionId}",
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task UpsertBillingProfileAsync(
        string userId,
        string? customerId,
        string subscriptionId,
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
        profile.ProviderSubscriptionId = subscriptionId;
        profile.Status = status;
        profile.CurrentPeriodEndsAtUtc = currentPeriodEndsAtUtc;
        profile.UpdatedAtUtc = nowUtc;
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string ResolveEntitlementTier(string status)
    {
        if (string.Equals(status, "active", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "trialing", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "past_due", StringComparison.OrdinalIgnoreCase))
        {
            return DarwinLinguaEntitlementTiers.Premium;
        }

        return DarwinLinguaEntitlementTiers.Free;
    }

    private static string NormalizeStripeIdentifier(string value, string expectedPrefix, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A Stripe identifier is required.", nameof(value));
        }

        string trimmed = value.Trim();
        if (trimmed.Length > maxLength ||
            !trimmed.StartsWith(expectedPrefix, StringComparison.Ordinal) ||
            !trimmed.All(static character =>
                char.IsAsciiLetterOrDigit(character) ||
                character == '_' ||
                character == '-'))
        {
            throw new ArgumentException("The Stripe identifier format is not supported.", nameof(value));
        }

        return trimmed;
    }

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

}
