using DarwinLingua.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public interface IBillingNotificationEmailService
{
    Task SendPremiumActivatedAsync(
        string recipientEmail,
        string userId,
        string billingStatus,
        string? subscriptionId,
        DateTimeOffset? currentPeriodEndsAtUtc,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);

    Task SendPaymentActionNeededAsync(
        string recipientEmail,
        string userId,
        string billingStatus,
        string? subscriptionId,
        DateTimeOffset? currentPeriodEndsAtUtc,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);

    Task SendPremiumEndedAsync(
        string recipientEmail,
        string userId,
        string billingStatus,
        string? subscriptionId,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);

    Task SendAdminReconciliationCompletedAsync(
        string subscriptionId,
        string userId,
        string billingStatus,
        string entitlementTier,
        string adminActor,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);
}

public sealed class BillingNotificationEmailService(
    WebIdentityDbContext dbContext,
    IEmailTemplateRenderer templateRenderer,
    ITransactionalEmailSender sender,
    IEmailDeliveryLogRepository deliveryLogRepository,
    IOptions<TransactionalEmailOptions> options) : IBillingNotificationEmailService
{
    public Task SendPremiumActivatedAsync(
        string recipientEmail,
        string userId,
        string billingStatus,
        string? subscriptionId,
        DateTimeOffset? currentPeriodEndsAtUtc,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken) =>
        SendToRecipientAsync(
            recipientEmail,
            userId,
            TransactionalEmailScenarios.BillingPremiumActivated,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["BillingStatus"] = billingStatus,
                ["CurrentPeriodEnd"] = FormatDate(currentPeriodEndsAtUtc),
            },
            BuildNotificationKey(TransactionalEmailScenarios.BillingPremiumActivated, userId, subscriptionId, billingStatus),
            userId,
            subscriptionId,
            billingStatus,
            correlationId,
            cancellationToken);

    public Task SendPaymentActionNeededAsync(
        string recipientEmail,
        string userId,
        string billingStatus,
        string? subscriptionId,
        DateTimeOffset? currentPeriodEndsAtUtc,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken) =>
        SendToRecipientAsync(
            recipientEmail,
            userId,
            TransactionalEmailScenarios.BillingPaymentActionNeeded,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["BillingStatus"] = billingStatus,
                ["CurrentPeriodEnd"] = FormatDate(currentPeriodEndsAtUtc),
            },
            BuildNotificationKey(TransactionalEmailScenarios.BillingPaymentActionNeeded, userId, subscriptionId, billingStatus),
            userId,
            subscriptionId,
            billingStatus,
            correlationId,
            cancellationToken);

    public Task SendPremiumEndedAsync(
        string recipientEmail,
        string userId,
        string billingStatus,
        string? subscriptionId,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken) =>
        SendToRecipientAsync(
            recipientEmail,
            userId,
            TransactionalEmailScenarios.BillingPremiumEnded,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["BillingStatus"] = billingStatus,
            },
            BuildNotificationKey(TransactionalEmailScenarios.BillingPremiumEnded, userId, subscriptionId, billingStatus),
            userId,
            subscriptionId,
            billingStatus,
            correlationId,
            cancellationToken);

    public Task SendAdminReconciliationCompletedAsync(
        string subscriptionId,
        string userId,
        string billingStatus,
        string entitlementTier,
        string adminActor,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken) =>
        SendToAdminsAsync(
            TransactionalEmailScenarios.AdminBillingReconciliationCompleted,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["SubscriptionId"] = subscriptionId,
                ["UserId"] = userId,
                ["BillingStatus"] = billingStatus,
                ["EntitlementTier"] = entitlementTier,
                ["AdminActor"] = adminActor,
            },
            BuildNotificationKey(TransactionalEmailScenarios.AdminBillingReconciliationCompleted, userId, subscriptionId, billingStatus),
            userId,
            subscriptionId,
            billingStatus,
            correlationId,
            cancellationToken);

    private async Task SendToAdminsAsync(
        string scenarioKey,
        string? culture,
        IReadOnlyDictionary<string, string> values,
        string notificationKey,
        string? notificationUserId,
        string? providerSubscriptionId,
        string? billingStatus,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        string[] recipients = options.Value.AdminNotificationEmails
            .Where(static email => !string.IsNullOrWhiteSpace(email))
            .Select(static email => email.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (string recipient in recipients)
        {
            await SendToRecipientAsync(
                    recipient,
                    null,
                    scenarioKey,
                    culture,
                    values,
                    notificationKey,
                    notificationUserId,
                    providerSubscriptionId,
                    billingStatus,
                    correlationId,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task SendToRecipientAsync(
        string recipientEmail,
        string? recipientUserId,
        string scenarioKey,
        string? culture,
        IReadOnlyDictionary<string, string> values,
        string notificationKey,
        string? notificationUserId,
        string? providerSubscriptionId,
        string? billingStatus,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            return;
        }

        if (!await TryRegisterNotificationAsync(
                notificationKey,
                scenarioKey,
                notificationUserId,
                providerSubscriptionId,
                billingStatus,
                cancellationToken)
            .ConfigureAwait(false))
        {
            return;
        }

        RenderedEmailTemplate template = templateRenderer.Render(scenarioKey, culture, values);
        TransactionalEmailMessage message = new(
            scenarioKey,
            template.TemplateKey,
            recipientEmail.Trim(),
            recipientUserId,
            template.Culture,
            template.Subject,
            template.PlainTextBody,
            template.HtmlBody,
            correlationId);

        if (await deliveryLogRepository.IsSuppressedAsync(message.RecipientEmail, cancellationToken).ConfigureAwait(false))
        {
            await deliveryLogRepository
                .AddSuppressedAsync(message, sender.ProviderName, "recipient-suppressed", cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        WebEmailDeliveryLog log = await deliveryLogRepository
            .AddQueuedAsync(message, sender.ProviderName, cancellationToken)
            .ConfigureAwait(false);

        TransactionalEmailSendResult result = await sender.SendAsync(message, cancellationToken).ConfigureAwait(false);
        if (result.Succeeded)
        {
            await deliveryLogRepository
                .MarkSentAsync(log.Id, result.ProviderMessageId, cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        await deliveryLogRepository
            .MarkFailedAsync(log.Id, result.FailureCode, result.FailureMessageSummary, cancellationToken)
            .ConfigureAwait(false);
    }

    private static string FormatDate(DateTimeOffset? value) =>
        value?.UtcDateTime.ToString("u") ?? "not available";

    private async Task<bool> TryRegisterNotificationAsync(
        string notificationKey,
        string scenarioKey,
        string? userId,
        string? providerSubscriptionId,
        string? billingStatus,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(notificationKey))
        {
            return true;
        }

        bool exists = await dbContext.WebBillingNotifications
            .AsNoTracking()
            .AnyAsync(notification => notification.NotificationKey == notificationKey, cancellationToken)
            .ConfigureAwait(false);
        if (exists)
        {
            return false;
        }

        dbContext.WebBillingNotifications.Add(new WebBillingNotification
        {
            Id = Guid.NewGuid(),
            NotificationKey = notificationKey,
            ScenarioKey = scenarioKey,
            UserId = userId,
            ProviderSubscriptionId = providerSubscriptionId,
            BillingStatus = billingStatus,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        });

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (DbUpdateException exception) when (
            exception.Message.Contains("IX_WebBillingNotifications_NotificationKey", StringComparison.OrdinalIgnoreCase) ||
            exception.InnerException?.Message.Contains("IX_WebBillingNotifications_NotificationKey", StringComparison.OrdinalIgnoreCase) == true ||
            exception.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
            exception.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true)
        {
            return false;
        }
    }

    private static string BuildNotificationKey(
        string scenarioKey,
        string userId,
        string? subscriptionId,
        string billingStatus)
    {
        string normalizedSubscriptionId = string.IsNullOrWhiteSpace(subscriptionId)
            ? "no-subscription"
            : subscriptionId.Trim().ToUpperInvariant();
        string normalizedStatus = string.IsNullOrWhiteSpace(billingStatus)
            ? "unknown"
            : billingStatus.Trim().ToUpperInvariant();
        return $"{scenarioKey}:{userId.Trim().ToUpperInvariant()}:{normalizedSubscriptionId}:{normalizedStatus}";
    }
}
