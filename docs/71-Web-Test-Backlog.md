# Web Test Backlog

## Purpose

This document is the handoff backlog for tests that must be implemented outside the current Web feature-development thread.

Web feature development is considered implementation-complete for the current backlog when:

- Web learner and admin features are implemented
- Web routes build and run against WebApi
- existing test suites pass
- remaining work is limited to explicit test automation, manual device/browser validation, or mobile parity

The items below are intentionally not implemented in this thread. They are owned by the separate test-development workflow.

## Current Web Development Status

Status: feature-complete for the current Web-focused backlog.

Remaining release gates:

- automated test coverage listed in this document
- manual browser/device validation listed in the Web validation worksheets
- mobile parity work, after Web sign-off

Latest local Web verification:

- 2026-05-12: `DarwinLingua.WebApi` build passed with 0 warnings and 0 errors.
- 2026-05-12: `DarwinLingua.Web` build passed with 0 warnings and 0 errors after duplicate localization key cleanup.
- 2026-05-12: `DarwinLingua.WebApi.Tests` passed 36/36.
- 2026-05-12: `DarwinLingua.ContentOps.Infrastructure.Tests` passed 23/23, including Phase 7 parser contract tests.
- 2026-05-12: `DarwinLingua.Catalog.Application.Tests` passed 21/21.
- 2026-05-12: `DarwinLingua.Learning.Application.Tests` passed 29/29.
- 2026-05-12: `DarwinLingua.Learning.Domain.Tests` passed 47/47.
- 2026-05-12: `DarwinLingua.Localization.Application.Tests` passed 24/24, including Phase 7 Web route, WebApi route, and English/German localization-key hardening checks.
- 2026-05-12: full `DarwinLingua.slnx` build passed with 0 warnings and 0 errors when run sequentially with `-m:1`.
- 2026-05-01: `DarwinLingua.Web` build passed with 0 warnings and 0 errors.
- 2026-05-01: `DarwinLingua.WebApi` build passed with 0 warnings and 0 errors after stopping the local smoke host that was locking build outputs.
- 2026-05-01: local GET smoke passed for the main learner routes: `/`, `/browse`, `/browse/cefr/A1`, `/search`, `/search?q=auto`, unknown-search suggestion state, `/collections`, `/dialogues`, `/conversation-starters`, `/conversation-events`, `/organizers`, `/install`, `/privacy`, and the Identity account pages.
- 2026-05-01: authenticated admin GET smoke passed for dashboard, reports, analytics, diagnostics, content import/history/draft/publishing/rollback pages, catalog management pages, taxonomy pages, users, moderation, word suggestions, organizer profiles, conversation events, billing diagnostics, and email diagnostics.
- 2026-05-01: smoke response bodies and server logs were checked for unhandled exception signatures; none were found.
- 2026-05-01: in-app browser opened `/admin/billing-diagnostics` successfully. Local Stripe readiness warnings were expected because local Stripe billing settings were not enabled/configured.
- 2026-05-01: local security smoke verified external `returnUrl` values are not rendered back into the Identity login hidden field; the value is normalized to `/`.
- 2026-05-01: local webhook smoke verified unsigned Stripe billing webhooks return `401`, and client telemetry accepts a bounded valid payload.
- 2026-05-01: provider-error logging was reviewed for Brevo and Stripe billing paths; external provider response bodies are not logged for email send, checkout, portal, fulfillment, or reconciliation failures.

## Automated Test Backlog

### Talk Topics

- [x] Import parser accepts valid `talkTopics` package contracts.
- [x] Import validation rejects German articles outside CEFR character target ranges.
- [x] Import validation rejects Talk Topic article and question translations.
- [x] Import validation enforces warm-up counts, discussion question counts by type, vocabulary count ranges, and speaking-goal count ranges.
- [x] Import validation rejects invalid content type, question type, and speaking goal values.
- [x] Persistence stores German-only article, German-only questions, vocabulary references, speaking goals, content type, and sensitivity fields.
- [x] Generated Talk Topic package validation checks CEFR character ranges, no article/question translations, required question counts, vocabulary counts, content types, speaking goals, duplicate slugs, and distinct topic groups.
- [x] List query filters by CEFR level, category, topic, content type, speaking goal, and sensitivity.
- [ ] Detail query resolves vocabulary references against the Word Catalog where possible.
- [x] WebApi list/detail endpoints return successful Talk Topic payloads in smoke coverage.
- [ ] Web detail rendering does not fail when a vocabulary `wordSlug` cannot be resolved.
- [ ] German and English localization keys cover Talk Topics, content types, speaking goals, and sensitivity warnings.
- [x] Catalog package export includes Talk Topics where expected.
- [ ] Mobile UI parity for Talk Topics after the Web flow is signed off.

### Dialogue Learning

- [x] Dialogue import rejects malformed dialogue packages with clear issue messages for missing metadata, unsupported values, missing useful words, missing speaking prompts, and insufficient sentence counts.
- [x] Dialogue import persists lesson metadata, topics, dialogue turns, useful phrases, questions, answers, translations, useful words, and speaking prompts.
- [x] Dialogue list query returns only published dialogues in stable sort order and supports CEFR, category, topic, exam profile, skill focus, task type, interaction mode, register, and search filters.
- [ ] Dialogue detail query applies primary and secondary meaning-language selection.
- [x] Web dialogue detail rendering shows dialogue metadata, useful phrases, useful words, speaking prompts, quick checks, related starter packs, and related preparation packs.
- [x] Web dialogue list includes prominent CEFR filtering and metadata badges.
- [ ] Web roleplay sequence builder skips learner prompts and pairs each non-learner prompt with the next learner model answer.
- [ ] Web roleplay page renders model answers, static feedback, and no-AI behavior.
- [ ] Empty or unknown dialogue detail payloads return safe 404 behavior instead of Web 500 errors.
- [ ] Human-review generated Dialogue translations before public launch; current starter generation preserves required language slots for import integrity.
- [ ] Add full mobile Dialogue metadata/detail parity after Web sign-off.

### Conversation Starters

- [ ] Starter import rejects invalid filters, linked dialogue slugs, and invalid dual-language payloads.
- [ ] Starter list query filters by CEFR level, situation, tone, and conversation goal.
- [ ] Starter detail query renders primary and optional secondary meaning languages.
- [ ] Dialogue detail query returns related starter packs for linked dialogues.
- [ ] Web starter detail page renders phrase alternatives, usage notes, register, common mistakes, and dual-language text.

### Event Preparation Packs

- [ ] Preparation-pack import rejects invalid linked dialogues, vocabulary references, starter links, and prompt types.
- [ ] Preparation-pack persistence stores prompts, vocabulary references, topic links, dialogue links, and starter-pack links.
- [ ] Event-to-preparation-pack mapping returns only published packs linked to the active event.
- [ ] Web event detail renders entitled preparation-pack links.
- [ ] Web preparation-pack detail renders prompts, vocabulary, related dialogues, and related starter packs.
- [ ] `Mark prepared` records aggregate completion analytics and redirects back to the preparation-pack detail page.
- [ ] Direct preparation-pack access is forbidden when the learner lacks the entitlement.

### Event Directory And Organizer Tools

- [ ] Event filtering covers city, online/offline, CEFR level, helper language, price type, date, and publication status.
- [ ] Event detail respects public/private projection rules.
- [ ] Organizer ownership rules restrict dashboard event management to assigned owners.
- [ ] Organizer dashboard enforces active-event limits by organizer plan.
- [ ] RSVP flow enforces capacity, duplicate registration rules, cancellation, waitlist behavior if enabled, and attendance updates.
- [ ] Organizer visibility rules hide archived/unpublished/private records from public pages.

### Learner Profiles And Partner Matching

- [ ] Public learner profile projection excludes private contact details.
- [ ] Learner profile anonymization clears public/private profile fields and disables matching.
- [ ] Partner request transitions cover pending, accepted, declined, cancelled, and blocked states.
- [ ] Partner request rate limits suppress excessive requests.
- [ ] Accepted partner requests reveal contact details only after mutual consent.
- [ ] Blocked users are hidden from matching results and cannot create new requests.

### Moderation And Safety

- [ ] User report creation validates target, reason, and reporter context.
- [ ] User block creation suppresses matching and contact surfaces.
- [ ] Admin moderation queue filters by status, reason, target type, and assigned state.
- [ ] Moderation decision audit captures decision type, actor, target, notes, and timestamps.
- [ ] Report/block visibility rules are enforced in public pages and matching APIs.

### Admin Reporting And Seed Coverage

- [x] Admin system report endpoint returns catalog, social-learning, moderation, operations, and Learning Portal counts from the server database.
- [x] Web Admin Reports page combines system report counts, Identity user count, Web analytics counters, and Learning Portal quality/coverage tables.
- [x] Admin Reports authorization requires the Admin policy.
- [x] Learning Portal report test covers content counts, CEFR coverage, unresolved content links, missing translations, and missing exercise coverage.
- [ ] Admin dashboard links to Reports and the implemented management pages.
- [ ] Web seed fixtures include multiple records for dialogues, starters, preparation packs, organizers, events, RSVPs, claims, learner profiles, partner requests, reports, blocks, and moderation audits.
- [ ] Operational seed loading path is validated once a dedicated event-directory/safety seed applier exists.
- [ ] Web rendering coverage exists for the Learning Portal coverage and issue-sample tables.

### Entitlements And Feature Gates

- [ ] Free, Trial, and Premium entitlement states resolve expected feature flags.
- [ ] Expired trial and expired premium states fall back to free access.
- [ ] Web feature gates hide or block premium preparation packs and advanced features.
- [ ] WebApi feature gates reject direct unauthorized calls.
- [ ] Admin entitlement changes write audit records and update effective access immediately.

### Billing And Payments

- [ ] Billing page renders the current entitlement and disables checkout when Stripe is off.
- [ ] Billing page starts Stripe Checkout only for authenticated users and only when Stripe is enabled.
- [ ] Billing page shows Stripe Customer Portal management only after a customer id is linked to the account.
- [ ] Stripe Customer Portal session creation redirects only for the authenticated user's linked Stripe customer.
- [ ] `/billing/success` fetches the returned Stripe checkout session and immediately syncs entitlement only when the session belongs to the authenticated user.
- [ ] `/billing/success` only fulfills completed subscription checkout sessions with paid or no-payment-required status and customer/subscription ids.
- [ ] `/billing/success` falls back to webhook-based activation when Stripe lookup fails or session is not complete.
- [ ] `checkout.session.completed` webhook rejects non-subscription, unpaid, or incomplete checkout sessions before granting Premium.
- [ ] Stripe Checkout, Stripe Customer Portal, and Admin reconciliation enforce bounded rate limits with safe user-facing messages.
- [ ] Stripe Checkout session creation sends the Darwin Lingua user id in session and subscription metadata.
- [ ] Stripe Checkout failures show a safe user-facing message and do not leak provider response bodies.
- [ ] Stripe webhook rejects missing, malformed, expired, or invalid signatures.
- [ ] Duplicate Stripe webhook event ids are idempotent.
- [ ] `checkout.session.completed` maps the Stripe session to the correct user and grants Premium.
- [ ] `customer.subscription.created` and `customer.subscription.updated` persist customer/subscription ids and current period end.
- [ ] Active or trialing Stripe subscription states keep Premium access.
- [ ] Cancelled, unpaid, or incomplete-expired Stripe subscription states downgrade the user to Free.
- [ ] Unmapped Stripe subscription events fail closed and are visible in billing-event diagnostics/logs.
- [ ] Admin Billing Diagnostics renders Stripe readiness without exposing secret values.
- [ ] Admin Billing Diagnostics filters billing events by status, event type, user id, customer id, and subscription id.
- [ ] Admin Billing Diagnostics filters billing profiles by user id, customer id, and subscription id.
- [ ] Admin-only Stripe subscription reconciliation rejects malformed subscription ids.
- [ ] Admin-only Stripe subscription reconciliation fetches current Stripe subscription state without storing raw provider payloads.
- [ ] Admin-only Stripe subscription reconciliation updates billing profile and entitlement for active, trialing, past-due, cancelled, unpaid, and expired states.
- [ ] Billing notification emails render for Premium activation, payment action needed, Premium ended, and Admin reconciliation completed.
- [ ] Billing notification emails are logged through the transactional email delivery log without storing Stripe raw payloads.
- [ ] Billing notification emails are idempotent per scenario, user, subscription, and billing status.
- [ ] Stripe webhook processing sends user billing emails for checkout completion, payment-action-needed states, and Premium-ended states.
- [ ] Admin reconciliation sends both user billing status email and admin reconciliation-completed email when recipients are configured.

### Web Runtime And Bootstrap

- [ ] PostgreSQL startup bootstrap retrofits Phase 6 catalog tables on an existing shared database.
- [ ] Empty optional Identity and catalog connection strings fall back to the shared server-content database.
- [ ] WebCatalogApiClient treats empty successful detail responses as `null`.
- [ ] Local server bootstrap handles both folder imports and single-file imports under `Set-StrictMode`.

## Learning Portal Expansion

### Foundation

- [x] Web learner navigation groups implemented routes under Learn, Practice, Speak, Prepare, and Resources.
- [x] Web learner navigation avoids dead routes for future Phase 7 modules.
- [x] Phase 7 Web learner routes are covered by structural route tests for Grammar, Expressions, Exercises, Courses, Exam Prep, Writing Templates, Cultural Notes, and Unified Search.
- [x] Phase 7 WebApi route registrations are covered by structural route tests for module list/detail endpoints, exercise attempts, unified search, progress summary/update, and recommendations.
- [x] Phase 7 English/German localization resource keys are covered by structural tests for the release route surface.
- [x] Shared CEFR filter conventions expose stable A1-C2 values for reusable Web filters.
- [ ] Cross-content linking helper coverage once a real Phase 7 module consumes the model.
- [x] Admin/system report coverage exists for persisted Phase 7 module counts and quality metrics.

### Grammar Guide

- [x] Parser coverage exists for the GrammarTopic content contract shape.
- [x] Parser/import coverage exists for the rich localized Grammar pilot package, including localized title/description and table blocks.
- [x] Navigation/localization smoke coverage includes the live Grammar Guide route.
- [x] Release route hardening covers `/grammar` and `/api/catalog/grammar`.
- [x] Import validation covers required rich section keys, supported block types, localized block language codes, and table/callout block shape.
- [x] List/detail query coverage includes the first rich pilot topic and localized fallback behavior.
- [ ] CEFR/category/topic/search filters return expected grammar topics.
- [x] Web/API rendering handles paragraph, table, and callout rich blocks from the pilot package.
- [x] Cross-level validation batch coverage confirms two imported rich Grammar topics per CEFR level from A1 through C2.
- [ ] Linked words/dialogues/Talk Topics/exercises render where available.
- [ ] Localized explanation rendering follows learner language preferences.
- [ ] Unresolved links fail safely without Web 500 errors.

### Everyday Expressions

- [x] Parser coverage exists for the ExpressionEntry content contract shape.
- [x] Navigation/localization smoke coverage includes the live Everyday Expressions route.
- [x] Release route hardening covers `/expressions` and `/api/catalog/expressions`.
- [x] Expression type/register validation rejects unsupported values.
- [x] Risky expression validation rejects entries without required warning text.
- [ ] List/detail queries return published expressions in stable order.
- [ ] CEFR/type/register/context filters return expected expressions.
- [ ] Risky expression warnings render for unsafe tone or context.
- [ ] Linked words and related expressions render where available.
- [ ] Unresolved links fail safely without Web 500 errors.

### Exercise Engine

- [x] Parser coverage exists for the Exercise and ExerciseSet content contract shape.
- [x] Release route hardening covers `/exercises`, exercise-set/detail endpoints, and exercise attempt submission route registration.
- [x] Exercise type validation rejects unsupported types.
- [x] Answer key validation rejects missing or malformed deterministic answers.
- [x] Deterministic feedback returns stable correct/incorrect outcomes.
- [ ] Exercise set linking resolves valid owner references.
- [x] Exercise runner behavior covers structured choice prompts and malformed prompt fallback.
- [x] Authenticated attempt persistence stores the authenticated user id and never falls back to an `anonymous` user id.
- [x] Public exercise evaluation is stateless and does not persist anonymous attempts.
- [x] Malformed and oversized submitted-answer JSON is rejected before persistence.
- [x] Exercise attempt and evaluation endpoints are covered by rate-limiting structural checks.
- [x] Attempt results do not expose answer keys.
- [x] Type-specific runner controls cover initial choice, single-answer, error-correction, sentence-ordering, and matching submission shapes.
- [ ] Seeded runner coverage verifies each initial exercise type against real package examples.

### Course Lessons

- [x] Parser coverage exists for the CoursePath/CourseModule/CourseLesson content contract shape.
- [x] Release route hardening covers `/courses`, course list/detail endpoints, and course lesson detail route registration.
- [ ] Lesson/module/course ordering is stable.
- [ ] Linked content rendering covers grammar, words, expressions, dialogues, Talk Topics, and exercises.
- [ ] Prerequisite and next-lesson navigation resolves correctly.
- [ ] WebApi list/detail endpoint coverage exists.
- [ ] Progress tracking works where implemented.

### Exam Preparation

- [x] Parser coverage exists for the Exam Prep content contract shape.
- [x] Import validation covers supported profile taxonomy.
- [x] Navigation/localization shell includes Exam Prep.
- [x] Release route hardening covers `/exam-prep`, exam profile, and exam prep list/detail route registrations.
- [ ] Exam profile taxonomy validates supported profiles and task types in Web API coverage.
- [ ] Exam unit linking resolves dialogues, exercises, grammar, writing templates, and Talk Topics.
- [ ] Exam filter behavior covers profile, CEFR, and task type.
- [ ] Sample task rendering works for initial exam-prep units.

### Writing Templates

- [x] Parser coverage exists for the WritingTemplate content contract shape.
- [x] Variable validation requires declared placeholders to exist in template text.
- [x] Release route hardening covers `/writing-templates` and writing-template list/detail route registrations.
- [ ] Variable rendering substitutes supported placeholders safely.
- [ ] Sample filled version rendering works for published templates.
- [ ] Linked grammar/words/exercises render where available.
- [ ] WebApi list/detail endpoint coverage exists.

### Cultural Notes

- [x] Parser coverage exists for the CulturalNote content contract shape.
- [x] Navigation/localization shell includes Cultural Notes.
- [x] Release route hardening covers `/cultural-notes` and cultural-note list/detail route registrations.
- [ ] List/detail queries return published cultural notes in stable order.
- [ ] Filtering covers CEFR/category/context where supported.
- [ ] WebApi list/detail endpoint coverage exists.
- [ ] Web list/detail rendering coverage exists.
- [ ] Linked content rendering covers dialogues, expressions, writing templates, and course lessons.

### Unified Search

- [x] Application-level empty query handling avoids repository calls.
- [x] Application-level short, long, and unsupported result-type query handling is covered.
- [x] Application-level result projection returns repository results unchanged.
- [x] Release route hardening covers `/search` and `/api/catalog/search`.
- [x] `/api/catalog/search` is covered by rate-limiting structural checks.
- [x] PostgreSQL trigram and filter-index migration coverage exists for the bulk-content search path.
- [x] Shared database startup applies PostgreSQL trigram/filter indexes for existing search tables and skips not-yet-created Phase 7 tables safely.
- [ ] Result type projection distinguishes words, grammar, expressions, dialogues, Talk Topics, exercises, lessons, exam prep, writing templates, cultural notes, events, and organizers with seeded data.
- [ ] Ranking behavior is deterministic for the same indexed content in repository/WebApi coverage.
- [ ] CEFR/content type/category filters return expected mixed results.
- [ ] WebApi endpoint coverage exists for `/api/catalog/search`.
- [ ] Web rendering coverage exists for learning result cards and filters.
- [ ] Missing content references fail safely.
- [ ] Seeded performance coverage verifies bounded result counts and acceptable query plans before bulk content generation.

### Progress And Personalization

- [x] Domain tests cover supported owner types and progress state transitions.
- [x] Application tests cover viewed/completed updates, summary counts, and deterministic recommendation exclusion for completed content.
- [x] Release route hardening covers progress summary/update and recommendations route registrations.
- [ ] WebApi endpoint coverage exists for authenticated `/api/learning/progress/summary`.
- [ ] WebApi endpoint coverage exists for authenticated `/api/learning/progress/content`.
- [ ] WebApi endpoint coverage exists for `/api/learning/recommendations`.
- [ ] Anonymous Web users fall back to existing guest actor behavior without breaking recent activity.
- [ ] Course lesson pages render viewed/completed state where progress exists.
- [ ] Recent activity dashboard renders cross-content progress summary.
- [ ] Recommendations remain deterministic and do not use AI ranking.

### Mobile Parity Tracking

- [x] Web sign-off is recorded before MAUI parity starts.
- [x] Mobile package export structural coverage confirms Phase 7 arrays are present in full/catalog-full packages.
- [x] Web startup structural coverage confirms the Web app does not register local SQLite learning/content initialization.
- [x] WebApi manifest/package tests cover module-scoped `catalog-module` packages.
- [x] MAUI route/localization structural coverage confirms Learning Portal list/detail/search routes and Learn/Practice/Speak/Prepare/Resources navigation labels.
- [x] Full mobile replacement script coverage confirms Phase 7 content tables are copied from remote package imports.
- [x] Module replacement script coverage confirms MAUI can request and apply selective module packages.
- [ ] Add seeded module-slice package tests that import one selected module without removing unrelated local modules.
- [ ] Add first-run onboarding UI automation for choose/skip flows.
- [ ] Add seeded mobile package export tests that import a package with all Phase 7 module types into a local SQLite database.
- [ ] Add MAUI smoke coverage for opening Learning Portal list/detail/search pages on target devices.
- [ ] Add manual mobile validation worksheet entries for Phase 7 offline behavior and local package update behavior.

## Manual Validation Backlog

### Browser And Device Matrix

- [ ] Validate English UI in desktop Chromium.
- [ ] Validate German UI in desktop Chromium.
- [ ] Validate responsive layout on narrow mobile viewport.
- [ ] Validate PWA install flow on Android Chrome.
- [ ] Validate PWA install flow on desktop Chromium.
- [ ] Validate offline behavior for installed Web/PWA shell.

### Learner Workflows

- [ ] Browse by CEFR.
- [ ] Browse by topic.
- [ ] Search and open word detail.
- [ ] Favorite/unfavorite word.
- [ ] Recent activity.
- [ ] Meaning-language preferences.
- [ ] Dialogue list/detail.
- [ ] Dialogue roleplay.
- [ ] Conversation starter list/detail.
- [ ] Event directory list/detail.
- [ ] Event preparation pack detail and `Mark prepared`.

### Account And Admin Workflows

- [ ] Register learner.
- [ ] Sign in/out.
- [ ] Seeded admin login.
- [ ] Seeded learner login.
- [ ] Admin import/drafts/history/publish/rollback.
- [ ] Admin user entitlement management.
- [ ] Admin organizer/event management.
- [ ] Admin moderation queue and decision logging.
- [ ] Admin reports summary.

## Out Of Scope For This Web Test Backlog

- Mobile screen implementation.
- MAUI UI automation.
- Mobile offline catalog validation.
- Payment-provider integration.
- AI roleplay or AI feedback.
