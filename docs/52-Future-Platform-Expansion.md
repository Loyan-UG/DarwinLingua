# Future Platform Expansion

## Purpose

This document defines the planned boundaries for future shared-platform work after the current mobile-first delivery.

It keeps later cross-product architecture explicit without mixing it into the current execution slices.

---

## Shared Platform Direction

Darwin Lingua should continue as one shared backend with multiple client products.

Planned products:

- mobile learner apps for multiple target languages
- an admin application for content operations
- a learner-facing web application

Shared backend responsibilities:

- canonical content storage
- publishing lifecycle and audit history
- manifest and package delivery
- future account and sync boundaries
- future analytics and monetization boundaries

Client responsibilities that remain local-first by default:

- on-device SQLite runtime storage
- cached downloaded content
- local preferences
- local learning state until sync is explicitly introduced

---

## Admin Application Planning Slices

Recommended implementation order:

1. authenticated operator shell
2. staged import review dashboard
3. draft batch inspection and publish controls
4. publication history, rollback, and cleanup views
5. package and manifest diagnostics
6. operator notes and audit review surfaces

The admin application should initially consume the current Web API admin endpoints and only expand the backend when UI workflows require it.

---

## Web Learner Application Planning Slices

Recommended implementation order:

1. shared authentication and session shell
2. browse and search surfaces equivalent to mobile catalog flows
3. word-detail flow on the same content contracts
4. favorites and lightweight learning-state views
5. practice overview and review sessions
6. sync-aware progress and account features

The web learner application should reuse the same manifest, package, and versioning rules as mobile wherever practical.

---

## Account And Sync Boundaries

Accounts and sync should remain outside the current content-distribution implementation.

The next approved direction is:

- one shared account system for web and mobile learners
- role-based authorization for administrative access
- entitlement-based feature gating for monetized learner features
- seeded non-production test accounts for validation and demos

First synced data when introduced:

- favorites
- meaning-language preferences
- UI-language preference
- known/difficult markers
- practice review state and attempt summaries

Server-authored data that stays outside user sync:

- lexical catalog content
- topics and localizations
- published package lifecycle

Rules:

- content stays package-versioned
- user state stays record-based
- conflict handling must be explicit per state type
- offline-first clients must still function without sign-in
- roles must not be reused as premium-feature flags
- entitlement evaluation must remain server-owned

---

## Analytics Boundaries

Recommended introduction order:

1. operational analytics for publishing and update success/failure
2. anonymous product analytics for screen and feature usage
3. authenticated learning analytics only after accounts are approved

Operational audit events and learner analytics must stay separate.

Analytics contracts should be schema-versioned.

---

## Monetization Boundaries

Monetization should remain outside the core content pipeline.

Planned boundaries:

- entitlement-aware feature gating
- package-access policy by product and tier
- premium practice or curated content packs
- no monetization logic inside lexical query or import workflows

Recommended first rollout shape:

- free catalog browsing and word viewing remain available
- trial or premium entitlements unlock selected learner-state and convenience features
- pricing, trial duration, and final premium feature scope remain product decisions outside this document

Required engineering rules:

- feature gates must be enforceable on both web and mobile
- a missing client-side gate must not grant access if the server denies entitlement
- seeded non-production learner and admin accounts must exist for validation
- entitlement changes must be auditable

Any future monetization layer should consume identity, entitlement state, and published content metadata rather than mutating publishing behavior itself.
