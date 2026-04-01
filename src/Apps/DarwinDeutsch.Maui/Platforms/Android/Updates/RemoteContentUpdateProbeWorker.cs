using Android.Content;
using AndroidX.Work;
using DarwinDeutsch.Maui.Services.Updates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DarwinDeutsch.Maui.Platforms.Android.Updates;

/// <summary>
/// Performs lightweight manifest checks in the Android background scheduler.
/// </summary>
public sealed class RemoteContentUpdateProbeWorker(Context context, WorkerParameters workerParameters)
    : Worker(context, workerParameters)
{
    /// <summary>
    /// The unique periodic work name used by WorkManager.
    /// </summary>
    public const string UniqueWorkName = "darwinlingua-remote-content-probe";

    private const string LastWorkerProbeAtPreferenceKey = "remote-content-background-worker-last-probe-at-utc";
    private const string LastWorkerPendingPackageIdPreferenceKey = "remote-content-background-worker-last-pending-package-id";
    private const string LastWorkerFailureMessagePreferenceKey = "remote-content-background-worker-last-failure-message";

    /// <inheritdoc />
    public override Result DoWork()
    {
        IServiceProvider? serviceProvider = MainApplication.Services;
        if (serviceProvider is null)
        {
            return Result.InvokeRetry()!;
        }

        ILogger<RemoteContentUpdateProbeWorker> logger =
            serviceProvider.GetRequiredService<ILogger<RemoteContentUpdateProbeWorker>>();

        try
        {
            using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(15));
            IRemoteContentUpdateService remoteContentUpdateService =
                serviceProvider.GetRequiredService<IRemoteContentUpdateService>();

            Context appContext = ApplicationContext!;
            string? filesDirectoryPath = appContext.FilesDir?.AbsolutePath;
            if (string.IsNullOrWhiteSpace(filesDirectoryPath))
            {
                return Result.InvokeRetry()!;
            }

            string databasePath = Path.Combine(filesDirectoryPath, "darwin-lingua.db");
            RemoteContentUpdateStatus status = remoteContentUpdateService
                .GetUpdateStatusAsync(databasePath, cancellationTokenSource.Token)
                .GetAwaiter()
                .GetResult();

            Preferences.Default.Set(LastWorkerProbeAtPreferenceKey, DateTimeOffset.UtcNow.ToString("O"));

            if (status.IsRemoteConfigured && status.IsServerReachable && status.IsUpdateAvailable && !string.IsNullOrWhiteSpace(status.RemotePackageId))
            {
                Preferences.Default.Set(LastWorkerPendingPackageIdPreferenceKey, status.RemotePackageId);
            }
            else
            {
                Preferences.Default.Remove(LastWorkerPendingPackageIdPreferenceKey);
            }

            Preferences.Default.Remove(LastWorkerFailureMessagePreferenceKey);

            logger.LogInformation(
                "Android background remote update probe completed. Reachable={IsServerReachable}, UpdateAvailable={IsUpdateAvailable}, RemotePackageId={RemotePackageId}.",
                status.IsServerReachable,
                status.IsUpdateAvailable,
                status.RemotePackageId);

            return Result.InvokeSuccess()!;
        }
        catch (OperationCanceledException exception)
        {
            Preferences.Default.Set(LastWorkerFailureMessagePreferenceKey, exception.Message);
            return Result.InvokeRetry()!;
        }
        catch (Exception exception)
        {
            Preferences.Default.Set(LastWorkerFailureMessagePreferenceKey, exception.Message);
            logger.LogWarning(exception, "Android background remote update probe failed.");
            return Result.InvokeRetry()!;
        }
    }
}
