# Web Release Checklist

## Purpose

This checklist is the release gate for `DarwinLingua.Web`.

Use it for both the learner-facing root site and the admin area.

---

## Latest Local Evidence

Last updated: 2026-05-13.

- Local `DarwinLingua.Web` build passed with 0 warnings and 0 errors.
- Local `DarwinLingua.WebApi` build passed with 0 warnings and 0 errors.
- Everyday Expressions pilot smoke on 2026-05-24 passed against local Web/API services backed by `darwinlingua_shared`: Expression list/detail, Web list/detail, Unified Search, and admin system report all returned HTTP 200 after importing `expressions-a1-a2-core-pilot-v1.json`.
- Everyday Expressions small batch 01 smoke on 2026-05-24 passed after importing `expressions-a1-a2-core-01-v1.json`: Web/API list/detail, Unified Search, and admin system report returned HTTP 200 with 37 total expressions, 0 missing translations, and 0 unresolved expression linked words.
- Everyday Expressions stricter eligibility cleanup on 2026-05-24 passed with `tools/Content/Audit-ExpressionContentQuality.js`: 0 content-quality issues, 32 published public Expressions, 5 unpublished ordinary-literal archive findings, 0 admin ordinary-literal leakage, 0 missing eligibility metadata, 0 low example counts, and 0 missing risky-content warnings.
- Full `DarwinLingua.slnx` build passed with 0 warnings and 0 errors when run sequentially with `-m:1`.
- Phase 7 WebApi tests passed for the current admin/report and Learning Portal endpoint surface.
- Phase 7 parser tests passed for Grammar, Expressions, Exercises, Courses, Exam Prep, Writing Templates, and Cultural Notes.
- Phase 7 structural route/localization tests cover the Web learner routes, WebApi route registrations, and English/German resource keys for the Learning Portal release surface.
- Exercise attempt hardening now separates stateless public evaluation from authenticated persisted attempts, with bounded submitted-answer JSON and endpoint rate limits.
- Exercise runner input now provides structured controls for initial choice, single-answer, error-correction, sentence-ordering, and matching submissions, with advanced JSON kept as a fallback.
- Unified Learning Search now has application-level query limits, a PostgreSQL trigram-index migration, and startup-applied trigram/filter indexes for existing shared database tables.
- `DarwinLingua.Web` no longer registers or initializes a local SQLite learning/content database; Web user/account state requires PostgreSQL/Npgsql through `WebIdentityDbContext`.
- WebApi mobile content distribution now supports module-scoped `catalog-module` manifests/downloads for selective mobile first-run content selection.
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
- [ ] registration requires Terms of Use acceptance
- [ ] registration shows a clear Privacy Policy notice link and acknowledgement without mislabeling the Privacy Policy as optional marketing consent
- [ ] versioned policy acceptance records are stored for account registration acknowledgements
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

## E2. Legal And Compliance Baseline

This section is a release blocker. See `86-Web-Legal-Compliance-Baseline.md`.

- [ ] Legal Notice / Impressum page renders and has reviewed production operator data
- [ ] Privacy Policy page is reviewed and includes account, learning, policy acceptance, sensitive preference, transactional email, billing-provider, retention, and data-subject-rights coverage
- [ ] Terms of Use page is reviewed and linked from registration
- [ ] Cookie / Storage Notice page renders and matches the latest cookie/storage inventory
- [ ] Contact page provides support and privacy-request routing
- [ ] footer links include Privacy, Terms, Legal Notice, Cookie / Storage Notice, and Contact
- [ ] non-essential cookies, browser storage, analytics, or marketing scripts are blocked until opt-in if introduced
- [ ] cookie/storage consent withdrawal is as easy as opt-in if a future consent manager is required
- [ ] data-subject request owner and process are documented
- [ ] account deletion/export/rectification plan is documented
- [ ] policy acceptance records are available for required registration acknowledgements
- [ ] Sensitive Educational Language opt-in remains separate from registration, off by default, and reversible
- [ ] explicit adult/pornographic content remains blocked until legal review and approved age-verification/closed-user-group design
- [ ] transactional email provider processing/DPA requirements are reviewed
- [ ] billing provider legal text, cancellation/refund flow, and Stripe processing are reviewed if billing is enabled
- [ ] production legal owner/sign-off is recorded
- [ ] mobile legal/privacy/store-compliance work remains deferred until the Web phase is signed off

---

## F. Operational Readiness

- [ ] production configuration applied
- [ ] database connectivity verified
- [ ] Web PostgreSQL/Npgsql identity connection string is configured; no local `darwin-lingua.web.db` startup path is used
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
- [ ] Everyday Expressions eligibility reviewed: no published ordinary literal sentence leakage, `meaningTransparency` present for new batches, teaching reason present, and at least two contextual German examples
- [ ] Standalone RoleplayScenario readiness reviewed: parser/import validation, persistence, WebApi list/detail, Web list/detail, Unified Search, admin visibility, deterministic no-AI behavior, image-slot missing-asset behavior, and local import/smoke all pass before any post-pilot batch generation
  - Evidence: the first A1-B2 pilot package imported into `darwinlingua_shared` with zero warnings on 2026-05-25; local `/roleplays`, detail, `/api/catalog/roleplays`, detail API, `/api/catalog/search?resultType=roleplay`, and authenticated `/api/admin/catalog/system-report` smoke returned HTTP 200. Service-level admin report tests cover RoleplayScenario count visibility.
- [ ] Sensitive Educational Language policy reviewed
- [ ] registration/legal notice coverage reviewed for Terms, Privacy, and Sensitive Educational Language default-off behavior
- [ ] Settings/profile explanation for Sensitive Educational Language is clear, localized, and does not claim age verification
- [ ] Sensitive Educational Language entries are hidden from anonymous users and users without opt-in
- [ ] Explicit-adult and blocked-illegal Expressions remain blocked even when Sensitive Educational Language is enabled
- [ ] Web/API/search filtering for Sensitive Educational Language is verified
- [ ] Unified Search excludes sensitive and adult-only Expressions by default
- [ ] Admin reports show Expression counts by safety rating, sensitive content kind, age requirement, opt-in requirement, missing warnings, missing teaching reasons, and ordinary-literal leakage
- [ ] Mobile package export excludes Sensitive Educational Language until mobile eligibility enforcement and warning rendering are ready
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
- [ ] Unified Search PostgreSQL trigram/filter indexes are applied in the target environment, with `pg_trgm` installed or extension-creation privileges available
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
