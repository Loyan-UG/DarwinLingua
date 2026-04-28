using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed class UserReport
{
    private UserReport()
    {
    }

    public UserReport(
        Guid id,
        string reporterEmail,
        string targetType,
        string targetKey,
        string? reportedUserEmail,
        string reason,
        string details,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("User report identifier cannot be empty.");
        }

        Id = id;
        ReporterEmail = LearnerConversationProfile.NormalizeEmail(reporterEmail);
        TargetType = ConversationEvent.NormalizeTaxonomyKey(targetType, ModerationTaxonomy.ReportTargetTypes, "Report target type");
        TargetKey = ConversationEvent.NormalizeRequiredText(targetKey, nameof(targetKey), 256);
        ReportedUserEmail = string.IsNullOrWhiteSpace(reportedUserEmail)
            ? null
            : LearnerConversationProfile.NormalizeEmail(reportedUserEmail);
        Reason = ConversationEvent.NormalizeTaxonomyKey(reason, ModerationTaxonomy.ReportReasons, "Report reason");
        Details = ConversationEvent.NormalizeRequiredText(details, nameof(details), 2000);
        Status = UserReportStatuses.Pending;
        CreatedAtUtc = ConversationEvent.NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string ReporterEmail { get; private set; } = string.Empty;

    public string TargetType { get; private set; } = string.Empty;

    public string TargetKey { get; private set; } = string.Empty;

    public string? ReportedUserEmail { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public string Details { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public string? DecisionNote { get; private set; }

    public string? DecidedBy { get; private set; }

    public DateTime? DecidedAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public void RecordDecision(string status, string? decisionNote, string decidedBy, DateTime decidedAtUtc)
    {
        string normalizedStatus = ConversationEvent.NormalizeTaxonomyKey(status, UserReportStatuses.All, "User report status");
        if (normalizedStatus == UserReportStatuses.Pending)
        {
            throw new DomainRuleException("Moderation decision status cannot be pending.");
        }

        Status = normalizedStatus;
        DecisionNote = ConversationEvent.NormalizeOptionalText(decisionNote, 1000);
        DecidedBy = LearnerConversationProfile.NormalizeEmail(decidedBy);
        DecidedAtUtc = ConversationEvent.NormalizeUtc(decidedAtUtc, nameof(decidedAtUtc));
        UpdatedAtUtc = DecidedAtUtc.Value;
    }
}

public static class UserReportStatuses
{
    public const string Pending = "pending";

    public const string Reviewed = "reviewed";

    public const string Dismissed = "dismissed";

    public const string ActionTaken = "action-taken";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Pending,
        Reviewed,
        Dismissed,
        ActionTaken,
    };
}

public static class ModerationTaxonomy
{
    public static readonly IReadOnlySet<string> ReportTargetTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "learner-profile",
        "partner-request",
        "conversation-event",
        "organizer-profile",
    };

    public static readonly IReadOnlySet<string> ReportReasons = new HashSet<string>(StringComparer.Ordinal)
    {
        "spam",
        "harassment",
        "unsafe-contact",
        "inaccurate-listing",
        "impersonation",
        "other",
    };
}
