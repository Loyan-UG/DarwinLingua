using DarwinLingua.Identity;
using DarwinLingua.Web.Data;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public interface IAccountEmailService
{
    Task SendEmailConfirmationAsync(
        DarwinLinguaIdentityUser user,
        string callbackUrl,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);

    Task SendPasswordResetAsync(
        DarwinLinguaIdentityUser user,
        string callbackUrl,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);

    Task SendPasswordResetCompletedAsync(
        DarwinLinguaIdentityUser user,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);

    Task SendEmailChangeConfirmationAsync(
        DarwinLinguaIdentityUser user,
        string newEmail,
        string callbackUrl,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);

    Task SendEmailChangedNotificationAsync(
        DarwinLinguaIdentityUser user,
        string oldEmail,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken);
}

public sealed class AccountEmailService(
    IEmailTemplateRenderer templateRenderer,
    ITransactionalEmailSender sender,
    IEmailDeliveryLogRepository deliveryLogRepository,
    IOptions<TransactionalEmailOptions> options) : IAccountEmailService
{
    public Task SendEmailConfirmationAsync(
        DarwinLinguaIdentityUser user,
        string callbackUrl,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        string expiration = $"{options.Value.EmailConfirmationTokenHours} hours";
        return SendAsync(
            user,
            callbackUrl,
            TransactionalEmailScenarios.AccountEmailConfirmation,
            culture,
            expiration,
            correlationId,
            cancellationToken);
    }

    public Task SendPasswordResetAsync(
        DarwinLinguaIdentityUser user,
        string callbackUrl,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        string expiration = $"{options.Value.PasswordResetTokenMinutes} minutes";
        return SendAsync(
            user,
            callbackUrl,
            TransactionalEmailScenarios.AccountPasswordReset,
            culture,
            expiration,
            correlationId,
            cancellationToken);
    }

    public Task SendPasswordResetCompletedAsync(
        DarwinLinguaIdentityUser user,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        RenderedEmailTemplate template = templateRenderer.Render(
            TransactionalEmailScenarios.AccountPasswordResetCompleted,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal));

        return SendRenderedAsync(user, template, correlationId, cancellationToken);
    }

    public Task SendEmailChangeConfirmationAsync(
        DarwinLinguaIdentityUser user,
        string newEmail,
        string callbackUrl,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        string expiration = $"{options.Value.EmailChangeTokenMinutes} minutes";
        RenderedEmailTemplate template = templateRenderer.Render(
            TransactionalEmailScenarios.AccountEmailChangeConfirmation,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ActionUrl"] = callbackUrl,
                ["ExpirationText"] = expiration,
            });

        DarwinLinguaIdentityUser recipient = new()
        {
            Id = user.Id,
            Email = newEmail,
        };

        return SendRenderedAsync(recipient, template, correlationId, cancellationToken);
    }

    public Task SendEmailChangedNotificationAsync(
        DarwinLinguaIdentityUser user,
        string oldEmail,
        string? culture,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        RenderedEmailTemplate template = templateRenderer.Render(
            TransactionalEmailScenarios.AccountEmailChangedNotification,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal));

        DarwinLinguaIdentityUser recipient = new()
        {
            Id = user.Id,
            Email = oldEmail,
        };

        return SendRenderedAsync(recipient, template, correlationId, cancellationToken);
    }

    private Task SendAsync(
        DarwinLinguaIdentityUser user,
        string callbackUrl,
        string scenarioKey,
        string? culture,
        string expirationText,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        RenderedEmailTemplate template = templateRenderer.Render(
            scenarioKey,
            culture,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ActionUrl"] = callbackUrl,
                ["ExpirationText"] = expirationText,
            });

        return SendRenderedAsync(user, template, correlationId, cancellationToken);
    }

    private async Task SendRenderedAsync(
        DarwinLinguaIdentityUser user,
        RenderedEmailTemplate template,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        TransactionalEmailMessage message = new(
            template.TemplateKey,
            template.TemplateKey,
            user.Email,
            user.Id,
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
