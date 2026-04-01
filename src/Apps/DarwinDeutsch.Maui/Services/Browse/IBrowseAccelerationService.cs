namespace DarwinDeutsch.Maui.Services.Browse;

/// <summary>
/// Coordinates browse warm-up and cache invalidation across learner-facing sections.
/// </summary>
public interface IBrowseAccelerationService
{
    /// <summary>
    /// Schedules a deferred startup warm-up for key browse surfaces.
    /// </summary>
    void ScheduleInitialWarmup();

    /// <summary>
    /// Clears all browse caches after local content changes.
    /// </summary>
    void ResetCaches();
}
