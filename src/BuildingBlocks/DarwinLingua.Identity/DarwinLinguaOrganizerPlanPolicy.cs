namespace DarwinLingua.Identity;

public static class DarwinLinguaOrganizerPlanKeys
{
    public const string Free = "free";
    public const string Lite = "lite";
    public const string Standard = "standard";
    public const string Pro = "pro";
}

public sealed record DarwinLinguaOrganizerPlanSnapshot(
    string PlanKey,
    int ActiveEventLimit,
    IReadOnlyList<string> EnabledFeatures);

public static class DarwinLinguaOrganizerPlanPolicy
{
    public static DarwinLinguaOrganizerPlanSnapshot Resolve(string? planKey)
    {
        string normalizedPlanKey = string.IsNullOrWhiteSpace(planKey)
            ? DarwinLinguaOrganizerPlanKeys.Free
            : planKey.Trim().ToLowerInvariant();

        return normalizedPlanKey switch
        {
            DarwinLinguaOrganizerPlanKeys.Pro => new DarwinLinguaOrganizerPlanSnapshot(
                DarwinLinguaOrganizerPlanKeys.Pro,
                100,
                [
                    DarwinLinguaFeatureKeys.OrganizerDashboard,
                    DarwinLinguaFeatureKeys.OrganizerProfileManagement,
                    DarwinLinguaFeatureKeys.OrganizerEventManagement,
                    DarwinLinguaFeatureKeys.OrganizerRsvpManagement,
                    DarwinLinguaFeatureKeys.OrganizerAnalytics,
                    DarwinLinguaFeatureKeys.OrganizerRecurringEvents,
                    DarwinLinguaFeatureKeys.OrganizerFeaturedPlacement,
                    DarwinLinguaFeatureKeys.OrganizerMultipleAdmins,
                    DarwinLinguaFeatureKeys.OrganizerBrandedProfile,
                ]),
            DarwinLinguaOrganizerPlanKeys.Standard => new DarwinLinguaOrganizerPlanSnapshot(
                DarwinLinguaOrganizerPlanKeys.Standard,
                20,
                [
                    DarwinLinguaFeatureKeys.OrganizerDashboard,
                    DarwinLinguaFeatureKeys.OrganizerProfileManagement,
                    DarwinLinguaFeatureKeys.OrganizerEventManagement,
                    DarwinLinguaFeatureKeys.OrganizerRsvpManagement,
                    DarwinLinguaFeatureKeys.OrganizerAnalytics,
                    DarwinLinguaFeatureKeys.OrganizerRecurringEvents,
                    DarwinLinguaFeatureKeys.OrganizerFeaturedPlacement,
                ]),
            DarwinLinguaOrganizerPlanKeys.Lite => new DarwinLinguaOrganizerPlanSnapshot(
                DarwinLinguaOrganizerPlanKeys.Lite,
                5,
                [
                    DarwinLinguaFeatureKeys.OrganizerDashboard,
                    DarwinLinguaFeatureKeys.OrganizerProfileManagement,
                    DarwinLinguaFeatureKeys.OrganizerEventManagement,
                    DarwinLinguaFeatureKeys.OrganizerRsvpManagement,
                    DarwinLinguaFeatureKeys.OrganizerAnalytics,
                ]),
            _ => new DarwinLinguaOrganizerPlanSnapshot(
                DarwinLinguaOrganizerPlanKeys.Free,
                1,
                [
                    DarwinLinguaFeatureKeys.OrganizerDashboard,
                    DarwinLinguaFeatureKeys.OrganizerProfileManagement,
                    DarwinLinguaFeatureKeys.OrganizerEventManagement,
                ]),
        };
    }
}
