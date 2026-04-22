using System.Diagnostics;

namespace DarwinLingua.Web.Services;

internal sealed class WebRequestTelemetryMiddleware(
    RequestDelegate next,
    IWebPerformanceTelemetryService telemetryService)
{
    private static readonly PathString[] IgnoredPrefixes =
    [
        new PathString("/css"),
        new PathString("/js"),
        new PathString("/icons"),
        new PathString("/lib"),
        new PathString("/_framework")
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkip(context.Request.Path))
        {
            await next(context);
            return;
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        WebTelemetryOutcome outcome = WebTelemetryOutcome.Success;

        try
        {
            await next(context);

            if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
            {
                outcome = WebTelemetryOutcome.Failure;
            }
        }
        catch
        {
            outcome = WebTelemetryOutcome.Failure;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            telemetryService.Record(
                BuildOperationKey(context),
                stopwatch.Elapsed,
                outcome);
        }
    }

    private static bool ShouldSkip(PathString path) =>
        string.IsNullOrWhiteSpace(path.Value) || IgnoredPrefixes.Any(path.StartsWithSegments);

    private static string BuildOperationKey(HttpContext context)
    {
        string requestKind = context.Request.Headers.ContainsKey("HX-Request") ? "htmx" : "page";
        string path = context.Request.Path.HasValue ? context.Request.Path.Value! : "/";
        return $"{requestKind}:{context.Request.Method}:{path}".ToLowerInvariant();
    }
}
