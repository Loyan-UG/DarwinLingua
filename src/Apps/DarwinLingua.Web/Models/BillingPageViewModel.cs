using DarwinLingua.Identity;

namespace DarwinLingua.Web.Models;

public sealed record BillingPageViewModel(
    UserEntitlementSnapshot Entitlement,
    bool StripeEnabled,
    string PlanKey,
    bool CanStartCheckout);
