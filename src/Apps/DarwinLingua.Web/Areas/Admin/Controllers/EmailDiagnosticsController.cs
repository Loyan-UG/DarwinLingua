using System.Text;
using System.ComponentModel.DataAnnotations;
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
    IAccountEmailService accountEmailService,
    IAccountEmailRateLimiter rateLimiter,
    ILogger<EmailDiagnosticsController> logger)
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
        string? normalizedStatus = NormalizeAllowedValue(status, IsAllowedDeliveryStatus, 32);
        string? normalizedScenario = NormalizeLength(scenario, 128);
        string? normalizedRecipientHashPrefix = NormalizeHexPrefix(recipientHashPrefix, 64);
        string? normalizedProviderMessageId = NormalizeLength(providerMessageId, 256);
        string? normalizedProviderEvent = NormalizeAllowedValue(providerEvent, IsAllowedProviderEvent, 64);
        string? normalizedSuppressionHashPrefix = NormalizeHexPrefix(suppressionHashPrefix, 64);
        string? normalizedSuppressionReason = NormalizeAllowedValue(suppressionReason, IsAllowedSuppressionReason, 64);
        string? queryErrorMessage = BuildQueryValidationMessage(
            status,
            normalizedStatus,
            recipientHashPrefix,
            normalizedRecipientHashPrefix,
            providerEvent,
            normalizedProviderEvent,
            suppressionHashPrefix,
            normalizedSuppressionHashPrefix,
            suppressionReason,
            normalizedSuppressionReason);

        IReadOnlyList<WebEmailDeliveryLog> logs = await deliveryLogRepository
            .GetRecentAsync(
                boundedTake,
                normalizedStatus,
                normalizedScenario,
                parsedFromUtc,
                parsedToUtc,
                normalizedRecipientHashPrefix,
                normalizedProviderMessageId,
                normalizedProviderEvent,
                cancellationToken)
            .ConfigureAwait(false);
        EmailSuppressionSummary suppressionSummary = await deliveryLogRepository
            .GetSuppressionSummaryAsync(cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<WebEmailSuppression> suppressions = await deliveryLogRepository
            .GetSuppressionsAsync(
                normalizedSuppressionHashPrefix,
                normalizedSuppressionReason,
                boundedTake,
                cancellationToken)
            .ConfigureAwait(false);

        AdminEmailDiagnosticsPageViewModel model = new(
            normalizedStatus,
            normalizedScenario,
            fromUtc,
            toUtc,
            normalizedRecipientHashPrefix,
            normalizedProviderMessageId,
            normalizedProviderEvent,
            normalizedSuppressionHashPrefix,
            normalizedSuppressionReason,
            boundedTake,
            emailOptions.Value.DeliveryLogRetentionDays,
            new EmailSuppressionSummaryViewModel(
                suppressionSummary.TotalCount,
                suppressionSummary.LastCreatedAtUtc,
                suppressionSummary.LastReason),
            TempData["StatusMessage"] as string,
            queryErrorMessage ?? TempData["ErrorMessage"] as string,
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
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> RemoveSuppression(
        [FromForm] string? recipientEmailHash,
        CancellationToken cancellationToken)
    {
        string normalizedHash = recipientEmailHash?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!IsSha256HexHash(normalizedHash))
        {
            TempData["ErrorMessage"] = "Suppression hash must be a full SHA-256 hex hash.";
            return RedirectToAction(nameof(Index));
        }

        bool removed = await deliveryLogRepository
            .DeleteSuppressionByHashAsync(normalizedHash, cancellationToken)
            .ConfigureAwait(false);
        logger.LogInformation(
            "Admin {AdminEmail} removed email suppression for hash prefix {HashPrefix}. Removed: {Removed}.",
            GetAdminEmail(),
            normalizedHash[..Math.Min(12, normalizedHash.Length)],
            removed);
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
        string normalizedProviderMessageId = providerMessageId?.Trim() ?? string.Empty;
        string normalizedProviderEvent = providerEvent?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedProviderMessageId) ||
            normalizedProviderMessageId.Length > 256 ||
            string.IsNullOrWhiteSpace(normalizedProviderEvent) ||
            normalizedProviderEvent.Length > 64)
        {
            TempData["ErrorMessage"] = "Provider message id and provider event are required and must fit expected lengths.";
            return RedirectToAction(nameof(Index));
        }

        if (!IsAllowedProviderEvent(normalizedProviderEvent))
        {
            TempData["ErrorMessage"] = "Provider event is not supported.";
            return RedirectToAction(nameof(Index));
        }

        bool updated = await deliveryLogRepository
            .MarkProviderEventAsync(
                normalizedProviderMessageId,
                normalizedProviderEvent,
                DateTimeOffset.UtcNow,
                TrimToNull(reason, 512),
                cancellationToken)
            .ConfigureAwait(false);
        logger.LogInformation(
            "Admin {AdminEmail} manually recorded provider event {ProviderEvent} for provider message id {ProviderMessageId}. Updated: {Updated}.",
            GetAdminEmail(),
            normalizedProviderEvent,
            normalizedProviderMessageId,
            updated);

        TempData[updated ? "StatusMessage" : "ErrorMessage"] = updated
            ? "Provider event recorded for the matching delivery log."
            : "No delivery log matched that provider message id.";
        return RedirectToAction(nameof(Index), new { providerMessageId = normalizedProviderMessageId });
    }

    [HttpPost("cleanup", Name = "Admin_EmailDiagnostics_Cleanup")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Cleanup(CancellationToken cancellationToken)
    {
        int retentionDays = emailOptions.Value.DeliveryLogRetentionDays;
        DateTimeOffset cutoffUtc = DateTimeOffset.UtcNow.AddDays(-retentionDays);
        int deletedCount = await deliveryLogRepository
            .DeleteOlderThanAsync(cutoffUtc, cancellationToken)
            .ConfigureAwait(false);
        logger.LogInformation(
            "Admin {AdminEmail} deleted {DeletedCount} email delivery log entries older than {RetentionDays} days.",
            GetAdminEmail(),
            deletedCount,
            retentionDays);

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
        string normalizedEmail = email?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedEmail) || !new EmailAddressAttribute().IsValid(normalizedEmail))
        {
            TempData["ErrorMessage"] = "A valid email is required.";
            return RedirectToAction(nameof(Index));
        }

        DarwinLinguaIdentityUser? user = await userManager.FindByEmailAsync(normalizedEmail).ConfigureAwait(false);
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

        if (!rateLimiter.TryConsume("admin-resend-confirmation", normalizedEmail, 3, TimeSpan.FromHours(1)))
        {
            TempData["ErrorMessage"] = "Too many confirmation resend attempts for that account. Please wait before trying again.";
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

    private static bool IsSha256HexHash(string value) =>
        value.Length == 64 && value.All(static character =>
            (character >= '0' && character <= '9') ||
            (character >= 'A' && character <= 'F'));

    private static string? NormalizeLength(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static string? NormalizeHexPrefix(string? value, int maxLength)
    {
        string? normalized = NormalizeLength(value, maxLength)?.ToUpperInvariant();
        return normalized is not null && normalized.All(static character =>
            (character >= '0' && character <= '9') ||
            (character >= 'A' && character <= 'F'))
            ? normalized
            : null;
    }

    private static string? NormalizeAllowedValue(string? value, Func<string, bool> isAllowed, int maxLength)
    {
        string? normalized = NormalizeLength(value, maxLength);
        return normalized is not null && isAllowed(normalized)
            ? normalized
            : null;
    }

    private static string? BuildQueryValidationMessage(params object?[] values)
    {
        for (int index = 0; index < values.Length; index += 2)
        {
            string? original = values[index] as string;
            string? normalized = values[index + 1] as string;
            if (!string.IsNullOrWhiteSpace(original) && string.IsNullOrWhiteSpace(normalized))
            {
                return "One or more email diagnostic filters were ignored because they were not supported.";
            }
        }

        return null;
    }

    private static bool IsAllowedDeliveryStatus(string status) =>
        string.Equals(status, "Queued", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Sent", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Skipped", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Suppressed", StringComparison.OrdinalIgnoreCase);

    private static bool IsAllowedProviderEvent(string providerEvent) =>
        string.Equals(providerEvent, "request", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "sent", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "delivered", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "soft_bounce", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "hard_bounce", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "blocked", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "invalid", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "invalid_email", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "error", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "spam", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "opened", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "unique_opened", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "click", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(providerEvent, "unsubscribed", StringComparison.OrdinalIgnoreCase);

    private static bool IsAllowedSuppressionReason(string suppressionReason) =>
        string.Equals(suppressionReason, "brevo:hard_bounce", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(suppressionReason, "brevo:blocked", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(suppressionReason, "brevo:invalid", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(suppressionReason, "brevo:invalid_email", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(suppressionReason, "brevo:spam", StringComparison.OrdinalIgnoreCase);

    private static string? TrimToNull(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
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

        if (brevoMode && options.BrevoAllowQuerySecretFallback)
        {
            warnings.Add("Brevo webhook query-string secret fallback is enabled; use only for local diagnostics.");
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
            options.BrevoAllowQuerySecretFallback,
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

    private string GetAdminEmail() =>
        WebUserIdentity.TryGetEmail(User)
        ?? User.Identity?.Name
        ?? "admin@local";
}
