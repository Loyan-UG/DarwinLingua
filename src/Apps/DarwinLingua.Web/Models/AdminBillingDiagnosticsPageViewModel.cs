namespace DarwinLingua.Web.Models;

public sealed record AdminBillingDiagnosticsPageViewModel(
    string? Status,
    string? EventType,
    string? UserId,
    string? ProviderCustomerId,
    string? ProviderSubscriptionId,
    int Take,
    string? StatusMessage,
    string? ErrorMessage,
    AdminBillingReadinessViewModel Readiness,
    IReadOnlyList<AdminBillingEventItemViewModel> Events,
    IReadOnlyList<AdminBillingProfileItemViewModel> Profiles);

public sealed record AdminBillingReadinessViewModel(
    bool StripeEnabled,
    string PublicBaseUrl,
    string StripeApiBaseUrl,
    bool HasStripeSecretKey,
    bool HasStripeWebhookSecret,
    bool HasPremiumMonthlyPriceId,
    int WebhookToleranceMinutes,
    string PremiumPlanKey,
    IReadOnlyList<string> Warnings);

public sealed record AdminBillingEventItemViewModel(
    Guid Id,
    string ProviderName,
    string ProviderEventId,
    string EventType,
    string Status,
    string? UserId,
    string? ProviderCustomerId,
    string? ProviderSubscriptionId,
    string? ErrorSummary,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ProcessedAtUtc);

public sealed record AdminBillingProfileItemViewModel(
    string UserId,
    string ProviderName,
    string? ProviderCustomerId,
    string? ProviderSubscriptionId,
    string PlanKey,
    string Status,
    DateTimeOffset? CurrentPeriodEndsAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
