using Android.App;
using AndroidX.Work;
using Microsoft.Extensions.Logging;
using JavaClass = Java.Lang.Class;
using JavaTimeUnit = Java.Util.Concurrent.TimeUnit;

namespace DarwinDeutsch.Maui.Platforms.Android.Updates;

/// <summary>
/// Registers the periodic Android WorkManager probe used for remote update checks.
/// </summary>
internal sealed class AndroidBackgroundUpdateScheduler(ILogger<AndroidBackgroundUpdateScheduler> logger)
    : Services.Updates.IPlatformBackgroundUpdateScheduler
{
    private const int ProbeIntervalHours = 6;
    private const int BackoffDelayMinutes = 30;

    /// <inheritdoc />
    public void EnsureScheduled()
    {
        try
        {
            Constraints constraints = new Constraints.Builder()
                .SetRequiredNetworkType(NetworkType.Unmetered!)
                .SetRequiresBatteryNotLow(true)
                .Build();

            PeriodicWorkRequest request = new PeriodicWorkRequest.Builder(
                    JavaClass.FromType(typeof(RemoteContentUpdateProbeWorker)),
                    ProbeIntervalHours,
                    JavaTimeUnit.Hours!)
                .SetBackoffCriteria(BackoffPolicy.Linear!, BackoffDelayMinutes, JavaTimeUnit.Minutes!)
                .SetConstraints(constraints)
                .Build();

            WorkManager
                .GetInstance(global::Android.App.Application.Context)
                .EnqueueUniquePeriodicWork(
                    RemoteContentUpdateProbeWorker.UniqueWorkName,
                    ExistingPeriodicWorkPolicy.Update!,
                    request);

            logger.LogInformation(
                "Scheduled Android background remote update probe via WorkManager every {IntervalHours} hours with unmetered-network and battery-not-low constraints.",
                ProbeIntervalHours);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to schedule Android background remote update probe.");
        }
    }
}
