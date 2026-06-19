using System.Security.Cryptography;
using System.Text;
using DarwinLingua.Web.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class StripeWebhookSafetyTests
{
    [Fact]
    public void StripeWebhookVerifier_ShouldAcceptOnlyCurrentValidV1Signatures()
    {
        DateTimeOffset nowUtc = new(2026, 6, 18, 12, 0, 0, TimeSpan.Zero);
        const string payload = "{\"id\":\"evt_valid\",\"type\":\"checkout.session.completed\"}";
        const string secret = "whsec_test_secret";
        StripeWebhookVerifier verifier = new(Options.Create(new BillingOptions
        {
            StripeWebhookSecret = secret,
            StripeWebhookToleranceMinutes = 5
        }));

        string validHeader = BuildStripeSignatureHeader(payload, secret, nowUtc);

        Assert.True(verifier.IsValid(payload, validHeader, nowUtc));
        Assert.False(verifier.IsValid(payload, null, nowUtc));
        Assert.False(verifier.IsValid(payload, "t=not-a-number,v1=abc", nowUtc));
        Assert.False(verifier.IsValid(payload, validHeader.Replace("v1=", "v1=bad", StringComparison.Ordinal), nowUtc));
        Assert.False(verifier.IsValid(payload, BuildStripeSignatureHeader(payload, secret, nowUtc.AddMinutes(-10)), nowUtc));
    }

    [Fact]
    public void StripeWebhookControllerAndHandler_ShouldFailClosedAndKeepEventsIdempotent()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string controllerSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Controllers",
            "StripeBillingWebhookController.cs"));
        string handlerSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Services",
            "StripeBillingWebhookHandler.cs"));

        Assert.Contains("[AllowAnonymous]", controllerSource, StringComparison.Ordinal);
        Assert.Contains("[RequestSizeLimit(64 * 1024)]", controllerSource, StringComparison.Ordinal);
        Assert.Contains("Request.Headers[\"Stripe-Signature\"]", controllerSource, StringComparison.Ordinal);
        Assert.Contains("!verifier.IsValid(payload, signatureHeader, DateTimeOffset.UtcNow)", controllerSource, StringComparison.Ordinal);
        Assert.Contains("return Unauthorized();", controllerSource, StringComparison.Ordinal);
        Assert.Contains("await handler.HandleAsync(payload", controllerSource, StringComparison.Ordinal);

        Assert.Contains("billingEvent?.Status == ProcessedStatus", handlerSource, StringComparison.Ordinal);
        Assert.Contains("return;", handlerSource, StringComparison.Ordinal);
        Assert.Contains("case \"checkout.session.completed\"", handlerSource, StringComparison.Ordinal);
        Assert.Contains("case \"customer.subscription.created\"", handlerSource, StringComparison.Ordinal);
        Assert.Contains("case \"customer.subscription.updated\"", handlerSource, StringComparison.Ordinal);
        Assert.Contains("case \"customer.subscription.deleted\"", handlerSource, StringComparison.Ordinal);
        Assert.Contains("Stripe checkout session is not a completed paid subscription checkout.", handlerSource, StringComparison.Ordinal);
        Assert.Contains("DarwinLinguaEntitlementTiers.Premium", handlerSource, StringComparison.Ordinal);
        Assert.Contains("DarwinLinguaEntitlementTiers.Free", handlerSource, StringComparison.Ordinal);
        Assert.Contains("IsEntitledSubscriptionStatus(status)", handlerSource, StringComparison.Ordinal);
        Assert.Contains("IsNonEntitledSubscriptionStatus(status)", handlerSource, StringComparison.Ordinal);
        Assert.Contains("Stripe subscription event could not be mapped to a Darwin Lingua user.", handlerSource, StringComparison.Ordinal);
        Assert.Contains("billingEvent.Status = FailedStatus", handlerSource, StringComparison.Ordinal);
        Assert.Contains("billingEvent.ErrorSummary = Summarize(exception.Message)", handlerSource, StringComparison.Ordinal);
    }

    private static string BuildStripeSignatureHeader(string payload, string secret, DateTimeOffset timestamp)
    {
        string unixTimestamp = timestamp.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);
        string signedPayload = $"{unixTimestamp}.{payload}";
        byte[] digest = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret),
            Encoding.UTF8.GetBytes(signedPayload));
        string signature = Convert.ToHexString(digest).ToLowerInvariant();
        return $"t={unixTimestamp},v1={signature}";
    }

    private static string ResolveRepositoryRoot()
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string candidateSolutionPath = Path.Combine(currentDirectory.FullName, "DarwinLingua.slnx");

            if (File.Exists(candidateSolutionPath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException("Unable to resolve repository root from test execution directory.");
    }
}
