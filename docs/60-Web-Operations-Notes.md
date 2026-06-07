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

## Learning Portal Operations

Admin Reports includes a Learning Portal subsection for Web-first Phase 7 operations.

Operators should review it after imports, before Web sign-off, and before any bulk content generation. The report covers:

- content counts by type and CEFR
- grammar, expression, exercise, course, exam prep, writing template, and cultural note coverage
- unresolved linked words and cross-content references
- missing translation rows
- unpublished drafts
- grammar topics without linked exercises
- course lessons without linked exercise sets
- seed coverage by CEFR, course, and module

The report is for content quality and operational readiness only. It must not expose private learner progress or account data.

---

## Phase Backup And Restore Operations

After every completed content or platform phase, create a restorable backup under `X:\Projects\DarwinLingua.Backup` before starting the next phase.

Each backup folder should be timestamped and phase-labeled, for example `YYYYMMDD-HHMMSS-course-c1-complete-pre-c2`, and should contain:

- logical PostgreSQL dumps for `darwinlingua_shared` and any other local `darwinlingua%` database that is part of the phase
- PostgreSQL global role metadata captured separately from the database dumps
- a repo restore overlay for local work that may not yet exist in GitHub, including reviewed content packages, docs, tests, planning artifacts, and validation artifacts
- a separate local config/secret bundle for ignored environment files, local appsettings, certificates, and keys needed to restore the environment
- Docker inspect metadata for the PostgreSQL and pgAdmin containers and their named volumes
- a manifest with timestamp, phase label, git commit, branch, dirty status, key database counts, backup file hashes, and restore commands

Verification rules:

1. run `pg_restore --list` against each custom-format dump
2. generate SHA256 hashes for backup files and store them in the manifest folder
3. when feasible, restore the main dump into a temporary PostgreSQL database, compare critical counts, and drop the temporary database afterward
4. do not treat build outputs, caches, `bin`, `obj`, `.vs`, or transient logs as restore sources

The backup is considered complete only when a fresh checkout from GitHub plus the backup folder can restore the same database state, local config, and non-Git source artifacts for the phase checkpoint.

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
- Manual unsuppress is Admin-only and should be used only after support confirms the recipient address is valid and the Brevo dashboard no longer shows a permanent delivery issue.
- Manual provider-event recording is Admin-only and should be used only for support reconciliation when a Brevo dashboard event did not arrive through webhook; it is not a replacement for webhook configuration.
- Manual suppression/provider-event actions write application log entries with the admin identity and target hash or provider message id.
- Public account email flows use neutral responses for existing/non-existing emails and combine per-IP with per-email rate limiting for reset/resend/register confirmation sends.
- Login uses neutral fallback copy for blocked/not-allowed sign-in states; only a correct password for an unconfirmed account routes to the confirmation recovery screen.
- Password reset keeps non-existing accounts neutral, shows generic unusable-link copy for malformed/invalid/expired tokens, and provides a direct new-link path.
- Email confirmation and email-change confirmation are idempotent where safe: already-confirmed email and already-applied email-change links show success instead of stranding the user.
- Treat repeated `Failed` delivery logs as an operational incident when account recovery is affected.
- The hosted email failure monitor sends `Admin.EmailDeliveryFailureAlert` when failures exceed `TransactionalEmail:FailureAlertThreshold` inside `TransactionalEmail:FailureAlertWindowMinutes`; it suppresses repeated alerts for `TransactionalEmail:FailureAlertCooldownMinutes`.
- Use the Admin-only diagnostics cleanup action to remove logs older than `TransactionalEmail:DeliveryLogRetentionDays`.
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

## Legal And Privacy Operations

The Web legal/compliance baseline is defined in `86-Web-Legal-Compliance-Baseline.md`. Operators must treat it as an engineering checklist, not final legal advice.

Before production release:

- configure real `LegalNotice` operator values for provider name, address, contact email, responsible person, registration details, VAT id where applicable, and data-protection contact
- review `/legal` or `/impressum`, `/privacy`, `/terms`, `/cookies`, and `/contact`
- update policy versions when Terms or Privacy text changes
- confirm registration still records versioned policy acceptance records for Terms and Privacy notice acknowledgement
- re-run the cookie/storage inventory after script, telemetry, billing, PWA, authentication, or analytics changes
- keep non-essential cookies/storage disabled until opt-in if they are introduced
- define the data-subject request owner, identity confirmation process, response timeline, and export/deletion tooling status
- review breach triage responsibilities against GDPR Articles 33 and 34 expectations
- review transactional email provider processing and DPA status before enabling production provider mode
- review Stripe provider, subscription, cancellation/refund, and legal text before enabling billing
- keep Sensitive Educational Language opt-in separate from registration and document that it is not age verification
- keep mobile legal/privacy/store-compliance work deferred until Web sign-off

Policy acceptance audit:

- registration source records should use `terms-of-use` and `privacy-notice`
- records should include policy version, accepted-at UTC timestamp, source, and locale/culture
- avoid storing raw IP addresses, full user agents, full birthdates, or identity documents for this baseline unless a later legal review explicitly designs that processing

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

## Local Public Tunnel Notes

The local Cloudflare-routed public domains require the same ports used by the checked-in launch profiles:

- `DarwinLingua.Web`: `https://0.0.0.0:7501` and `http://0.0.0.0:5192`
- `DarwinLingua.WebApi`: `https://0.0.0.0:53944` and `http://0.0.0.0:53945`

For private local-only smoke, `http://localhost:5192` and `http://localhost:5099` may still be used when Web is configured to call the API on `5099`. For public-routed smoke through `lingua.vafadar.pro` and `linguaapi.vafadar.pro`, start the launch-profile ports or ensure the tunnel ingress points at the active local ports. A public `502 Bad Gateway` with healthy localhost responses usually means the tunnel origin port is not running or the Web host is calling a local API port that is not active.
