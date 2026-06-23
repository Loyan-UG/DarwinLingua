# Web Release Checklist

## Purpose

This checklist is the release gate for `DarwinLingua.Web`.

Use it for both the learner-facing root site and the admin area.

---

## Latest Local Evidence

Last updated: 2026-06-23.

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
- PWA shell readiness was hardened on 2026-06-18: manifest delivery, service-worker registration, first-party shell cache creation, and offline navigation fallback were verified in local desktop Chromium. With the Web host stopped, a controlled browser session loaded `/offline-smoke-check` from the cached `offline.html` shell. Targeted structural tests for the manifest, service worker, install prompt wiring, and offline shell passed.
- PWA follow-up on 2026-06-19 regenerated `artifacts/installability-report.json` against `https://localhost:7501`: `17` automated checks passed, `0` failed, and `2` manual checks remain for real Desktop Chrome/Edge and Android Chrome prompt acceptance. The homepage 404 collection image and htmx inline-style CSP console error were fixed before the report was regenerated.
- Web readiness checkpoint commits are selected: `a7fef927 Complete web readiness checkpoint` and `07833401 Document Brevo production handoff`, with follow-up hardening in `2a59a0a6 Cover account email workflow structure`, `093d8001 Refresh Life in Germany legal research gate`, `bf3035f7 Add Life in Germany legal content gate`, `5c3aed83 Polish Brevo email handoff`, and `c57f7169 Cover identity email token failures`.
- Production sign-off still requires the unchecked release gates below, especially manual target-browser install validation, production configuration, mailbox/webhook event observation, legal/operator review, and Stripe test-mode/staging validation where billing is enabled.
- Brevo production readiness now has repeatable operator gates: `tools/Web/Invoke-BrevoProductionReadinessCheck.ps1` writes JSON/Markdown evidence under `artifacts/validation/brevo-readiness/`; `tools/Web/Invoke-BrevoRealDeliverySmoke.ps1` verifies direct provider delivery; `tools/Web/Invoke-WebAccountEmailFlowSmoke.ps1` verifies registration and password-reset email flow through the public Web UI and `WebEmailDeliveryLogs`; `tools/Web/Invoke-BrevoWebhookSuppressionSmoke.ps1` verifies the public webhook endpoint, Bearer-token authentication, failed delivery-log update, and internal suppression creation without sending to a real recipient; `tools/Web/Invoke-BrevoSuppressedSendSmoke.ps1` verifies that a later account email to a suppressed recipient is logged as `Suppressed` without calling Brevo; and `tools/Web/Invoke-WebEmailDiagnosticsAdminSmoke.ps1` verifies the authenticated Admin Email Diagnostics UI shows provider message ids, provider events, suppression data, readiness state, and Admin-only controls. The production handoff also has a Persian operator guide in `docs/89-Brevo-Operator-Handoff.fa.md`, and transactional email templates now render action links as email-safe CTA buttons.
- Runtime/bootstrap readiness now has repeatable evidence from `tools/Web/Invoke-WebOperationsBootstrapCheck.ps1`, which verifies local secret-backed connection settings, WebApi routing settings, admin notification recipient presence, public Web/API health, required Web identity/user-state tables, and Brevo event columns without printing secret values.
- Operational incident ownership is documented in `docs/90-Web-Operational-Incident-Runbook.md` for Brevo key rotation, sender-domain failure, webhook failure, DNS/Cloudflare incidents, privacy requests, security/breach triage, community escalation, and backup/restore.

---

## A. Build And Test

- [x] release commit selected
  - Evidence: Web readiness is captured in `a7fef927 Complete web readiness checkpoint`; Brevo operator handoff follow-up is captured in `07833401 Document Brevo production handoff`; follow-up hardening is captured in `2a59a0a6 Cover account email workflow structure`, `093d8001 Refresh Life in Germany legal research gate`, `bf3035f7 Add Life in Germany legal content gate`, `5c3aed83 Polish Brevo email handoff`, and `c57f7169 Cover identity email token failures`.
- [x] solution build succeeded
  - Evidence: 2026-06-18 active Web-scope source build passed for all 22 non-MAUI source projects, including `DarwinLingua.Web`, `DarwinLingua.WebApi`, `DarwinLingua.ImportTool`, shared building blocks, and Catalog/ContentOps/Learning/Localization/Practice modules. Mobile/MAUI remains intentionally deferred and is not part of the active Web release gate.
- [x] automated tests succeeded
  - Evidence: 2026-06-18 active Web-scope automated test sweep passed for 13 non-MAUI test projects. The deferred `Localization.Application.Tests` project still contains MAUI source-level readiness tests and is excluded from the active Web release gate until mobile work resumes.
- [x] web-specific structural tests succeeded
  - Evidence: 2026-06-18 `DarwinLingua.WebApi.Tests` passed 194/194 after hardening Dialogues anonymous-user handling, transactional email diagnostics, account recovery flows, and WebApi/Web structural coverage.

---

## B. Learner Web

- [x] learner shell renders correctly
  - Evidence: 2026-06-18 public-routed tester preflight `artifacts/validation/web-tester-runs/20260618-190132-web-tester-expanded-learner-admin-smoke/preflight/web-tester-preflight-20260618-190135.json` returned HTTP 200 for the Web home page with the expected Darwin Lingua shell.
- [x] browse by CEFR works
  - Evidence: the same preflight returned HTTP 200 for `/browse/cefr/A1`.
- [x] browse by topic works
  - Evidence: the same preflight returned HTTP 200 for `/browse/topic/everyday-life`.
- [x] search works
  - Evidence: the same preflight returned HTTP 200 for `/search?q=Termin` and found the expected query text.
- [x] word detail works
  - Evidence: the same preflight returned HTTP 200 for `/words/das-haus` and found the expected German word title.
- [x] favorites work
  - Evidence: the same preflight returned HTTP 200 for `/favorites`.
- [x] recent activity works
  - Evidence: the same preflight returned HTTP 200 for `/recent`.
- [x] settings work
  - Evidence: the same preflight returned HTTP 200 for `/settings`.

---

## C. Admin Web

- [x] admin layout renders correctly
  - Evidence: 2026-06-18 authenticated local admin smoke `artifacts/validation/web-admin-smoke/web-admin-authenticated-smoke-20260618-190535.json` logged in with the local seed admin and returned HTTP 200 for the dashboard, reports, diagnostics, publishing, imports, words, topics, users, moderation, billing diagnostics, and email diagnostics pages without falling back to the login page.
- [x] admin overview loads
  - Evidence: the same authenticated smoke returned HTTP 200 for `/admin`.
- [x] publishing page loads
  - Evidence: the same authenticated smoke returned HTTP 200 for `/admin/publishing`.
- [x] diagnostics page loads
  - Evidence: the same authenticated smoke returned HTTP 200 for `/admin/diagnostics`.
- [x] reports page loads Learning Portal coverage and quality checks
  - Evidence: the same authenticated smoke returned HTTP 200 for `/admin/reports` and found the expected Learning Portal report text.
- [x] transactional email diagnostics page loads
  - Evidence: the same authenticated smoke returned HTTP 200 for `/admin/email-diagnostics`.
- [x] admin/learner separation is preserved
  - Evidence: 2026-06-18 public-routed tester preflight `artifacts/validation/web-tester-runs/20260618-190132-web-tester-expanded-learner-admin-smoke/preflight/web-tester-preflight-20260618-190135.json` verified anonymous requests to `/admin`, `/admin/reports`, and `/admin/email-diagnostics` redirect to the login surface with `ReturnUrl` preserved.

---

## D. PWA

- [x] manifest is delivered
  - Evidence: 2026-06-18 local HTTPS smoke returned HTTP 200 and `application/manifest+json` for `/manifest.webmanifest`; desktop Chromium page inspection loaded the manifest link and parsed `name=Darwin Lingua`, `display=standalone`, `id=/`, and `scope=/`.
- [x] service worker registers
  - Evidence: 2026-06-18 desktop Chromium registered the service worker at scope `https://localhost:7501/`, created cache `darwin-lingua-shell-v3`, and cached `/offline.html`, manifest, and icon assets.
- [x] offline shell fallback is available
  - Evidence: 2026-06-18 with the Web host stopped after online load, desktop Chromium navigated to `/offline-smoke-check` and rendered cached `Offline - Darwin Lingua` from the service worker.
- [ ] install flow validated on target browsers
- Desktop installability baseline evidence is generated by `tools/Web/New-WebPwaInstallabilityReport.ps1`; the 2026-06-19 report at `artifacts/installability-report.json` passed 17 automated checks and left 2 manual checks for real install prompt acceptance on desktop Chrome/Edge and Android Chrome.
- Validation evidence may be attached from `artifacts/installability-report.json` and `artifacts/installability-report-android.json`.

---

## E. Account And Transactional Email Readiness

This section is a release blocker. See `73-Transactional-Email-And-Account-Communication-Backlog.md`.

- [x] email confirmation is sent after registration
  - Evidence: 2026-06-18 `WebRegistrationLegalAcknowledgementTests` verifies registration calls `GenerateEmailConfirmationTokenAsync` and `SendEmailConfirmationAsync`; public smoke returned HTTP 200 for `/Identity/Account/Register`. 2026-06-23 app-level public smoke `artifacts/validation/web-account-email-smoke/web-account-email-smoke-20260623-070951.md` registered a test learner through `https://darwinlingua.com`, created the user, and logged `Account.EmailConfirmation` through provider `brevo-api` with a provider message id.
- [x] registration requires Terms of Use acceptance
  - Evidence: 2026-06-18 `WebRegistrationLegalAcknowledgementTests` verifies the registration page includes `Input.AcceptTermsOfUse`, links to Terms, validates required acknowledgements, and records registration policy acceptances.
- [x] registration shows a clear Privacy Policy notice link and acknowledgement without mislabeling the Privacy Policy as optional marketing consent
  - Evidence: 2026-06-18 `WebRegistrationLegalAcknowledgementTests` verifies `Input.AcknowledgePrivacyNotice`, the Privacy link, and separate privacy acknowledgement copy. `WebLegalComplianceBaselineTests` verifies English/German resource keys for the Privacy acknowledgement.
- [x] versioned policy acceptance records are stored for account registration acknowledgements
  - Evidence: 2026-06-18 `PolicyAcceptanceService_RecordsVersionedRegistrationAcceptances` passed against PostgreSQL and verified Terms and Privacy records store policy key, current version, source, and culture.
- [x] confirmation callback works
  - Evidence: 2026-06-18 `WebAccountAuthenticationWorkflowTests` verifies the confirmation callback decodes Base64Url tokens, handles malformed tokens safely, and calls `ConfirmEmailAsync`.
- [x] resend confirmation flow works
  - Evidence: 2026-06-18 `WebAccountAuthenticationWorkflowTests` verifies resend confirmation rate limiting, token generation, confirmation email sending, and enumeration-resistant redirect to `/Account/CheckEmail`.
- [x] forgot-password request works without account enumeration
  - Evidence: 2026-06-18 `WebAccountAuthenticationWorkflowTests` verifies forgot-password IP/email rate limits, redirects to `/Account/ForgotPasswordConfirmation` for blocked or unknown/unconfirmed accounts, and only sends reset emails for confirmed users. Public smoke returned HTTP 200 for `/Identity/Account/ForgotPassword`. 2026-06-23 app-level public smoke `artifacts/validation/web-account-email-smoke/web-account-email-smoke-20260623-070951.md` requested password reset through `https://darwinlingua.com` and logged `Account.PasswordReset` through provider `brevo-api` with a provider message id.
- [x] password reset link works
  - Evidence: 2026-06-18 `WebAccountAuthenticationWorkflowTests` verifies reset links decode Base64Url tokens and call `ResetPasswordAsync`.
- [x] expired and invalid reset tokens fail safely
  - Evidence: 2026-06-18 `WebAccountAuthenticationWorkflowTests` verifies malformed reset codes and `InvalidToken`/`ExpiredToken` errors render a safe reusable-link error instead of completing the reset.
- [x] password reset success notification is sent
  - Evidence: 2026-06-18 `WebAccountAuthenticationWorkflowTests` verifies successful reset updates the security stamp and calls `SendPasswordResetCompletedAsync`.
- [x] localized English email templates render correctly
  - Evidence: 2026-06-18 `TransactionalEmailTemplateRenderer_ShouldRenderEnglishGermanAndFallbackSafely` verifies English account confirmation subject/body rendering, value substitution, HTML encoding, and fallback behavior.
- [x] localized German email templates render correctly
  - Evidence: 2026-06-18 `TransactionalEmailTemplateRenderer_ShouldRenderEnglishGermanAndFallbackSafely` verifies German account confirmation subject/body rendering for `de-DE`.
- [x] transactional email delivery log is available to admins/operators
  - Evidence: 2026-06-18 `/admin/email-diagnostics` is linked from the admin dashboard and protected by the `Operator` policy. `AdminDashboardRouteStructuralTests` verifies the diagnostics page exposes delivery status, scenario, provider metadata, recipient hash, failure summary, suppressions, readiness warnings, filtering, retention cleanup, manual provider-event reconciliation, and admin-only suppression controls without exposing email body fields.
- [x] email-triggering endpoints are rate-limited
  - Evidence: 2026-06-18 `WebAccountAuthenticationWorkflowTests` verifies registration, resend confirmation, and forgot-password flows call `IAccountEmailRateLimiter`; login lockout notification is also guarded by `account-lockout` rate limiting.
- [x] production public base URL is configured correctly for email links
  - Evidence: 2026-06-23 Brevo readiness and app-level account email smoke use expected public base URL `https://darwinlingua.com`; the app-level account email smoke exercised public Web account flows through the same origin and verified Brevo delivery logs.
- [x] no reset/confirmation token or full recovery URL is logged
  - Evidence: 2026-06-18 `DeliveryLogRepository_ShouldStoreDiagnosticsWithoutEmailBodyOrRecoveryUrl` verifies delivery logs persist operational diagnostics without storing email body, reset/confirmation token values, or full recovery URLs. `EmailDeliveryLogRepository` now redacts URL-like diagnostic text before persisting failure summaries and provider event reasons; `EmailDiagnostics_ShouldExposeDeliveryLogWithoutEmailBodyOrRecoveryUrls` verifies the admin diagnostics model/view do not expose `PlainTextBody`, `HtmlBody`, `ActionUrl`, callback URLs, reset URLs, or confirmation URLs.

---

## E2. Legal And Compliance Baseline

This section is a release blocker. See `86-Web-Legal-Compliance-Baseline.md`.

- [x] Legal Notice / Impressum page renders and has reviewed production operator data
  - Evidence: 2026-06-22 operator data is configured in `src/Apps/DarwinLingua.Web/appsettings.json` for Shahram Vafadar, Achterkirchenstrasse 10, 37154 Northeim, Germany, and `info@darwinlingua.com`. `/legal` and `/impressum` read the values from `LegalNotice:*` configuration instead of showing placeholder launch copy. `WebLegalComplianceBaselineTests` verifies the configured operator baseline. Local HTTPS smoke returned 200 for `/legal` and `/impressum`, found `Shahram Vafadar`, and did not find the missing-configuration warning.
- [x] Privacy Policy page is reviewed and includes account, learning, policy acceptance, sensitive preference, transactional email, billing-provider, retention, and data-subject-rights coverage
  - Evidence: 2026-06-22 `Privacy.cshtml` covers account/learning data, policy acceptance, sensitive preference, transactional emails, provider/support processing, retention, export/delete controls, breach/privacy request routing, and the likely Lower Saxony supervisory authority. Local HTTPS smoke returned 200 for `/privacy` and found the Niedersachsen authority text. This is an engineering/legal baseline pending final owner/counsel review.
- [x] Terms of Use page is reviewed and linked from registration
  - Evidence: 2026-06-18 registration structural tests verify Terms acceptance and policy acceptance records; 2026-06-20 Terms/Contact copy added illegal-content, security-abuse, privacy, and rights-reporting boundaries.
- [x] current legal research pass covers DDG, TDDDG, GDPR/BDSG risk areas, UWG email-marketing boundaries, DSA/community-report boundaries, BFSG accessibility/e-commerce risk, BGB subscription-cancellation risk, VSBG consumer-dispute monitoring, and StGB/KJM content and security-abuse risk
  - Evidence: `docs/86-Web-Legal-Compliance-Baseline.md` has a 2026-06-20 legal research snapshot with official-source references and crime/fine review notes, plus 2026-06-20, 2026-06-22, and 2026-06-23 official-source refreshes for BAMF LiD/Einbuergerungstest wording, TDDDG/DSK terminal-device guidance, DSC/Bundesnetzagentur DSA routing, BMG/KCanG cannabis limits, DDG provider-information/fine risks, GDPR data-subject and breach duties, BFSG accessibility/e-commerce applicability before paid consumer flows, VSBG/BGB billing deferrals, and the likely Niedersachsen data-protection authority. This is engineering research, not final legal counsel sign-off.
- [x] Life in Germany legal/civic source-refresh gate is documented before further legal-adjacent content expansion
  - Evidence: `artifacts/planning/life-in-germany-content-plan.md` records the 2026-06-20 official-source refresh for BAMF Orientierungskurs/LiD/Einbuergerungstest framing, Grundgesetz basics, 2024 nationality-law reform, Chancenkarte, Cannabisgesetz, SBGG, everyday crime/fine boundaries, and help/escalation routes. It now explicitly separates real test-sheet numbers from the interactive and downloadable BAMF catalog descriptions so future content does not hard-code a misleading single total. `docs/86-Web-Legal-Compliance-Baseline.md` treats that gate as required for future Life in Germany legal/civic batches.
- [x] Cookie / Storage Notice page renders and matches the latest cookie/storage inventory
  - Evidence: 2026-06-18 `WebLegalComplianceBaselineTests` verifies the Cookie/Storage Notice route, page copy, and `artifacts/validation/web-cookie-storage-inventory.md`; public smoke returned HTTP 200 for `/cookies`.
- [x] Contact page provides support and privacy-request routing
  - Evidence: 2026-06-18 `WebLegalComplianceBaselineTests` verifies Contact route/configuration usage; public smoke returned HTTP 200 for `/contact`.
- [x] footer links include Privacy, Terms, Legal Notice, Cookie / Storage Notice, and Contact
  - Evidence: 2026-06-18 `Footer_ShouldLinkRequiredLegalPages` verifies the shared footer links all required legal/contact pages.
- [x] non-essential cookies, browser storage, analytics, or marketing scripts are blocked until opt-in if introduced
  - Evidence: 2026-06-18 `WebLegalComplianceBaselineTests` verifies the cookie/storage inventory states no marketing cookies and no cookie banner requirement; no analytics or marketing scripts are present in the Web legal baseline.
- [ ] cookie/storage consent withdrawal is as easy as opt-in if a future consent manager is required
- [x] data-subject request owner and process are documented
  - Evidence: Shahram Vafadar is recorded as the responsible operator. `/account` provides self-service export/delete, `/contact` routes privacy requests, and `docs/86-Web-Legal-Compliance-Baseline.md` records the one-month GDPR response timing baseline plus the need for operator escalation for non-self-service requests.
- [x] account deletion/export/rectification plan is documented
  - Evidence: `/account` exposes self-service account data export, rectification links, and account deletion with password/`DELETE` confirmation where applicable. `docs/86-Web-Legal-Compliance-Baseline.md` records the implementation, retention boundaries, and remaining operator escalation duties. 2026-06-23 `WebAccountDataSelfServiceStructuralTests` verifies export/deletion routes, anti-forgery, password/confirmation controls, retained operational-audit boundaries, account-deleted email notification wiring, service registration, and explicit transaction use around Web identity/user-state and shared learning cleanup.
- [x] breach triage owner, illegal-content report owner, and community moderation escalation process are assigned before broad public community release
  - Evidence: `docs/90-Web-Operational-Incident-Runbook.md` assigns Shahram Vafadar as the Web testing operator, privacy/data-subject owner, Brevo/DNS owner, community moderation and illegal-content escalation owner, and backup/restore decision owner. It defines severity levels and breach, privacy, DNS, webhook, and Brevo incident flows.
- [x] policy acceptance records are available for required registration acknowledgements
  - Evidence: 2026-06-18 `WebIdentityBootstrapper_ShouldCreatePolicyAcceptanceTable` verifies `WebPolicyAcceptances` and its unique user/policy/version index; `PolicyAcceptanceService_RecordsVersionedRegistrationAcceptances` verifies records are persisted.
- [x] Sensitive Educational Language opt-in remains separate from registration, off by default, and reversible
  - Evidence: 2026-06-18 policy and structural coverage verify registration/legal acknowledgement is separate from the Settings-only `Show sensitive educational language` preference. Web Expressions read the persisted learner profile setting, Settings exposes a reversible checkbox, and public API query-string requests cannot unlock sensitive educational content without admin credentials.
- [x] explicit adult/pornographic content remains blocked until legal review and approved age-verification/closed-user-group design
  - Evidence: `docs/85-Sensitive-Educational-Language-Policy.md` blocks pornographic, arousing, explicit adult, exploitative, coercive, and minor-related sexual content. The policy requires a separate verified-adult access system and legal review before any future reconsideration, and current content generation/import gates keep that category out of Web testing.
- [x] Terms and Contact pages include user-facing misuse/reporting boundaries for illegal content, security abuse, privacy concerns, and rights issues
  - Evidence: 2026-06-20 Terms/Contact copy was updated to describe prohibited illegal/security-abuse content and the support route for security, abuse, illegal-content, privacy, and rights reports.
- [x] transactional email provider processing/DPA requirements are reviewed
  - Evidence: Brevo DPA was accepted by the operator. 2026-06-23 readiness evidence `artifacts/validation/brevo-readiness/brevo-production-readiness-20260623-070910.md` confirms `operator.dpaAccepted`, sender verification, DNS authentication, webhook configuration, API key, webhook secret, sandbox disabled, query-string fallback disabled, and live Brevo API reachability with `0` blockers and `0` warnings.
- [ ] billing provider legal text, cancellation/refund flow, and Stripe processing are reviewed if billing is enabled
- [x] production legal owner/sign-off is recorded for controlled Web testing
  - Evidence: Shahram Vafadar is recorded as provider, responsible person, legal/data-protection contact, and operational owner for the current controlled Web testing phase. Final legal counsel review remains recommended before broad public launch.
- [x] mobile legal/privacy/store-compliance work remains deferred until the Web phase is signed off
  - Evidence: mobile/MAUI work is explicitly out of active scope for the current Web release readiness goal; no MAUI/mobile files were changed in this pass.

---

## F. Operational Readiness

- [ ] production configuration applied
- [x] database connectivity verified
  - Evidence: 2026-06-18 local Web/WebApi smoke returned `200` for `https://localhost:7501/`, `https://localhost:7501/courses`, and `http://localhost:5099/health`; public smoke returned `200` for `https://lingua.vafadar.pro/`, `/courses`, and `https://linguaapi.vafadar.pro/health`. 2026-06-23 Darwin-domain public stack helper `tools/Web/Start-WebPublicDevStack.ps1 -StopExisting` generated `artifacts/validation/web-public-stack/web-public-stack-20260623-070918.md` with smoke passed for `https://darwinlingua.com`, `/legal`, `/privacy`, and `https://api.darwinlingua.com/health`. PostgreSQL verification against `darwinlingua_shared` returned `CourseLessons=560`, `WritingTemplates=120`, `ExamPrepUnits=246`, `CulturalNotes=30`, and `AspNetUsers=7`.
- [x] Web PostgreSQL/Npgsql identity connection string is configured; no local `darwin-lingua.web.db` startup path is used
  - Evidence: 2026-06-18 `WebRuntimeBootstrapStructuralTests` verified Web/WebApi PostgreSQL/Npgsql startup configuration, and source search found no `darwin-lingua.web.db` startup path under Web/WebApi.
- [x] security headers verified
  - Evidence: 2026-06-18 public checks against `https://lingua.vafadar.pro/` and `https://linguaapi.vafadar.pro/health` returned `Strict-Transport-Security: max-age=31536000; includeSubDomains`, `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: strict-origin-when-cross-origin`, and a restrictive `Permissions-Policy`; Web also returns the expected app CSP. Targeted `WebRuntimeBootstrapStructuralTests` and Web/WebApi builds passed after adding the runtime guard.
- [x] logging baseline verified
  - Evidence: 2026-06-18 Web/WebApi appsettings keep `Microsoft.AspNetCore` framework logging at `Warning`, Web registers `WebRequestTelemetryMiddleware` and `WebPerformanceTelemetryService`, and restart/smoke logs recorded startup, database commands, WebApi calls, and `Web perf` request summaries for `/` and `/courses` without startup errors.
- [x] Learning Portal unresolved-link, missing-translation, unpublished-draft, and seed coverage reports reviewed
  - Evidence: 2026-06-18 targeted Admin Reports tests passed for Learning Portal issue drilldown, summary counters, and `WebsiteAdminQueryService` report coverage. Authenticated local admin smoke after Web/WebApi restart passed 11/11 admin routes including `/admin/reports`; report artifact: `artifacts/validation/web-admin-smoke/web-admin-authenticated-smoke-20260618-191753.json`.
- [x] phase-completion backup created under `X:\Projects\DarwinLingua.Backup` with PostgreSQL dumps, repo restore overlay, Git restore bundle, separate local config/secret bundle, manifest, restore notes, and checksum verification
  - Evidence: latest Web/domain/Brevo/legal checkpoint is `X:\Projects\DarwinLingua.Backup\20260623-082328-web-email-change-links-ready-pre-user-testing`. It contains PostgreSQL dump, restore list, globals, repo overlay, `repo-overlay/darwinlingua-current-head.bundle` for local commits that are not yet on the remote, separate `secrets/` bundle including ASP.NET user-secrets, Docker metadata, `manifest.md`, and `checksums.sha256`. The backup verification confirmed the Brevo API key and webhook token are present in the separate secret bundle without printing values.
- [x] production email provider configured
  - Evidence: 2026-06-23 Brevo readiness report `artifacts/validation/brevo-readiness/brevo-production-readiness-20260623-070910.md` passed `config.mode=BrevoApi`, `secret.apiKey`, `secret.webhookSecret`, `config.sandbox=false`, `config.querySecretFallback=false`, `operator.webhookConfigured`, and `brevo.accountApi` with `0` blockers and `0` warnings.
- [x] Brevo live API reachability from the current host is verified
  - Evidence: `artifacts/validation/brevo-readiness/brevo-production-readiness-20260623-070910.md` was generated with `-VerifyBrevoApi -RequireRealDelivery`, `blockerCount=0`, and `warningCount=0`.
- [x] Brevo real inbox/provider smoke is sent
  - Evidence: `artifacts/validation/brevo-real-delivery-smoke/brevo-real-delivery-smoke-20260622-230358.md` reports `Status: sent` for `Account.EmailConfirmation` and `Account.PasswordReset` to `info@darwinlingua.com`, with Brevo provider message ids recorded. 2026-06-23 app-level Web flow smoke `artifacts/validation/web-account-email-smoke/web-account-email-smoke-20260623-070951.md` also sent registration confirmation and password reset through public Web and logged provider message ids. Manual inbox and webhook-log review should still confirm visual rendering and provider event arrival before broad tester self-registration.
- [x] sender address and reply-to address configured
  - Evidence: 2026-06-22 Brevo readiness report passed syntactic checks for FromEmail, ReplyToEmail, and SupportEmail and confirmed FromEmail uses `darwinlingua.com`.
- [x] SPF, DKIM, and DMARC verified for the sender domain
  - Evidence: 2026-06-22 Brevo readiness report passed `operator.dnsAuthenticated`, `dns.spf`, and `dns.dmarc`; DKIM is covered by the operator-confirmed Brevo domain-authentication gate because the exact DKIM record is account/domain-specific.
- [x] email provider processing/DPA requirements reviewed where applicable
  - Evidence: 2026-06-22 Brevo readiness report passed `operator.dpaAccepted`; `docs/89-Brevo-Operator-Handoff.fa.md` records the Brevo DPA and webhook event setup.
- [x] rollback owner identified
  - Evidence: rollback owner is the operator/release owner during Web testing. `tools/Web/Invoke-WebReadinessRollback.ps1` provides dry-run by default and requires `-ConfirmRollback` before applying repo overlay or database restore.

---

## G. Phase 7 Learning Portal Release Gates

- [x] Grammar Guide readiness reviewed: import validation, WebApi list/detail, Web list/detail, localization, safe missing-link behavior
  - Evidence: 2026-06-18 `ContentImportParserGrammarTopicTests` passed 9/9, `GrammarTopicRepositoryTests` passed 2/2 against PostgreSQL, and `GrammarRouteStructuralTests` passed 2/2 after adding canonical `/api/catalog/grammar-topics` list/detail aliases while keeping legacy `/api/catalog/grammar` routes. Public smoke returned HTTP 200 for `https://lingua.vafadar.pro/grammar`, `https://linguaapi.vafadar.pro/api/catalog/grammar-topics`, and legacy `/api/catalog/grammar`; detail smoke for `a1-personal-pronouns-ich-du-er-sie-es?primaryMeaningLanguageCode=fa` returned Persian helper title/sections/examples. PostgreSQL `darwinlingua_shared` has `225` active GrammarTopics.
- [x] Everyday Expressions readiness reviewed: import validation, warning metadata, WebApi list/detail, Web list/detail, localization, safe missing-link behavior
  - Evidence: 2026-06-18 targeted Expression tests passed for `ContentOps.Application` 12/12, `ContentOps.Infrastructure` 3/3, `Catalog.Infrastructure` 5/5, and WebApi sensitive/search structural coverage 14/14. `tools/Content/Audit-ExpressionContentQuality.js` reported 18 packages and 0 issues. Public smoke returned HTTP 200 for `https://lingua.vafadar.pro/expressions` and `https://linguaapi.vafadar.pro/api/catalog/expressions`; detail smoke for `alles-klar?primaryMeaningLanguageCode=fa` returned Persian meaning, usage explanation, and example translations.
- [x] Everyday Expressions eligibility reviewed: no published ordinary literal sentence leakage, `meaningTransparency` present for new batches, teaching reason present, and at least two contextual German examples
  - Evidence: 2026-06-18 PostgreSQL verification against `darwinlingua_shared` found `575` total Expressions, `570` active Expressions, `159` active sensitive-opt-in Expressions, and `0` active Expressions missing `TeachingReason`; the content-quality audit reported 0 issues. `SensitiveEducationalLanguageStructuralTests` verifies Web profile-bound sensitive eligibility, no public query-string unlock, admin API guardrails, repository/search default filters, admin counters, Settings copy, and usage policy copy.
- [x] Standalone RoleplayScenario readiness reviewed: parser/import validation, persistence, WebApi list/detail, Web list/detail, Unified Search, admin visibility, deterministic no-AI behavior, image-slot missing-asset behavior, and local import/smoke all pass before any post-pilot batch generation
  - Evidence: 2026-06-18 targeted RoleplayScenario tests passed for `ContentOps.Infrastructure` 1/1 parser coverage, `ContentOps.Application` 9/9 import/validation coverage, `Catalog.Infrastructure` 1/1 PostgreSQL repository/search projection coverage, and `RoleplayScenarioRouteStructuralTests` 2/2 WebApi/Web structural coverage. Public smoke returned HTTP 200 for `https://lingua.vafadar.pro/roleplays`, `https://lingua.vafadar.pro/roleplays/c1-eine-strategie-im-team-kritisch-pruefen`, `https://linguaapi.vafadar.pro/api/catalog/roleplays`, `https://linguaapi.vafadar.pro/api/catalog/roleplays/c1-eine-strategie-im-team-kritisch-pruefen?primaryMeaningLanguageCode=fa`, and `/api/catalog/search?q=Termin&resultType=roleplay`. PostgreSQL `darwinlingua_shared` verification found `402` RoleplayScenarios, all `402` active, `0` drafts, `0` active scenarios missing required translations, `0` active scenarios without answer choices, and `0` active scenarios without static feedback. The structural Web test verifies deterministic answer-choice/static-feedback rendering, missing image asset fallback, and no `ImagePrompt` leakage.
- [x] Sensitive Educational Language policy reviewed
  - Evidence: `docs/85-Sensitive-Educational-Language-Policy.md` defines the Web-only policy, allowed sensitive educational scope, blocked pornographic/explicit/adult-illegal categories, opt-in behavior, metadata, validation rules, mobile deferral, and launch blockers. The 2026-06-18 expression content audit reports 18 packages and 0 issues.
- [x] registration/legal notice coverage reviewed for Terms, Privacy, and Sensitive Educational Language default-off behavior
  - Evidence: roadmap and policy record that registration Terms/Privacy acknowledgement is separate from Sensitive Educational Language; Settings owns the later opt-in. `SensitiveEducationalLanguageStructuralTests` verifies Settings copy and Web Expressions use profile-bound eligibility instead of public query-string unlocks.
- [x] Settings/profile explanation for Sensitive Educational Language is clear, localized, and does not claim age verification
  - Evidence: 2026-06-18 `SensitiveEducationalLanguageStructuralTests` passed with assertions for English and German Settings copy: the feature may include rude words/slang/mild relationship or emotional expressions, does not show pornographic or explicit adult content, and does not verify age.
- [x] Sensitive Educational Language entries are hidden from anonymous users and users without opt-in
  - Evidence: 2026-06-18 expanded `SensitiveEducationalLanguageStructuralTests` verifies Web Expressions are profile-bound, public API `includeSensitiveEducationalLanguage` requests require admin role or admin API key, and public expression repository/search visibility requires general/non-sensitive/safe-to-use content when opt-in is false.
- [x] Explicit-adult and blocked-illegal Expressions remain blocked even when Sensitive Educational Language is enabled
  - Evidence: 2026-06-18 expanded `SensitiveEducationalLanguageStructuralTests` verifies `ExpressionRepository` and `UnifiedLearningSearchRepository` always exclude adult-access, verified-adult, explicit-adult, blocked-illegal, discriminatory-slur, blocked sensitive kind, slur-educational kind, and blocked usage-policy entries before applying learner opt-in visibility.
- [x] Web/API/search filtering for Sensitive Educational Language is verified
  - Evidence: 2026-06-18 public smoke returned HTTP 200 for `/expressions`, `/api/catalog/expressions`, and `/api/catalog/search?q=Schlauch&resultType=expression`; structural tests verify Web profile filtering, API guarded sensitive unlocks, and repository/search visibility rules.
- [x] Unified Search excludes sensitive and adult-only Expressions by default
  - Evidence: 2026-06-18 expanded `SensitiveEducationalLanguageStructuralTests` verifies default Unified Search expression filtering excludes `RequiresSensitiveOptIn`, adult-access, verified-adult, explicit-adult, blocked-illegal, discriminatory-slur, blocked/slur sensitive kinds, and blocked usage-policy entries.
- [x] Admin reports show Expression counts by safety rating, sensitive content kind, age requirement, opt-in requirement, missing warnings, missing teaching reasons, and ordinary-literal leakage
  - Evidence: 2026-06-18 expanded `SensitiveEducationalLanguageStructuralTests` verifies admin response models, service counters, issue drilldown messages, and admin report view sections for safety rating, sensitive content kind, usage policy, opt-in/adult/verified-adult, missing warnings, missing teaching reasons, missing sensitive usage policy, blocked/explicit entries, and ordinary-literal leakage. PostgreSQL verification reports `active=570`, `ordinary_literal_active=0`, `explicit_or_blocked=0`, `requires_sensitive_optin=159`, and `missing_warnings_for_sensitive=0`.
- [x] Deferred mobile package export excludes Sensitive Educational Language before any later mobile work resumes
- [x] Exercise Engine readiness reviewed: deterministic answer evaluation, answer-key safety, attempts, WebApi runner endpoints, Web runner behavior, localized helper projections, and admin quality counters
- [x] Exercise attempt persistence requires authorization and stores only authenticated user ids
  - Evidence: 2026-06-18 `ExerciseAttemptServiceTests` passed with coverage that persisted attempts trim/store only the authenticated user id supplied by the protected API path and reject missing user ids without saving. `ExerciseAttemptAndSearchHardeningStructuralTests` verifies `/api/learning/exercises/{slug}/attempts` calls `GetRequiredUserId(principal)`, requires authorization, and uses the `ExerciseAttempts` rate-limit policy.
- [x] Public exercise evaluation is stateless, rate-limited, and does not persist anonymous progress
  - Evidence: 2026-06-18 `ExerciseAttemptServiceTests` passed with coverage that `EvaluateAttemptAsync` returns an evaluation result and leaves the attempt repository empty. `ExerciseAttemptAndSearchHardeningStructuralTests` verifies `/api/catalog/exercises/{slug}/evaluate` uses `EvaluateAttemptAsync` and applies the shared `ExerciseAttempts` rate-limit policy. The Web learner runner posts to the public evaluate endpoint through `WebCatalogApiClient`, not to the persisted learning-attempt endpoint.
- [x] Exercise submitted-answer JSON is bounded, shape-checked, and malformed input returns safe validation errors
  - Evidence: 2026-06-18 `ExerciseAttemptServiceTests` passed with coverage for malformed JSON, oversized JSON, and primitive JSON shape rejection on both persisted submit and stateless public evaluate paths, without saving attempts. The shared WebApi request wrapper catches `DomainRuleException` and returns `400 invalid_request`; the Web runner also validates the generated/advanced answer payload client-side before posting.
- [x] Course Lessons readiness reviewed: course/module/lesson ordering, linked-content projections, lesson routes, localized helper projections, progress hooks where implemented
  - Evidence: Course lesson detail pages record `viewed` progress and expose antiforgery-protected controls for `in-progress`, `completed`, and `needs-review`; 2026-06-18 targeted domain and structural tests verified progress state transitions, stale completion timestamp clearing, and Web route/view wiring.
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
  - Evidence: all 120 C2 lessons now include reviewed `activityBlocks` with 600 ordered activities. PostgreSQL verification after import reports `CourseLessons=560`, `C2ActivityEnabled=120`, `TotalActivityEnabled=560`, `ActiveLessonsWithoutActivityBlocks=0`, and zero unresolved C2 activity targets. Public Web smoke passed for the final C2 lesson detail, and API detail with `primaryMeaningLanguageCode=fa` returned 5 activity blocks with Persian helper text. The phase backup is `X:\Projects\DarwinLingua.Backup\20260618-073641-course-c2-activity-flow-complete-pre-user-testing`; file count and byte size match staging with hidden files included (`617` files, `239493739` bytes), and SHA256 verification passed for all `616` listed files.
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
- [x] Unified Search rejects empty, too-short, too-long, and unsupported result-type queries consistently
  - Evidence: 2026-06-18 `UnifiedLearningSearchServiceTests` verifies empty, one-character, over-100-character, and unsupported `resultType` requests throw `DomainRuleException` before repository access; `ExerciseAttemptAndSearchHardeningStructuralTests` verifies `/api/catalog/search` rate limiting plus Web-side one-character query suppression before the learning-content API call.
- [x] Unified Search PostgreSQL trigram/filter indexes are applied in the target environment, with `pg_trgm` installed or extension-creation privileges available
  - Evidence: 2026-06-18 `docker exec darwinlingua-postgres psql -U postgres -d darwinlingua_shared` verified `pg_trgm=1` and `41` public trigram/filter search indexes, including Course Lessons, Writing Templates, Life in Germany/CulturalNotes, Exam Prep, Exercises, Expressions, Grammar, Dialogues, Talk Topics, Roleplays, Events, Organizers, and WordEntries.
- [x] Unified Search seeded performance coverage passes before bulk Phase 7 content generation starts
  - Evidence: PostgreSQL seeded bulk-corpus performance coverage verifies bounded results and positive relevance across Course Lessons, Grammar, Writing Templates, and Life in Germany before larger content runs.
- [x] Progress readiness reviewed: user state separated from content, authenticated persistence, anonymous fallback, deterministic recommendations
  - Evidence: structural endpoint coverage verifies authenticated progress summary/update/recommendation routes; Web view coverage verifies course progress indicators, manual lesson progress controls, recent activity summary, weak-exercise recommendations, difficult-word recommendations, and deterministic non-AI ranking.
- [x] Admin reports readiness reviewed: coverage counts, unresolved links, missing translations, unpublished drafts, missing exercise coverage
  - Evidence: service and structural tests cover Learning Portal counts, quality counters, issue samples, and the full issue drill-down page with filters and CSV export.
- [x] Controlled Web tester onboarding runbook exists
  - Evidence: `docs/87-Web-Tester-Onboarding-Runbook.md` defines tester profiles, repeatable pre-test smoke via `tools/Web/Invoke-WebTesterPreflight.ps1`, task scripts for Course flow, helper languages, Writing Templates, Exam Prep, Life in Germany, Search/Recent, feedback fields, triage rules, and completion criteria. The public-routed preflight passed on 2026-06-18 for Web home, Courses, A1/C2 lesson details, Exercises, Exam Prep, Writing Templates, Life in Germany, Search, Recent, API health, Course Persian helper projection, and representative Unified Search queries; the post-search-hardening rerun is recorded at `artifacts/validation/web-tester-preflight/web-tester-preflight-20260618-140534.json`, the post-lesson-progress-control rerun is recorded at `artifacts/validation/web-tester-preflight/web-tester-preflight-20260618-141256.json`, and the post-Talk-Topic-vocabulary-link rerun is recorded at `artifacts/validation/web-tester-preflight/web-tester-preflight-20260618-142345.json`.
- [x] Controlled Web tester feedback triage helper exists
  - Evidence: `tools/Web/Convert-WebTesterFeedbackToReport.ps1` validates the tester feedback CSV shape, ignores the template sample row, sorts blocker/major/fix-now issues first, counts RTL-language observations, and writes JSON and Markdown triage reports.
- [x] Controlled Web tester validation bundle helper exists
  - Evidence: `tools/Web/New-WebTesterValidationBundle.ps1` creates a timestamped tester-pass folder with the operator runbook, tester quick start, feedback CSV, manifest, README, and optional pass-specific preflight report. A current public-routed tester bundle was generated at `artifacts/validation/web-tester-runs/20260620-011001-web-tester-pass` with preflight status `passed` and 25/25 checks green for public learner routes, anonymous admin login redirects, public API health, Course Persian helper projection, and representative search queries. `Convert-WebTesterFeedbackToReport.ps1` also passed a dry-run against the bundle feedback CSV with 0 validation errors at `artifacts/validation/web-tester-feedback/web-tester-feedback-triage-20260620-011012.md`.
- [x] Mobile parity is explicitly deferred and is not required for this Web release
  - Evidence: target-device validation worksheet is tracked at `artifacts/validation/phase7-mobile-validation-worksheet.md`; mobile exercise runner and account-bound progress sync remain deferred post-Web features.
- [x] Bulk Phase 7 content generation remains blocked until module contracts, validation, rendering, admin reports, and release checks are stable
  - Evidence: current next step is Web readiness and tester feedback; further Life in Germany B2+ content is deferred until this checkpoint is closed.

---

## H. Sign-Off

- Release owner: Darwin Lingua operator / Web release owner during controlled testing.
- Validation owner: Darwin Lingua operator, with engineering evidence from this checklist and generated validation artifacts.
- Known accepted issues:
  - This is not a broad public production launch sign-off. The primary Web test domain is `https://darwinlingua.com`; `https://www.darwinlingua.com` is not a required route unless DNS and redirects are explicitly added later.
  - Brevo production sending is now configured and smoke-tested for the controlled local public stack with verified provider/API readiness, direct real-delivery smoke, app-level registration/password-reset delivery logs, controlled public webhook/suppression smoke, controlled suppressed-send logging smoke, and authenticated Admin Email Diagnostics UI smoke. Manual mailbox render review and ongoing real provider webhook/event monitoring still remain operational checks before broad public launch.
  - Production legal/operator review remains external: real Impressum/operator data, Privacy Policy, Terms, data-subject request owner/process, breach triage owner, illegal-content report owner, and final legal sign-off.
  - Target-browser PWA install prompt acceptance still needs manual validation on real desktop Chrome/Edge and Android Chrome.
  - Public paid billing remains disabled for Web testing; Stripe legal/provider/customer-portal validation must be re-opened before self-service paid subscriptions are exposed.
  - Mobile/MAUI remains deferred until the Web product has passed real-user testing and feedback-driven improvements.
- Final release recommendation: proceed with operator-side Brevo/domain/legal setup and a controlled Web tester pass only. Do not treat this checkpoint as approval for broad public launch until the unchecked provider, legal, install, and real-email validation gates above are closed with evidence.
