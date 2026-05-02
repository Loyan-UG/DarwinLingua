using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public sealed class EmailDeliveryFailureMonitorService(
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<TransactionalEmailOptions> optionsMonitor,
    ILogger<EmailDeliveryFailureMonitorService> logger) : BackgroundService
{
    private DateTimeOffset? lastAlertAtUtc;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            TransactionalEmailOptions options = optionsMonitor.CurrentValue;
            TimeSpan interval = TimeSpan.FromMinutes(Math.Max(1, options.FailureAlertMonitorIntervalMinutes));

            try
            {
                await CheckAsync(options, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Email delivery failure monitor failed.");
            }

            try
            {
                await Task.Delay(interval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private async Task CheckAsync(
        TransactionalEmailOptions options,
        CancellationToken cancellationToken)
    {
        if (!options.EnableFailureAlerts ||
            options.AdminNotificationEmails.All(static email => string.IsNullOrWhiteSpace(email)) ||
            string.Equals(options.Mode, "Disabled", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        TimeSpan cooldown = TimeSpan.FromMinutes(Math.Max(1, options.FailureAlertCooldownMinutes));
        if (lastAlertAtUtc is not null && nowUtc - lastAlertAtUtc.Value < cooldown)
        {
            return;
        }

        DateTimeOffset sinceUtc = nowUtc.AddMinutes(-Math.Max(1, options.FailureAlertWindowMinutes));
        using IServiceScope scope = scopeFactory.CreateScope();
        IEmailDeliveryLogRepository repository = scope.ServiceProvider.GetRequiredService<IEmailDeliveryLogRepository>();
        EmailDeliveryFailureAlertSnapshot snapshot = await repository
            .GetFailureAlertSnapshotAsync(
                sinceUtc,
                TransactionalEmailScenarios.AdminEmailDeliveryFailureAlert,
                cancellationToken)
            .ConfigureAwait(false);

        if (snapshot.FailureCount < Math.Max(1, options.FailureAlertThreshold))
        {
            return;
        }

        ICommunityNotificationEmailService notificationEmailService = scope.ServiceProvider
            .GetRequiredService<ICommunityNotificationEmailService>();
        await notificationEmailService.SendAdminEmailDeliveryFailureAlertAsync(
                snapshot.FailureCount,
                Math.Max(1, options.FailureAlertWindowMinutes),
                snapshot.LastFailureScenarioKey,
                snapshot.LastFailureCode,
                "en",
                $"email-failure-monitor:{nowUtc:O}",
                cancellationToken)
            .ConfigureAwait(false);

        lastAlertAtUtc = nowUtc;
    }
}
