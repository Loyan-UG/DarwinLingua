# Web Release Checklist

## Purpose

This checklist is the release gate for `DarwinLingua.Web`.

Use it for both the learner-facing root site and the admin area.

---

## Latest Local Evidence

Last updated: 2026-06-14.

- Local `DarwinLingua.Web` build passed with 0 warnings and 0 errors.
- Local `DarwinLingua.WebApi` build passed with 0 warnings and 0 errors.
- Everyday Expressions pilot smoke on 2026-05-24 passed against local Web/API services backed by `darwinlingua_shared`: Expression list/detail, Web list/detail, Unified Search, and admin system report all returned HTTP 200 after importing `expressions-a1-a2-core-pilot-v1.json`.
- Everyday Expressions small batch 01 smoke on 2026-05-24 passed after importing `expressions-a1-a2-core-01-v1.json`: Web/API list/detail, Unified Search, and admin system report returned HTTP 200 with 37 total expressions, 0 missing translations, and 0 unresolved expression linked words.
- Everyday Expressions stricter eligibility cleanup on 2026-05-24 passed with `tools/Content/Audit-ExpressionContentQuality.js`: 0 content-quality issues, 32 published public Expressions, 5 unpublished ordinary-literal archive findings, 0 admin ordinary-literal leakage, 0 missing eligibility metadata, 0 low example counts, and 0 missing risky-content warnings.
- Full `DarwinLingua.slnx` build passed with 0 warnings and 0 errors when run sequentially with `-m:1`.
- Phase 7 WebApi tests passed for the current admin/report and Learning Portal endpoint surface.
- Phase 7 parser tests passed for Grammar, Expressions, Exercises, Courses, Exam Prep, Writing Templates, and Life in Germany/CulturalNotes.
- Phase 7 structural route/localization tests cover the Web learner routes, WebApi route registrations, and English/German resource keys for the Learning Portal release surface.
- Exercise attempt hardening now separates stateless public evaluation from authenticated persisted attempts, with bounded submitted-answer JSON and endpoint rate limits.
- Exercise runner input now provides structured controls for initial choice, single-answer, error-correction, sentence-ordering, and matching submissions, with advanced JSON kept as a fallback.
- Exercise Engine v1 now projects localized helper text for active learner meaning languages while keeping German source text canonical.
- Course Lessons A1-C2 are generated and imported in local shared PostgreSQL with 6 paths, 56 modules, and 560 lessons (`A1=60`, `A2=80`, `B1=100`, `B2=80`, `C1=120`, `C2=120`).
- Exam Prep Web/API v1 now supports German-first source plus learner-language helper translations for profiles and units. The first generated A1/A2 pilot was rejected and removed on 2026-06-08; release readiness now requires regenerated content from reviewed level/profile planning, clean titles, natural helper translations, and reviewed linked-practice slugs.
- Writing Templates A1-C2 are generated and imported in local shared PostgreSQL with `WritingTemplates=120`, distributed as 20 templates per CEFR level. Local Writing Templates smoke on 2026-06-13 returned HTTP 200 for `/writing-templates`, A1 detail, C2 detail, Persian API detail, and Unified Search `resultType=writing-template`; targeted WritingTemplate ContentOps tests passed.
- Life in Germany A1/A2/B1 foundation content is generated and imported in local shared PostgreSQL with `CulturalNotes=30` (`A1=10`, `A2=10`, `B1=10`). The public Web route is `/life-in-germany`; the internal API/backing store remains `/api/catalog/cultural-notes` and `CulturalNote` until a deliberate internal migration is scheduled.
- Web readiness manual smoke on 2026-06-14 passed in the in-app Chromium browser for `/courses`, `/courses/a1-einstieg-in-den-alltag`, `/exercises`, `/exam-prep`, `/exam-prep/profile/goethe-c1`, a C1 exam-prep detail, `/writing-templates`, an A1 writing-template detail, `/life-in-germany`, an A1 Life in Germany detail, `/search?q=Demokratie&resultType=cultural-note`, and `/recent`. No horizontal overflow was detected on desktop or a 390px mobile viewport for the text-heavy detail/search pages.
- API smoke on 2026-06-14 returned HTTP 200 for Persian helper projection on Life in Germany, Writing Templates, Exam Prep, and Unified Search content queries. Anonymous direct progress/recommendation API calls returned 401 as expected, while the Web `/recent` page rendered its guest fallback.
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
- [x] phase-completion backup created under `X:\Projects\DarwinLingua.Backup` with PostgreSQL dumps, repo restore overlay, separate local config/secret bundle, manifest, restore notes, and checksum verification
  - Evidence: `X:\Projects\DarwinLingua.Backup\20260614-214709-web-readiness-pre-user-testing`.
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
- [x] Deferred mobile package export excludes Sensitive Educational Language before any later mobile work resumes
- [x] Exercise Engine readiness reviewed: deterministic answer evaluation, answer-key safety, attempts, WebApi runner endpoints, Web runner behavior, localized helper projections, and admin quality counters
- [ ] Exercise attempt persistence requires authorization and stores only authenticated user ids
- [ ] Public exercise evaluation is stateless, rate-limited, and does not persist anonymous progress
- [ ] Exercise submitted-answer JSON is bounded, shape-checked, and malformed input returns safe validation errors
- [x] Course Lessons readiness reviewed: course/module/lesson ordering, linked-content projections, lesson routes, localized helper projections, progress hooks where implemented
  - Current status: Course Lessons A1-C2 are generated and imported into `darwinlingua_shared` with 6 paths, 56 modules, and 560 lessons (`A1=60`, `A2=80`, `B1=100`, `B2=80`, `C1=120`, `C2=120`). The A1-C2 source packages are present under `content/learning-portal/courses/packages`. C2 import completed with zero warnings. Local Web/WebApi smoke passed after the PostgreSQL startup retrofit was repaired for the remaining Phase 7 extension tables; service-level admin report tests passed and anonymous admin API access correctly returns 401.
- [x] Course A1 activity-flow checkpoint reviewed
  - Evidence: all 60 A1 lessons now include reviewed `activityBlocks` with 297 ordered activities. PostgreSQL verification after import reports `CourseLessons=560`, `A1ActivityEnabled=60`, `TotalActivityEnabled=60`, `PublishedLessonsWithoutActivityBlocks=500`, and zero unresolved activity targets. API/Web smoke passed for representative A1 lesson details with Persian helper projection. The phase backup is `X:\Projects\DarwinLingua.Backup\20260616-190633-course-a1-activity-flow-complete-pre-a2-activity-flow`.
- [x] Course A2 activity-flow checkpoint reviewed
  - Evidence: all 80 A2 lessons now include reviewed `activityBlocks` with 400 ordered activities. PostgreSQL verification after import reports `CourseLessons=560`, `A1ActivityEnabled=60`, `A2ActivityEnabled=80`, `TotalActivityEnabled=140`, `ActiveLessonsWithoutActivityBlocks=420`, and zero unresolved A2 activity targets. The phase backup is `X:\Projects\DarwinLingua.Backup\20260617-153803-course-a2-activity-flow-complete-pre-b1-activity-flow`.
- [x] Course B1 activity-flow checkpoint reviewed
  - Evidence: all 100 B1 lessons now include reviewed `activityBlocks` with 500 ordered activities. PostgreSQL verification after import reports `CourseLessons=560`, `B1ActivityEnabled=100`, `TotalActivityEnabled=240`, `ActiveLessonsWithoutActivityBlocks=320`, and zero unresolved B1 Module 10 activity targets. API/Web smoke passed for representative B1 lesson details with Persian helper projection. The phase backup is `X:\Projects\DarwinLingua.Backup\20260617-171229-course-b1-activity-flow-complete-pre-b2-activity-flow`.
- [x] Course C1 activity-flow checkpoint reviewed
  - Evidence: all 120 C1 lessons now include reviewed `activityBlocks` with 600 ordered activities. PostgreSQL verification after import reports `CourseLessons=560`, `C1ActivityEnabled=120`, `TotalActivityEnabled=440`, `ActiveLessonsWithoutActivityBlocks=120`, and zero unresolved C1 Module 12 activity targets. API/Web smoke passed for representative C1 lesson details with Persian helper projection. The phase backup is `X:\Projects\DarwinLingua.Backup\20260617-210237-course-c1-activity-flow-complete-pre-c2-activity-flow`.
- [x] Course C2 activity-flow checkpoint reviewed
  - Evidence: all 120 C2 lessons now include reviewed `activityBlocks` with 600 ordered activities. PostgreSQL verification after import reports `CourseLessons=560`, `C2ActivityEnabled=120`, `TotalActivityEnabled=560`, `ActiveLessonsWithoutActivityBlocks=0`, and zero unresolved C2 activity targets. Public Web smoke passed for the final C2 lesson detail, and API detail with `primaryMeaningLanguageCode=fa` returned 5 activity blocks with Persian helper text. A restore-ready staging backup exists at `D:\_Projects\DarwinLingua.Backup.Staging\20260618-073641-course-c2-activity-flow-complete-pre-user-testing`; final sync to `X:\Projects\DarwinLingua.Backup` remains pending while the mapped network drive is disconnected.
- [x] Course A1-C2 restore checkpoint exists after Web/WebApi smoke and PostgreSQL retrofit repair
  - Evidence: `X:\Projects\DarwinLingua.Backup\20260608-154803-course-c2-complete-post-webapi-web-smoke` contains the shared PostgreSQL dump, globals, restore list, dry-run restore Course counts, repo overlay, secret bundle, Docker metadata, manifest, and SHA256 checksums.
- [x] Exam Prep readiness reviewed: profile taxonomy, filters, linked-content projections, WebApi/Web pages, original authored-content policy, title quality, helper-translation quality, and linked-practice coverage
  - Evidence: reviewed Exam Prep foundation and depth packages for A1/A2/DTZ, C1, B1, B2, and Goethe C2 are imported into `darwinlingua_shared` with `ExamProfiles=17`, `ExamPrepUnits=246`, and `GoetheC2Units=86`. Goethe C2 is balanced across reading/listening/speaking/writing/strategy with 17 units each plus one overview. The phase checkpoint is `X:\Projects\DarwinLingua.Backup\20260612-142146-exam-prep-complete-pre-writing-templates`.
- [x] Writing Templates readiness reviewed for v1 baseline: variables, sample filled versions, filters, WebApi/Web pages, linked-content behavior, helper translations, admin quality counters, and Unified Search
  - Evidence: reviewed A1-C2 Writing Templates packages imported into `darwinlingua_shared` with `WritingTemplates=120` (`20` per CEFR level). Local smoke for `/writing-templates`, A1 detail, C2 detail, Persian API detail, and `/api/catalog/search?q=Abschlussstatement&resultType=writing-template` returned HTTP 200 on 2026-06-13; targeted WritingTemplate ContentOps tests passed.
- [x] Life in Germany readiness reviewed: category/context filters, neutral/safe content handling, WebApi/Web pages, linked-content behavior
  - Evidence: Life in Germany A1/A2/B1 foundation packages are imported into `darwinlingua_shared` with `CulturalNotes=30`; public Web route is `/life-in-germany`, API remains `/api/catalog/cultural-notes`, and targeted PostgreSQL repository coverage verifies stable filtering, helper projection, linked slugs, and Unified Search URLs.
- [x] Unified Search readiness reviewed: deterministic ranking, filters, result-type labels, empty-state behavior, preservation of existing word search
  - Evidence: seeded repository/API/Web structural coverage verifies deterministic ranking, filters, labels, safe missing references, and bounded performance; 2026-06-14 local Web smoke returned results for `/search?q=Demokratie&resultType=cultural-note`, and API smoke returned HTTP 200 for `resultType=cultural-note` and `resultType=writing-template`.
- [ ] Unified Search rejects empty, too-short, too-long, and unsupported result-type queries consistently
- [ ] Unified Search PostgreSQL trigram/filter indexes are applied in the target environment, with `pg_trgm` installed or extension-creation privileges available
- [x] Unified Search seeded performance coverage passes before bulk Phase 7 content generation starts
  - Evidence: PostgreSQL seeded bulk-corpus performance coverage verifies bounded results and positive relevance across Course Lessons, Grammar, Writing Templates, and Life in Germany before larger content runs.
- [x] Progress readiness reviewed: user state separated from content, authenticated persistence, anonymous fallback, deterministic recommendations
  - Evidence: structural endpoint coverage verifies authenticated progress summary/update/recommendation routes; Web view coverage verifies course progress indicators, recent activity summary, weak-exercise recommendations, difficult-word recommendations, and deterministic non-AI ranking.
- [x] Admin reports readiness reviewed: coverage counts, unresolved links, missing translations, unpublished drafts, missing exercise coverage
  - Evidence: service and structural tests cover Learning Portal counts, quality counters, issue samples, and the full issue drill-down page with filters and CSV export.
- [x] Mobile parity is explicitly deferred and is not required for this Web release
  - Evidence: target-device validation worksheet is tracked at `artifacts/validation/phase7-mobile-validation-worksheet.md`; mobile exercise runner and account-bound progress sync remain deferred post-Web features.
- [x] Bulk Phase 7 content generation remains blocked until module contracts, validation, rendering, admin reports, and release checks are stable
  - Evidence: current next step is Web readiness and tester feedback; further Life in Germany B2+ content is deferred until this checkpoint is closed.

---

## H. Sign-Off

- Release owner:
- Validation owner:
- Known accepted issues:
- Final release recommendation:
