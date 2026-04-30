# Production Operations Setup Checklist

This checklist covers the server-side setup tasks required before running Darwin Lingua in staging or production.

## Secrets and Configuration

- [ ] Store database connection strings in platform secret storage, not repository files.
- [ ] Set `ConnectionStrings__Identity` for the application database user.
- [ ] Set `ConnectionStrings__IdentityAdmin` only where bootstrap/admin schema operations are expected.
- [ ] Set `WebApi__BaseUrl` to the internal or public WebApi origin used by the web host.
- [ ] Set `WebApi__IgnoreSslErrors=false` outside local development.
- [ ] Set `IdentityBootstrap__RequireSeedAccounts` according to the environment.
- [ ] Set seed account emails/passwords only through secret storage when seed accounts are enabled.

## Transactional Email With Brevo

- [ ] Create or select the Brevo account for Darwin Lingua transactional email.
- [ ] Start with the Brevo free plan for initial staging/low-volume use.
- [ ] Review Brevo sending limits before public launch and upgrade plan before user growth exceeds the free-plan envelope.
- [ ] Verify the sender domain or sender identity in Brevo.
- [ ] Configure SPF, DKIM, and DMARC DNS records for the sender domain.
- [ ] Generate a Brevo API key with transactional email sending permission.
- [ ] Store the API key as `TransactionalEmail__BrevoApiKey`.
- [ ] Set `TransactionalEmail__Mode=BrevoApi` in staging and production.
- [ ] Set `TransactionalEmail__BrevoApiBaseUrl=https://api.brevo.com`.
- [ ] Use `TransactionalEmail__BrevoSandboxMode=true` only for integration validation where no email should be sent.
- [ ] Set `TransactionalEmail__BrevoSandboxMode=false` before real staging delivery tests and production.
- [ ] Set `TransactionalEmail__FromEmail` to a verified Brevo sender address.
- [ ] Set `TransactionalEmail__FromName`.
- [ ] Set `TransactionalEmail__ReplyToEmail`.
- [ ] Set `TransactionalEmail__SupportEmail`.
- [ ] Set at least one `TransactionalEmail__AdminNotificationEmails__0` recipient.
- [ ] Generate a strong random `TransactionalEmail__BrevoWebhookSecret`.
- [ ] Store the webhook secret in platform secret storage.
- [ ] Confirm every environment using `TransactionalEmail__Mode=BrevoApi` has a non-empty webhook secret.
- [ ] Keep `TransactionalEmail__BrevoAllowQuerySecretFallback=false` outside local/manual diagnostics.
- [ ] Configure a Brevo transactional webhook to call `/webhooks/brevo/transactional-email`.
- [ ] Configure Brevo webhook security with Bearer auth and set the bearer token to `TransactionalEmail__BrevoWebhookSecret`.
- [ ] If Bearer auth is not available, configure a custom header `X-DarwinLingua-Brevo-Webhook-Secret` with the same secret.
- [ ] Avoid query-string webhook secrets except for short local/manual diagnostics because URLs can be logged by infrastructure.
- [ ] Enable Brevo webhook events for delivered, hard bounce, soft bounce, blocked, invalid, error, spam, opened, and clicked where supported.
- [ ] Confirm webhook calls reach the public HTTPS origin.
- [ ] Confirm `admin/email-diagnostics` shows provider message ids and provider events.
- [ ] Confirm failed Brevo delivery events update delivery logs without storing email tokens or recovery URLs.
- [ ] Confirm permanent Brevo failures create internal hashed suppressions and later sends are logged as `Suppressed`.
- [ ] Confirm admins can filter suppressions by hash/reason and manually unsuppress only after support review.
- [ ] Confirm manual suppression changes are Admin-only and appear in application logs.
- [ ] Confirm manual provider-event recording works for support reconciliation when a webhook event is missing.
- [ ] Confirm manual provider-event recording is Admin-only and appears in application logs.
- [ ] Confirm delivery-log cleanup is Admin-only and appears in application logs.
- [ ] Configure `TransactionalEmail__EnableFailureAlerts=true`.
- [ ] Tune `TransactionalEmail__FailureAlertThreshold`, `FailureAlertWindowMinutes`, `FailureAlertCooldownMinutes`, and `FailureAlertMonitorIntervalMinutes`.

## Billing With Stripe

- [ ] Create or select the Stripe account for Darwin Lingua billing.
- [ ] Start with Stripe test mode for staging and pre-launch validation.
- [ ] Create the first recurring premium monthly product and price in Stripe.
- [ ] Store the recurring price id as `Billing__StripePremiumMonthlyPriceId`.
- [ ] Store the Stripe secret key as `Billing__StripeSecretKey`.
- [ ] Generate a Stripe webhook signing secret for the Web billing endpoint.
- [ ] Store the webhook signing secret as `Billing__StripeWebhookSecret`.
- [ ] Set `Billing__EnableStripe=true` only after all required Stripe secrets and price ids are configured.
- [ ] Set `Billing__PublicBaseUrl` to the public HTTPS web origin used in Stripe Checkout success/cancel URLs.
- [ ] Keep `Billing__StripeApiBaseUrl=https://api.stripe.com` unless using a controlled test proxy.
- [ ] Keep `Billing__StripeWebhookToleranceMinutes` within the accepted operational window.
- [ ] Configure a Stripe webhook endpoint for `/webhooks/stripe/billing`.
- [ ] Enable Stripe webhook events: `checkout.session.completed`, `customer.subscription.created`, `customer.subscription.updated`, and `customer.subscription.deleted`.
- [ ] Confirm webhook delivery uses HTTPS and the configured Stripe signing secret.
- [ ] Confirm `WebBillingProfiles` and `WebBillingEvents` are created during Web bootstrap.
- [ ] Confirm Stripe Checkout redirects a signed-in learner to Stripe and returns to `/billing/success` or `/billing/cancel`.
- [ ] Confirm Stripe webhooks grant Premium only after a verified Stripe event.
- [ ] Confirm subscription cancellation, unpaid, or incomplete-expired states downgrade the entitlement to Free.
- [ ] Assign an owner for Stripe account access, API key rotation, tax settings, invoices, refunds, chargebacks, and plan upgrades.

## Public URLs and Account Links

- [ ] Set `TransactionalEmail__PublicBaseUrl` to the public HTTPS web origin.
- [ ] Verify confirmation links use `TransactionalEmail__PublicBaseUrl`.
- [ ] Verify password reset links use `TransactionalEmail__PublicBaseUrl`.
- [ ] Verify email-change links use `TransactionalEmail__PublicBaseUrl`.

## Database Bootstrap

- [ ] Run WebApi once with the shared server database reachable.
- [ ] Run Web once with the shared server database reachable.
- [ ] Confirm Identity tables exist.
- [ ] Confirm web user-state tables exist.
- [ ] Confirm `WebEmailDeliveryLogs` exists.
- [ ] Confirm `WebEmailDeliveryLogs` has Brevo event columns: `ProviderLastEvent`, `ProviderLastEventAtUtc`, `ProviderLastEventReason`.

## Validation

- [ ] Register a test learner and confirm the email is received through Brevo.
- [ ] Confirm the registration email link works.
- [ ] Request a password reset and confirm the email is received through Brevo.
- [ ] Complete password reset and confirm the success notification is received.
- [ ] Change account email and confirm both new-email confirmation and old-email notification are sent.
- [ ] Force one delivery failure in staging and confirm the failed event is visible in `admin/email-diagnostics`.
- [ ] Confirm repeated failures trigger an admin alert.
- [ ] Check Brevo dashboard logs for message id, delivery state, bounce/error details, and webhook status.

## Operational Ownership

- [ ] Assign an owner for Brevo account billing and plan upgrades.
- [ ] Assign an owner for DNS records and sender-domain verification.
- [ ] Review the Brevo data processing agreement before production launch.
- [ ] Document the incident process for API key rotation, sender-domain failure, and account suspension.
