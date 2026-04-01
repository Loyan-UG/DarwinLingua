namespace DarwinDeutsch.Maui.Services.Updates;

/// <summary>
/// Default scheduler used on platforms without native background scheduling support.
/// </summary>
internal sealed class NoOpPlatformBackgroundUpdateScheduler : IPlatformBackgroundUpdateScheduler
{
    /// <inheritdoc />
    public void EnsureScheduled()
    {
    }
}
