namespace DarwinDeutsch.Maui.Services.Updates;

/// <summary>
/// Schedules lightweight platform-native background update checks when supported.
/// </summary>
public interface IPlatformBackgroundUpdateScheduler
{
    /// <summary>
    /// Ensures the platform-native background job is registered.
    /// </summary>
    void EnsureScheduled();
}
