namespace DarwinLingua.Identity;

public sealed class DarwinLinguaIdentityBootstrapOptions
{
    public bool RequireSeedAccounts { get; init; }

    public string? SeedAdminEmail { get; init; }

    public string? SeedAdminPassword { get; init; }

    public string? SeedLearnerEmail { get; init; }

    public string? SeedLearnerPassword { get; init; }
}
