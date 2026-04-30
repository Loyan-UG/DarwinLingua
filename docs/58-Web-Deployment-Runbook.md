# Web Deployment Runbook

## Purpose

This runbook describes the minimum deployment shape for `DarwinLingua.Web`.

It assumes one ASP.NET Core MVC host, one PostgreSQL database for shared catalog data, and a reverse proxy or platform edge terminating HTTPS.

---

## Recommended Hosting Shape

- runtime: `ASP.NET Core`
- app type: server-rendered MVC host with progressive enhancement via `htmx`
- database: `PostgreSQL` for shared catalog/admin data
- HTTPS: required
- static assets: served by the ASP.NET Core host or fronted by the platform edge

---

## Required Configuration

Set these values per environment:

- `ConnectionStrings__SharedCatalog`
- `ConnectionStrings__SharedCatalogAdmin` when admin and learner read paths intentionally diverge
- `ConnectionStrings__WebIdentity`
- `ASPNETCORE_ENVIRONMENT`
- logging overrides as needed for production

For Identity bootstrap and authorization, keep these out of repository-tracked production files:

- `IdentityBootstrap__SeedAdminEmail`
- `IdentityBootstrap__SeedAdminPassword`

Transactional email is release-critical. Set these values per staging and production environment:

- `TransactionalEmail__Mode=BrevoApi`
- `TransactionalEmail__PublicBaseUrl`
- `TransactionalEmail__FromEmail`
- `TransactionalEmail__FromName`
- `TransactionalEmail__ReplyToEmail`
- `TransactionalEmail__SupportEmail`
- `TransactionalEmail__AdminNotificationEmails__0`
- `TransactionalEmail__SmtpHost`
- `TransactionalEmail__SmtpPort`
- `TransactionalEmail__SmtpUseSsl`
- `TransactionalEmail__SmtpUserName`
- `TransactionalEmail__SmtpPassword`
- `TransactionalEmail__BrevoApiBaseUrl=https://api.brevo.com`
- `TransactionalEmail__BrevoApiKey`
- `TransactionalEmail__BrevoWebhookSecret` for every environment that uses `BrevoApi`
- `TransactionalEmail__BrevoSandboxMode=false` for production
- `TransactionalEmail__BrevoAllowQuerySecretFallback=false` outside local diagnostics
- `TransactionalEmail__EmailConfirmationTokenHours`
- `TransactionalEmail__PasswordResetTokenMinutes`
- `TransactionalEmail__EmailChangeTokenMinutes`
- `TransactionalEmail__DeliveryLogRetentionDays`
- `TransactionalEmail__EnableFailureAlerts`
- `TransactionalEmail__FailureAlertThreshold`
- `TransactionalEmail__FailureAlertWindowMinutes`
- `TransactionalEmail__FailureAlertCooldownMinutes`
- `TransactionalEmail__FailureAlertMonitorIntervalMinutes`

Staging and production should use Brevo API mode for transactional email. Staging must use a real Brevo sender domain or subdomain. Production must use the final sender domain with DNS fully verified.

Stripe billing is required before paid premium launch. Set these values per staging and production environment:

- `Billing__EnableStripe`
- `Billing__PublicBaseUrl`
- `Billing__StripeApiBaseUrl=https://api.stripe.com`
- `Billing__StripeSecretKey`
- `Billing__StripeWebhookSecret`
- `Billing__StripePremiumMonthlyPriceId`
- `Billing__StripeWebhookToleranceMinutes`
- `Billing__PremiumPlanKey`

Environment rule:

- local development may use local appsettings overrides or user secrets
- shared development/staging/production must use platform environment variables or secret storage
- do not commit live PostgreSQL credentials or seed-admin credentials into `appsettings.json`
- do not commit live SMTP credentials, Brevo API keys, Stripe API keys, webhook secrets, or live price ids into `appsettings*.json`

---

## Transactional Email DNS And Sender Identity

Before staging or production sign-off, verify:

- SPF includes the selected email provider for the sender domain.
- DKIM is enabled and the provider's DKIM records are published.
- DMARC exists for the sender domain and is at least in monitoring mode before public launch.
- Bounce or return-path handling is configured according to the provider's requirements.
- `FromEmail` uses the verified sender domain.
- `ReplyToEmail` reaches a monitored support inbox.
- `SupportEmail` matches user-facing support copy and is monitored.
- `PublicBaseUrl` is the external HTTPS origin users will open from email links.
- Provider rate limits are known for registration, password reset, resend confirmation, organizer claim, RSVP, moderation, and admin notification flows.
- Provider pricing assumptions are recorded by the release owner before production launch.
- A data processing agreement or equivalent provider processing review is completed before production launch.

---

## Deployment Steps

1. Build the solution on the intended release commit.
2. Run `dotnet test DarwinLingua.slnx -c Debug -m:1` or the release-appropriate equivalent in CI.
3. Publish `src/Apps/DarwinLingua.Web`.
4. Provide production connection strings and Identity bootstrap secrets through the deployment platform.
5. Provide transactional email, billing, and provider webhook secrets through the deployment platform.
6. Start the host and verify it completes startup initialization.
7. Verify learner routes and admin routes respond successfully.
8. Verify registration and password-reset emails in the configured provider inbox.
9. Verify Stripe Checkout and Stripe webhook delivery in staging when `Billing__EnableStripe=true`.
10. Run the learner-web, auth, transactional-email, billing, and PWA validation worksheets.

---

## Post-Deploy Checks

- verify `Home`, `Browse`, `Search`, `Favorites`, `Recent`, and `Settings`
- verify admin routes render with the admin shell
- verify recent activity and word-state interactions persist
- verify CSP/security headers are present
- verify service worker registration and manifest delivery
- verify `admin/email-diagnostics` shows recent delivery attempts
- verify no email delivery log stores raw reset/confirmation tokens or full recovery URLs
- verify a failed email delivery is visible to admins/operators without sensitive token data
- verify email delivery-log cleanup is Admin-only and writes an application log entry
- verify repeated delivery failures trigger an admin alert after the configured threshold and cooldown
- verify Brevo transactional webhooks use Bearer auth or `X-DarwinLingua-Brevo-Webhook-Secret`, not a production query-string secret
- verify `/billing` renders for authenticated users and redirects anonymous users to sign-in
- verify Stripe Checkout session creation redirects to Stripe when billing is enabled
- verify Stripe billing webhooks reject invalid signatures
- verify verified Stripe checkout/subscription events update Premium entitlements
- verify cancelled, unpaid, or incomplete-expired Stripe subscription states remove Premium access

---

## Rollback Rule

Rollback the web deployment when:

- the host fails startup
- browse/search/detail flows return errors
- admin shell routes are broken
- CSP/security headers block first-party functionality unexpectedly
- the deployed build serves stale or invalid content against the intended shared database
