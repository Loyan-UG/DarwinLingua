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

## Automated Test Backlog

### Scenario Learning

- [ ] Scenario import rejects malformed scenario packages with clear issue messages.
- [ ] Scenario import persists lesson metadata, topics, dialogue turns, useful phrases, questions, answers, and translations.
- [ ] Scenario list query returns only published scenarios in stable sort order.
- [ ] Scenario detail query applies primary and secondary meaning-language selection.
- [ ] Web scenario detail rendering shows dialogue, useful phrases, quick checks, related starter packs, and related preparation packs.
- [ ] Web roleplay sequence builder skips learner prompts and pairs each non-learner prompt with the next learner model answer.
- [ ] Web roleplay page renders model answers, static feedback, and no-AI behavior.
- [ ] Empty or unknown scenario detail payloads return safe 404 behavior instead of Web 500 errors.

### Conversation Starters

- [ ] Starter import rejects invalid filters, linked scenario slugs, and invalid dual-language payloads.
- [ ] Starter list query filters by CEFR level, situation, tone, and conversation goal.
- [ ] Starter detail query renders primary and optional secondary meaning languages.
- [ ] Scenario detail query returns related starter packs for linked scenarios.
- [ ] Web starter detail page renders phrase alternatives, usage notes, register, common mistakes, and dual-language text.

### Event Preparation Packs

- [ ] Preparation-pack import rejects invalid linked scenarios, vocabulary references, starter links, and prompt types.
- [ ] Preparation-pack persistence stores prompts, vocabulary references, topic links, scenario links, and starter-pack links.
- [ ] Event-to-preparation-pack mapping returns only published packs linked to the active event.
- [ ] Web event detail renders entitled preparation-pack links.
- [ ] Web preparation-pack detail renders prompts, vocabulary, related scenarios, and related starter packs.
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
- [ ] Web seed fixtures include multiple records for scenarios, starters, preparation packs, organizers, events, RSVPs, claims, learner profiles, partner requests, reports, blocks, and moderation audits.
- [ ] Operational seed loading path is validated once a dedicated event-directory/safety seed applier exists.

### Entitlements And Feature Gates

- [ ] Free, Trial, and Premium entitlement states resolve expected feature flags.
- [ ] Expired trial and expired premium states fall back to free access.
- [ ] Web feature gates hide or block premium preparation packs and advanced features.
- [ ] WebApi feature gates reject direct unauthorized calls.
- [ ] Admin entitlement changes write audit records and update effective access immediately.

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
- [ ] Scenario list/detail.
- [ ] Scenario roleplay.
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
