using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class BillingSafetyStructuralTests
{
    [Fact]
    public void BillingController_ShouldGateCheckoutPortalAndSuccessSafely()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string controllerSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Controllers",
            "BillingController.cs"));
        string viewSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Views",
            "Billing",
            "Index.cshtml"));
        string adminBillingDiagnosticsControllerSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Areas",
            "Admin",
            "Controllers",
            "BillingDiagnosticsController.cs"));

        Assert.Contains("[Authorize]", controllerSource, StringComparison.Ordinal);
        Assert.Contains("userEntitlementService.GetCurrentAsync(userId", controllerSource, StringComparison.Ordinal);
        Assert.Contains("billingOptions.EnableStripe && !string.Equals(entitlement.Tier", controllerSource, StringComparison.Ordinal);
        Assert.Contains("billingOptions.EnableStripe && !string.IsNullOrWhiteSpace(billingProfile?.ProviderCustomerId)", controllerSource, StringComparison.Ordinal);
        Assert.Contains("IsStripeConfigured(billingOptions)", controllerSource, StringComparison.Ordinal);
        Assert.Contains("if (!billingOptions.EnableStripe)", controllerSource, StringComparison.Ordinal);
        Assert.Contains("if (!IsStripeConfigured(billingOptions))", controllerSource, StringComparison.Ordinal);
        Assert.Contains("rateLimiter.TryConsume(\"stripe-checkout\", userId, 5, TimeSpan.FromMinutes(10))", controllerSource, StringComparison.Ordinal);
        Assert.Contains("rateLimiter.TryConsume(\"stripe-portal\", userId, 10, TimeSpan.FromMinutes(10))", controllerSource, StringComparison.Ordinal);
        Assert.Contains("SingleOrDefaultAsync(profile => profile.UserId == userId", controllerSource, StringComparison.Ordinal);
        Assert.Contains("CreateCustomerPortalSessionAsync(billingProfile.ProviderCustomerId", controllerSource, StringComparison.Ordinal);
        Assert.Contains("CreatePremiumCheckoutSessionAsync(userId, email", controllerSource, StringComparison.Ordinal);
        Assert.Contains("FulfillCheckoutSessionAsync(sessionId, GetUserId()", controllerSource, StringComparison.Ordinal);

        Assert.Contains("Model.Entitlement.Tier", viewSource, StringComparison.Ordinal);
        Assert.Contains("!Model.StripeEnabled", viewSource, StringComparison.Ordinal);
        Assert.Contains("!Model.StripeConfigured", viewSource, StringComparison.Ordinal);
        Assert.Contains("Model.CanStartCheckout", viewSource, StringComparison.Ordinal);
        Assert.Contains("Model.CanManageStripeSubscription", viewSource, StringComparison.Ordinal);
        Assert.Contains("Continue to Stripe", viewSource, StringComparison.Ordinal);
        Assert.Contains("Manage in Stripe", viewSource, StringComparison.Ordinal);

        Assert.Contains("[Authorize(Policy = \"Admin\")]", adminBillingDiagnosticsControllerSource, StringComparison.Ordinal);
        Assert.Contains("IsAllowedStripeSubscriptionId(normalizedSubscriptionId)", adminBillingDiagnosticsControllerSource, StringComparison.Ordinal);
        Assert.Contains("rateLimiter.TryConsume(\"stripe-reconcile\", adminActor, 10, TimeSpan.FromHours(1))", adminBillingDiagnosticsControllerSource, StringComparison.Ordinal);
        Assert.Contains("Too many reconciliation attempts. Please wait before trying again.", adminBillingDiagnosticsControllerSource, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminBillingDiagnostics_ShouldExposeReadinessFiltersAndSafeReconciliation()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string controllerSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Areas",
            "Admin",
            "Controllers",
            "BillingDiagnosticsController.cs"));
        string viewSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Areas",
            "Admin",
            "Views",
            "BillingDiagnostics",
            "Index.cshtml"));
        string viewModelSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Models",
            "AdminBillingDiagnosticsPageViewModel.cs"));
        string reconciliationServiceSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Services",
            "StripeBillingReconciliationService.cs"));

        Assert.Contains("[Authorize(Policy = \"Operator\")]", controllerSource, StringComparison.Ordinal);
        Assert.Contains("[Authorize(Policy = \"Admin\")]", controllerSource, StringComparison.Ordinal);
        Assert.Contains("[FromQuery] string? status", controllerSource, StringComparison.Ordinal);
        Assert.Contains("[FromQuery] string? eventType", controllerSource, StringComparison.Ordinal);
        Assert.Contains("[FromQuery] string? userId", controllerSource, StringComparison.Ordinal);
        Assert.Contains("[FromQuery] string? providerCustomerId", controllerSource, StringComparison.Ordinal);
        Assert.Contains("[FromQuery] string? providerSubscriptionId", controllerSource, StringComparison.Ordinal);
        Assert.Contains("Math.Clamp(take, 1, 200)", controllerSource, StringComparison.Ordinal);
        Assert.Contains("billingEvent.Status == normalizedStatus", controllerSource, StringComparison.Ordinal);
        Assert.Contains("billingEvent.EventType == normalizedEventType", controllerSource, StringComparison.Ordinal);
        Assert.Contains("billingEvent.UserId == normalizedUserId", controllerSource, StringComparison.Ordinal);
        Assert.Contains("billingEvent.ProviderCustomerId == normalizedProviderCustomerId", controllerSource, StringComparison.Ordinal);
        Assert.Contains("billingEvent.ProviderSubscriptionId == normalizedProviderSubscriptionId", controllerSource, StringComparison.Ordinal);
        Assert.Contains("profile.UserId == normalizedUserId", controllerSource, StringComparison.Ordinal);
        Assert.Contains("profile.ProviderCustomerId == normalizedProviderCustomerId", controllerSource, StringComparison.Ordinal);
        Assert.Contains("profile.ProviderSubscriptionId == normalizedProviderSubscriptionId", controllerSource, StringComparison.Ordinal);
        Assert.Contains("BuildReadiness()", controllerSource, StringComparison.Ordinal);
        Assert.Contains("!string.IsNullOrWhiteSpace(options.StripeSecretKey)", controllerSource, StringComparison.Ordinal);
        Assert.Contains("!string.IsNullOrWhiteSpace(options.StripeWebhookSecret)", controllerSource, StringComparison.Ordinal);
        Assert.Contains("IsAllowedStripeSubscriptionId(normalizedSubscriptionId)", controllerSource, StringComparison.Ordinal);
        Assert.Contains("ReconcileSubscriptionAsync(", controllerSource, StringComparison.Ordinal);

        Assert.Contains("HasStripeSecretKey", viewModelSource, StringComparison.Ordinal);
        Assert.Contains("HasStripeWebhookSecret", viewModelSource, StringComparison.Ordinal);
        Assert.Contains("HasPremiumMonthlyPriceId", viewModelSource, StringComparison.Ordinal);
        Assert.Contains("Model.Readiness.HasStripeSecretKey ? T[\"Configured\"] : T[\"Missing\"]", viewSource, StringComparison.Ordinal);
        Assert.Contains("Model.Readiness.HasStripeWebhookSecret ? T[\"Configured\"] : T[\"Missing\"]", viewSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Model.Readiness.StripeSecretKey", viewSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Model.Readiness.StripeWebhookSecret", viewSource, StringComparison.Ordinal);
        Assert.Contains("name=\"status\"", viewSource, StringComparison.Ordinal);
        Assert.Contains("name=\"eventType\"", viewSource, StringComparison.Ordinal);
        Assert.Contains("name=\"userId\"", viewSource, StringComparison.Ordinal);
        Assert.Contains("name=\"providerCustomerId\"", viewSource, StringComparison.Ordinal);
        Assert.Contains("name=\"providerSubscriptionId\"", viewSource, StringComparison.Ordinal);
        Assert.Contains("Model.Events", viewSource, StringComparison.Ordinal);
        Assert.Contains("Model.Profiles", viewSource, StringComparison.Ordinal);

        Assert.Contains("NormalizeStripeIdentifier(subscriptionId, \"sub_\", 128)", reconciliationServiceSource, StringComparison.Ordinal);
        Assert.Contains("responseBody", reconciliationServiceSource, StringComparison.Ordinal);
        Assert.Contains("Stripe subscription could not be mapped to a Darwin Lingua user.", reconciliationServiceSource, StringComparison.Ordinal);
        Assert.Contains("ResolveEntitlementTier(status)", reconciliationServiceSource, StringComparison.Ordinal);
        Assert.Contains("string.Equals(status, \"active\"", reconciliationServiceSource, StringComparison.Ordinal);
        Assert.Contains("string.Equals(status, \"trialing\"", reconciliationServiceSource, StringComparison.Ordinal);
        Assert.Contains("string.Equals(status, \"past_due\"", reconciliationServiceSource, StringComparison.Ordinal);
        Assert.Contains("return DarwinLinguaEntitlementTiers.Free", reconciliationServiceSource, StringComparison.Ordinal);
        Assert.Contains("SendPremiumActivatedAsync", reconciliationServiceSource, StringComparison.Ordinal);
        Assert.Contains("SendPremiumEndedAsync", reconciliationServiceSource, StringComparison.Ordinal);
        Assert.Contains("SendAdminReconciliationCompletedAsync", reconciliationServiceSource, StringComparison.Ordinal);
    }

    [Fact]
    public void StripeCheckoutAndFulfillmentServices_ShouldUseOwnedMetadataAndFailClosed()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string checkoutServiceSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Services",
            "StripeBillingCheckoutService.cs"));
        string fulfillmentServiceSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Services",
            "StripeCheckoutFulfillmentService.cs"));

        Assert.Contains("[\"mode\"] = \"subscription\"", checkoutServiceSource, StringComparison.Ordinal);
        Assert.Contains("[\"client_reference_id\"] = userId", checkoutServiceSource, StringComparison.Ordinal);
        Assert.Contains("[\"metadata[darwin_user_id]\"] = userId", checkoutServiceSource, StringComparison.Ordinal);
        Assert.Contains("[\"subscription_data[metadata][darwin_user_id]\"] = userId", checkoutServiceSource, StringComparison.Ordinal);
        Assert.Contains("NormalizeProviderRedirectUrl(url, \"checkout\")", checkoutServiceSource, StringComparison.Ordinal);
        Assert.Contains("NormalizeProviderRedirectUrl(url, \"customer portal\")", checkoutServiceSource, StringComparison.Ordinal);

        Assert.Contains("NormalizeStripeIdentifier(sessionId, \"cs_\", 256)", fulfillmentServiceSource, StringComparison.Ordinal);
        Assert.Contains("GetMetadataValue(session, \"darwin_user_id\") ?? GetString(session, \"client_reference_id\")", fulfillmentServiceSource, StringComparison.Ordinal);
        Assert.Contains("Stripe checkout session does not belong to the authenticated user.", fulfillmentServiceSource, StringComparison.Ordinal);
        Assert.Contains("string.Equals(mode, \"subscription\"", fulfillmentServiceSource, StringComparison.Ordinal);
        Assert.Contains("string.Equals(status, \"complete\"", fulfillmentServiceSource, StringComparison.Ordinal);
        Assert.Contains("IsPaidCheckoutStatus(paymentStatus)", fulfillmentServiceSource, StringComparison.Ordinal);
        Assert.Contains("!string.IsNullOrWhiteSpace(customerId)", fulfillmentServiceSource, StringComparison.Ordinal);
        Assert.Contains("!string.IsNullOrWhiteSpace(subscriptionId)", fulfillmentServiceSource, StringComparison.Ordinal);
        Assert.Contains("userEntitlementService.SetTierAsync", fulfillmentServiceSource, StringComparison.Ordinal);
        Assert.Contains("DarwinLinguaEntitlementTiers.Premium", fulfillmentServiceSource, StringComparison.Ordinal);
    }

    [Fact]
    public void BillingNotificationEmails_ShouldRenderLogAndDeduplicateAllBillingScenarios()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string scenariosSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Services",
            "TransactionalEmailModels.cs"));
        string templatesSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Services",
            "TransactionalEmailTemplates.cs"));
        string notificationServiceSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Services",
            "BillingNotificationEmailService.cs"));
        string webhookHandlerSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Services",
            "StripeBillingWebhookHandler.cs"));
        string reconciliationServiceSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Services",
            "StripeBillingReconciliationService.cs"));
        string dbContextSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Data",
            "WebIdentityDbContext.cs"));

        string[] scenarioConstants =
        [
            "BillingPremiumActivated",
            "BillingPaymentActionNeeded",
            "BillingPremiumEnded",
            "AdminBillingReconciliationCompleted"
        ];

        foreach (string scenario in scenarioConstants)
        {
            Assert.Contains(scenario, scenariosSource, StringComparison.Ordinal);
            Assert.Contains($"TransactionalEmailScenarios.{scenario}", templatesSource, StringComparison.Ordinal);
            Assert.Contains($"TransactionalEmailScenarios.{scenario}", notificationServiceSource, StringComparison.Ordinal);
        }

        Assert.Contains("IEmailTemplateRenderer templateRenderer", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("ITransactionalEmailSender sender", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("IEmailDeliveryLogRepository deliveryLogRepository", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("templateRenderer.Render(scenarioKey, culture, values)", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("IsSuppressedAsync", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("AddSuppressedAsync", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("AddQueuedAsync", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("sender.SendAsync(message", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("MarkSentAsync", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("MarkFailedAsync", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("TryRegisterNotificationAsync", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("WebBillingNotifications", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("IX_WebBillingNotifications_NotificationKey", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("BuildNotificationKey", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("userId.Trim().ToUpperInvariant()", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("subscriptionId.Trim().ToUpperInvariant()", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("billingStatus.Trim().ToUpperInvariant()", notificationServiceSource, StringComparison.Ordinal);
        Assert.Contains("ProviderSubscriptionId", dbContextSource, StringComparison.Ordinal);
        Assert.Contains("HasIndex(notification => notification.NotificationKey)", dbContextSource, StringComparison.Ordinal);
        Assert.Contains(".IsUnique()", dbContextSource, StringComparison.Ordinal);

        Assert.Contains("SendPremiumActivatedAsync", webhookHandlerSource, StringComparison.Ordinal);
        Assert.Contains("SendPaymentActionNeededAsync", webhookHandlerSource, StringComparison.Ordinal);
        Assert.Contains("SendPremiumEndedAsync", webhookHandlerSource, StringComparison.Ordinal);
        Assert.Contains("SendPremiumActivatedAsync", reconciliationServiceSource, StringComparison.Ordinal);
        Assert.Contains("SendPremiumEndedAsync", reconciliationServiceSource, StringComparison.Ordinal);
        Assert.Contains("SendAdminReconciliationCompletedAsync", reconciliationServiceSource, StringComparison.Ordinal);
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
