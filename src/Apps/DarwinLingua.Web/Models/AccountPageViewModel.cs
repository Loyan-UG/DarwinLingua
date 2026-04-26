using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Identity;

namespace DarwinLingua.Web.Models;

public sealed record AccountPageViewModel(
    string DisplayName,
    string? Email,
    IReadOnlyList<string> Roles,
    UserLearningProfileModel Profile,
    UserEntitlementSnapshot Entitlement,
    IReadOnlyList<UserEntitlementAuditEventModel> EntitlementAuditEvents);
