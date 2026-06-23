using DarwinLingua.Web.Data;
using DarwinLingua.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class EmailDeliveryFailureMonitorTests
{
    [Fact]
    public async Task Monitor_ShouldSendAdminAlertWhenFailureThresholdIsReached()
    {
        FakeEmailDeliveryLogRepository repository = new(
            new EmailDeliveryFailureAlertSnapshot(
                FailureCount: 3,
                LastFailureAtUtc: DateTimeOffset.UtcNow,
                LastFailureScenarioKey: TransactionalEmailScenarios.AccountPasswordReset,
                LastFailureCode: "brevo:hard_bounce"));
        FakeCommunityNotificationEmailService notifications = new();
        EmailDeliveryFailureMonitorService monitor = CreateMonitor(repository, notifications, threshold: 3);

        await monitor.StartAsync(CancellationToken.None);
        await notifications.WaitForAlertAsync(TimeSpan.FromSeconds(5));
        await monitor.StopAsync(CancellationToken.None);

        Assert.Equal(3, notifications.FailureCount);
        Assert.Equal(15, notifications.WindowMinutes);
        Assert.Equal(TransactionalEmailScenarios.AccountPasswordReset, notifications.LastFailureScenarioKey);
        Assert.Equal("brevo:hard_bounce", notifications.LastFailureCode);
        Assert.Equal(TransactionalEmailScenarios.AdminEmailDeliveryFailureAlert, repository.ExcludedScenarioKey);
        Assert.True(repository.SinceUtc < DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Monitor_ShouldNotSendAdminAlertBelowThreshold()
    {
        FakeEmailDeliveryLogRepository repository = new(
            new EmailDeliveryFailureAlertSnapshot(
                FailureCount: 2,
                LastFailureAtUtc: DateTimeOffset.UtcNow,
                LastFailureScenarioKey: TransactionalEmailScenarios.AccountPasswordReset,
                LastFailureCode: "brevo:soft_bounce"));
        FakeCommunityNotificationEmailService notifications = new();
        EmailDeliveryFailureMonitorService monitor = CreateMonitor(repository, notifications, threshold: 3);

        await monitor.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromMilliseconds(250));
        await monitor.StopAsync(CancellationToken.None);

        Assert.Equal(0, notifications.AlertCount);
        Assert.Equal(TransactionalEmailScenarios.AdminEmailDeliveryFailureAlert, repository.ExcludedScenarioKey);
    }

    private static EmailDeliveryFailureMonitorService CreateMonitor(
        FakeEmailDeliveryLogRepository repository,
        FakeCommunityNotificationEmailService notifications,
        int threshold)
    {
        ServiceCollection services = new();
        services.AddSingleton<IEmailDeliveryLogRepository>(repository);
        services.AddSingleton<ICommunityNotificationEmailService>(notifications);
        ServiceProvider provider = services.BuildServiceProvider();

        TransactionalEmailOptions options = new()
        {
            Mode = "BrevoApi",
            EnableFailureAlerts = true,
            AdminNotificationEmails = ["admin@example.test"],
            FailureAlertThreshold = threshold,
            FailureAlertWindowMinutes = 15,
            FailureAlertCooldownMinutes = 60,
            FailureAlertMonitorIntervalMinutes = 1,
        };

        return new EmailDeliveryFailureMonitorService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            new StaticOptionsMonitor<TransactionalEmailOptions>(options),
            NullLogger<EmailDeliveryFailureMonitorService>.Instance);
    }

    private sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue => value;

        public T Get(string? name) => value;

        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }

    private sealed class FakeEmailDeliveryLogRepository(EmailDeliveryFailureAlertSnapshot snapshot)
        : IEmailDeliveryLogRepository
    {
        public DateTimeOffset SinceUtc { get; private set; }

        public string? ExcludedScenarioKey { get; private set; }

        public Task<EmailDeliveryFailureAlertSnapshot> GetFailureAlertSnapshotAsync(
            DateTimeOffset sinceUtc,
            string excludedScenarioKey,
            CancellationToken cancellationToken)
        {
            SinceUtc = sinceUtc;
            ExcludedScenarioKey = excludedScenarioKey;
            return Task.FromResult(snapshot);
        }

        public Task<WebEmailDeliveryLog> AddQueuedAsync(
            TransactionalEmailMessage message,
            string providerName,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<WebEmailDeliveryLog> AddSuppressedAsync(
            TransactionalEmailMessage message,
            string providerName,
            string reason,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<bool> IsSuppressedAsync(string recipientEmail, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task MarkSentAsync(Guid id, string? providerMessageId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task MarkFailedAsync(
            Guid id,
            string? failureCode,
            string? failureMessageSummary,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<bool> MarkProviderEventAsync(
            string providerMessageId,
            string providerEvent,
            DateTimeOffset providerEventAtUtc,
            string? reason,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<WebEmailDeliveryLog>> GetRecentAsync(
            int take,
            string? status,
            string? dialogue,
            DateTimeOffset? fromUtc,
            DateTimeOffset? toUtc,
            string? recipientHashPrefix,
            string? providerMessageId,
            string? providerEvent,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<int> DeleteOlderThanAsync(DateTimeOffset cutoffUtc, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<EmailDeliverySummary> GetSummarySinceAsync(DateTimeOffset sinceUtc, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<EmailSuppressionSummary> GetSuppressionSummaryAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<WebEmailSuppression>> GetSuppressionsAsync(
            string? recipientHashPrefix,
            string? reason,
            int take,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<bool> DeleteSuppressionByHashAsync(string recipientEmailHash, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class FakeCommunityNotificationEmailService : ICommunityNotificationEmailService
    {
        private readonly TaskCompletionSource alertSent =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int AlertCount { get; private set; }

        public int FailureCount { get; private set; }

        public int WindowMinutes { get; private set; }

        public string? LastFailureScenarioKey { get; private set; }

        public string? LastFailureCode { get; private set; }

        public Task WaitForAlertAsync(TimeSpan timeout) =>
            alertSent.Task.WaitAsync(timeout);

        public Task SendAdminEmailDeliveryFailureAlertAsync(
            int failureCount,
            int windowMinutes,
            string? lastFailureScenarioKey,
            string? lastFailureCode,
            string? culture,
            string? correlationId,
            CancellationToken cancellationToken)
        {
            AlertCount++;
            FailureCount = failureCount;
            WindowMinutes = windowMinutes;
            LastFailureScenarioKey = lastFailureScenarioKey;
            LastFailureCode = lastFailureCode;
            alertSent.TrySetResult();
            return Task.CompletedTask;
        }

        public Task SendOrganizerClaimSubmittedAsync(
            string requesterEmail,
            string organizerName,
            string? culture,
            string? correlationId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task SendAdminNewOrganizerClaimAsync(
            string organizerName,
            string requesterName,
            string? culture,
            string? correlationId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task SendOrganizerClaimDecisionAsync(
            string requesterEmail,
            string organizerName,
            string status,
            string? culture,
            string? correlationId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task SendOrganizerProfileOwnershipChangedAsync(
            string ownerEmail,
            string organizerProfileSlug,
            string? culture,
            string? correlationId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task SendEventRsvpConfirmationAsync(
            string participantEmail,
            string eventTitle,
            string rsvpStatus,
            string? culture,
            string? correlationId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task SendPartnerRequestAcceptedAsync(
            string requesterEmail,
            string displayName,
            string? culture,
            string? correlationId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task SendAdminHighSeverityReportAsync(
            string reason,
            string targetType,
            string targetKey,
            string? culture,
            string? correlationId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task SendModerationReportOutcomeAsync(
            string reporterEmail,
            string targetType,
            string status,
            string? culture,
            string? correlationId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
