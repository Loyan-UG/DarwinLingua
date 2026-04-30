using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public sealed class BillingOptions
{
    public const string SectionName = "Billing";

    public bool EnableStripe { get; set; }

    public string PublicBaseUrl { get; set; } = string.Empty;

    public string StripeApiBaseUrl { get; set; } = "https://api.stripe.com";

    public string StripeSecretKey { get; set; } = string.Empty;

    public string StripeWebhookSecret { get; set; } = string.Empty;

    public string StripePremiumMonthlyPriceId { get; set; } = string.Empty;

    public int StripeWebhookToleranceMinutes { get; set; } = 5;

    public string PremiumPlanKey { get; set; } = "premium-monthly";
}

public sealed class BillingOptionsValidator : IValidateOptions<BillingOptions>
{
    public ValidateOptionsResult Validate(string? name, BillingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.EnableStripe)
        {
            return ValidateOptionsResult.Success;
        }

        List<string> failures = [];

        if (string.IsNullOrWhiteSpace(options.StripeSecretKey))
        {
            failures.Add("Billing:StripeSecretKey is required when Stripe billing is enabled.");
        }

        if (string.IsNullOrWhiteSpace(options.StripeWebhookSecret))
        {
            failures.Add("Billing:StripeWebhookSecret is required when Stripe billing is enabled.");
        }

        if (string.IsNullOrWhiteSpace(options.StripePremiumMonthlyPriceId))
        {
            failures.Add("Billing:StripePremiumMonthlyPriceId is required when Stripe billing is enabled.");
        }

        if (!Uri.TryCreate(options.StripeApiBaseUrl, UriKind.Absolute, out _))
        {
            failures.Add("Billing:StripeApiBaseUrl must be an absolute URL.");
        }

        if (!string.IsNullOrWhiteSpace(options.PublicBaseUrl) &&
            !Uri.TryCreate(options.PublicBaseUrl, UriKind.Absolute, out _))
        {
            failures.Add("Billing:PublicBaseUrl must be an absolute URL when set.");
        }

        if (options.StripeWebhookToleranceMinutes is < 1 or > 60)
        {
            failures.Add("Billing:StripeWebhookToleranceMinutes must be between 1 and 60.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
