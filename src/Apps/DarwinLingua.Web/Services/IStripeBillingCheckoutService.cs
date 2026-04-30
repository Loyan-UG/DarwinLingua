namespace DarwinLingua.Web.Services;

public interface IStripeBillingCheckoutService
{
    Task<StripeCheckoutSessionResult> CreatePremiumCheckoutSessionAsync(
        string userId,
        string? email,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken);

    Task<StripeCheckoutSessionResult> CreateCustomerPortalSessionAsync(
        string customerId,
        string returnUrl,
        CancellationToken cancellationToken);
}
