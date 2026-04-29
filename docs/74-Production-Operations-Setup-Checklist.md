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
- [ ] Confirm manual provider-event recording works for support reconciliation when a webhook event is missing.
- [ ] Configure `TransactionalEmail__EnableFailureAlerts=true`.
- [ ] Tune `TransactionalEmail__FailureAlertThreshold`, `FailureAlertWindowMinutes`, `FailureAlertCooldownMinutes`, and `FailureAlertMonitorIntervalMinutes`.

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
