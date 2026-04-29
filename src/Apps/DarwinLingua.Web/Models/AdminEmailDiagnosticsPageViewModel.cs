using DarwinLingua.Web.Data;

namespace DarwinLingua.Web.Models;

public sealed record AdminEmailDiagnosticsPageViewModel(
    string? Status,
    string? Scenario,
    string? FromUtc,
    string? ToUtc,
    string? RecipientHashPrefix,
    int Take,
    int RetentionDays,
    string? StatusMessage,
    string? ErrorMessage,
    IReadOnlyList<AdminEmailDeliveryLogItemViewModel> Logs);

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
    WebEmailDeliveryStatus Status,
    string? FailureCode,
    string? FailureMessageSummary,
    int RetryCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? SentAtUtc,
    DateTimeOffset? LastAttemptAtUtc,
    string? CorrelationId);
