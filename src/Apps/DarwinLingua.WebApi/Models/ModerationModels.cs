namespace DarwinLingua.WebApi.Models;

public sealed record SubmitUserReportRequest(
    string TargetType,
    string TargetKey,
    string? ReportedUserEmail,
    string Reason,
    string Details);

public sealed record BlockUserRequest(
    string? BlockedEmail,
    string? Reason,
    Guid? SourcePartnerRequestId,
    Guid? TargetLearnerProfileId = null);

public sealed record ModerationDecisionRequest(
    string Status,
    string? DecisionNote,
    string DecidedBy);

public sealed record UserReportResponse(
    Guid Id,
    string ReporterEmail,
    string TargetType,
    string TargetKey,
    string? ReportedUserEmail,
    string Reason,
    string Details,
    string Status,
    string? DecisionNote,
    string? DecidedBy,
    DateTime? DecidedAtUtc,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record UserBlockResponse(
    Guid Id,
    string BlockerEmail,
    string BlockedEmail,
    string? Reason,
    Guid? SourcePartnerRequestId,
    DateTime CreatedAtUtc);

public sealed record ModerationDecisionAuditResponse(
    Guid Id,
    Guid UserReportId,
    string DecisionStatus,
    string DecidedBy,
    string? DecisionNote,
    DateTime CreatedAtUtc);
