using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DarwinLingua.Web.Services;

public interface IWebPerformanceTelemetryService
{
    void Record(string operationKey, TimeSpan elapsed, WebTelemetryOutcome outcome, int itemCount = 0);

    void RecordClientEvent(string eventName, string pagePath, double? durationMs = null, bool isFailure = false);
}

public enum WebTelemetryOutcome
{
    Success = 0,
    Failure = 1
}

internal sealed class WebPerformanceTelemetryService(ILogger<WebPerformanceTelemetryService> logger) : IWebPerformanceTelemetryService
{
    private const int SummaryInterval = 5;
    private static readonly TimeSpan SlowOperationThreshold = TimeSpan.FromMilliseconds(700);
    private readonly ConcurrentDictionary<string, OperationStats> stats = new(StringComparer.OrdinalIgnoreCase);

    public void Record(string operationKey, TimeSpan elapsed, WebTelemetryOutcome outcome, int itemCount = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationKey);

        OperationStats operationStats = stats.GetOrAdd(operationKey, static _ => new OperationStats());
        Snapshot snapshot = operationStats.Update(elapsed, outcome, itemCount);

        bool shouldEmitSummary =
            snapshot.TotalCount == 1 ||
            elapsed >= SlowOperationThreshold ||
            outcome == WebTelemetryOutcome.Failure ||
            snapshot.TotalCount % SummaryInterval == 0;

        if (!shouldEmitSummary)
        {
            return;
        }

        logger.LogInformation(
            "Web perf [{OperationKey}] count={Count} success={SuccessCount} failed={FailedCount} avgMs={AverageMs} maxMs={MaxMs} lastMs={LastMs} lastItems={LastItemCount}.",
            operationKey,
            snapshot.TotalCount,
            snapshot.SuccessCount,
            snapshot.FailedCount,
            snapshot.AverageElapsedMs,
            snapshot.MaxElapsedMs,
            snapshot.LastElapsedMs,
            snapshot.LastItemCount);
    }

    public void RecordClientEvent(string eventName, string pagePath, double? durationMs = null, bool isFailure = false)
    {
        if (string.IsNullOrWhiteSpace(eventName) || string.IsNullOrWhiteSpace(pagePath))
        {
            return;
        }

        logger.LogInformation(
            "Web UX event {EventName} page={PagePath} durationMs={DurationMs} failed={Failed}.",
            eventName.Trim(),
            pagePath.Trim(),
            durationMs is null ? null : Math.Round(durationMs.Value, 1),
            isFailure);
    }

    private sealed class OperationStats
    {
        private readonly object gate = new();
        private int successCount;
        private int failedCount;
        private double totalElapsedMs;
        private double maxElapsedMs;

        public Snapshot Update(TimeSpan elapsed, WebTelemetryOutcome outcome, int itemCount)
        {
            lock (gate)
            {
                if (outcome == WebTelemetryOutcome.Success)
                {
                    successCount++;
                }
                else
                {
                    failedCount++;
                }

                double elapsedMs = elapsed.TotalMilliseconds;
                totalElapsedMs += elapsedMs;
                maxElapsedMs = Math.Max(maxElapsedMs, elapsedMs);

                int totalCount = successCount + failedCount;
                return new Snapshot(
                    totalCount,
                    successCount,
                    failedCount,
                    Math.Round(totalElapsedMs / totalCount, 1),
                    Math.Round(maxElapsedMs, 1),
                    Math.Round(elapsedMs, 1),
                    itemCount);
            }
        }
    }

    private sealed record Snapshot(
        int TotalCount,
        int SuccessCount,
        int FailedCount,
        double AverageElapsedMs,
        double MaxElapsedMs,
        double LastElapsedMs,
        int LastItemCount);
}
