using System.Net;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    IHttpClientFactory httpClientFactory,
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

            if (string.Equals(mode, "BrevoApi", StringComparison.OrdinalIgnoreCase))
            {
                string brevoProviderMessageId = await SendBrevoApiAsync(message, cancellationToken).ConfigureAwait(false);
                return new TransactionalEmailSendResult(true, ProviderName, brevoProviderMessageId, null, null);
            }

            string providerMessageId = await WriteFileAsync(message, cancellationToken).ConfigureAwait(false);
            return new TransactionalEmailSendResult(true, ProviderName, providerMessageId, null, null);
        }
        catch (Exception ex) when (ex is SmtpException or IOException or InvalidOperationException or HttpRequestException or JsonException or TaskCanceledException)
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

    private async Task<string> SendBrevoApiAsync(
        TransactionalEmailMessage message,
        CancellationToken cancellationToken)
    {
        TransactionalEmailOptions emailOptions = options.Value;
        if (string.IsNullOrWhiteSpace(emailOptions.BrevoApiKey))
        {
            throw new InvalidOperationException("Brevo API key is not configured.");
        }

        Uri baseUri = Uri.TryCreate(emailOptions.BrevoApiBaseUrl, UriKind.Absolute, out Uri? configuredBaseUri)
            ? configuredBaseUri
            : new Uri("https://api.brevo.com");

        using HttpClient httpClient = httpClientFactory.CreateClient("BrevoTransactionalEmail");
        httpClient.BaseAddress = baseUri;

        using HttpRequestMessage request = new(HttpMethod.Post, "/v3/smtp/email");
        request.Headers.TryAddWithoutValidation("api-key", emailOptions.BrevoApiKey);
        request.Headers.TryAddWithoutValidation("accept", "application/json");

        Dictionary<string, string> headers = new(StringComparer.Ordinal)
        {
            ["X-DarwinLingua-Scenario"] = message.ScenarioKey,
            ["X-DarwinLingua-Template"] = message.TemplateKey,
            ["X-DarwinLingua-CorrelationId"] = message.CorrelationId ?? string.Empty,
        };
        if (emailOptions.BrevoSandboxMode)
        {
            headers["X-Sib-Sandbox"] = "drop";
        }

        BrevoSendEmailRequest payload = new(
            new BrevoEmailAddress(emailOptions.FromEmail, emailOptions.FromName),
            [new BrevoEmailAddress(message.RecipientEmail, null)],
            string.IsNullOrWhiteSpace(emailOptions.ReplyToEmail)
                ? null
                : new BrevoEmailAddress(emailOptions.ReplyToEmail, null),
            message.Subject,
            message.HtmlBody,
            SanitizeBrevoTags(message.ScenarioKey),
            headers);

        request.Content = JsonContent.Create(payload, options: BrevoJsonSerializerOptions);
        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Brevo API returned {(int)response.StatusCode}: {Truncate(responseBody, 512)}");
        }

        BrevoSendEmailResponse? sendResponse = JsonSerializer.Deserialize<BrevoSendEmailResponse>(
            responseBody,
            BrevoJsonSerializerOptions);
        return string.IsNullOrWhiteSpace(sendResponse?.MessageId)
            ? $"brevo-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"
            : sendResponse.MessageId;
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
            : string.Equals(mode, "BrevoApi", StringComparison.OrdinalIgnoreCase)
                ? "brevo-api"
                : string.Equals(mode, "Disabled", StringComparison.OrdinalIgnoreCase)
                ? "disabled"
                : "file";

    private static string[] SanitizeBrevoTags(string scenarioKey) =>
        [
            "darwinlingua",
            new string(scenarioKey
                .Select(static character => char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '-')
                .ToArray()).Trim('-')
        ];

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    private static readonly JsonSerializerOptions BrevoJsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private sealed record BrevoSendEmailRequest(
        BrevoEmailAddress Sender,
        IReadOnlyList<BrevoEmailAddress> To,
        BrevoEmailAddress? ReplyTo,
        string Subject,
        string HtmlContent,
        IReadOnlyList<string> Tags,
        IReadOnlyDictionary<string, string> Headers);

    private sealed record BrevoEmailAddress(string Email, string? Name);

    private sealed record BrevoSendEmailResponse(string? MessageId);
}
