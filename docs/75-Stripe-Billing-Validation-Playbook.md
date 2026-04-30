# Stripe Billing Validation Playbook

This playbook covers the local and staging validation path for Darwin Lingua Web billing.

It validates:

- Stripe Checkout for the first Premium subscription plan
- signed Stripe webhook delivery
- Premium entitlement activation and removal
- Stripe Customer Portal access
- admin billing diagnostics and manual reconciliation
- billing notification emails through the transactional email pipeline

Do not put live Stripe API keys, webhook secrets, customer ids, subscription ids, or price ids in repository files.

## 1. Prerequisites

- WebApi and Web can both run against the shared development database.
- The Web local settings file is outside source control.
- A Stripe account is available in test mode.
- Stripe CLI is installed and authenticated for the test Stripe account.
- A test recurring Stripe price exists for the Darwin Lingua Premium monthly plan.
- Transactional email is configured in `File` mode locally or Brevo sandbox/test mode in staging.

## 2. Local Configuration

Set these values in local secret storage or `appsettings.Development.Local.json`:

```json
{
  "Billing": {
    "EnableStripe": true,
    "PublicBaseUrl": "http://localhost:5192",
    "StripeApiBaseUrl": "https://api.stripe.com",
    "StripeSecretKey": "sk_test_...",
    "StripeWebhookSecret": "whsec_...",
    "StripePremiumMonthlyPriceId": "price_...",
    "StripeWebhookToleranceMinutes": 5,
    "PremiumPlanKey": "premium-monthly"
  }
}
```

Use the webhook secret printed by the Stripe CLI command in the next section.

## 3. Start Local Services

Start WebApi and Web:

```powershell
dotnet run --project src/Apps/DarwinLingua.WebApi/DarwinLingua.WebApi.csproj --launch-profile DarwinLingua.WebApi
dotnet run --project src/Apps/DarwinLingua.Web/DarwinLingua.Web.csproj --urls http://localhost:5192
```

Confirm:

- `http://localhost:5192/billing` redirects anonymous users to sign-in.
- a signed-in learner can open `/billing`.
- admin can open `/admin/billing-diagnostics`.
- admin can open `/admin/email-diagnostics`.

## 4. Forward Stripe Webhooks Locally

Forward only the events the Web billing integration handles:

```powershell
stripe listen --forward-to localhost:5192/webhooks/stripe/billing --events checkout.session.completed,customer.subscription.created,customer.subscription.updated,customer.subscription.deleted
```

Copy the printed `whsec_...` value into `Billing__StripeWebhookSecret` for the running Web host, then restart Web.

## 5. Validate Checkout

1. Sign in as a learner.
2. Open `/billing`.
3. Click `Continue to Stripe`.
4. Complete Checkout with Stripe test card data.
5. Return to `/billing/success`.
6. Confirm Stripe CLI shows `checkout.session.completed`.
7. Confirm `/billing` shows Premium access immediately or after the webhook arrives.
8. Confirm `/admin/billing-diagnostics` shows a processed Stripe event and a billing profile.
9. Confirm `/admin/email-diagnostics` shows a `Billing.PremiumActivated` delivery attempt.

Expected outcome:

- `UserEntitlementStates.Tier` becomes `Premium`.
- `WebBillingProfiles.ProviderCustomerId` is populated.
- `WebBillingProfiles.ProviderSubscriptionId` is populated.
- Customer Portal button is available on `/billing`.
- if the success-page lookup succeeds first, the webhook may later confirm the same state idempotently.
- repeated checkout, portal, and reconciliation actions are rate-limited with safe messages and without exposing Stripe response bodies.
- Premium is granted only for a completed subscription checkout whose payment status is `paid` or `no_payment_required` and whose customer/subscription ids are present.
- billing notification emails are de-duplicated per scenario, user, subscription, and billing status.

## 6. Validate Customer Portal

Before this step, configure Stripe Customer Portal settings in the Stripe Dashboard for the test account.

1. Sign in as the same paid learner.
2. Open `/billing`.
3. Click `Manage in Stripe`.
4. Confirm the Stripe-hosted portal opens.
5. Return to `/billing`.

Expected outcome:

- portal session opens only for the authenticated account's linked Stripe customer id
- no Stripe secret values are shown in the browser or admin pages

## 7. Validate Subscription Changes

Use Stripe Dashboard or Stripe CLI to move the test subscription through important states.

Minimum states to verify:

- `active` or `trialing`: entitlement remains `Premium`
- `past_due` or `incomplete`: billing notification asks the user to take action
- `unpaid`, `incomplete_expired`, or `canceled`: entitlement becomes `Free`

For each state:

1. Confirm Stripe sends the subscription webhook.
2. Confirm `/admin/billing-diagnostics` shows a processed event.
3. Confirm `/admin/email-diagnostics` shows the expected billing notification.
4. Confirm `/billing` shows the expected entitlement and billing status.

## 8. Validate Manual Reconciliation

Use this only for support recovery when webhook delivery was delayed, missed, or failed.

1. Open `/admin/billing-diagnostics` as an Admin.
2. Enter a known `sub_...` subscription id.
3. Click `Reconcile subscription`.
4. Confirm the admin page reports reconciliation success.
5. Confirm the billing profile and entitlement match the current Stripe subscription.
6. Confirm `/admin/email-diagnostics` shows:
   - user billing status email
   - `Admin.BillingReconciliationCompleted` notification when admin recipients are configured

Expected failure cases:

- malformed ids are rejected before any Stripe API call
- unmapped subscriptions fail closed and do not create an entitlement for an unknown user
- Stripe API failures show a safe admin message and no provider response body to users

## 9. Staging Sign-Off

Before enabling Stripe in production:

- use a staging Stripe test-mode account or restricted test-mode keys
- configure the public staging webhook endpoint `/webhooks/stripe/billing`
- store all secrets in platform secret storage
- run Checkout, webhook, portal, cancellation, payment-action-needed, and reconciliation flows
- verify billing emails in the configured provider dashboard and `admin/email-diagnostics`
- verify `admin/billing-diagnostics` never exposes API keys or webhook secrets
- document the owner for refunds, chargebacks, subscription support, tax settings, and Stripe key rotation

## 10. Production Readiness

Production launch is blocked until:

- Stripe live product and recurring price are created
- `Billing__EnableStripe=true` is set only with live keys and live webhook secret
- Customer Portal production settings are reviewed
- support knows how to use `admin/billing-diagnostics`
- test automation from `docs/71-Web-Test-Backlog.md` is implemented in the separate test workflow
- manual validation has been completed and recorded for the release
