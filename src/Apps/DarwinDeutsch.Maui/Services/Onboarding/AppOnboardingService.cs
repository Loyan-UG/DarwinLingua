using Microsoft.Maui.Storage;

namespace DarwinDeutsch.Maui.Services.Onboarding;

/// <summary>
/// Stores one-time onboarding state in app preferences.
/// </summary>
internal sealed class AppOnboardingService : IAppOnboardingService
{
    private const string WelcomeCompletedKey = "app.welcome.completed";

    /// <inheritdoc />
    public bool ShouldShowWelcomeExperience()
    {
        return !Preferences.Default.Get(WelcomeCompletedKey, false);
    }

    /// <inheritdoc />
    public void MarkWelcomeExperienceCompleted()
    {
        Preferences.Default.Set(WelcomeCompletedKey, true);
    }
}
