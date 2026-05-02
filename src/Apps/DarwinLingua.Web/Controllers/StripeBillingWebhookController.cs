using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[AllowAnonymous]
[ApiController]
[Route("webhooks/stripe/billing")]
public sealed class StripeBillingWebhookController(
    StripeWebhookVerifier verifier,
    IStripeBillingWebhookHandler handler,
    ILogger<StripeBillingWebhookController> logger) : ControllerBase
{
    [HttpPost]
    [IgnoreAntiforgeryToken]
    [RequestSizeLimit(64 * 1024)]
    public async Task<IActionResult> Post(CancellationToken cancellationToken)
    {
        using StreamReader reader = new(Request.Body);
        string payload = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        string? signatureHeader = Request.Headers["Stripe-Signature"].FirstOrDefault();

        if (!verifier.IsValid(payload, signatureHeader, DateTimeOffset.UtcNow))
        {
            logger.LogWarning("Rejected Stripe billing webhook with an invalid signature.");
            return Unauthorized();
        }

        try
        {
            await handler.HandleAsync(payload, cancellationToken).ConfigureAwait(false);
            return Ok();
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Stripe billing webhook processing failed.");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
