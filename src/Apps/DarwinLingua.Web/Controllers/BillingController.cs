using System.Security.Claims;
using DarwinLingua.Identity;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Controllers;

[Authorize]
[Route("billing")]
public sealed class BillingController(
    IUserEntitlementService userEntitlementService,
    IStripeBillingCheckoutService checkoutService,
    IOptions<BillingOptions> options,
    ILogger<BillingController> logger) : Controller
{
    [HttpGet("", Name = "Billing_Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        string userId = GetUserId();
        UserEntitlementSnapshot entitlement = await userEntitlementService.GetCurrentAsync(userId, cancellationToken);
        BillingOptions billingOptions = options.Value;

        return View(new BillingPageViewModel(
            entitlement,
            billingOptions.EnableStripe,
            billingOptions.PremiumPlanKey,
            billingOptions.EnableStripe && !string.Equals(entitlement.Tier, DarwinLinguaEntitlementTiers.Premium, StringComparison.Ordinal)));
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
    public IActionResult Success()
    {
        TempData["BillingStatus"] = "Payment completed. Your premium access will update as soon as Stripe confirms the subscription.";
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
