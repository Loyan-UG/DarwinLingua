using System.Net.Http.Headers;
using System.Text.Json;
using DarwinLingua.Identity;
using DarwinLingua.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public sealed class StripeCheckoutFulfillmentService(
    IHttpClientFactory httpClientFactory,
    WebIdentityDbContext dbContext,
    IUserEntitlementService userEntitlementService,
    IOptions<BillingOptions> options,
    ILogger<StripeCheckoutFulfillmentService> logger) : IStripeCheckoutFulfillmentService
{
    private const string ProviderName = "Stripe";

    public async Task<StripeCheckoutFulfillmentResult> FulfillCheckoutSessionAsync(
        string sessionId,
        string expectedUserId,
        CancellationToken cancellationToken)
    {
        string normalizedSessionId = NormalizeStripeIdentifier(sessionId, "cs_", 256);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedUserId);

        BillingOptions billingOptions = options.Value;
        if (!billingOptions.EnableStripe)
        {
            throw new InvalidOperationException("Stripe billing is not enabled.");
        }

        using HttpRequestMessage request = new(
            HttpMethod.Get,
            $"/v1/checkout/sessions/{Uri.EscapeDataString(normalizedSessionId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", billingOptions.StripeSecretKey);

        HttpClient client = httpClientFactory.CreateClient("StripeBilling");
        using HttpResponseMessage response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Stripe checkout session fulfillment failed for {SessionId} with status {StatusCode} ({ReasonPhrase}).",
                normalizedSessionId,
                (int)response.StatusCode,
                response.ReasonPhrase ?? "no reason phrase");
            throw new InvalidOperationException("Stripe checkout session fulfillment failed.");
        }

        using JsonDocument document = JsonDocument.Parse(responseBody);
        JsonElement session = document.RootElement;
        string returnedSessionId = GetString(session, "id") ?? normalizedSessionId;
        string? userId = GetMetadataValue(session, "darwin_user_id") ?? GetString(session, "client_reference_id");
        if (!string.Equals(userId, expectedUserId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Stripe checkout session does not belong to the authenticated user.");
        }

        string status = GetString(session, "status") ?? string.Empty;
        string mode = GetString(session, "mode") ?? string.Empty;
        string paymentStatus = GetString(session, "payment_status") ?? string.Empty;
        string? customerId = GetString(session, "customer");
        string? subscriptionId = GetString(session, "subscription");
        bool fulfilled =
            string.Equals(mode, "subscription", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(status, "complete", StringComparison.OrdinalIgnoreCase) &&
            IsPaidCheckoutStatus(paymentStatus) &&
            !string.IsNullOrWhiteSpace(customerId) &&
            !string.IsNullOrWhiteSpace(subscriptionId);

        if (fulfilled)
        {
            await UpsertBillingProfileAsync(
                    expectedUserId,
                    customerId,
                    subscriptionId,
                    "checkout.session.completed",
                    cancellationToken)
                .ConfigureAwait(false);

            await userEntitlementService.SetTierAsync(
                    expectedUserId,
                    DarwinLinguaEntitlementTiers.Premium,
                    expiresAtUtc: null,
                    updatedBy: $"stripe-success:{returnedSessionId}",
                    cancellationToken)
                .ConfigureAwait(false);
        }

        return new StripeCheckoutFulfillmentResult(
            returnedSessionId,
            expectedUserId,
            customerId,
            subscriptionId,
            status,
            fulfilled);
    }

    private async Task UpsertBillingProfileAsync(
        string userId,
        string? customerId,
        string? subscriptionId,
        string status,
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
        profile.UpdatedAtUtc = nowUtc;
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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

    private static bool IsPaidCheckoutStatus(string paymentStatus) =>
        string.Equals(paymentStatus, "paid", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(paymentStatus, "no_payment_required", StringComparison.OrdinalIgnoreCase);
}
