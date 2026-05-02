# Web Release Checklist

## Purpose

This checklist is the release gate for `DarwinLingua.Web`.

Use it for both the learner-facing root site and the admin area.

---

## Latest Local Evidence

Last updated: 2026-05-01.

- Local `DarwinLingua.Web` build passed with 0 warnings and 0 errors.
- Local `DarwinLingua.WebApi` build passed with 0 warnings and 0 errors after stopping the smoke host that was locking output DLLs.
- Local learner route smoke passed for the main browse, search, collection, scenario, conversation, organizer, install, privacy, and Identity pages.
- Local authenticated admin route smoke passed for the dashboard, reports, analytics, diagnostics, content operations, catalog management, taxonomy, user, moderation, billing diagnostics, and email diagnostics pages.
- No unhandled exception signature was found in smoke response bodies or server logs.
- Local Identity return-url hardening smoke passed for login and registration; external URLs are normalized before rendering hidden form state.
- Local webhook smoke passed for unsigned Stripe billing webhook rejection, and bounded telemetry payload handling was verified.
- Brevo and Stripe provider-error paths were reviewed so provider response bodies are not logged or surfaced in diagnostics.
- Production sign-off still requires the unchecked release gates below, especially automated tests, manual device/browser validation, Brevo DNS/domain verification, and Stripe test-mode/staging validation.

---

## A. Build And Test

- [ ] release commit selected
- [ ] solution build succeeded
- [ ] automated tests succeeded
- [ ] web-specific structural tests succeeded

---

## B. Learner Web

- [ ] learner shell renders correctly
- [ ] browse by CEFR works
- [ ] browse by topic works
- [ ] search works
- [ ] word detail works
- [ ] favorites work
- [ ] recent activity works
- [ ] settings work

---

## C. Admin Web

- [ ] admin layout renders correctly
- [ ] admin overview loads
- [ ] publishing page loads
- [ ] diagnostics page loads
- [ ] transactional email diagnostics page loads
- [ ] admin/learner separation is preserved

---

## D. PWA

- [ ] manifest is delivered
- [ ] service worker registers
- [ ] install flow validated on target browsers
- Validation evidence may be attached from `artifacts/installability-report.json` and `artifacts/installability-report-android.json`

---

## E. Account And Transactional Email Readiness

This section is a release blocker. See `73-Transactional-Email-And-Account-Communication-Backlog.md`.

- [ ] email confirmation is sent after registration
- [ ] confirmation callback works
- [ ] resend confirmation flow works
- [ ] forgot-password request works without account enumeration
- [ ] password reset link works
- [ ] expired and invalid reset tokens fail safely
- [ ] password reset success notification is sent
- [ ] localized English email templates render correctly
- [ ] localized German email templates render correctly
- [ ] transactional email delivery log is available to admins/operators
- [ ] email-triggering endpoints are rate-limited
- [ ] production public base URL is configured correctly for email links
- [ ] no reset/confirmation token or full recovery URL is logged

---

## F. Operational Readiness

- [ ] production configuration applied
- [ ] database connectivity verified
- [ ] security headers verified
- [ ] logging baseline verified
- [ ] production email provider configured
- [ ] sender address and reply-to address configured
- [ ] SPF, DKIM, and DMARC verified for the sender domain
- [ ] email provider processing/DPA requirements reviewed where applicable
- [ ] rollback owner identified

---

## G. Sign-Off

- Release owner:
- Validation owner:
- Known accepted issues:
- Final release recommendation:
