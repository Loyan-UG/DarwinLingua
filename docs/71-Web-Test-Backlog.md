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

- [ ] Dialogue import rejects malformed dialogue packages with clear issue messages.
- [ ] Dialogue import persists lesson metadata, topics, dialogue turns, useful phrases, questions, answers, and translations.
- [ ] Dialogue list query returns only published dialogues in stable sort order.
- [ ] Dialogue detail query applies primary and secondary meaning-language selection.
- [ ] Web dialogue detail rendering shows dialogue, useful phrases, quick checks, related starter packs, and related preparation packs.
- [ ] Web roleplay sequence builder skips learner prompts and pairs each non-learner prompt with the next learner model answer.
- [ ] Web roleplay page renders model answers, static feedback, and no-AI behavior.
- [ ] Empty or unknown dialogue detail payloads return safe 404 behavior instead of Web 500 errors.

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

- [ ] Admin system report endpoint returns catalog, social-learning, moderation, and operations counts from the server database.
- [ ] Web Admin Reports page combines system report counts, Identity user count, and Web analytics counters.
- [ ] Admin dashboard links to Reports and the implemented management pages.
- [ ] Web seed fixtures include multiple records for dialogues, starters, preparation packs, organizers, events, RSVPs, claims, learner profiles, partner requests, reports, blocks, and moderation audits.
- [ ] Operational seed loading path is validated once a dedicated event-directory/safety seed applier exists.

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
