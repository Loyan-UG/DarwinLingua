using System.ComponentModel.DataAnnotations;
using DarwinLingua.Web.Services;

namespace DarwinLingua.Web.Models;

public sealed class UserReportInputModel
{
    [Required]
    [StringLength(64)]
    public string TargetType { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    public string TargetKey { get; set; } = string.Empty;

    [StringLength(320)]
    [EmailAddress]
    public string? ReportedUserEmail { get; set; }

    [Required]
    [StringLength(64)]
    public string Reason { get; set; } = "other";

    [Required]
    [StringLength(2000)]
    public string Details { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public sealed class UserBlockInputModel
{
    [StringLength(320)]
    [EmailAddress]
    public string BlockedEmail { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Reason { get; set; }

    public Guid? SourcePartnerRequestId { get; set; }

    public Guid? TargetLearnerProfileId { get; set; }

    public string? ReturnUrl { get; set; }
}

public sealed record AdminModerationPageViewModel(
    string? StatusFilter,
    IReadOnlyList<UserReportModel> Reports,
    IReadOnlyList<ModerationDecisionAuditModel> Audits,
    string? StatusMessage,
    string? ErrorMessage);

public sealed class AdminModerationDecisionInputModel
{
    [Required]
    public string Status { get; set; } = "reviewed";

    [StringLength(1000)]
    public string? DecisionNote { get; set; }
}
