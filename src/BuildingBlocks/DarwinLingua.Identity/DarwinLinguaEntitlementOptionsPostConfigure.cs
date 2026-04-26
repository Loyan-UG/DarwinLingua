using Microsoft.Extensions.Options;

namespace DarwinLingua.Identity;

public sealed class DarwinLinguaEntitlementOptionsPostConfigure
    : IPostConfigureOptions<DarwinLinguaEntitlementOptions>
{
    public void PostConfigure(string? name, DarwinLinguaEntitlementOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        string? trialDaysValue = Environment.GetEnvironmentVariable(DarwinLinguaIdentityEnvironmentVariables.NewUserTrialDays);
        if (string.IsNullOrWhiteSpace(trialDaysValue))
        {
            return;
        }

        if (int.TryParse(trialDaysValue, out int trialDays) && trialDays >= 0)
        {
            options.NewUserTrialDays = trialDays;
        }
    }
}
