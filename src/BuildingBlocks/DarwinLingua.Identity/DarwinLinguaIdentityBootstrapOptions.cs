namespace DarwinLingua.Identity;

public sealed class DarwinLinguaIdentityBootstrapOptions
{
    public bool RequireSeedAccounts { get; set; }

    public string? SeedAdminEmail { get; set; }

    public string? SeedAdminPassword { get; set; }

    public string? SeedLearnerEmail { get; set; }

    public string? SeedLearnerPassword { get; set; }
}
