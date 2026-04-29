using DarwinLingua.Web.Data;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/email-diagnostics")]
public sealed class EmailDiagnosticsController(
    IEmailDeliveryLogRepository deliveryLogRepository,
    IOptions<TransactionalEmailOptions> emailOptions)
    : Controller
{
    [HttpGet("", Name = "Admin_EmailDiagnostics")]
    public async Task<IActionResult> Index(
        [FromQuery] string? status,
        [FromQuery] string? scenario,
        [FromQuery] string? fromUtc,
        [FromQuery] string? toUtc,
        [FromQuery] string? recipientHashPrefix,
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
                cancellationToken)
            .ConfigureAwait(false);

        AdminEmailDiagnosticsPageViewModel model = new(
            status,
            scenario,
            fromUtc,
            toUtc,
            recipientHashPrefix,
            boundedTake,
            emailOptions.Value.DeliveryLogRetentionDays,
            TempData["StatusMessage"] as string,
            TempData["ErrorMessage"] as string,
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
                    log.Status,
                    log.FailureCode,
                    log.FailureMessageSummary,
                    log.RetryCount,
                    log.CreatedAtUtc,
                    log.SentAtUtc,
                    log.LastAttemptAtUtc,
                    log.CorrelationId))
                .ToArray());

        return View(model);
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
}
