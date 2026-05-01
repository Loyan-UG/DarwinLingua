using DarwinLingua.Identity;

namespace DarwinLingua.Web.Models;

public sealed record BillingPageViewModel(
    UserEntitlementSnapshot Entitlement,
    bool StripeEnabled,
    string PlanKey,
    bool CanStartCheckout,
    bool CanManageStripeSubscription,
    string? BillingStatus,
    DateTimeOffset? CurrentPeriodEndsAtUtc,
    bool StripeConfigured,
    string? ProviderCustomerId,
    string? ProviderSubscriptionId,
    IReadOnlyList<BillingEventItemViewModel> RecentEvents);

public sealed record BillingEventItemViewModel(
    string ProviderName,
    string EventType,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ProcessedAtUtc,
    string? ErrorSummary);
