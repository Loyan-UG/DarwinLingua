namespace DarwinDeutsch.Maui.Services.Updates;

/// <summary>
/// Performs deferred remote-update checks after the app is already responsive.
/// </summary>
public interface IBackgroundRemoteUpdateCoordinator
{
    /// <summary>
    /// Schedules the first deferred update check for a foreground window.
    /// </summary>
    /// <param name="window">The active application window.</param>
    void ScheduleInitialCheck(Window window);

    /// <summary>
    /// Schedules a throttled update check after the app resumes.
    /// </summary>
    /// <param name="window">The active application window.</param>
    void ScheduleResumeCheck(Window window);
}
