using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("telemetry")]
public sealed class TelemetryController(IWebPerformanceTelemetryService telemetryService) : Controller
{
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

        string pagePath = string.IsNullOrWhiteSpace(input.PagePath) ? "/" : input.PagePath.Trim();
        telemetryService.RecordClientEvent(input.EventName, pagePath, input.DurationMs, input.IsFailure);
        return Ok();
    }
}
