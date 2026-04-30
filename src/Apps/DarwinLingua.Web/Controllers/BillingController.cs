using System.Security.Claims;
using DarwinLingua.Identity;
using DarwinLingua.Web.Data;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Controllers;

[Authorize]
[Route("billing")]
public sealed class BillingController(
    WebIdentityDbContext dbContext,
    IUserEntitlementService userEntitlementService,
    IStripeBillingCheckoutService checkoutService,
    IStripeCheckoutFulfillmentService checkoutFulfillmentService,
    IBillingOperationRateLimiter rateLimiter,
    IOptions<BillingOptions> options,
    ILogger<BillingController> logger) : Controller
{
    [HttpGet("", Name = "Billing_Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        string userId = GetUserId();
        UserEntitlementSnapshot entitlement;
        WebBillingProfile? billingProfile;
        BillingOptions billingOptions = options.Value;

        try
        {
            entitlement = await userEntitlementService.GetCurrentAsync(userId, cancellationToken);
            billingProfile = await dbContext.WebBillingProfiles
                .AsNoTracking()
                .SingleOrDefaultAsync(profile => profile.UserId == userId, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Billing profile could not be loaded for user {UserId}.", userId);
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
            {
                Title = "Billing is temporarily unavailable",
                Message = "Your plan information could not be loaded right now. Please try again.",
                RequestId = HttpContext.TraceIdentifier
            });
        }

        return View(new BillingPageViewModel(
            entitlement,
            billingOptions.EnableStripe,
            billingOptions.PremiumPlanKey,
            billingOptions.EnableStripe && !string.Equals(entitlement.Tier, DarwinLinguaEntitlementTiers.Premium, StringComparison.Ordinal),
            billingOptions.EnableStripe && !string.IsNullOrWhiteSpace(billingProfile?.ProviderCustomerId),
            billingProfile?.Status,
            billingProfile?.CurrentPeriodEndsAtUtc));
    }

    [HttpPost("stripe/portal", Name = "Billing_StripePortal")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OpenStripePortal(CancellationToken cancellationToken)
    {
        string userId = GetUserId();
        BillingOptions billingOptions = options.Value;
        if (!billingOptions.EnableStripe)
        {
            TempData["BillingStatus"] = "Stripe billing is not enabled for this environment.";
            return RedirectToAction(nameof(Index));
        }

        if (!rateLimiter.TryConsume("stripe-portal", userId, 10, TimeSpan.FromMinutes(10)))
        {
            TempData["BillingStatus"] = "Too many subscription management attempts. Please wait before trying again.";
            return RedirectToAction(nameof(Index));
        }

        WebBillingProfile? billingProfile;
        try
        {
            billingProfile = await dbContext.WebBillingProfiles
                .AsNoTracking()
                .SingleOrDefaultAsync(profile => profile.UserId == userId, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Stripe customer profile could not be loaded for user {UserId}.", userId);
            TempData["BillingStatus"] = "Subscription management could not be opened. Please try again later.";
            return RedirectToAction(nameof(Index));
        }
        if (string.IsNullOrWhiteSpace(billingProfile?.ProviderCustomerId))
        {
            TempData["BillingStatus"] = "No Stripe customer record is linked to this account yet.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            string returnUrl = $"{ResolvePublicOrigin(billingOptions)}/billing";
            StripeCheckoutSessionResult session = await checkoutService
                .CreateCustomerPortalSessionAsync(billingProfile.ProviderCustomerId, returnUrl, cancellationToken)
                .ConfigureAwait(false);

            return Redirect(session.Url);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Stripe customer portal could not be started for user {UserId}.", userId);
            TempData["BillingStatus"] = "Subscription management could not be opened. Please try again later.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("stripe/checkout", Name = "Billing_StripeCheckout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartStripeCheckout(CancellationToken cancellationToken)
    {
        string userId = GetUserId();
        string? email = WebUserIdentity.TryGetEmail(User);
        BillingOptions billingOptions = options.Value;
        if (!billingOptions.EnableStripe)
        {
            TempData["BillingStatus"] = "Stripe billing is not enabled for this environment.";
            return RedirectToAction(nameof(Index));
        }

        if (!rateLimiter.TryConsume("stripe-checkout", userId, 5, TimeSpan.FromMinutes(10)))
        {
            TempData["BillingStatus"] = "Too many checkout attempts. Please wait before trying again.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            string origin = ResolvePublicOrigin(billingOptions);
            string successUrl = $"{origin}/billing/success?session_id={{CHECKOUT_SESSION_ID}}";
            string cancelUrl = $"{origin}/billing/cancel";
            StripeCheckoutSessionResult session = await checkoutService
                .CreatePremiumCheckoutSessionAsync(userId, email, successUrl, cancelUrl, cancellationToken)
                .ConfigureAwait(false);

            return Redirect(session.Url);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Stripe checkout could not be started for user {UserId}.", userId);
            TempData["BillingStatus"] = "Checkout could not be started. Please try again later.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("success", Name = "Billing_Success")]
    public async Task<IActionResult> Success(
        [FromQuery(Name = "session_id")] string? sessionId,
        CancellationToken cancellationToken)
    {
        BillingOptions billingOptions = options.Value;
        if (!billingOptions.EnableStripe || string.IsNullOrWhiteSpace(sessionId))
        {
            TempData["BillingStatus"] = "Payment completed. Your premium access will update as soon as Stripe confirms the subscription.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            StripeCheckoutFulfillmentResult result = await checkoutFulfillmentService
                .FulfillCheckoutSessionAsync(sessionId, GetUserId(), cancellationToken)
                .ConfigureAwait(false);
            TempData["BillingStatus"] = result.Fulfilled
                ? "Payment completed. Premium access is active."
                : "Checkout returned before Stripe marked the session complete. Your premium access will update as soon as Stripe confirms the subscription.";
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Stripe checkout success fulfillment failed for user {UserId}.", GetUserId());
            TempData["BillingStatus"] = "Payment completed. Your premium access will update as soon as Stripe confirms the subscription.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("cancel", Name = "Billing_Cancel")]
    public IActionResult Cancel()
    {
        TempData["BillingStatus"] = "Checkout was cancelled. No plan change was made.";
        return RedirectToAction(nameof(Index));
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("The authenticated user does not have a stable identifier.");

    private string ResolvePublicOrigin(BillingOptions billingOptions)
    {
        if (!string.IsNullOrWhiteSpace(billingOptions.PublicBaseUrl))
        {
            return billingOptions.PublicBaseUrl.Trim().TrimEnd('/');
        }

        return $"{Request.Scheme}://{Request.Host}".TrimEnd('/');
    }
}
