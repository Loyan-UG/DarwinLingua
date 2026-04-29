using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public sealed class TransactionalEmailOptions
{
    public const string SectionName = "TransactionalEmail";

    public string Mode { get; set; } = "File";

    public string PublicBaseUrl { get; set; } = string.Empty;

    public string ProductName { get; set; } = "Darwin Lingua";

    public string FromEmail { get; set; } = "no-reply@example.local";

    public string FromName { get; set; } = "Darwin Lingua";

    public string ReplyToEmail { get; set; } = "support@example.local";

    public string SupportEmail { get; set; } = "support@example.local";

    public string[] AdminNotificationEmails { get; set; } = [];

    public string FileSinkDirectory { get; set; } = "App_Data/EmailSink";

    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;

    public bool SmtpUseSsl { get; set; } = true;

    public string SmtpUserName { get; set; } = string.Empty;

    public string SmtpPassword { get; set; } = string.Empty;

    public int EmailConfirmationTokenHours { get; set; } = 24;

    public int PasswordResetTokenMinutes { get; set; } = 60;

    public int EmailChangeTokenMinutes { get; set; } = 60;

    public int DeliveryLogRetentionDays { get; set; } = 90;
}

public sealed class TransactionalEmailOptionsValidator(IHostEnvironment hostEnvironment)
    : IValidateOptions<TransactionalEmailOptions>
{
    public ValidateOptionsResult Validate(string? name, TransactionalEmailOptions options)
    {
        List<string> failures = [];
        string mode = options.Mode.Trim();

        if (!IsMode(mode, "File") && !IsMode(mode, "Smtp") && !IsMode(mode, "Disabled"))
        {
            failures.Add("TransactionalEmail:Mode must be File, Smtp, or Disabled.");
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

        if (hostEnvironment.IsProduction())
        {
            if (!IsMode(mode, "Smtp"))
            {
                failures.Add("TransactionalEmail:Mode must be Smtp in Production.");
            }

            if (string.IsNullOrWhiteSpace(options.PublicBaseUrl))
            {
                failures.Add("TransactionalEmail:PublicBaseUrl is required in Production.");
            }

            if (string.IsNullOrWhiteSpace(options.SmtpHost))
            {
                failures.Add("TransactionalEmail:SmtpHost is required in Production.");
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static bool IsMode(string actual, string expected) =>
        string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
}
