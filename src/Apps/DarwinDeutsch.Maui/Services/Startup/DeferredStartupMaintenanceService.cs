using System.Diagnostics;
using DarwinDeutsch.Maui.Services.Audio;
using DarwinDeutsch.Maui.Services.Browse;
using DarwinDeutsch.Maui.Services.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DarwinDeutsch.Maui.Services.Startup;

/// <summary>
/// Runs lightweight maintenance after startup so the first shell render stays as light as possible.
/// </summary>
internal sealed class DeferredStartupMaintenanceService : IDeferredStartupMaintenanceService
{
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(1);
    private readonly ISpeechPlaybackService _speechPlaybackService;
    private readonly IBrowseAccelerationService _browseAccelerationService;
    private readonly IPerformanceTelemetryService _performanceTelemetryService;
    private readonly ILogger<DeferredStartupMaintenanceService> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private int _isScheduled;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeferredStartupMaintenanceService"/> class.
    /// </summary>
    public DeferredStartupMaintenanceService(
        ISpeechPlaybackService speechPlaybackService,
        IBrowseAccelerationService browseAccelerationService,
        IPerformanceTelemetryService performanceTelemetryService,
        ILogger<DeferredStartupMaintenanceService> logger)
    {
        ArgumentNullException.ThrowIfNull(speechPlaybackService);
        ArgumentNullException.ThrowIfNull(browseAccelerationService);
        ArgumentNullException.ThrowIfNull(performanceTelemetryService);
        ArgumentNullException.ThrowIfNull(logger);

        _speechPlaybackService = speechPlaybackService;
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
            _performanceTelemetryService.Record(
                "startup.maintenance",
                stopwatch.Elapsed,
                PerformanceTelemetryOutcome.Success);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Deferred startup maintenance failed after {ElapsedMs} ms.", stopwatch.ElapsedMilliseconds);
            _performanceTelemetryService.Record("startup.maintenance", stopwatch.Elapsed, PerformanceTelemetryOutcome.Failed);
        }
        finally
        {
            _browseAccelerationService.ScheduleInitialWarmup();
            _ = WarmUpSpeechAsync();
            _gate.Release();
        }
    }

    private async Task WarmUpSpeechAsync()
    {
        try
        {
            await _speechPlaybackService.WarmUpAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }
}
