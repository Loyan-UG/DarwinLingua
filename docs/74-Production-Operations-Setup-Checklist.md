# Production Operations Setup Checklist

This checklist covers the server-side setup tasks required before running Darwin Lingua in staging or production.

Current scope note: the active Web testing phase uses the primary product domain `https://darwinlingua.com` and the API domain `https://api.darwinlingua.com`, while public paid billing remains disabled. For the next controlled tester pass, the required external setup is Brevo transactional email, Cloudflare routing, database/runtime secrets, and legal/operator ownership. Stripe tasks in this document remain required before self-service paid subscriptions are exposed, but they are not blockers for a no-billing controlled Web test.

## Secrets and Configuration

- [x] Store database connection strings in platform secret storage, not repository files.
  - Evidence: local development connection strings are in `appsettings.Development.Local.json`/secret bundle outside Git; `tools/Web/Invoke-WebOperationsBootstrapCheck.ps1` verifies presence without printing values.
- [x] Set `ConnectionStrings__Identity` for the application database user.
- [x] Set `ConnectionStrings__IdentityAdmin` only where bootstrap/admin schema operations are expected.
- [x] Set `WebApi__BaseUrl` to the internal or public WebApi origin used by the web host.
- [x] Set `WebApi__IgnoreSslErrors=false` outside local development.
- [x] Set `IdentityBootstrap__RequireSeedAccounts` according to the environment.
- [x] Set seed account emails/passwords only through secret storage when seed accounts are enabled.
  - Evidence: 2026-06-23 `artifacts/validation/web-operations-bootstrap/web-operations-bootstrap-20260623-075413.md` passed `identityConnectionConfigured`, `identityAdminConnectionConfigured`, `webApiBaseUrlConfigured`, `webApiIgnoreSslErrorsFalse`, `identityBootstrapConfigured`, and `seedCredentialsConfiguredWhenRequired`.

Local development helper:

- Use `scripts/Set-Web-LocalSecrets.ps1 -ConfigureBrevo` to store Brevo settings in ASP.NET Core user-secrets without writing API keys to repository files.
- Use `scripts/Set-Web-LocalSecrets.ps1 -ConfigureStripe` to store Stripe test-mode settings in ASP.NET Core user-secrets without writing API keys or webhook secrets to repository files.
- The helper writes directly to the local ASP.NET Core user-secrets JSON file for the Web project, so secret values are not passed to `dotnet` as command-line arguments.
- Optional non-interactive local setup can read user/process environment variables with `-UseEnvironment`: `DARWINLINGUA_BREVO_API_KEY`, `DARWINLINGUA_BREVO_WEBHOOK_SECRET`, `DARWINLINGUA_STRIPE_SECRET_KEY`, `DARWINLINGUA_STRIPE_WEBHOOK_SECRET`, and `DARWINLINGUA_STRIPE_PRICE_ID`.
- Set the local public origin with `-PublicBaseUrl`, for example `scripts/Set-Web-LocalSecrets.ps1 -ConfigureBrevo -PublicBaseUrl http://localhost:5192`.
- Keep `-BrevoSandboxMode $true` until you intentionally want Brevo to deliver real messages from local development.
- `-BrevoAllowQuerySecretFallback` defaults to `$false`; enable it only for a short local/manual webhook diagnostic when Bearer or custom-header auth cannot be used.
- Restart `DarwinLingua.Web` after changing user-secrets; options are read at host startup.
- Keep real API keys out of chat logs, screenshots, terminal transcripts, and checked-in `appsettings*.json` files.

## Transactional Email With Brevo

- [x] Create or select the Brevo account for Darwin Lingua transactional email.
- [ ] Start with the Brevo free plan for initial staging/low-volume use.
- [ ] Review Brevo sending limits before public launch and upgrade plan before user growth exceeds the free-plan envelope.
- [x] Verify the sender domain or sender identity in Brevo.
- [x] Configure SPF, DKIM, and DMARC DNS records for the sender domain.
- [x] Generate a Brevo API key with transactional email sending permission.
- [x] Store the API key as `TransactionalEmail__BrevoApiKey`.
- [x] Set `TransactionalEmail__Mode=BrevoApi` in staging and production.
- [x] Set `TransactionalEmail__BrevoApiBaseUrl=https://api.brevo.com`.
- [ ] Use `TransactionalEmail__BrevoSandboxMode=true` only for integration validation where no email should be sent.
- [x] Set `TransactionalEmail__BrevoSandboxMode=false` before real staging delivery tests and production.
- [x] Set `TransactionalEmail__FromEmail` to a verified Brevo sender address, preferably `no-reply@darwinlingua.com`.
- [x] Set `TransactionalEmail__FromName`.
- [x] Set `TransactionalEmail__ReplyToEmail=support@darwinlingua.com`.
- [x] Set `TransactionalEmail__SupportEmail=support@darwinlingua.com`.
- [x] Set at least one `TransactionalEmail__AdminNotificationEmails__0` recipient.
  - Evidence: 2026-06-23 `web-operations-bootstrap-20260623-075413.md` reports `Admin notification recipients: 1` and `transactionalAdminRecipientConfigured=True` without printing the configured address.
- [x] Generate a strong random `TransactionalEmail__BrevoWebhookSecret`.
- [x] Store the webhook secret in platform secret storage.
- [x] Confirm every environment using `TransactionalEmail__Mode=BrevoApi` has a non-empty webhook secret.
- [x] Keep `TransactionalEmail__BrevoAllowQuerySecretFallback=false` outside local/manual diagnostics.
- [x] Configure a Brevo transactional webhook to call `https://darwinlingua.com/webhooks/brevo/transactional-email`.
- [x] Configure Brevo webhook security with `Token` authentication and set the token to `TransactionalEmail__BrevoWebhookSecret`.
- [x] If Bearer auth is not available, configure a custom header `X-DarwinLingua-Brevo-Webhook-Secret` with the same secret.
- [x] Avoid query-string webhook secrets except for short local/manual diagnostics because URLs can be logged by infrastructure.
- [x] Set the Brevo webhook event category to `Transactional email` and enable the official transactional events exposed by the current Brevo UI: request/sent, delivered, deferred, hard bounce, soft bounce, blocked, invalid/invalid email, spam, opened, unique opened, click/clicked, and unsubscribed. Brevo's API lists `sent` or `request`, `delivered`, `hardBounce`, `softBounce`, `blocked`, `spam`, `invalid`, `deferred`, `click`, `opened`, `uniqueOpened`, and `unsubscribed`; if provider logs later send `error`, `proxyOpen`, `uniqueProxyOpen`, complaint-like reasons, or equivalent failure labels, Darwin Lingua normalizes them for diagnostics/suppression.
- [x] Add the current server/operator IP in Brevo Authorized IPs at `https://app.brevo.com/security/authorised_ips`.
- [x] Rerun `Invoke-BrevoProductionReadinessCheck.ps1` with `-VerifyBrevoApi` and confirm `brevo.accountApi` passes before real inbox/webhook smoke.
- [x] Run `tools/Web/Invoke-BrevoRealDeliverySmoke.ps1 -RecipientEmail "info@darwinlingua.com" -SenderVerified -DnsAuthenticated -WebhookConfigured -DpaAccepted -ConfirmSend` and archive the generated `artifacts/validation/brevo-real-delivery-smoke/` report.
- [x] Run `tools/Web/Invoke-WebAccountEmailFlowSmoke.ps1` against `https://darwinlingua.com` and archive the generated `artifacts/validation/web-account-email-smoke/` report.
- [ ] Manually confirm the two smoke messages reached `info@darwinlingua.com` and render correctly in the actual mailbox.
- [x] Confirm webhook calls reach the public HTTPS origin.
  - Evidence: `tools/Web/Invoke-BrevoWebhookSuppressionSmoke.ps1` posts a controlled `hardBounce` event to `https://darwinlingua.com/webhooks/brevo/transactional-email` with Bearer token authentication and writes evidence under `artifacts/validation/brevo-webhook-suppression-smoke/`. The 2026-06-23 run returned HTTP 200.
- [x] Confirm `WebEmailDeliveryLogs` shows provider message ids for app-level registration and password-reset sends.
- [x] Confirm `admin/email-diagnostics` shows provider message ids and provider events in the operator UI.
  - Evidence: `tools/Web/Invoke-WebEmailDiagnosticsAdminSmoke.ps1 -UseLocalDevelopmentSeed` signs in as the local admin seed, opens `/admin/email-diagnostics` with filters for a real Brevo provider message id, provider event, and suppression hash/reason, and verifies the UI shows Brevo readiness, provider message id, provider event, suppression data, and Admin-only controls. The 2026-06-23 run passed and wrote evidence under `artifacts/validation/web-email-diagnostics-admin-smoke/`.
- [x] Confirm failed Brevo delivery events update delivery logs without storing email tokens or recovery URLs.
  - Evidence: `tools/Web/Invoke-BrevoWebhookSuppressionSmoke.ps1` verifies the synthetic delivery row becomes `Failed` with `ProviderLastEvent=hard_bounce` and `FailureCode=brevo:hard_bounce`. `DeliveryLogRepository_ShouldStoreDiagnosticsWithoutEmailBodyOrRecoveryUrl` verifies diagnostic text redacts recovery URLs/tokens before persistence.
- [x] Confirm permanent Brevo failures create internal hashed suppressions.
  - Evidence: `tools/Web/Invoke-BrevoWebhookSuppressionSmoke.ps1` verifies a `WebEmailSuppressions` row with `Reason=brevo:hard_bounce` and provider `brevo-api` is created for the synthetic recipient hash.
- [x] Confirm later sends to a suppressed recipient are logged as `Suppressed`.
  - Evidence: `tools/Web/Invoke-BrevoSuppressedSendSmoke.ps1` creates a temporary suppression for a confirmed account, submits `/Identity/Account/ForgotPassword`, verifies a `WebEmailDeliveryLogs` row with `Status=Suppressed`, `FailureCode=recipient-suppressed`, no provider message id, and then removes the temporary suppression. The 2026-06-23 run passed and wrote evidence under `artifacts/validation/brevo-suppressed-send-smoke/`.
- [x] Confirm admins can filter suppressions by hash/reason and manually unsuppress only after support review.
  - Evidence: `tools/Web/Invoke-WebEmailDiagnosticsAdminActionsSmoke.ps1 -UseLocalDevelopmentSeed` inserts a temporary suppression, opens `/admin/email-diagnostics` with a suppression hash filter, posts the Admin-only unsuppress form, and verifies the row was deleted. The 2026-06-23 run passed and wrote evidence under `artifacts/validation/web-email-diagnostics-admin-actions-smoke/`.
- [x] Confirm manual suppression changes are Admin-only and appear in application logs.
  - Evidence: `Invoke-WebEmailDiagnosticsAdminActionsSmoke.ps1` verifies the suppression action is protected by the `Admin` policy and that `EmailDiagnosticsController` logs `removed email suppression` with the admin identity and hash prefix.
- [x] Confirm manual provider-event recording works for support reconciliation when a webhook event is missing.
  - Evidence: `Invoke-WebEmailDiagnosticsAdminActionsSmoke.ps1` inserts a temporary delivery log, posts the Admin-only manual provider-event form with `hard_bounce`, verifies the delivery log becomes `Failed` with `ProviderLastEvent=hard_bounce` and `FailureCode=brevo:hard_bounce`, and verifies a hashed internal suppression is created.
- [x] Confirm manual provider-event recording is Admin-only and appears in application logs.
  - Evidence: `Invoke-WebEmailDiagnosticsAdminActionsSmoke.ps1` verifies the provider-event action is protected by the `Admin` policy and that `EmailDiagnosticsController` logs `manually recorded provider event` with the admin identity and provider message id.
- [x] Confirm delivery-log cleanup is Admin-only and appears in application logs.
  - Evidence: `Invoke-WebEmailDiagnosticsAdminActionsSmoke.ps1` verifies the cleanup action is protected by the `Admin` policy and that `EmailDiagnosticsController` logs `deleted {DeletedCount} email delivery log entries`. The smoke intentionally does not run cleanup because it deletes operational audit data older than the retention window.
- [x] Configure `TransactionalEmail__EnableFailureAlerts=true`.
- [x] Tune `TransactionalEmail__FailureAlertThreshold`, `FailureAlertWindowMinutes`, `FailureAlertCooldownMinutes`, and `FailureAlertMonitorIntervalMinutes`.
  - Evidence: checked-in Web defaults enable failure alerts with threshold `3`, window `15` minutes, cooldown `60` minutes, and monitor interval `5` minutes. `/admin/email-diagnostics` exposes those readiness values, and the failure-alert trigger itself remains a separate validation item below.

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
- [ ] Configure Stripe Customer Portal settings for subscription updates, payment method updates, invoices, and cancellation policy.
- [ ] Confirm `/billing` opens Stripe Customer Portal for a learner with a linked Stripe customer id.
- [ ] Confirm Stripe webhooks grant Premium only after a verified Stripe event.
- [ ] Confirm subscription cancellation, unpaid, or incomplete-expired states downgrade the entitlement to Free.
- [ ] Confirm `admin/billing-diagnostics` shows Stripe readiness, billing profiles, and billing events without exposing secret values.
- [ ] Confirm Admin-only Stripe subscription reconciliation works for a known test subscription id.
- [ ] Confirm reconciliation is used only for support recovery when Stripe webhooks are delayed, missed, or failed.
- [ ] Confirm billing notification emails are sent for Premium activation, payment action needed, Premium ended, and reconciliation completed.
- [ ] Confirm billing notification emails appear in `admin/email-diagnostics` with provider message ids and no raw Stripe payloads.
- [ ] Complete `docs/75-Stripe-Billing-Validation-Playbook.md` in staging before enabling live Stripe billing.
- [x] Assign an owner for Stripe account access, API key rotation, tax settings, invoices, refunds, chargebacks, and plan upgrades.
  - Evidence: `docs/90-Web-Operational-Incident-Runbook.md` assigns Shahram Vafadar as the operator/release owner. Stripe remains disabled, but ownership is assigned before the deferred Stripe validation phase is re-opened.

## Public URLs and Account Links

- [x] Set `TransactionalEmail__PublicBaseUrl` to the public HTTPS web origin.
- [x] Verify confirmation links use `TransactionalEmail__PublicBaseUrl`.
- [x] Verify password reset links use `TransactionalEmail__PublicBaseUrl`.
- [x] Verify email-change links use `TransactionalEmail__PublicBaseUrl`.
  - Evidence: `WebAccountAuthenticationWorkflowTests.EmailChangeWorkflow_ShouldValidatePasswordAndNotifyOldEmailAfterSuccessfulChange` verifies `Manage/Email.cshtml.cs` builds the change-email confirmation URL through `BuildPublicPageUrl`, reads `emailOptions.Value.PublicBaseUrl`, uses it when it is an absolute URL, and falls back to `Request.Scheme` only when no configured public base URL is available.

## Database Bootstrap

- [x] Run WebApi once with the shared server database reachable.
- [x] Run Web once with the shared server database reachable.
- [x] Confirm Identity tables exist.
- [x] Confirm web user-state tables exist.
- [x] Confirm `WebEmailDeliveryLogs` exists.
- [x] Confirm `WebEmailDeliveryLogs` has Brevo event columns: `ProviderLastEvent`, `ProviderLastEventAtUtc`, `ProviderLastEventReason`.
  - Evidence: 2026-06-23 `tools/Web/Invoke-WebOperationsBootstrapCheck.ps1` passed against `https://darwinlingua.com`, `https://api.darwinlingua.com/health`, and `darwinlingua_shared`. It verified required tables `AspNetUsers`, `WebUserPreferences`, `WebEmailDeliveryLogs`, `WebEmailSuppressions`, `WebPolicyAcceptances`, `UserContentProgress`, plus Brevo event columns `ProviderLastEvent`, `ProviderLastEventAtUtc`, and `ProviderLastEventReason`.

## Validation

- [x] Register a test learner and confirm the email is sent through Brevo.
- [ ] Confirm the registration email link works.
- [x] Request a password reset and confirm the email is sent through Brevo.
- [ ] Complete password reset and confirm the success notification is received.
- [ ] Change account email and confirm both new-email confirmation and old-email notification are sent.
- [x] Force one delivery failure in staging and confirm the failed event is visible in `admin/email-diagnostics`.
  - Evidence: `tools/Web/Invoke-BrevoWebhookSuppressionSmoke.ps1` forced a controlled `hardBounce` through the public webhook and verified the delivery row became `Failed`; `tools/Web/Invoke-WebEmailDiagnosticsAdminSmoke.ps1` and `tools/Web/Invoke-WebEmailDiagnosticsAdminActionsSmoke.ps1` verified provider events, failed status, suppressions, and Admin-only controls are visible in `/admin/email-diagnostics`.
- [x] Confirm repeated failures trigger an admin alert.
  - Evidence: `EmailDeliveryFailureMonitorTests.Monitor_ShouldSendAdminAlertWhenFailureThresholdIsReached` executes `EmailDeliveryFailureMonitorService` with a failure snapshot at the configured threshold and verifies `SendAdminEmailDeliveryFailureAlertAsync` is called with failure count, window, scenario, and failure code. `Monitor_ShouldNotSendAdminAlertBelowThreshold` verifies no admin alert is sent below threshold and that `Admin.EmailDeliveryFailureAlert` is excluded from the failure-count snapshot to prevent alert loops.
- [ ] Check Brevo dashboard logs for message id, delivery state, bounce/error details, and webhook status.

## Operational Ownership

- [x] Assign an owner for Brevo account billing and plan upgrades.
  - Evidence: `docs/90-Web-Operational-Incident-Runbook.md` assigns Shahram Vafadar as the Brevo account, sender-domain, API key rotation, and plan-upgrade owner.
- [x] Assign an owner for DNS records and sender-domain verification.
  - Evidence: `docs/90-Web-Operational-Incident-Runbook.md` assigns Shahram Vafadar as the DNS/Cloudflare routing and sender-domain verification owner.
- [x] Review the Brevo data processing agreement before production launch.
  - Evidence: operator confirmed Brevo DPA accepted on 2026-06-23; the Brevo readiness report records `operator.dpaAccepted`.
- [x] Document the incident process for API key rotation, sender-domain failure, and account suspension.
  - Evidence: `docs/90-Web-Operational-Incident-Runbook.md` defines Brevo API key rotation, Brevo sender-domain/account failure, webhook failure, DNS/Cloudflare incidents, privacy/security triage, and backup/restore response flows.
