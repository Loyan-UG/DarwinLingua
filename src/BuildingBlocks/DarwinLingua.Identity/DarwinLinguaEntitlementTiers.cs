namespace DarwinLingua.Identity;

public static class DarwinLinguaEntitlementTiers
{
    public const string Free = "Free";
    public const string Trial = "Trial";
    public const string Premium = "Premium";

    public static IReadOnlyList<string> All { get; } = [Free, Trial, Premium];
}
