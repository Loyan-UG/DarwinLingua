namespace DarwinDeutsch.Maui.Services.Diagnostics;

/// <summary>
/// Describes the final outcome of one timed runtime operation.
/// </summary>
public enum PerformanceTelemetryOutcome
{
    Success = 0,
    Cancelled = 1,
    Failed = 2,
}
