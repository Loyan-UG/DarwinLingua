namespace DarwinLingua.Web.Services;

public sealed record StripeBillingReconciliationResult(
    string UserId,
    string SubscriptionId,
    string? CustomerId,
    string Status,
    DateTimeOffset? CurrentPeriodEndsAtUtc,
    string EntitlementTier);
