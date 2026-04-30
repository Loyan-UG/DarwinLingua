using DarwinLingua.Web.Data;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/billing-diagnostics")]
public sealed class BillingDiagnosticsController(
    WebIdentityDbContext dbContext,
    IOptions<BillingOptions> billingOptions,
    IStripeBillingReconciliationService reconciliationService,
    IBillingOperationRateLimiter rateLimiter,
    ILogger<BillingDiagnosticsController> logger) : Controller
{
    [HttpGet("", Name = "Admin_BillingDiagnostics")]
    public async Task<IActionResult> Index(
        [FromQuery] string? status,
        [FromQuery] string? eventType,
        [FromQuery] string? userId,
        [FromQuery] string? providerCustomerId,
        [FromQuery] string? providerSubscriptionId,
        CancellationToken cancellationToken,
        [FromQuery] int take = 100)
    {
        int boundedTake = Math.Clamp(take, 1, 200);
        string? normalizedStatus = NormalizeAllowedValue(status, IsAllowedEventStatus, 64);
        string? normalizedEventType = NormalizeAllowedValue(eventType, IsAllowedStripeEventType, 128);
        string? normalizedUserId = NormalizeLength(userId, 450);
        string? normalizedProviderCustomerId = NormalizeLength(providerCustomerId, 128);
        string? normalizedProviderSubscriptionId = NormalizeLength(providerSubscriptionId, 128);
        string? errorMessage = BuildQueryValidationMessage(
            status,
            normalizedStatus,
            eventType,
            normalizedEventType);

        IQueryable<WebBillingEvent> eventQuery = dbContext.WebBillingEvents.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            eventQuery = eventQuery.Where(billingEvent => billingEvent.Status == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(normalizedEventType))
        {
            eventQuery = eventQuery.Where(billingEvent => billingEvent.EventType == normalizedEventType);
        }

        if (!string.IsNullOrWhiteSpace(normalizedUserId))
        {
            eventQuery = eventQuery.Where(billingEvent => billingEvent.UserId == normalizedUserId);
        }

        if (!string.IsNullOrWhiteSpace(normalizedProviderCustomerId))
        {
            eventQuery = eventQuery.Where(billingEvent => billingEvent.ProviderCustomerId == normalizedProviderCustomerId);
        }

        if (!string.IsNullOrWhiteSpace(normalizedProviderSubscriptionId))
        {
            eventQuery = eventQuery.Where(billingEvent => billingEvent.ProviderSubscriptionId == normalizedProviderSubscriptionId);
        }

        WebBillingEvent[] events = await eventQuery
            .OrderByDescending(billingEvent => billingEvent.CreatedAtUtc)
            .Take(boundedTake)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        IQueryable<WebBillingProfile> profileQuery = dbContext.WebBillingProfiles.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(normalizedUserId))
        {
            profileQuery = profileQuery.Where(profile => profile.UserId == normalizedUserId);
        }

        if (!string.IsNullOrWhiteSpace(normalizedProviderCustomerId))
        {
            profileQuery = profileQuery.Where(profile => profile.ProviderCustomerId == normalizedProviderCustomerId);
        }

        if (!string.IsNullOrWhiteSpace(normalizedProviderSubscriptionId))
        {
            profileQuery = profileQuery.Where(profile => profile.ProviderSubscriptionId == normalizedProviderSubscriptionId);
        }

        WebBillingProfile[] profiles = await profileQuery
            .OrderByDescending(profile => profile.UpdatedAtUtc)
            .Take(boundedTake)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        AdminBillingDiagnosticsPageViewModel model = new(
            normalizedStatus,
            normalizedEventType,
            normalizedUserId,
            normalizedProviderCustomerId,
            normalizedProviderSubscriptionId,
            boundedTake,
            TempData["StatusMessage"] as string,
            errorMessage ?? TempData["ErrorMessage"] as string,
            BuildReadiness(),
            events.Select(static billingEvent => new AdminBillingEventItemViewModel(
                    billingEvent.Id,
                    billingEvent.ProviderName,
                    billingEvent.ProviderEventId,
                    billingEvent.EventType,
                    billingEvent.Status,
                    billingEvent.UserId,
                    billingEvent.ProviderCustomerId,
                    billingEvent.ProviderSubscriptionId,
                    billingEvent.ErrorSummary,
                    billingEvent.CreatedAtUtc,
                    billingEvent.ProcessedAtUtc))
                .ToArray(),
            profiles.Select(static profile => new AdminBillingProfileItemViewModel(
                    profile.UserId,
                    profile.ProviderName,
                    profile.ProviderCustomerId,
                    profile.ProviderSubscriptionId,
                    profile.PlanKey,
                    profile.Status,
                    profile.CurrentPeriodEndsAtUtc,
                    profile.CreatedAtUtc,
                    profile.UpdatedAtUtc))
                .ToArray());

        return View(model);
    }

    [HttpPost("stripe/subscriptions/reconcile", Name = "Admin_BillingDiagnostics_ReconcileStripeSubscription")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ReconcileStripeSubscription(
        [FromForm] string? providerSubscriptionId,
        CancellationToken cancellationToken)
    {
        string normalizedSubscriptionId = providerSubscriptionId?.Trim() ?? string.Empty;
        if (!IsAllowedStripeSubscriptionId(normalizedSubscriptionId))
        {
            TempData["ErrorMessage"] = "A valid Stripe subscription id is required.";
            return RedirectToAction(nameof(Index));
        }

        string adminActor = WebUserIdentity.TryGetEmail(User) ?? User.Identity?.Name ?? "admin";
        if (!rateLimiter.TryConsume("stripe-reconcile", adminActor, 10, TimeSpan.FromHours(1)))
        {
            TempData["ErrorMessage"] = "Too many reconciliation attempts. Please wait before trying again.";
            return RedirectToAction(nameof(Index), new { providerSubscriptionId = normalizedSubscriptionId });
        }

        try
        {
            StripeBillingReconciliationResult result = await reconciliationService
                .ReconcileSubscriptionAsync(
                    normalizedSubscriptionId,
                    adminActor,
                    cancellationToken)
                .ConfigureAwait(false);

            logger.LogInformation(
                "Admin reconciled Stripe subscription {SubscriptionId} for user {UserId}; Stripe status {StripeStatus}, entitlement {EntitlementTier}.",
                result.SubscriptionId,
                result.UserId,
                result.Status,
                result.EntitlementTier);
            TempData["StatusMessage"] =
                $"Subscription {result.SubscriptionId} reconciled for user {result.UserId}; entitlement is {result.EntitlementTier}.";
            return RedirectToAction(nameof(Index), new { providerSubscriptionId = result.SubscriptionId });
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Stripe subscription reconciliation failed for {SubscriptionId}.", normalizedSubscriptionId);
            TempData["ErrorMessage"] = "Stripe subscription reconciliation failed. Check configuration, subscription id, and Stripe account access.";
            return RedirectToAction(nameof(Index), new { providerSubscriptionId = normalizedSubscriptionId });
        }
    }

    private AdminBillingReadinessViewModel BuildReadiness()
    {
        BillingOptions options = billingOptions.Value;
        List<string> warnings = [];

        if (!options.EnableStripe)
        {
            warnings.Add("Stripe billing is disabled in this environment.");
        }

        if (string.IsNullOrWhiteSpace(options.PublicBaseUrl))
        {
            warnings.Add("Billing PublicBaseUrl is not configured; checkout URLs will use request host fallback.");
        }

        if (options.EnableStripe && string.IsNullOrWhiteSpace(options.StripeSecretKey))
        {
            warnings.Add("Stripe billing is enabled but StripeSecretKey is missing.");
        }

        if (options.EnableStripe && string.IsNullOrWhiteSpace(options.StripeWebhookSecret))
        {
            warnings.Add("Stripe billing is enabled but StripeWebhookSecret is missing.");
        }

        if (options.EnableStripe && string.IsNullOrWhiteSpace(options.StripePremiumMonthlyPriceId))
        {
            warnings.Add("Stripe billing is enabled but StripePremiumMonthlyPriceId is missing.");
        }

        if (!Uri.TryCreate(options.StripeApiBaseUrl, UriKind.Absolute, out _))
        {
            warnings.Add("StripeApiBaseUrl is not a valid absolute URL.");
        }

        return new AdminBillingReadinessViewModel(
            options.EnableStripe,
            options.PublicBaseUrl,
            options.StripeApiBaseUrl,
            !string.IsNullOrWhiteSpace(options.StripeSecretKey),
            !string.IsNullOrWhiteSpace(options.StripeWebhookSecret),
            !string.IsNullOrWhiteSpace(options.StripePremiumMonthlyPriceId),
            options.StripeWebhookToleranceMinutes,
            options.PremiumPlanKey,
            warnings);
    }

    private static string? NormalizeLength(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static string? NormalizeAllowedValue(string? value, Func<string, bool> isAllowed, int maxLength)
    {
        string? normalized = NormalizeLength(value, maxLength);
        return normalized is not null && isAllowed(normalized)
            ? normalized
            : null;
    }

    private static string? BuildQueryValidationMessage(params object?[] values)
    {
        for (int index = 0; index < values.Length; index += 2)
        {
            string? original = values[index] as string;
            string? normalized = values[index + 1] as string;
            if (!string.IsNullOrWhiteSpace(original) && string.IsNullOrWhiteSpace(normalized))
            {
                return "One or more billing diagnostic filters were ignored because they were not supported.";
            }
        }

        return null;
    }

    private static bool IsAllowedEventStatus(string status) =>
        string.Equals(status, "Received", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Processed", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase);

    private static bool IsAllowedStripeEventType(string eventType) =>
        string.Equals(eventType, "checkout.session.completed", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(eventType, "customer.subscription.created", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(eventType, "customer.subscription.updated", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(eventType, "customer.subscription.deleted", StringComparison.OrdinalIgnoreCase);

    private static bool IsAllowedStripeSubscriptionId(string value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Length <= 128 &&
        value.StartsWith("sub_", StringComparison.Ordinal) &&
        value.All(static character =>
            char.IsAsciiLetterOrDigit(character) ||
            character == '_' ||
            character == '-');
}
