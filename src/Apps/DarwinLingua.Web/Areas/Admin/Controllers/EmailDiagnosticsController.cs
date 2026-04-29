using System.Text;
using DarwinLingua.Identity;
using DarwinLingua.Web.Data;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/email-diagnostics")]
public sealed class EmailDiagnosticsController(
    IEmailDeliveryLogRepository deliveryLogRepository,
    IOptions<TransactionalEmailOptions> emailOptions,
    UserManager<DarwinLinguaIdentityUser> userManager,
    IAccountEmailService accountEmailService)
    : Controller
{
    [HttpGet("", Name = "Admin_EmailDiagnostics")]
    public async Task<IActionResult> Index(
        [FromQuery] string? status,
        [FromQuery] string? scenario,
        [FromQuery] string? fromUtc,
        [FromQuery] string? toUtc,
        [FromQuery] string? recipientHashPrefix,
        [FromQuery] string? providerMessageId,
        [FromQuery] string? providerEvent,
        [FromQuery] string? suppressionHashPrefix,
        [FromQuery] string? suppressionReason,
        CancellationToken cancellationToken,
        [FromQuery] int take = 100)
    {
        DateTimeOffset? parsedFromUtc = ParseDateTimeOffset(fromUtc);
        DateTimeOffset? parsedToUtc = ParseDateTimeOffset(toUtc);
        int boundedTake = Math.Clamp(take, 1, 200);

        IReadOnlyList<WebEmailDeliveryLog> logs = await deliveryLogRepository
            .GetRecentAsync(
                boundedTake,
                status,
                scenario,
                parsedFromUtc,
                parsedToUtc,
                recipientHashPrefix,
                providerMessageId,
                providerEvent,
                cancellationToken)
            .ConfigureAwait(false);
        EmailSuppressionSummary suppressionSummary = await deliveryLogRepository
            .GetSuppressionSummaryAsync(cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<WebEmailSuppression> suppressions = await deliveryLogRepository
            .GetSuppressionsAsync(
                suppressionHashPrefix,
                suppressionReason,
                boundedTake,
                cancellationToken)
            .ConfigureAwait(false);

        AdminEmailDiagnosticsPageViewModel model = new(
            status,
            scenario,
            fromUtc,
            toUtc,
            recipientHashPrefix,
            providerMessageId,
            providerEvent,
            suppressionHashPrefix,
            suppressionReason,
            boundedTake,
            emailOptions.Value.DeliveryLogRetentionDays,
            new EmailSuppressionSummaryViewModel(
                suppressionSummary.TotalCount,
                suppressionSummary.LastCreatedAtUtc,
                suppressionSummary.LastReason),
            TempData["StatusMessage"] as string,
            TempData["ErrorMessage"] as string,
            BuildReadiness(),
            logs.Select(static log => new AdminEmailDeliveryLogItemViewModel(
                    log.Id,
                    log.ScenarioKey,
                    log.RecipientEmailHash,
                    log.RecipientUserId,
                    log.TemplateKey,
                    log.Culture,
                    log.Subject,
                    log.ProviderName,
                    log.ProviderMessageId,
                    log.ProviderLastEvent,
                    log.ProviderLastEventAtUtc,
                    log.ProviderLastEventReason,
                    log.Status,
                    log.FailureCode,
                    log.FailureMessageSummary,
                    log.RetryCount,
                    log.CreatedAtUtc,
                    log.SentAtUtc,
                    log.LastAttemptAtUtc,
                    log.CorrelationId))
                .ToArray(),
            suppressions.Select(static suppression => new AdminEmailSuppressionItemViewModel(
                    suppression.RecipientEmailHash,
                    suppression.Reason,
                    suppression.ProviderName,
                    suppression.ProviderMessageId,
                    suppression.CreatedAtUtc,
                    suppression.LastSeenAtUtc))
                .ToArray());

        return View(model);
    }

    [HttpPost("suppressions/remove", Name = "Admin_EmailDiagnostics_RemoveSuppression")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveSuppression(
        [FromForm] string? recipientEmailHash,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(recipientEmailHash))
        {
            TempData["ErrorMessage"] = "Suppression hash is required.";
            return RedirectToAction(nameof(Index));
        }

        bool removed = await deliveryLogRepository
            .DeleteSuppressionByHashAsync(recipientEmailHash, cancellationToken)
            .ConfigureAwait(false);
        TempData[removed ? "StatusMessage" : "ErrorMessage"] = removed
            ? "Suppression removed. Future sends to that hashed recipient are allowed again."
            : "No suppression was found for that hash.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("provider-events", Name = "Admin_EmailDiagnostics_RecordProviderEvent")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> RecordProviderEvent(
        [FromForm] string? providerMessageId,
        [FromForm] string? providerEvent,
        [FromForm] string? reason,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(providerMessageId) || string.IsNullOrWhiteSpace(providerEvent))
        {
            TempData["ErrorMessage"] = "Provider message id and provider event are required.";
            return RedirectToAction(nameof(Index));
        }

        bool updated = await deliveryLogRepository
            .MarkProviderEventAsync(
                providerMessageId.Trim(),
                providerEvent.Trim(),
                DateTimeOffset.UtcNow,
                reason,
                cancellationToken)
            .ConfigureAwait(false);

        TempData[updated ? "StatusMessage" : "ErrorMessage"] = updated
            ? "Provider event recorded for the matching delivery log."
            : "No delivery log matched that provider message id.";
        return RedirectToAction(nameof(Index), new { providerMessageId });
    }

    [HttpPost("cleanup", Name = "Admin_EmailDiagnostics_Cleanup")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cleanup(CancellationToken cancellationToken)
    {
        int retentionDays = emailOptions.Value.DeliveryLogRetentionDays;
        DateTimeOffset cutoffUtc = DateTimeOffset.UtcNow.AddDays(-retentionDays);
        int deletedCount = await deliveryLogRepository
            .DeleteOlderThanAsync(cutoffUtc, cancellationToken)
            .ConfigureAwait(false);

        TempData["StatusMessage"] =
            $"Deleted {deletedCount} email delivery log entries older than {retentionDays} days.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("resend-confirmation", Name = "Admin_EmailDiagnostics_ResendConfirmation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmation(
        [FromForm] string? email,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["ErrorMessage"] = "Email is required.";
            return RedirectToAction(nameof(Index));
        }

        DarwinLinguaIdentityUser? user = await userManager.FindByEmailAsync(email.Trim()).ConfigureAwait(false);
        if (user is null)
        {
            TempData["StatusMessage"] = "If an unconfirmed account exists for that email, a new confirmation email has been sent.";
            return RedirectToAction(nameof(Index));
        }

        if (await userManager.IsEmailConfirmedAsync(user).ConfigureAwait(false))
        {
            TempData["StatusMessage"] = "That account is already confirmed; no confirmation email was sent.";
            return RedirectToAction(nameof(Index));
        }

        string code = await userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        string callbackUrl = BuildPublicPageUrl(
            "/Account/ConfirmEmail",
            new { area = "Identity", userId = user.Id, code });

        await accountEmailService.SendEmailConfirmationAsync(
                user,
                callbackUrl,
                ResolveCulture(),
                HttpContext.TraceIdentifier,
                cancellationToken)
            .ConfigureAwait(false);

        TempData["StatusMessage"] = "If an unconfirmed account exists for that email, a new confirmation email has been sent.";
        return RedirectToAction(nameof(Index));
    }

    private static DateTimeOffset? ParseDateTimeOffset(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTimeOffset.TryParse(value.Trim(), out DateTimeOffset parsed)
            ? parsed.ToUniversalTime()
            : null;
    }

    private AdminEmailReadinessViewModel BuildReadiness()
    {
        TransactionalEmailOptions options = emailOptions.Value;
        List<string> warnings = [];
        bool smtpMode = string.Equals(options.Mode, "Smtp", StringComparison.OrdinalIgnoreCase);
        bool fileMode = string.Equals(options.Mode, "File", StringComparison.OrdinalIgnoreCase);

        if (fileMode)
        {
            warnings.Add("File email mode is for local development only.");
        }

        if (string.IsNullOrWhiteSpace(options.PublicBaseUrl))
        {
            warnings.Add("PublicBaseUrl is not configured; email links may use request host fallback.");
        }

        if (smtpMode && string.IsNullOrWhiteSpace(options.SmtpHost))
        {
            warnings.Add("SMTP mode is enabled but SmtpHost is missing.");
        }

        if (smtpMode && string.IsNullOrWhiteSpace(options.SmtpUserName))
        {
            warnings.Add("SMTP mode is enabled but SmtpUserName is missing.");
        }

        if (smtpMode && string.IsNullOrWhiteSpace(options.SmtpPassword))
        {
            warnings.Add("SMTP mode is enabled but SmtpPassword is missing.");
        }

        bool brevoMode = string.Equals(options.Mode, "BrevoApi", StringComparison.OrdinalIgnoreCase);
        if (brevoMode && string.IsNullOrWhiteSpace(options.BrevoApiKey))
        {
            warnings.Add("Brevo API mode is enabled but BrevoApiKey is missing.");
        }

        if (brevoMode && string.IsNullOrWhiteSpace(options.BrevoWebhookSecret))
        {
            warnings.Add("Brevo API mode is enabled but BrevoWebhookSecret is missing; webhook delivery events cannot be trusted.");
        }

        if (brevoMode && options.BrevoSandboxMode)
        {
            warnings.Add("Brevo sandbox mode is enabled; API calls are validated but no real email is delivered.");
        }

        if (options.AdminNotificationEmails.Length == 0)
        {
            warnings.Add("No admin notification recipients are configured.");
        }

        if (options.EnableFailureAlerts && options.AdminNotificationEmails.Length == 0)
        {
            warnings.Add("Failure alerts are enabled but no admin notification recipients are configured.");
        }

        if (!string.IsNullOrWhiteSpace(options.FromEmail) &&
            !string.IsNullOrWhiteSpace(options.ReplyToEmail) &&
            !string.Equals(GetDomain(options.FromEmail), GetDomain(options.ReplyToEmail), StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("FromEmail and ReplyToEmail use different domains; confirm this is intentional and provider-approved.");
        }

        return new AdminEmailReadinessViewModel(
            options.Mode,
            options.PublicBaseUrl,
            options.FromEmail,
            options.ReplyToEmail,
            options.SupportEmail,
            options.AdminNotificationEmails.Count(static email => !string.IsNullOrWhiteSpace(email)),
            !string.IsNullOrWhiteSpace(options.SmtpHost),
            !string.IsNullOrWhiteSpace(options.SmtpUserName),
            !string.IsNullOrWhiteSpace(options.SmtpPassword),
            !string.IsNullOrWhiteSpace(options.BrevoApiKey),
            !string.IsNullOrWhiteSpace(options.BrevoWebhookSecret),
            options.BrevoSandboxMode,
            options.EnableFailureAlerts,
            options.FailureAlertThreshold,
            options.FailureAlertWindowMinutes,
            options.FailureAlertCooldownMinutes,
            options.FailureAlertMonitorIntervalMinutes,
            warnings);
    }

    private static string? GetDomain(string email)
    {
        int atIndex = email.LastIndexOf('@');
        return atIndex >= 0 && atIndex < email.Length - 1
            ? email[(atIndex + 1)..]
            : null;
    }

    private string BuildPublicPageUrl(string page, object values)
    {
        string path = Url.Page(page, null, values) ?? "/";
        if (!string.IsNullOrWhiteSpace(emailOptions.Value.PublicBaseUrl) &&
            Uri.TryCreate(emailOptions.Value.PublicBaseUrl, UriKind.Absolute, out Uri? baseUri))
        {
            return new Uri(baseUri, path).ToString();
        }

        return Url.Page(page, null, values, Request.Scheme) ?? path;
    }

    private string ResolveCulture() =>
        Request.HttpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()
            ?.RequestCulture.UICulture.Name
        ?? Request.Headers.AcceptLanguage.ToString()
        ?? "en";
}
