using System.Diagnostics;
using DarwinDeutsch.Maui.Services.Browse;
using DarwinDeutsch.Maui.Services.Diagnostics;
using DarwinDeutsch.Maui.Services.Storage;
using Microsoft.Extensions.Logging;

namespace DarwinDeutsch.Maui.Services.Startup;

/// <summary>
/// Runs packaged-seed maintenance after startup so the first shell render stays as light as possible.
/// </summary>
internal sealed class DeferredStartupMaintenanceService : IDeferredStartupMaintenanceService
{
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(1);
    private readonly ISeedDatabaseProvisioningService _seedDatabaseProvisioningService;
    private readonly IBrowseAccelerationService _browseAccelerationService;
    private readonly IPerformanceTelemetryService _performanceTelemetryService;
    private readonly ILogger<DeferredStartupMaintenanceService> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private int _isScheduled;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeferredStartupMaintenanceService"/> class.
    /// </summary>
    public DeferredStartupMaintenanceService(
        ISeedDatabaseProvisioningService seedDatabaseProvisioningService,
        IBrowseAccelerationService browseAccelerationService,
        IPerformanceTelemetryService performanceTelemetryService,
        ILogger<DeferredStartupMaintenanceService> logger)
    {
        ArgumentNullException.ThrowIfNull(seedDatabaseProvisioningService);
        ArgumentNullException.ThrowIfNull(browseAccelerationService);
        ArgumentNullException.ThrowIfNull(performanceTelemetryService);
        ArgumentNullException.ThrowIfNull(logger);

        _seedDatabaseProvisioningService = seedDatabaseProvisioningService;
        _browseAccelerationService = browseAccelerationService;
        _performanceTelemetryService = performanceTelemetryService;
        _logger = logger;
    }

    /// <inheritdoc />
    public void Schedule()
    {
        if (Interlocked.Exchange(ref _isScheduled, 1) == 1)
        {
            return;
        }

        _ = RunAsync();
    }

    private async Task RunAsync()
    {
        if (!await _gate.WaitAsync(0).ConfigureAwait(false))
        {
            return;
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            await Task.Delay(InitialDelay).ConfigureAwait(false);

            string databasePath = Path.Combine(FileSystem.Current.AppDataDirectory, "darwin-lingua.db");
            SeedDatabaseUpdateResult seedUpdateResult = await _seedDatabaseProvisioningService
                .ApplySeedUpdateAsync(databasePath, CancellationToken.None)
                .ConfigureAwait(false);

            if (!seedUpdateResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Deferred startup seed maintenance failed after {ElapsedMs} ms: {ErrorMessage}",
                    stopwatch.ElapsedMilliseconds,
                    seedUpdateResult.ErrorMessage ?? "Unknown error");
                _performanceTelemetryService.Record("startup.maintenance", stopwatch.Elapsed, PerformanceTelemetryOutcome.Failed);
                return;
            }

            if (seedUpdateResult.AppliedChanges)
            {
                _browseAccelerationService.ResetCaches();
                _logger.LogInformation(
                    "Deferred startup seed maintenance applied {PackageCount} packages / {WordCount} words in {ElapsedMs} ms.",
                    seedUpdateResult.ImportedPackages,
                    seedUpdateResult.ImportedWords,
                    stopwatch.ElapsedMilliseconds);
            }
            _performanceTelemetryService.Record(
                "startup.maintenance",
                stopwatch.Elapsed,
                PerformanceTelemetryOutcome.Success,
                seedUpdateResult.ImportedWords);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Deferred startup maintenance failed after {ElapsedMs} ms.", stopwatch.ElapsedMilliseconds);
            _performanceTelemetryService.Record("startup.maintenance", stopwatch.Elapsed, PerformanceTelemetryOutcome.Failed);
        }
        finally
        {
            _browseAccelerationService.ScheduleInitialWarmup();
            _gate.Release();
        }
    }
}
