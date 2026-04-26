using Microsoft.Extensions.Options;

namespace DarwinLingua.Identity;

public sealed class DarwinLinguaIdentityBootstrapOptionsPostConfigure
    : IPostConfigureOptions<DarwinLinguaIdentityBootstrapOptions>
{
    public void PostConfigure(string? name, DarwinLinguaIdentityBootstrapOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        string? requireSeedAccountsValue = Environment.GetEnvironmentVariable(DarwinLinguaIdentityEnvironmentVariables.RequireSeedAccounts);
        if (!string.IsNullOrWhiteSpace(requireSeedAccountsValue) &&
            bool.TryParse(requireSeedAccountsValue, out bool requireSeedAccounts))
        {
            options.RequireSeedAccounts = requireSeedAccounts;
        }

        options.SeedAdminEmail ??= GetTrimmedValue(DarwinLinguaIdentityEnvironmentVariables.SeedAdminEmail);
        options.SeedAdminPassword ??= GetTrimmedValue(DarwinLinguaIdentityEnvironmentVariables.SeedAdminPassword);
        options.SeedLearnerEmail ??= GetTrimmedValue(DarwinLinguaIdentityEnvironmentVariables.SeedLearnerEmail);
        options.SeedLearnerPassword ??= GetTrimmedValue(DarwinLinguaIdentityEnvironmentVariables.SeedLearnerPassword);
    }

    private static string? GetTrimmedValue(string key)
    {
        string? value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
