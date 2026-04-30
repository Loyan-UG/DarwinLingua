using System.ComponentModel.DataAnnotations;

namespace DarwinLingua.Web.Models;

public sealed record AdminUsersPageViewModel(
    IReadOnlyList<AdminUserListItemViewModel> Users,
    string? StatusMessage,
    string? ErrorMessage);

public sealed record AdminUserListItemViewModel(
    string UserId,
    string Email,
    IReadOnlyList<string> Roles,
    string EntitlementTier,
    DateTimeOffset? TrialEndsAtUtc,
    DateTimeOffset? PremiumEndsAtUtc,
    IReadOnlyList<string> EnabledFeatures,
    IReadOnlyList<AdminUserEntitlementAuditEventViewModel> RecentEntitlementEvents);

public sealed record AdminUserEntitlementAuditEventViewModel(
    string EventType,
    string? PreviousTier,
    string NewTier,
    string UpdatedBy,
    DateTimeOffset CreatedAtUtc);

public sealed class AdminUpdateUserEntitlementInputModel
{
    [Required]
    [StringLength(128)]
    public string UserId { get; init; } = string.Empty;

    [Required]
    [StringLength(32)]
    public string Tier { get; init; } = string.Empty;

    [StringLength(64)]
    public string? ExpiresAtUtc { get; init; }
}

public sealed class AdminUpdateUserRoleInputModel
{
    [Required]
    [StringLength(128)]
    public string UserId { get; init; } = string.Empty;

    [Required]
    [StringLength(64)]
    public string Role { get; init; } = string.Empty;

    public bool IsEnabled { get; init; }
}
