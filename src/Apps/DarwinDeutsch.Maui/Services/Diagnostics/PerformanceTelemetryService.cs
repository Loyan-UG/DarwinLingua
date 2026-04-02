using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace DarwinDeutsch.Maui.Services.Diagnostics;

/// <summary>
/// Aggregates lightweight runtime timings and emits compact summary logs.
/// </summary>
internal sealed class PerformanceTelemetryService : IPerformanceTelemetryService
{
    private const int SummaryInterval = 5;
    private static readonly TimeSpan SlowOperationThreshold = TimeSpan.FromMilliseconds(700);

    private readonly ILogger<PerformanceTelemetryService> _logger;
    private readonly ConcurrentDictionary<string, OperationStats> _stats = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceTelemetryService"/> class.
    /// </summary>
    public PerformanceTelemetryService(ILogger<PerformanceTelemetryService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <inheritdoc />
    public void Record(string operationKey, TimeSpan elapsed, PerformanceTelemetryOutcome outcome, int itemCount = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationKey);

        OperationStats stats = _stats.GetOrAdd(operationKey, static _ => new OperationStats());
        Snapshot snapshot = stats.Update(elapsed, outcome, itemCount);

        bool shouldEmitSummary =
            snapshot.TotalCount == 1 ||
            elapsed >= SlowOperationThreshold ||
            outcome != PerformanceTelemetryOutcome.Success ||
            snapshot.TotalCount % SummaryInterval == 0;

        if (!shouldEmitSummary)
        {
            return;
        }

        _logger.LogInformation(
            "Perf summary [{OperationKey}] count={Count} success={SuccessCount} cancelled={CancelledCount} failed={FailedCount} avgMs={AverageMs} maxMs={MaxMs} lastMs={LastMs} lastItems={LastItemCount}.",
            operationKey,
            snapshot.TotalCount,
            snapshot.SuccessCount,
            snapshot.CancelledCount,
            snapshot.FailedCount,
            snapshot.AverageElapsedMs,
            snapshot.MaxElapsedMs,
            snapshot.LastElapsedMs,
            snapshot.LastItemCount);
    }

    private sealed class OperationStats
    {
        private readonly object _gate = new();
        private int _successCount;
        private int _cancelledCount;
        private int _failedCount;
        private double _totalElapsedMs;
        private double _maxElapsedMs;

        public Snapshot Update(TimeSpan elapsed, PerformanceTelemetryOutcome outcome, int itemCount)
        {
            lock (_gate)
            {
                switch (outcome)
                {
                    case PerformanceTelemetryOutcome.Success:
                        _successCount++;
                        break;
                    case PerformanceTelemetryOutcome.Cancelled:
                        _cancelledCount++;
                        break;
                    default:
                        _failedCount++;
                        break;
                }

                double elapsedMs = elapsed.TotalMilliseconds;
                _totalElapsedMs += elapsedMs;
                _maxElapsedMs = Math.Max(_maxElapsedMs, elapsedMs);

                int totalCount = _successCount + _cancelledCount + _failedCount;
                return new Snapshot(
                    totalCount,
                    _successCount,
                    _cancelledCount,
                    _failedCount,
                    Math.Round(_totalElapsedMs / totalCount, 1),
                    Math.Round(_maxElapsedMs, 1),
                    Math.Round(elapsedMs, 1),
                    itemCount);
            }
        }
    }

    private sealed record Snapshot(
        int TotalCount,
        int SuccessCount,
        int CancelledCount,
        int FailedCount,
        double AverageElapsedMs,
        double MaxElapsedMs,
        double LastElapsedMs,
        int LastItemCount);
}
