using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public sealed class TransactionalEmailOptions
{
    public const string SectionName = "TransactionalEmail";

    public string Mode { get; set; } = "File";

    public string PublicBaseUrl { get; set; } = string.Empty;

    public string ProductName { get; set; } = "Darwin Lingua";

    public string FromEmail { get; set; } = "no-reply@darwinlingua.com";

    public string FromName { get; set; } = "Darwin Lingua";

    public string ReplyToEmail { get; set; } = "support@darwinlingua.com";

    public string SupportEmail { get; set; } = "support@darwinlingua.com";

    public string[] AdminNotificationEmails { get; set; } = [];

    public string FileSinkDirectory { get; set; } = "App_Data/EmailSink";

    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;

    public bool SmtpUseSsl { get; set; } = true;

    public string SmtpUserName { get; set; } = string.Empty;

    public string SmtpPassword { get; set; } = string.Empty;

    public string BrevoApiBaseUrl { get; set; } = "https://api.brevo.com";

    public string BrevoApiKey { get; set; } = string.Empty;

    public string BrevoWebhookSecret { get; set; } = string.Empty;

    public bool BrevoSandboxMode { get; set; }

    public bool BrevoAllowQuerySecretFallback { get; set; }

    public int EmailConfirmationTokenHours { get; set; } = 24;

    public int PasswordResetTokenMinutes { get; set; } = 60;

    public int EmailChangeTokenMinutes { get; set; } = 60;

    public int DeliveryLogRetentionDays { get; set; } = 90;

    public int MaxSendAttempts { get; set; } = 3;

    public int SendRetryDelayMilliseconds { get; set; } = 500;

    public bool EnableFailureAlerts { get; set; } = true;

    public int FailureAlertThreshold { get; set; } = 3;

    public int FailureAlertWindowMinutes { get; set; } = 15;

    public int FailureAlertCooldownMinutes { get; set; } = 60;

    public int FailureAlertMonitorIntervalMinutes { get; set; } = 5;
}

public sealed class TransactionalEmailOptionsValidator(IHostEnvironment hostEnvironment)
    : IValidateOptions<TransactionalEmailOptions>
{
    public ValidateOptionsResult Validate(string? name, TransactionalEmailOptions options)
    {
        List<string> failures = [];
        string mode = options.Mode.Trim();

        if (!IsMode(mode, "File") && !IsMode(mode, "Smtp") && !IsMode(mode, "BrevoApi") && !IsMode(mode, "Disabled"))
        {
            failures.Add("TransactionalEmail:Mode must be File, Smtp, BrevoApi, or Disabled.");
        }

        if (string.IsNullOrWhiteSpace(options.FromEmail))
        {
            failures.Add("TransactionalEmail:FromEmail is required.");
        }

        if (string.IsNullOrWhiteSpace(options.SupportEmail))
        {
            failures.Add("TransactionalEmail:SupportEmail is required.");
        }

        if (options.EmailConfirmationTokenHours <= 0)
        {
            failures.Add("TransactionalEmail:EmailConfirmationTokenHours must be greater than zero.");
        }

        if (options.PasswordResetTokenMinutes <= 0)
        {
            failures.Add("TransactionalEmail:PasswordResetTokenMinutes must be greater than zero.");
        }

        if (options.EmailChangeTokenMinutes <= 0)
        {
            failures.Add("TransactionalEmail:EmailChangeTokenMinutes must be greater than zero.");
        }

        if (options.DeliveryLogRetentionDays <= 0)
        {
            failures.Add("TransactionalEmail:DeliveryLogRetentionDays must be greater than zero.");
        }

        if (options.MaxSendAttempts <= 0)
        {
            failures.Add("TransactionalEmail:MaxSendAttempts must be greater than zero.");
        }

        if (options.SendRetryDelayMilliseconds < 0)
        {
            failures.Add("TransactionalEmail:SendRetryDelayMilliseconds cannot be negative.");
        }

        Uri? brevoApiBaseUri = null;
        if (!string.IsNullOrWhiteSpace(options.BrevoApiBaseUrl) &&
            !Uri.TryCreate(options.BrevoApiBaseUrl, UriKind.Absolute, out brevoApiBaseUri))
        {
            failures.Add("TransactionalEmail:BrevoApiBaseUrl must be an absolute URL.");
        }

        if (IsMode(mode, "BrevoApi") &&
            brevoApiBaseUri is not null &&
            !string.Equals(brevoApiBaseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("TransactionalEmail:BrevoApiBaseUrl must use HTTPS when Mode is BrevoApi.");
        }

        Uri? publicBaseUri = null;
        if (!string.IsNullOrWhiteSpace(options.PublicBaseUrl) &&
            !Uri.TryCreate(options.PublicBaseUrl, UriKind.Absolute, out publicBaseUri))
        {
            failures.Add("TransactionalEmail:PublicBaseUrl must be an absolute URL when set.");
        }

        if (IsMode(mode, "BrevoApi") && string.IsNullOrWhiteSpace(options.BrevoApiKey))
        {
            failures.Add("TransactionalEmail:BrevoApiKey is required when Mode is BrevoApi.");
        }

        if (IsMode(mode, "BrevoApi") && string.IsNullOrWhiteSpace(options.BrevoWebhookSecret))
        {
            failures.Add("TransactionalEmail:BrevoWebhookSecret is required when Mode is BrevoApi.");
        }

        if (hostEnvironment.IsProduction() && options.BrevoAllowQuerySecretFallback)
        {
            failures.Add("TransactionalEmail:BrevoAllowQuerySecretFallback must be false in Production.");
        }

        if (options.FailureAlertThreshold <= 0)
        {
            failures.Add("TransactionalEmail:FailureAlertThreshold must be greater than zero.");
        }

        if (options.FailureAlertWindowMinutes <= 0)
        {
            failures.Add("TransactionalEmail:FailureAlertWindowMinutes must be greater than zero.");
        }

        if (options.FailureAlertCooldownMinutes <= 0)
        {
            failures.Add("TransactionalEmail:FailureAlertCooldownMinutes must be greater than zero.");
        }

        if (options.FailureAlertMonitorIntervalMinutes <= 0)
        {
            failures.Add("TransactionalEmail:FailureAlertMonitorIntervalMinutes must be greater than zero.");
        }

        if (hostEnvironment.IsProduction())
        {
            if (!IsMode(mode, "Smtp") && !IsMode(mode, "BrevoApi"))
            {
                failures.Add("TransactionalEmail:Mode must be Smtp or BrevoApi in Production.");
            }

            if (string.IsNullOrWhiteSpace(options.PublicBaseUrl))
            {
                failures.Add("TransactionalEmail:PublicBaseUrl is required in Production.");
            }
            else if (publicBaseUri is not null &&
                !string.Equals(publicBaseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                failures.Add("TransactionalEmail:PublicBaseUrl must use HTTPS in Production.");
            }

            if (string.IsNullOrWhiteSpace(options.SmtpHost))
            {
                if (IsMode(mode, "Smtp"))
                {
                    failures.Add("TransactionalEmail:SmtpHost is required in Production when Mode is Smtp.");
                }
            }

        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static bool IsMode(string actual, string expected) =>
        string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
}
