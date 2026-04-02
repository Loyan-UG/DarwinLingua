namespace DarwinDeutsch.Maui.Services.Diagnostics;

/// <summary>
/// Records lightweight in-memory timing summaries for key runtime flows.
/// </summary>
public interface IPerformanceTelemetryService
{
    /// <summary>
    /// Records one completed runtime operation.
    /// </summary>
    void Record(string operationKey, TimeSpan elapsed, PerformanceTelemetryOutcome outcome, int itemCount = 0);
}
