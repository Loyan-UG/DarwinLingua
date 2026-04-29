# Web Operations Notes

## Purpose

This document captures the production-focused logging and diagnostics expectations for `DarwinLingua.Web`.

---

## Logging Baseline

- keep structured ASP.NET Core request logging enabled
- keep application/service errors visible at `Error`
- avoid noisy framework logging at `Information` in production unless actively diagnosing an issue
- log startup failures, database initialization failures, and configuration faults clearly

---

## Diagnostics Focus Areas

Prioritize visibility into:

- startup initialization success/failure
- shared database connectivity
- browse/search/detail errors
- htmx POST failures for learner state changes
- service worker registration or manifest delivery failures
- admin-route authorization failures after Identity is added
- transactional email delivery failures
- repeated SMTP/provider failures
- unusually high password-reset or resend-confirmation volume

---

## Compression and Asset Caching Review

Current baseline:

- response compression is enabled for HTTPS responses
- static assets are referenced with `asp-append-version` where applicable
- PWA shell assets are explicitly versioned by the service worker
- output caching is already applied to landing and catalog browse slices

Review rule before release:

1. confirm text responses are compressed in the deployed environment
2. confirm versioned CSS and JS assets receive long-lived cache headers at the edge or host
3. confirm manifest and service worker updates are not pinned behind stale caching rules
4. confirm htmx partial endpoints are not cached in a way that leaks user-specific state
5. confirm any future image set uses appropriately sized assets instead of oversized originals

---

## UX Telemetry Direction

Current lightweight telemetry baseline now covers:

- slow server-side learner and admin requests through request-duration aggregation
- failed `htmx` request paths through request and client-event logging
- slow client page-load events above the current threshold
- install prompt availability and install completion events

When telemetry is expanded further, it should continue to focus on:

- slow browse pages
- slow search responses
- word-detail load latency
- failed favorite/known/difficult state updates
- install prompt display vs install completion

This telemetry should stay proportional and privacy-aware. Avoid collecting unnecessary learner content data.

---

## Transactional Email Operations

Operational rules:

- Use the local file sink only for development.
- Use Brevo API mode for staging and production transactional email.
- Keep Brevo API keys, webhook secrets, and SMTP fallback credentials in platform secret storage, not repository-tracked files.
- Keep `TransactionalEmail:PublicBaseUrl` aligned with the public HTTPS origin for each environment.
- Review `admin/email-diagnostics` during account-flow validation and after provider incidents.
- Review Brevo dashboard delivery logs and webhook status when provider events stop updating diagnostics.
- Use the provider message id and provider event filters in `admin/email-diagnostics` to reconcile Brevo dashboard events with internal delivery logs.
- Permanent Brevo events such as hard bounce, blocked, invalid email, and spam add the hashed recipient to the internal suppression list; later sends are logged as `Suppressed` without calling Brevo.
- Use manual unsuppress only after support confirms the recipient address is valid and the Brevo dashboard no longer shows a permanent delivery issue.
- Use manual provider-event recording only for support reconciliation when a Brevo dashboard event did not arrive through webhook; it is not a replacement for webhook configuration.
- Treat repeated `Failed` delivery logs as an operational incident when account recovery is affected.
- The hosted email failure monitor sends `Admin.EmailDeliveryFailureAlert` when failures exceed `TransactionalEmail:FailureAlertThreshold` inside `TransactionalEmail:FailureAlertWindowMinutes`; it suppresses repeated alerts for `TransactionalEmail:FailureAlertCooldownMinutes`.
- Use the diagnostics cleanup action to remove logs older than `TransactionalEmail:DeliveryLogRetentionDays`.
- Do not mix marketing email with transactional account email.

Provider readiness checklist:

- sender domain SPF verified
- sender domain DKIM verified
- sender domain DMARC configured
- bounce/return-path handling configured
- provider rate limits reviewed
- provider cost assumptions reviewed
- provider processing/DPA reviewed before production launch

---

## Incident Triage Checklist

When the web host misbehaves:

1. confirm the deployment commit and configuration
2. confirm database connectivity
3. confirm learner routes vs admin routes separately
4. confirm CSP/security headers are not blocking first-party assets
5. confirm static asset versions match the deployed build
6. confirm service worker cache is not serving stale shell assets
7. confirm transactional email diagnostics for `Failed` or repeated `Queued` delivery attempts
8. confirm SMTP/provider credentials and sender-domain DNS when account recovery emails fail
