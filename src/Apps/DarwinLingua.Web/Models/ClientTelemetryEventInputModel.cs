namespace DarwinLingua.Web.Models;

public sealed class ClientTelemetryEventInputModel
{
    public string EventName { get; init; } = string.Empty;

    public string PagePath { get; init; } = "/";

    public double? DurationMs { get; init; }

    public bool IsFailure { get; init; }
}
