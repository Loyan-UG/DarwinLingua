namespace DarwinLingua.Web.Services;

public sealed record StripeCheckoutFulfillmentResult(
    string SessionId,
    string UserId,
    string? CustomerId,
    string? SubscriptionId,
    string Status,
    bool Fulfilled);
