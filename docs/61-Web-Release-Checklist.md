# Web Release Checklist

## Purpose

This checklist is the release gate for `DarwinLingua.Web`.

Use it for both the learner-facing root site and the admin area.

---

## Latest Local Evidence

Last updated: 2026-05-12.

- Local `DarwinLingua.Web` build passed with 0 warnings and 0 errors.
- Local `DarwinLingua.WebApi` build passed with 0 warnings and 0 errors.
- Full `DarwinLingua.slnx` build passed with 0 warnings and 0 errors when run sequentially with `-m:1`.
- Phase 7 WebApi tests passed for the current admin/report and Learning Portal endpoint surface.
- Phase 7 parser tests passed for Grammar, Expressions, Exercises, Courses, Exam Prep, Writing Templates, and Cultural Notes.
- Phase 7 structural route/localization tests cover the Web learner routes, WebApi route registrations, and English/German resource keys for the Learning Portal release surface.
- Exercise attempt hardening now separates stateless public evaluation from authenticated persisted attempts, with bounded submitted-answer JSON and endpoint rate limits.
- Unified Learning Search now has application-level query limits and a PostgreSQL trigram-index migration for the bulk-content path.
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
- [ ] reports page loads Learning Portal coverage and quality checks
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
- [ ] Learning Portal unresolved-link, missing-translation, unpublished-draft, and seed coverage reports reviewed
- [ ] production email provider configured
- [ ] sender address and reply-to address configured
- [ ] SPF, DKIM, and DMARC verified for the sender domain
- [ ] email provider processing/DPA requirements reviewed where applicable
- [ ] rollback owner identified

---

## G. Phase 7 Learning Portal Release Gates

- [ ] Grammar Guide readiness reviewed: import validation, WebApi list/detail, Web list/detail, localization, safe missing-link behavior
- [ ] Everyday Expressions readiness reviewed: import validation, warning metadata, WebApi list/detail, Web list/detail, localization, safe missing-link behavior
- [ ] Exercise Engine readiness reviewed: deterministic answer evaluation, answer-key safety, attempts, WebApi runner endpoints, Web runner behavior
- [ ] Exercise attempt persistence requires authorization and stores only authenticated user ids
- [ ] Public exercise evaluation is stateless, rate-limited, and does not persist anonymous progress
- [ ] Exercise submitted-answer JSON is bounded, shape-checked, and malformed input returns safe validation errors
- [ ] Course Lessons readiness reviewed: course/module/lesson ordering, linked-content projections, lesson routes, progress hooks where implemented
- [ ] Exam Prep readiness reviewed: profile taxonomy, filters, linked-content projections, WebApi/Web pages, original authored-content policy
- [ ] Writing Templates readiness reviewed: variables, sample filled versions, filters, WebApi/Web pages, linked-content behavior
- [ ] Cultural Notes readiness reviewed: category/context filters, neutral/safe content handling, WebApi/Web pages, linked-content behavior
- [ ] Unified Search readiness reviewed: deterministic ranking, filters, result-type labels, empty-state behavior, preservation of existing word search
- [ ] Unified Search rejects empty, too-short, too-long, and unsupported result-type queries consistently
- [ ] Unified Search PostgreSQL trigram/filter indexes are applied or an explicit production fallback is documented
- [ ] Unified Search seeded performance coverage passes before bulk Phase 7 content generation starts
- [ ] Progress readiness reviewed: user state separated from content, authenticated persistence, anonymous fallback, deterministic recommendations
- [ ] Admin reports readiness reviewed: coverage counts, unresolved links, missing translations, unpublished drafts, missing exercise coverage
- [ ] Mobile parity is explicitly tracked as post-Web work and is not required for this Web release
- [ ] Bulk Phase 7 content generation remains blocked until module contracts, validation, rendering, admin reports, and release checks are stable

---

## H. Sign-Off

- Release owner:
- Validation owner:
- Known accepted issues:
- Final release recommendation:
