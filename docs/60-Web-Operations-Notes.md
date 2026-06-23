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

## Security Header Baseline

Current Web/WebApi runtime baseline:

- public HTTPS responses send `Strict-Transport-Security: max-age=31536000; includeSubDomains`
- Web and WebApi send `X-Content-Type-Options: nosniff`
- Web and WebApi send `X-Frame-Options: DENY`
- Web and WebApi send `Referrer-Policy: strict-origin-when-cross-origin`
- Web and WebApi disable camera, microphone, and geolocation through `Permissions-Policy`
- Web sends the app content security policy for scripts, styles, images, fonts, manifests, forms, frames, and workers
- localhost and loopback HTTPS requests do not receive HSTS, so local development certificates are not accidentally pinned

Before release, verify the public Web and API hosts after deployment rather than only inspecting local configuration, because reverse proxies and edge providers can change response headers.

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

Each backup folder should be timestamped and phase-labeled, for example `YYYYMMDD-HHMMSS-course-c2-complete`, and should contain:

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

Current helper scripts:

- `tools/Web/New-WebReadinessPhaseBackup.ps1` creates a timestamped Web phase backup folder with PostgreSQL dump/restore-list where PostgreSQL client tools or the configured Docker container are available, repo overlay, local secret bundle, Docker inspect metadata, manifest, and SHA256 checksums.
- `tools/Web/Invoke-WebReadinessRollback.ps1` is intentionally dry-run by default. It prints the selected backup, dump, and restore actions. It restores the repo overlay or database only when the operator passes `-ApplyRepoOverlay` and/or `-ApplyDatabaseRestore` together with `-ConfirmRollback`.

Rollback owner:

- During Web testing, the operator/release owner is the rollback owner.
- Before any destructive rollback, create a fresh pre-rollback backup with `New-WebReadinessPhaseBackup.ps1`.
- Stop Web/WebApi processes, restore only from a verified backup folder, then run local Web/WebApi smoke before re-opening the public tunnel.
- Database rollback is a PostgreSQL restore operation and may overwrite current data. Do not run it without an explicit restore target and operator confirmation.

Latest verified phase checkpoint:

- `X:\Projects\DarwinLingua.Backup\20260609-125945-exam-prep-b2-foundation-complete`
- Scope: Exam Prep foundation through B2, with `ExamProfiles=16`, `ExamPrepUnits=80`, and `B2Units=15`.
- Verification: `pg_restore --list` exists, restore dry-run counts match live counts, repo overlay and separate local config/secret bundle are present, and SHA256 checksums were generated.
- `X:\Projects\DarwinLingua.Backup\20260609-140831-exam-prep-c2-foundation-complete`
- Scope: Exam Prep foundation through Goethe C2, with `ExamProfiles=17`, `ExamPrepUnits=95`, and `C2Units=15`.
- Verification: `pg_restore --list` exists, restore dry-run counts match live counts, repo overlay and separate local config/secret bundle are present, and SHA256 checksums were generated.
- `X:\Projects\DarwinLingua.Backup\20260612-142146-exam-prep-complete-pre-writing-templates`
- Scope: closed Exam Prep phase before Writing Templates, with `ExamProfiles=17`, `ExamPrepUnits=246`, `GoetheC2Units=86`, and `WritingTemplates=0` at backup time.
- Verification: `pg_restore --list` exists, restore dry-run counts match live counts, repo overlay and separate local config/secret bundle are present, Docker metadata is captured, and SHA256 checksums were generated.
- `X:\Projects\DarwinLingua.Backup\20260613-132057-writing-templates-complete-pre-cultural-notes`
- Scope: closed Writing Templates phase before Cultural Notes, with `WritingTemplates=120` (`A1=20`, `A2=20`, `B1=20`, `B2=20`, `C1=20`, `C2=20`) and `CulturalNotes=0` at backup time.
- Verification: `pg_restore --list` exists, restore dry-run counts match live counts, repo overlay and separate local config/secret bundle are present, Docker metadata is captured, manifest restore notes are present, and SHA256 checksums were generated.
- `X:\Projects\DarwinLingua.Backup\20260613-220312-life-in-germany-a1-b1-foundation-complete`
- Scope: Life in Germany A1-B1 foundation checkpoint, with `CulturalNotes=30` (`A1=10`, `A2=10`, `B1=10`) at backup time.
- Verification: `pg_restore --list` exists, restore dry-run counts match live counts, repo overlay and separate local config/secret bundle are present, Docker metadata is captured, manifest restore notes are present, and SHA256 checksums were generated.
- `X:\Projects\DarwinLingua.Backup\20260614-214709-web-readiness-pre-user-testing`
- Scope: Web readiness checkpoint before external tester onboarding, with `CourseLessons=560`, `WritingTemplates=120`, `ExamPrepUnits=246`, and `CulturalNotes=30` (`A1=10`, `A2=10`, `B1=10`) at backup time.
- Verification: `pg_restore --list` exists, restore dry-run counts match live counts, repo overlay and separate local config/secret bundle are present, Docker metadata is captured, manifest restore notes are present, and SHA256 checksums were generated.
- `X:\Projects\DarwinLingua.Backup\20260619-222010-web-readiness-account-legal-pwa-pre-user-testing`
- Scope: Web readiness checkpoint after account self-service, legal/compliance research update, designed transactional email HTML, Brevo runbook, PWA console cleanup, and rollback helper scripts.
- Verification: custom PostgreSQL dump and restore list exist, dry-run restore to temporary database reported `CourseLessons=560`, `WritingTemplates=120`, `ExamPrepUnits=246`, and `CulturalNotes=30`, repo overlay patch and dirty-file snapshot are present, separate local config/secret bundle and Docker metadata are present, and SHA256 checksums were regenerated after dry-run evidence was written.
- Note: `X:\Projects\DarwinLingua.Backup\20260619-221410-web-readiness-account-legal-pwa-pre-user-testing` is explicitly marked incomplete because the first backup run was stopped while mirroring the full repo overlay directly to `X:`.
- `X:\Projects\DarwinLingua.Backup\20260620-011255-web-readiness-final-pre-user-testing`
- Scope: Web readiness checkpoint after tester-gate documentation and Brevo handoff commits, before controlled real-user testing.
- Verification: custom PostgreSQL dump and restore list exist, dry-run restore to temporary database reported `CourseLessons=560`, `WritingTemplates=120`, `ExamPrepUnits=246`, `CulturalNotes=30`, and `ActivityEnabled=560`; repo overlay, selected validation artifacts, separate local config/secret bundle, Docker metadata, manifest, and regenerated SHA256 checksums are present.
- `X:\Projects\DarwinLingua.Backup\20260620-140609-web-readiness-current-pre-user-testing`
- Scope: Current Web readiness checkpoint after account email-token hardening, Brevo HTML/handoff polish, and refreshed readiness evidence docs.
- Verification: custom PostgreSQL dump and restore list exist, dry-run restore to a temporary PostgreSQL database reported `CourseLessons=560`, `WritingTemplates=120`, `ExamPrepUnits=246`, `CulturalNotes=30`, and `ActivityEnabled=560`; repo overlay, selected validation artifacts, separate local config/secret bundle, Docker metadata, manifest, restore dry-run evidence, and regenerated SHA256 checksums are present.
- `X:\Projects\DarwinLingua.Backup\20260620-141903-web-readiness-current-legal-refresh-pre-user-testing`
- Scope: Current Web readiness checkpoint after the follow-up legal-source refresh gate for Life in Germany/LiD catalog wording, DSA/TDDDG, and cannabis limits.
- Verification: custom PostgreSQL dump and restore list exist, dry-run restore to a temporary PostgreSQL database reported `CourseLessons=560`, `WritingTemplates=120`, `ExamPrepUnits=246`, `CulturalNotes=30`, and `ActivityEnabled=560`; repo overlay, selected validation artifacts, separate local config/secret bundle, Docker metadata, manifest, restore dry-run evidence, and regenerated SHA256 checksums are present.
- `X:\Projects\DarwinLingua.Backup\20260616-190633-course-a1-activity-flow-complete-pre-a2-activity-flow`
- Scope: Course A1 activity-flow checkpoint before A2 backfill, with `CourseLessons=560`, `A1ActivityEnabled=60`, `TotalActivityEnabled=60`, `PublishedLessonsWithoutActivityBlocks=500`, and zero unresolved activity targets at backup time.
- Verification: `pg_restore --list` exists, restore dry-run counts match live counts, repo overlay and separate local config/secret bundle are present, Docker metadata is captured, manifest restore notes are present, and SHA256 checksums were generated.
- `X:\Projects\DarwinLingua.Backup\20260617-153803-course-a2-activity-flow-complete-pre-b1-activity-flow`
- Scope: Course A2 activity-flow checkpoint before B1 backfill, with `CourseLessons=560`, `A1ActivityEnabled=60`, `A2ActivityEnabled=80`, `TotalActivityEnabled=140`, `ActiveLessonsWithoutActivityBlocks=420`, and zero unresolved A2 activity targets at backup time.
- Verification: `pg_restore --list` exists, restore dry-run counts match live counts, repo overlay and separate local config/secret bundle are present, Docker metadata is captured, manifest restore notes are present, and SHA256 checksums were generated.
- `X:\Projects\DarwinLingua.Backup\20260617-171229-course-b1-activity-flow-complete-pre-b2-activity-flow`
- Scope: Course B1 activity-flow checkpoint before B2 backfill, with `CourseLessons=560`, `A1ActivityEnabled=60`, `A2ActivityEnabled=80`, `B1ActivityEnabled=100`, `TotalActivityEnabled=240`, `ActiveLessonsWithoutActivityBlocks=320`, `WritingTemplates=120`, `ExamPrepUnits=246`, and `CulturalNotes=30` at backup time.
- Verification: `pg_restore --list` exists, restore dry-run counts match live counts, repo overlay and separate local config/secret bundle are present, Docker metadata is captured, manifest restore notes are present, and SHA256 checksums were generated.

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
- verify `/account` export/delete remains available after any identity, billing, or learning-state schema changes
- review breach triage responsibilities against GDPR Articles 33 and 34 expectations
- review transactional email provider processing and DPA status before enabling production provider mode
- review Stripe provider, subscription, cancellation/refund, and legal text before enabling billing
- keep Sensitive Educational Language opt-in separate from registration and document that it is not age verification
- keep transactional email separate from marketing email; any newsletter, promotional, or win-back campaign needs a separate consent and unsubscribe design before activation
- keep a clear intake path for security, abuse, illegal-content, privacy, and rights reports while organizer, partner, RSVP, claim, profile, and report workflows are exposed to real users
- review DSA classification, notice/action handling, moderation transparency, and appeal/escalation requirements before broad public community release
- keep public paid subscriptions disabled until BGB section 312k cancellation-button, refund/cancellation text, Stripe customer portal, and legal copy are reviewed together
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

Current public hostnames:

- Web: `https://darwinlingua.com`
- API: `https://api.darwinlingua.com`

Recommended Cloudflare Tunnel mapping for the current single-machine setup:

- `darwinlingua.com` -> Web origin `http://localhost:5192` or `https://localhost:7501`
- `api.darwinlingua.com` -> WebApi origin `http://localhost:53945` or `https://localhost:53944`

If HTTPS origins are used with local development certificates, disable origin certificate verification for those tunnel services or install a trusted origin certificate. If HTTP origins are used, keep Cloudflare edge TLS enabled and ensure the tunnel is the only public ingress to those local ports.

For private local-only smoke, `http://localhost:5192` and `http://localhost:5099` may still be used when Web is configured to call the API on `5099`. For public-routed smoke through `darwinlingua.com` and `api.darwinlingua.com`, start the launch-profile ports or ensure the tunnel ingress points at the active local ports. A public `502 Bad Gateway` with healthy localhost responses usually means the tunnel origin port is not running or the Web host is calling a local API port that is not active.

Do not start WebApi with only `--urls http://localhost:5099` when the public `api.darwinlingua.com` tunnel must be tested. That local-only launch leaves the checked-in tunnel origin ports closed and produces public `502` responses even though `http://localhost:5099/health` is healthy. Use the checked-in launch profile for public smoke so WebApi listens on `https://0.0.0.0:53944`, `http://0.0.0.0:53945`, and `http://localhost:5099`.

Repeatable local public-stack helper:

```powershell
.\tools\Web\Start-WebPublicDevStack.ps1 -StopExisting
```

The helper starts WebApi and Web with the checked-in launch profiles, starts the installed Cloudflared tunnel if needed, falls back to a user-process `cloudflared` run when the Windows service cannot be started from the current shell, and writes JSON/Markdown evidence under `artifacts/validation/web-public-stack/`. It must not print the tunnel token.

## Phase Backup Register

### 2026-06-23 Web Public PWA Ready / Pre User Testing

Backup path:

`X:\Projects\DarwinLingua.Backup\20260623-091250-web-public-pwa-ready-pre-user-testing`

Scope:

- Web public-stack checkpoint after the Darwin-domain/Brevo/legal readiness pass.
- Email-change confirmation links are verified to use `TransactionalEmail__PublicBaseUrl` when configured, with request-host fallback only when no absolute public base URL is available.
- Repeated email-delivery failures are verified to trigger `Admin.EmailDeliveryFailureAlert` once the configured threshold is reached, while below-threshold snapshots do not alert.
- Brevo provider-side transactional logs are verified through the official `/v3/smtp/statistics/events` API for recent real delivery message ids.
- Public-domain automated PWA shell/installability preconditions are verified for `https://darwinlingua.com`; real install prompt acceptance remains a target-browser manual check.
- Shared PostgreSQL backup refreshed from `darwinlingua_shared`; local secret bundle includes the configured Brevo API key and webhook token without writing them to Git.

Verification evidence:

- `db/darwinlingua_shared_20260623-091250.dump` created from `darwinlingua_shared`.
- `db/darwinlingua_shared_20260623-091250.restore-list.txt` generated with `pg_restore --list`.
- `manifest.md`, `checksums.sha256`, `repo-overlay/`, `repo-overlay/darwinlingua-current-head.bundle`, `secrets/`, and `docker/` are present.
- `SecretFileCount=3`, `BrevoApiKeyPresent=True`, and `WebhookTokenPresent=True` were verified without printing secret values.
- `artifacts/validation/brevo-transactional-log-check/brevo-transactional-log-check-20260623-084312.md` passed with recent real Brevo message ids matching provider events.
- `artifacts/validation/pwa-installability/pwa-installability-darwinlingua-20260623.json` passed 17 public-domain automated PWA checks with 0 failures and 2 manual install-acceptance checks.
- Follow-up PWA evidence `artifacts/validation/pwa-installability/pwa-installability-darwinlingua-20260623-205307.json` also passed 17 public-domain automated PWA checks with 0 failures. It observed the desktop install-prompt event in headless Chromium, but real desktop installed-window behavior and Android Chrome install flow remain manual target-browser checks.
- Public routed smoke returned HTTP 200 for `https://darwinlingua.com`, `/legal`, `/privacy`, and `https://api.darwinlingua.com/health`.

### 2026-06-23 Brevo Domain Ready / Pre User Testing

Backup path:

`X:\Projects\DarwinLingua.Backup\20260623-190324-brevo-domain-ready-pre-user-testing`

Scope:

- Brevo production-domain checkpoint after `no-reply@darwinlingua.com` creation, `support@darwinlingua.com` forwarding confirmation, operator DPA acceptance, and live Brevo webhook/API verification.
- Public Web stack restarted from commit `0d69c309` and verified through `https://darwinlingua.com` and `https://api.darwinlingua.com/health`.
- Webhook normalization was aligned with the currently configured Brevo transactional events, including `proxyOpen`, `uniqueProxyOpen`, and `error`.
- Shared PostgreSQL backup refreshed from `darwinlingua_shared`; local secret bundle includes the configured Brevo API key and webhook token without writing them to Git.

Verification evidence:

- `db/darwinlingua_shared_20260623-190324.dump` created from `darwinlingua_shared`.
- `db/darwinlingua_shared_20260623-190324.restore-list.txt` generated with `pg_restore --list`.
- `manifest.md`, `checksums.sha256`, `repo-overlay/`, `secrets/`, and `docker/` are present.
- `SecretFileCount=3`, `BrevoApiKeyPresent=True`, and `WebhookTokenShapePresent=True` were verified without printing secret values.
- `artifacts/validation/brevo-readiness/brevo-production-readiness-20260623-185124.md` passed with `Blockers=0` and `Warnings=0`.
- `artifacts/validation/brevo-real-delivery-smoke/brevo-real-delivery-smoke-20260623-185122.md` sent a controlled real delivery through Brevo.
- `artifacts/validation/brevo-webhook-suppression-smoke/brevo-webhook-suppression-smoke-20260623-185056.md` passed with Bearer-token webhook authentication and internal suppression creation.
- `artifacts/validation/brevo-transactional-log-check/brevo-transactional-log-check-20260623-185106.md` passed against Brevo provider logs.
- `artifacts/validation/web-email-diagnostics-admin-smoke/web-email-diagnostics-admin-smoke-20260623-190230.md` and `artifacts/validation/web-email-diagnostics-admin-actions-smoke/web-email-diagnostics-admin-actions-smoke-20260623-190236.md` passed.
- Public routed smoke returned HTTP 200 for `https://darwinlingua.com`, `/legal`, `/privacy`, `/cookies`, and `https://api.darwinlingua.com/health`.

### 2026-06-23 Brevo Webhook Configuration Check Ready / Pre User Testing

Backup path:

`X:\Projects\DarwinLingua.Backup\20260623-191546-brevo-webhook-config-check-ready-pre-user-testing`

Scope:

- Added repeatable provider-side Brevo webhook configuration verification through `tools/Web/Invoke-BrevoWebhookConfigurationCheck.ps1`.
- The tool verifies Brevo `/v3/webhooks` metadata for the Darwin Lingua transactional webhook without storing API keys, bearer tokens, custom header values, or raw webhook responses.
- Public Web stack restarted from commit `96235b8c` and verified through `https://darwinlingua.com` and `https://api.darwinlingua.com/health`.
- Shared PostgreSQL backup refreshed from `darwinlingua_shared`; local secret bundle includes the configured Brevo API key and webhook token without writing them to Git.

Verification evidence:

- `db/darwinlingua_shared_20260623-191546.dump` created from `darwinlingua_shared`.
- `db/darwinlingua_shared_20260623-191546.restore-list.txt` generated with `pg_restore --list`.
- `manifest.md`, `checksums.sha256`, `repo-overlay/`, `secrets/`, and `docker/` are present.
- `SecretFileCount=3`, `BrevoApiKeyPresent=True`, and `WebhookTokenShapePresent=True` were verified without printing secret values.
- `artifacts/validation/brevo-webhook-configuration-check/brevo-webhook-configuration-check-20260623-191243.md` passed with provider webhook URL `https://darwinlingua.com/webhooks/brevo/transactional-email`, type `transactional`, auth type `bearer`, and zero missing expected event keys.
- `TransactionalEmailBrevoTests` passed `30/30`.
- Public routed smoke returned HTTP 200 for `https://darwinlingua.com`, `/legal`, `/privacy`, `/cookies`, and `https://api.darwinlingua.com/health`.

### 2026-06-23 Controlled Tester Readiness Audit Ready / Pre User Testing

Backup path:

`X:\Projects\DarwinLingua.Backup\20260623-193207-controlled-tester-readiness-audit-ready-pre-user-testing`

Scope:

- Added repeatable controlled-tester readiness aggregation through `tools/Web/New-WebControlledTesterReadinessAudit.ps1`.
- The audit consolidates public-stack, operations bootstrap, Brevo readiness, webhook configuration, delivery/link/suppression/log smokes, Admin Email Diagnostics smokes, tester bundle preflight, and manual external-review status.
- The latest audit report shows `Automated ready=True`, `Automated failures=0`, `Human start ready=False`, `Controlled tester ready to invite=False`, and `Human start open gates=4`.
- Open human gates remain: mailbox rendering review, desktop PWA install review, Android PWA install review, and tester-pass start status.
- Public Web stack restarted from commit `56d2a6af` and verified through `https://darwinlingua.com` and `https://api.darwinlingua.com/health`.
- Shared PostgreSQL backup refreshed from `darwinlingua_shared`; local secret bundle includes the configured Brevo API key and webhook token without writing them to Git.

Verification evidence:

- `db/darwinlingua_shared_20260623-193207.dump` created from `darwinlingua_shared`.
- `db/darwinlingua_shared_20260623-193207.restore-list.txt` generated with `pg_restore --list`.
- `manifest.md`, `checksums.sha256`, `repo-overlay/`, `repo-overlay/darwinlingua-current-head.bundle`, `secrets/`, `docker/`, and `verification/` are present.
- `SecretFileCount=3`, `BrevoApiKeyPresent=True`, and `WebhookTokenShapePresent=True` were verified without printing secret values.
- `artifacts/validation/web-controlled-tester-readiness/web-controlled-tester-readiness-20260623-193121.md` passed the automated gate with zero automated failures and preserved the four open human gates.
- `WebTesterOperatorToolingTests` passed with `--no-build`.
- Public routed smoke returned HTTP 200 for `https://darwinlingua.com`, `/legal`, `/privacy`, `/cookies`, and `https://api.darwinlingua.com/health`.

### 2026-06-23 Public PWA Evidence Refresh / Pre User Testing

Backup path:

`X:\Projects\DarwinLingua.Backup\20260623-205446-public-pwa-evidence-refresh-pre-user-testing`

Scope:

- Refreshed public-domain PWA installability evidence for `https://darwinlingua.com`.
- `artifacts/validation/pwa-installability/pwa-installability-darwinlingua-20260623-205307.json` reports 17 automated checks passed, 0 failed checks, and 2 manual checks.
- Headless Chromium observed the desktop install-prompt event, but real desktop installed-window behavior and Android Chrome install flow remain manual target-browser checks.
- Shared PostgreSQL backup refreshed from `darwinlingua_shared`; local secret bundle includes the configured Brevo API key and webhook token without writing them to Git.

Verification evidence:

- `db/darwinlingua_shared_20260623-205446.dump` created from `darwinlingua_shared`.
- `db/darwinlingua_shared_20260623-205446.restore-list.txt` generated with `pg_restore --list`.
- `manifest.md`, `checksums.sha256`, `repo-overlay/`, `repo-overlay/darwinlingua-current-head.bundle`, `secrets/`, `docker/`, and `verification/` are present.
- Because `artifacts/validation/` is intentionally ignored by Git and excluded from the repo overlay, the PWA JSON report was copied into the backup at `validation-artifacts/pwa-installability/pwa-installability-darwinlingua-20260623-205307.json` and its SHA256 was appended to `checksums.sha256`.
- `SecretFileCount=3`, `BrevoApiKeyPresent=True`, and `WebhookTokenShapePresent=True` were verified without printing secret values.
- Public routed smoke returned HTTP 200 for `https://darwinlingua.com`, `/manifest.webmanifest`, `/offline.html`, `/legal`, and `https://api.darwinlingua.com/health`.

### 2026-06-17 Course B2 Activity Flow Complete / Pre-C1

Backup path:

`X:\Projects\DarwinLingua.Backup\20260617-183717-course-b2-activity-flow-complete-pre-c1-activity-flow`

Scope:

- Course activity-flow backfill completed through B2: A1 `60/60`, A2 `80/80`, B1 `100/100`, B2 `80/80`.
- Shared PostgreSQL counts: `CourseLessons=560`, `B2ActivityEnabled=80`, `TotalActivityEnabled=320`, `ActiveLessonsWithoutActivityBlocks=240`, `WritingTemplates=120`, `ExamPrepUnits=246`, `CulturalNotes=30`.
- Runtime schema repair included: `UserContentProgress` exists in the live database and in the refreshed restore dump.

Verification evidence:

- `db/darwinlingua_shared_20260617-183717.dump` refreshed after the `UserContentProgress` repair.
- `db/darwinlingua_shared_20260617-183717.restore-list.txt` generated with `pg_restore --list`.
- `verification/live-counts.txt` and `verification/restore-dry-run-counts.txt` match, including `has_user_content_progress=t`.
- Temporary dry-run database `darwinlingua_restore_check_20260617183717` was restored and dropped.
- `manifest.md`, `checksums.sha256`, `repo-overlay/`, `secrets/`, and `docker/` are present.

Restore note:

If restoring from an older pre-B2 or pre-progress checkpoint, verify that `UserContentProgress` exists before running Web learner pages that track lesson views or `/recent`. If it is missing, use the current migration/initializer path or create the table and indexes before public smoke.

### 2026-06-17 Course C1 Activity Flow Complete / Pre-C2

Backup path:

`X:\Projects\DarwinLingua.Backup\20260617-210237-course-c1-activity-flow-complete-pre-c2-activity-flow`

Scope:

- Course activity-flow backfill completed through C1: A1 `60/60`, A2 `80/80`, B1 `100/100`, B2 `80/80`, C1 `120/120`.
- Shared PostgreSQL counts: `CourseLessons=560`, `C1ActivityEnabled=120`, `TotalActivityEnabled=440`, `ActiveLessonsWithoutActivityBlocks=120`, `WritingTemplates=120`, `ExamPrepUnits=246`, `CulturalNotes=30`.
- C1 Module 12 unresolved activity target count is zero.

Verification evidence:

- `db/darwinlingua_shared_20260617-210237.dump` created from `darwinlingua_shared`.
- `db/darwinlingua_shared_20260617-210237.restore-list.txt` generated with `pg_restore --list`.
- `verification/live-counts.txt` and `verification/restore-counts.txt` match.
- Temporary dry-run database `darwinlingua_restore_check_20260617_210237` was restored and dropped.
- `manifest.md`, `checksums.sha256`, `repo-overlay/`, `secrets/`, and `docker/` are present.

### 2026-06-18 Course C2 Activity Flow Complete / Pre User Testing

Backup status:

- Restore-ready staging backup exists at `D:\_Projects\DarwinLingua.Backup.Staging\20260618-073641-course-c2-activity-flow-complete-pre-user-testing`.
- Final external target is synced at `X:\Projects\DarwinLingua.Backup\20260618-073641-course-c2-activity-flow-complete-pre-user-testing`.
- At creation time, `X:` was disconnected from `\\MYCLOUD-GXS39C\shahramvafadar`, so the backup was first staged locally. After `X:` reconnected, the full staging folder was copied to the external target and verified.

Scope:

- Course activity-flow backfill completed through C2: A1 `60/60`, A2 `80/80`, B1 `100/100`, B2 `80/80`, C1 `120/120`, C2 `120/120`.
- Shared PostgreSQL counts: `CourseLessons=560`, `C2ActivityEnabled=120`, `TotalActivityEnabled=560`, `ActiveLessonsWithoutActivityBlocks=0`, `WritingTemplates=120`, `ExamPrepUnits=246`, `CulturalNotes=30`, `UserContentProgress=true`.
- C2 unresolved activity target count is zero.

Verification evidence:

- `db/darwinlingua_shared_20260618-073641.dump` created from `darwinlingua_shared`.
- `db/darwinlingua_shared_20260618-073641.restore-list.txt` generated with `pg_restore --list`.
- `verification/live-counts.txt` and `verification/restore-counts.txt` match.
- Temporary dry-run restore database was restored and dropped.
- `manifest.md`, `checksums.sha256`, `repo-overlay/`, `secrets/`, and `docker/` are present at the external target.
- `external-sync-verification.md` records the later successful sync because `manifest.md` was created during the initial local staging step while `X:` was unavailable.
- External target file count and byte size match staging when hidden files are included: `617` files, `239493739` bytes.
- SHA256 verification at the external target passed for all `616` listed files.

Operational follow-up:

- This phase backup is closed. Keep the local staging folder until the next phase backup has also been verified, then it may be pruned manually if disk space is needed.
