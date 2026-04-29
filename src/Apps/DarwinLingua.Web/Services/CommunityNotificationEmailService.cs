using Microsoft.Extensions.Options;
using DarwinLingua.Web.Data;

namespace DarwinLingua.Web.Services;

public interface ICommunityNotificationEmailService
{
    Task SendOrganizerClaimSubmittedAsync(
        string requesterEmail,
        string organizerName,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);

    Task SendAdminNewOrganizerClaimAsync(
        string organizerName,
        string requesterName,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);

    Task SendOrganizerClaimDecisionAsync(
        string requesterEmail,
        string organizerName,
        string status,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);

    Task SendOrganizerProfileOwnershipChangedAsync(
        string ownerEmail,
        string organizerProfileSlug,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);

    Task SendEventRsvpConfirmationAsync(
        string participantEmail,
        string eventTitle,
        string rsvpStatus,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);

    Task SendPartnerRequestAcceptedAsync(
        string requesterEmail,
        string displayName,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);

    Task SendAdminHighSeverityReportAsync(
        string reason,
        string targetType,
        string targetKey,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);

    Task SendAdminEmailDeliveryFailureAlertAsync(
        int failureCount,
        int windowMinutes,
        string? lastFailureScenarioKey,
        string? lastFailureCode,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);

    Task SendModerationReportOutcomeAsync(
        string reporterEmail,
        string targetType,
        string status,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);
}

public sealed class CommunityNotificationEmailService(
    IEmailTemplateRenderer templateRenderer,
    ITransactionalEmailSender sender,
    IEmailDeliveryLogRepository deliveryLogRepository,
    IOptions<TransactionalEmailOptions> options) : ICommunityNotificationEmailService
{
    public Task SendOrganizerClaimSubmittedAsync(
        string requesterEmail,
        string organizerName,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken) =>
        SendToRecipientAsync(
            requesterEmail,
            null,
            TransactionalEmailScenarios.OrganizerClaimSubmitted,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["OrganizerName"] = organizerName,
            },
            correlationId,
            cancellationToken);

    public Task SendAdminNewOrganizerClaimAsync(
        string organizerName,
        string requesterName,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken) =>
        SendToAdminsAsync(
            TransactionalEmailScenarios.AdminNewOrganizerClaim,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["OrganizerName"] = organizerName,
                ["RequesterName"] = requesterName,
            },
            correlationId,
            cancellationToken);

    public Task SendOrganizerClaimDecisionAsync(
        string requesterEmail,
        string organizerName,
        string status,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        string scenarioKey = string.Equals(status, "approved", StringComparison.OrdinalIgnoreCase)
            ? TransactionalEmailScenarios.OrganizerClaimApproved
            : TransactionalEmailScenarios.OrganizerClaimRejected;

        return SendToRecipientAsync(
            requesterEmail,
            null,
            scenarioKey,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["OrganizerName"] = organizerName,
            },
            correlationId,
            cancellationToken);
    }

    public Task SendOrganizerProfileOwnershipChangedAsync(
        string ownerEmail,
        string organizerProfileSlug,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken) =>
        SendToRecipientAsync(
            ownerEmail,
            null,
            TransactionalEmailScenarios.OrganizerProfileOwnershipChanged,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["OrganizerProfileSlug"] = organizerProfileSlug,
            },
            correlationId,
            cancellationToken);

    public Task SendEventRsvpConfirmationAsync(
        string participantEmail,
        string eventTitle,
        string rsvpStatus,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken) =>
        SendToRecipientAsync(
            participantEmail,
            null,
            TransactionalEmailScenarios.EventRsvpConfirmation,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["EventTitle"] = eventTitle,
                ["RsvpStatus"] = rsvpStatus,
            },
            correlationId,
            cancellationToken);

    public Task SendPartnerRequestAcceptedAsync(
        string requesterEmail,
        string displayName,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken) =>
        SendToRecipientAsync(
            requesterEmail,
            null,
            TransactionalEmailScenarios.PartnerRequestAccepted,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["DisplayName"] = displayName,
            },
            correlationId,
            cancellationToken);

    public Task SendAdminHighSeverityReportAsync(
        string reason,
        string targetType,
        string targetKey,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken) =>
        SendToAdminsAsync(
            TransactionalEmailScenarios.ModerationHighSeverityReport,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Reason"] = reason,
                ["TargetType"] = targetType,
                ["TargetKey"] = targetKey,
            },
            correlationId,
            cancellationToken);

    public Task SendAdminEmailDeliveryFailureAlertAsync(
        int failureCount,
        int windowMinutes,
        string? lastFailureScenarioKey,
        string? lastFailureCode,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken) =>
        SendToAdminsAsync(
            TransactionalEmailScenarios.AdminEmailDeliveryFailureAlert,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["FailureCount"] = failureCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["WindowMinutes"] = windowMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["LastFailureScenarioKey"] = string.IsNullOrWhiteSpace(lastFailureScenarioKey) ? "unknown" : lastFailureScenarioKey,
                ["LastFailureCode"] = string.IsNullOrWhiteSpace(lastFailureCode) ? "unknown" : lastFailureCode,
            },
            correlationId,
            cancellationToken);

    public Task SendModerationReportOutcomeAsync(
        string reporterEmail,
        string targetType,
        string status,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken) =>
        SendToRecipientAsync(
            reporterEmail,
            null,
            TransactionalEmailScenarios.ModerationReportOutcome,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["TargetType"] = targetType,
                ["Status"] = status,
            },
            correlationId,
            cancellationToken);

    private async Task SendToAdminsAsync(
        string scenarioKey,
        string? culture,
        IReadOnlyDictionary<string, string> values,
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
        string? correlationId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
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

        TransactionalEmailSendResult result = await SendWithRetryAsync(message, cancellationToken)
            .ConfigureAwait(false);

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

    private async Task<TransactionalEmailSendResult> SendWithRetryAsync(
        TransactionalEmailMessage message,
        CancellationToken cancellationToken)
    {
        int maxAttempts = Math.Max(1, options.Value.MaxSendAttempts);
        TimeSpan retryDelay = TimeSpan.FromMilliseconds(Math.Max(0, options.Value.SendRetryDelayMilliseconds));
        TransactionalEmailSendResult? lastResult = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            lastResult = await sender.SendAsync(message, cancellationToken).ConfigureAwait(false);
            if (lastResult.Succeeded || IsNonRetryable(lastResult.FailureCode) || attempt == maxAttempts)
            {
                return lastResult;
            }

            if (retryDelay > TimeSpan.Zero)
            {
                await Task.Delay(retryDelay, cancellationToken).ConfigureAwait(false);
            }
        }

        return lastResult ?? new TransactionalEmailSendResult(
            false,
            sender.ProviderName,
            null,
            "send-not-attempted",
            "No email send attempt was made.");
    }

    private static bool IsNonRetryable(string? failureCode) =>
        string.Equals(failureCode, "email-disabled", StringComparison.OrdinalIgnoreCase);
}
