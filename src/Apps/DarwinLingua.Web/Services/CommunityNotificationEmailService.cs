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

        WebEmailDeliveryLog log = await deliveryLogRepository
            .AddQueuedAsync(message, sender.ProviderName, cancellationToken)
            .ConfigureAwait(false);

        TransactionalEmailSendResult result = await sender
            .SendAsync(message, cancellationToken)
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
}
