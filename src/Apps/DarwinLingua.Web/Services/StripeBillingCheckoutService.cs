using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public sealed class StripeBillingCheckoutService(
    IHttpClientFactory httpClientFactory,
    IOptions<BillingOptions> options,
    ILogger<StripeBillingCheckoutService> logger) : IStripeBillingCheckoutService
{
    public async Task<StripeCheckoutSessionResult> CreatePremiumCheckoutSessionAsync(
        string userId,
        string? email,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(successUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(cancelUrl);

        BillingOptions billingOptions = options.Value;
        if (!billingOptions.EnableStripe)
        {
            throw new InvalidOperationException("Stripe billing is not enabled.");
        }

        using HttpRequestMessage request = new(HttpMethod.Post, "/v1/checkout/sessions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", billingOptions.StripeSecretKey);
        request.Headers.Add("Idempotency-Key", $"darwin-premium-checkout-{userId}-{Guid.NewGuid():N}");

        Dictionary<string, string> fields = new(StringComparer.Ordinal)
        {
            ["mode"] = "subscription",
            ["client_reference_id"] = userId,
            ["line_items[0][price]"] = billingOptions.StripePremiumMonthlyPriceId,
            ["line_items[0][quantity]"] = "1",
            ["success_url"] = successUrl,
            ["cancel_url"] = cancelUrl,
            ["metadata[darwin_user_id]"] = userId,
            ["metadata[plan_key]"] = billingOptions.PremiumPlanKey,
            ["subscription_data[metadata][darwin_user_id]"] = userId,
            ["subscription_data[metadata][plan_key]"] = billingOptions.PremiumPlanKey,
            ["allow_promotion_codes"] = "true",
        };

        if (!string.IsNullOrWhiteSpace(email))
        {
            fields["customer_email"] = email.Trim();
        }

        request.Content = new FormUrlEncodedContent(fields);

        HttpClient client = httpClientFactory.CreateClient("StripeBilling");
        using HttpResponseMessage response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Stripe checkout session creation failed with status {StatusCode}: {BodySummary}",
                (int)response.StatusCode,
                Summarize(responseBody));
            throw new InvalidOperationException("Stripe checkout session creation failed.");
        }

        using JsonDocument document = JsonDocument.Parse(responseBody);
        JsonElement root = document.RootElement;
        string? sessionId = root.TryGetProperty("id", out JsonElement idElement) ? idElement.GetString() : null;
        string? url = root.TryGetProperty("url", out JsonElement urlElement) ? urlElement.GetString() : null;

        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(url))
        {
            logger.LogWarning("Stripe checkout response did not contain a usable session id and URL.");
            throw new InvalidOperationException("Stripe checkout response was incomplete.");
        }

        return new StripeCheckoutSessionResult(sessionId, url);
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
