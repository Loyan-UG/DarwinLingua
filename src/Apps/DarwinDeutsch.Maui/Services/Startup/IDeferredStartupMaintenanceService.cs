namespace DarwinDeutsch.Maui.Services.Startup;

/// <summary>
/// Schedules non-critical startup maintenance after the app shell is already visible.
/// </summary>
public interface IDeferredStartupMaintenanceService
{
    /// <summary>
    /// Schedules one deferred maintenance run for the current app session.
    /// </summary>
    void Schedule();
}
