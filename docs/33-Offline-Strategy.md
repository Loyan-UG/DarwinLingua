# Offline Strategy

## Purpose

This document defines the offline strategy for Darwin Deutsch.

The product is intended to begin as a local-first application, with future potential for cloud sync, server-driven content updates, and cross-device continuity.

The offline strategy must support:

- a strong Phase 1 user experience without requiring backend infrastructure
- reliable local access to learning content
- reliable local storage of user preferences and lightweight learning state
- a future path toward synchronization without breaking the current architecture

This document builds on:

- `25-Phase-1-Domain-Cut.md`
- `26-Bounded-Contexts.md`
- `31-Solution-Architecture.md`
- `32-Storage-Strategy.md`

---

# 1. Offline Philosophy

## 1.1 Core Principle

Darwin Deutsch should begin as a **local-first** application.

This means the app should be usable for its main learning purpose even when:

- there is no internet connection
- the connection is unstable
- the user has limited data access
- the backend does not yet exist

## 1.2 Why Local-First Is the Right Choice

This is the correct strategy for Phase 1 because:

- learning content is mostly read-heavy
- user state is lightweight
- the first version does not require real-time multi-user coordination
- it reduces technical and operational complexity
- it improves reliability for the end user
- it aligns with MAUI cross-platform local app behavior

## 1.3 What Offline Means in This Product

In Darwin Deutsch, offline support means:

- content is locally available
- browsing words does not depend on network access
- example sentences and meanings remain available offline
- user preferences remain available offline
- favorites and lightweight word state remain available offline
- app startup and main learning flows do not depend on remote APIs

It does **not** initially mean:

- cross-device sync
- live shared accounts
- remote content publishing
- real-time collaboration
- cloud-based progress aggregation

---

# 2. Phase 1 Offline Scope

## 2.1 Phase 1 Must Work Fully Offline for Core Learning

The following Phase 1 features should work fully offline:

- browse by CEFR
- browse by topic
- search by German lemma
- view word details
- display meanings in selected meaning languages
- display example sentences
- mark favorites
- update user meaning language preferences
- update user word state
- use local content already imported into the database

## 2.2 Audio in Offline Context

If Phase 1 uses platform text-to-speech, audio behavior depends on platform capabilities.

### Rule

The system should treat TTS availability as a platform/service capability, not a guaranteed domain feature.

### Practical Interpretation

- if platform TTS works offline, the feature works offline
- if a platform or voice requires connectivity, the app should fail gracefully
- the app should never depend on remote audio files in Phase 1

## 2.3 Import in Offline Context

The import workflow can also be offline if it reads local JSON files and writes to the local database.

This is a strong advantage in early development and operations.

---

# 3. Phase 1 Source of Truth

## 3.1 Local Database Is the Source of Truth

In Phase 1, the local SQLite database is the operational source of truth for the app installation.

That means:

- content lives locally
- user state lives locally
- there is no server authority
- no sync conflict model is needed yet

## 3.2 Implication

The app should not be built around assumptions such as:

- remote content must be fetched at startup
- remote user settings must be downloaded
- the user must log in before learning can begin

Those assumptions belong to later product stages, not Phase 1.

---

# 4. Offline Data Categories

## 4.1 Category A - Fully Offline Core Content

These data categories must be fully local in Phase 1:

- WordEntry
- WordSense
- SenseTranslation
- ExampleSentence
- ExampleTranslation
- Topic
- TopicLocalization
- Language

This is the content base of the product.

## 4.2 Category B - Fully Offline User State

These data categories must also be local in Phase 1:

- UserLearningProfile
- UserFavoriteWord
- UserWordState

## 4.3 Category C - Offline Operational Data

These data categories should be stored locally in Phase 1:

- ContentPackage
- ContentPackageEntry

This allows import reporting and content traceability without backend dependency.

---

# 5. Offline User Experience Rules

## 5.1 No Network Dependency for Core Screens

Core learning screens should not require internet in Phase 1.

Examples:

- home/browse screen
- CEFR browsing screen
- topic browsing screen
- word detail screen
- favorites screen
- settings/preferences screen

## 5.2 Graceful Failure for Non-Core Capabilities

If a capability may depend on platform services or future connectivity, failure must be non-destructive.

Examples:

- TTS unavailable
- optional update check unavailable
- optional future sync unavailable

### Rule

The app should remain usable even when optional capabilities are unavailable.

## 5.3 No Blocking Startup Based on Remote Calls

App startup must not block on any remote dependency in Phase 1.

This is critical.

---

# 6. Offline Content Packaging Model

## 6.1 Phase 1 Content Delivery Model

Content should arrive locally through one of these approaches:

- bundled starter content imported into the local database
- operator-driven import through the import tool
- prebuilt app database for development/testing scenarios

## 6.2 Recommended Main Model

The cleanest model is:

- stable app schema
- content imported into the local database
- content then consumed locally by the app

## 6.3 Future Pack Thinking

Even if Phase 1 uses a single database, the model should still conceptually support:

- starter packs
- thematic packs
- CEFR packs
- later downloadable content packs

---

# 7. Local-Only Assumptions in Phase 1

## 7.1 Valid Assumptions

In Phase 1, it is safe to assume:

- one app installation has one local database
- content is available locally
- user state is local to that installation
- no server-side reconciliation is required
- no device-to-device sync is required

## 7.2 Invalid Assumptions

Do not assume:

- local user state automatically exists on another device
- imported content automatically reaches all devices
- app reinstall preserves local data unless platform backup handles it
- cloud-based recovery exists

These assumptions would be false in Phase 1.

---

# 8. Offline Update Model

## 8.1 Content Update Reality

In Phase 1, content updates are not live synchronized.

Content changes happen through local database update paths such as:

- importing a new content package
- replacing the local database in controlled development scenarios
- future app-delivered content package update flow

## 8.2 Rule

The app must not pretend there is automatic remote freshness if there is no backend.

## 8.3 Future Hybrid Extension

When the Web API is introduced, the mobile app should still behave as a local-first client:

- startup should still load from SQLite first
- update checks must be optional and non-blocking
- manual update actions may exist in Settings for:
  - full content update
  - content-area update
  - CEFR-level word update

## 8.4 User Expectation

The product should behave honestly:

- it uses the content currently stored locally
- that content remains available offline
- new content appears only after a deliberate update path

---

# 9. Sync Is Not Phase 1

## 9.1 Explicit Rule

Synchronization is not a Phase 1 requirement.

This includes:

- cross-device sync
- cloud favorites sync
- cloud progress sync
- remote content delta sync
- user account–based content continuity

## 9.2 Why This Boundary Matters

If sync is treated as “almost there” from day one, the design gets polluted by:

- unnecessary conflict models
- premature server assumptions
- authentication complexity
- remote source-of-truth logic
- extra infrastructure that provides no Phase 1 value

---

# 10. Future Sync Strategy Direction

Even though sync is out of scope for Phase 1, the architecture should support it later.

## 10.1 Future Sync Candidates

Likely future sync categories:

### User Sync
- learning preferences
- favorites
- known/difficult markers
- review state later

### Content Sync
- new or updated content packages
- localized topic updates
- curated content improvements

## 10.2 Recommended Future Sync Separation

Future sync should treat **content sync** and **user-state sync** as different concerns.

This is critical.

### Content Sync

Shared and global.
Likely server-authored.

### User-State Sync

Per-user and account-bound.
Likely cloud-linked.

### Rule

Do not design one generic sync mechanism that treats both categories as identical.

---

# 11. Conflict Model for the Future

## 11.1 Phase 1 Conflict Model

There is effectively no cross-device sync conflict model in Phase 1 because the local database is the source of truth for that installation.

## 11.2 Future Conflict Areas

If sync is added later, conflicts will mainly appear in:

- user preferences changed on multiple devices
- favorites changed on multiple devices
- review/progress state changed on multiple devices
- locally cached content vs newer server content

## 11.3 Recommended Future Principle

When sync is introduced later:

- content should usually be server-authoritative
- user state may require merge strategies
- some local state may remain device-local by design

---

# 12. Merge Strategy Thinking for the Future

## 12.1 User Preferences

Likely simple merge options later:

- last-write-wins
- server-wins
- explicit user choice in rare cases

## 12.2 Favorites

Favorites are often easy to merge using additive union.

Example:
If one device favorites a word and another does not remove it, the union strategy is usually acceptable.

## 12.3 Review State

Review scheduling and progress are harder.
These should be treated as a distinct future problem inside the Practice context.

### Rule

Do not solve future review-state conflicts with Phase 1 data structures.

---

# 13. Offline-First UI Behavior

## 13.1 UI Must Assume Local Availability

The UI should be designed to load from local storage first.

This affects:

- app startup
- browse pages
- filters
- word detail pages
- favorites
- preferences

## 13.2 Loading States

Loading states in Phase 1 should mostly reflect:

- local database access
- search/query execution
- local preference update

They should not imply remote fetching when there is none.

## 13.3 Messaging

The UI should avoid misleading wording such as:

- syncing...
- downloading latest...
- connecting to server...

unless those features actually exist.

---

# 14. Offline Import Strategy

## 14.1 Import Tool Should Be Offline-Capable

The import tool should work entirely from:

- local JSON files
- local validation
- local database writes

This is a major advantage in early product development.

## 14.2 Operator Model

An operator should be able to:

- generate content file
- review content file
- run import locally
- inspect warnings/errors
- verify app behavior locally

without needing backend infrastructure

---

# 15. Future Content Distribution Options

These are future options, not Phase 1 requirements.

## 15.1 Option A - App-Embedded Starter Pack

The app ships with a starter content package.

## 15.2 Option B - Side-Loaded Content Import

A local import utility adds content to the database.

## 15.3 Option C - Server-Downloaded Content Packs

A backend later provides downloadable versioned content packages and manifests.

## 15.4 Recommendation

Phase 1 supports A and B.

The next content-distribution step should implement C while still preserving local-first runtime behavior.

---

# 16. Offline Reliability Rules

## 16.1 Core Rule

The app should remain stable and usable even when there is no network at all.

## 16.2 Secondary Rule

Any optional feature that may fail because of platform/service conditions must degrade gracefully.

## 16.3 Data Integrity Rule

Local data writes should be transactional where appropriate.

This is especially important for:

- import operations
- updating favorites
- updating user preferences
- updating user word state

---

# 17. Local Data Reset Reality

## 17.1 Risk

In a local-first Phase 1 design, local data may be lost if:

- the app is uninstalled
- the database file is deleted
- platform storage is cleared
- development builds reset local storage

## 17.2 Product Truth

This should be understood as a real limitation of Phase 1.

## 17.3 Recommendation

Document this clearly for yourself during development.
Do not assume cloud recovery exists when it does not.

---

# 18. Backup and Export Thinking

## 18.1 Phase 1 Recommendation

Do not make backup/export a required Phase 1 feature.

## 18.2 But Keep the Door Open

Because SQLite is used, future capabilities may include:

- exporting favorites
- exporting preferences
- exporting/importing user state
- database backup for development or admin scenarios

This is a future product opportunity, not a current requirement.

---

# 19. Future Hybrid Model

## 19.1 Likely Long-Term Model

A likely future model for Darwin Deutsch is:

- local database for offline access and caching
- remote backend for shared content publishing and package manifests
- remote account storage for user continuity
- local queued operations for sync when back online

## 19.2 Why the Current Design Supports This

The current design already separates:

- content
- user state
- operational import data

This is exactly what makes future hybrid sync possible.

---

# 20. Offline Strategy Decisions

## Decision 1

Phase 1 is local-first.

## Decision 2

The local SQLite database is the source of truth per installation.

## Decision 3

Core learning features must not require network access.

## Decision 4

Platform TTS is treated as a capability, not as guaranteed server-backed media.

## Decision 5

Sync is explicitly out of scope for Phase 1.

## Decision 6

Future sync must separate shared content sync from user-state sync.

## Decision 7

The UI must not imply remote behavior that does not exist.

---

# 21. Final Offline Summary

Darwin Deutsch Phase 1 should behave as a strong offline-capable local application.

The correct strategy is:

- local-first
- no backend dependency for core learning
- local content database
- local user state
- offline-capable import workflow
- future-ready but not sync-driven

This keeps the first version honest, reliable, and practical while preserving a clean path to later cloud expansion.
