namespace DarwinDeutsch.Maui.Services.Onboarding;

/// <summary>
/// Tracks whether the one-time mobile welcome flow should be shown.
/// </summary>
public interface IAppOnboardingService
{
    /// <summary>
    /// Returns a value indicating whether the welcome flow should be shown.
    /// </summary>
    bool ShouldShowWelcomeExperience();

    /// <summary>
    /// Marks the welcome flow as completed for this app installation.
    /// </summary>
    void MarkWelcomeExperienceCompleted();
}
