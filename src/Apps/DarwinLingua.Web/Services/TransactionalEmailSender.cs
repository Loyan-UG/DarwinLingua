using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public interface ITransactionalEmailSender
{
    string ProviderName { get; }

    Task<TransactionalEmailSendResult> SendAsync(
        TransactionalEmailMessage message,
        CancellationToken cancellationToken);
}

public sealed class TransactionalEmailSender(
    IOptions<TransactionalEmailOptions> options,
    IWebHostEnvironment hostEnvironment,
    ILogger<TransactionalEmailSender> logger) : ITransactionalEmailSender
{
    public string ProviderName => ResolveProviderName(options.Value.Mode);

    public async Task<TransactionalEmailSendResult> SendAsync(
        TransactionalEmailMessage message,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        string mode = options.Value.Mode.Trim();
        if (string.Equals(mode, "Disabled", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(
                "Transactional email scenario {ScenarioKey} to user {UserId} was suppressed because email is disabled.",
                message.ScenarioKey,
                message.RecipientUserId);
            return new TransactionalEmailSendResult(false, ProviderName, null, "email-disabled", "Transactional email is disabled.");
        }

        try
        {
            if (string.Equals(mode, "Smtp", StringComparison.OrdinalIgnoreCase))
            {
                await SendSmtpAsync(message, cancellationToken).ConfigureAwait(false);
                return new TransactionalEmailSendResult(true, ProviderName, null, null, null);
            }

            string providerMessageId = await WriteFileAsync(message, cancellationToken).ConfigureAwait(false);
            return new TransactionalEmailSendResult(true, ProviderName, providerMessageId, null, null);
        }
        catch (Exception ex) when (ex is SmtpException or IOException or InvalidOperationException)
        {
            logger.LogWarning(
                ex,
                "Transactional email scenario {ScenarioKey} failed for user {UserId}.",
                message.ScenarioKey,
                message.RecipientUserId);
            return new TransactionalEmailSendResult(false, ProviderName, null, ex.GetType().Name, ex.Message);
        }
    }

    private async Task SendSmtpAsync(
        TransactionalEmailMessage message,
        CancellationToken cancellationToken)
    {
        TransactionalEmailOptions emailOptions = options.Value;
        using MailMessage mailMessage = new()
        {
            From = new MailAddress(emailOptions.FromEmail, emailOptions.FromName),
            Subject = message.Subject,
            Body = message.HtmlBody,
            IsBodyHtml = true,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8,
        };
        mailMessage.To.Add(message.RecipientEmail);
        if (!string.IsNullOrWhiteSpace(emailOptions.ReplyToEmail))
        {
            mailMessage.ReplyToList.Add(emailOptions.ReplyToEmail);
        }

        AlternateView plainTextView = AlternateView.CreateAlternateViewFromString(
            message.PlainTextBody,
            Encoding.UTF8,
            "text/plain");
        mailMessage.AlternateViews.Add(plainTextView);

        using SmtpClient smtpClient = new(emailOptions.SmtpHost, emailOptions.SmtpPort)
        {
            EnableSsl = emailOptions.SmtpUseSsl,
        };
        if (!string.IsNullOrWhiteSpace(emailOptions.SmtpUserName))
        {
            smtpClient.Credentials = new NetworkCredential(
                emailOptions.SmtpUserName,
                emailOptions.SmtpPassword);
        }

        using CancellationTokenRegistration registration = cancellationToken.Register(smtpClient.SendAsyncCancel);
        await smtpClient.SendMailAsync(mailMessage, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> WriteFileAsync(
        TransactionalEmailMessage message,
        CancellationToken cancellationToken)
    {
        string configuredDirectory = options.Value.FileSinkDirectory;
        string sinkDirectory = Path.IsPathRooted(configuredDirectory)
            ? configuredDirectory
            : Path.Combine(hostEnvironment.ContentRootPath, configuredDirectory);
        Directory.CreateDirectory(sinkDirectory);

        string providerMessageId = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}";
        string path = Path.Combine(sinkDirectory, providerMessageId + ".eml");
        string body =
            $"Scenario: {message.ScenarioKey}{Environment.NewLine}" +
            $"Template: {message.TemplateKey}{Environment.NewLine}" +
            $"Culture: {message.Culture}{Environment.NewLine}" +
            $"To: {message.RecipientEmail}{Environment.NewLine}" +
            $"Subject: {message.Subject}{Environment.NewLine}" +
            $"CorrelationId: {message.CorrelationId}{Environment.NewLine}" +
            Environment.NewLine +
            message.PlainTextBody +
            Environment.NewLine +
            Environment.NewLine +
            "--- HTML ---" +
            Environment.NewLine +
            message.HtmlBody;

        await File.WriteAllTextAsync(path, body, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        return providerMessageId;
    }

    private static string ResolveProviderName(string mode) =>
        string.Equals(mode, "Smtp", StringComparison.OrdinalIgnoreCase)
            ? "smtp"
            : string.Equals(mode, "Disabled", StringComparison.OrdinalIgnoreCase)
                ? "disabled"
                : "file";
}
