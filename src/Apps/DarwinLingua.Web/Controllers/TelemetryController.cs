using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("telemetry")]
public sealed class TelemetryController(IWebPerformanceTelemetryService telemetryService) : Controller
{
    private const int MaxPagePathLength = 256;
    private const double MaxDurationMs = 300_000;

    private static readonly HashSet<string> AllowedEvents =
    [
        "page.load.slow",
        "install.prompt.available",
        "install.completed",
        "htmx.response.error",
        "htmx.network.error"
    ];

    [HttpPost("client-event", Name = "Telemetry_ClientEvent")]
    [IgnoreAntiforgeryToken]
    public IActionResult ClientEvent([FromBody] ClientTelemetryEventInputModel input)
    {
        if (input is null || string.IsNullOrWhiteSpace(input.EventName) || !AllowedEvents.Contains(input.EventName))
        {
            return BadRequest();
        }

        double? durationMs = NormalizeDuration(input.DurationMs);
        if (input.DurationMs.HasValue && !durationMs.HasValue)
        {
            return BadRequest();
        }

        string pagePath = NormalizePagePath(input.PagePath);
        telemetryService.RecordClientEvent(input.EventName, pagePath, durationMs, input.IsFailure);
        return Ok();
    }

    private static double? NormalizeDuration(double? durationMs)
    {
        if (!durationMs.HasValue)
        {
            return null;
        }

        if (!double.IsFinite(durationMs.Value) || durationMs.Value < 0 || durationMs.Value > MaxDurationMs)
        {
            return null;
        }

        return durationMs.Value;
    }

    private static string NormalizePagePath(string? pagePath)
    {
        if (string.IsNullOrWhiteSpace(pagePath))
        {
            return "/";
        }

        string trimmed = pagePath.Trim();
        Span<char> buffer = stackalloc char[Math.Min(trimmed.Length, MaxPagePathLength)];
        int length = 0;
        foreach (char character in trimmed)
        {
            if (length >= buffer.Length)
            {
                break;
            }

            if (!char.IsControl(character))
            {
                buffer[length++] = character;
            }
        }

        return length == 0 ? "/" : new string(buffer[..length]);
    }
}
