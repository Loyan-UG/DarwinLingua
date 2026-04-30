namespace DarwinLingua.Web.Services;

public interface IStripeCheckoutFulfillmentService
{
    Task<StripeCheckoutFulfillmentResult> FulfillCheckoutSessionAsync(
        string sessionId,
        string expectedUserId,
        CancellationToken cancellationToken);
}
