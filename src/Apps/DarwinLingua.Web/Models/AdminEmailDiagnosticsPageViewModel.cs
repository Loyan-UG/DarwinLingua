using DarwinLingua.Web.Data;

namespace DarwinLingua.Web.Models;

public sealed record AdminEmailDiagnosticsPageViewModel(
    string? Status,
    string? Scenario,
    string? FromUtc,
    string? ToUtc,
    string? RecipientHashPrefix,
    string? ProviderMessageId,
    string? ProviderEvent,
    string? SuppressionHashPrefix,
    string? SuppressionReason,
    int Take,
    int RetentionDays,
    EmailSuppressionSummaryViewModel SuppressionSummary,
    string? StatusMessage,
    string? ErrorMessage,
    AdminEmailReadinessViewModel Readiness,
    IReadOnlyList<AdminEmailDeliveryLogItemViewModel> Logs,
    IReadOnlyList<AdminEmailSuppressionItemViewModel> Suppressions);

public sealed record EmailSuppressionSummaryViewModel(
    int TotalCount,
    DateTimeOffset? LastCreatedAtUtc,
    string? LastReason);

public sealed record AdminEmailSuppressionItemViewModel(
    string RecipientEmailHash,
    string Reason,
    string ProviderName,
    string? ProviderMessageId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastSeenAtUtc);

public sealed record AdminEmailReadinessViewModel(
    string Mode,
    string PublicBaseUrl,
    string FromEmail,
    string ReplyToEmail,
    string SupportEmail,
    int AdminNotificationRecipientCount,
    bool HasSmtpHost,
    bool HasSmtpUserName,
    bool HasSmtpPassword,
    bool HasBrevoApiKey,
    bool HasBrevoWebhookSecret,
    bool BrevoSandboxMode,
    bool BrevoAllowQuerySecretFallback,
    bool FailureAlertsEnabled,
    int FailureAlertThreshold,
    int FailureAlertWindowMinutes,
    int FailureAlertCooldownMinutes,
    int FailureAlertMonitorIntervalMinutes,
    IReadOnlyList<string> Warnings);

public sealed record AdminEmailDeliveryLogItemViewModel(
    Guid Id,
    string ScenarioKey,
    string RecipientEmailHash,
    string? RecipientUserId,
    string TemplateKey,
    string Culture,
    string Subject,
    string ProviderName,
    string? ProviderMessageId,
    string? ProviderLastEvent,
    DateTimeOffset? ProviderLastEventAtUtc,
    string? ProviderLastEventReason,
    WebEmailDeliveryStatus Status,
    string? FailureCode,
    string? FailureMessageSummary,
    int RetryCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? SentAtUtc,
    DateTimeOffset? LastAttemptAtUtc,
    string? CorrelationId);
