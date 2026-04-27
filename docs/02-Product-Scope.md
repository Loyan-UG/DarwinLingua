# Product Scope

## Purpose

This document defines the practical scope of Darwin Deutsch, with special focus on the Phase 1 delivery boundary.

It exists to answer:

- what this product is responsible for now
- what must be implemented in Phase 1
- what must remain out of scope for Phase 1
- what platform, localization, and delivery constraints are active from day one
- which later expansion areas are planned but must stay outside the early MVP unless explicitly selected

This document should be read together with:

- `01-Product-Vision.md`
- `03-Product-Phases.md`
- `21-Early-Product-Decisions.md`
- `25-Phase-1-Domain-Cut.md`
- `63-Market-Product-And-Organizer-Strategy.md`
- `64-Conversation-And-Organizer-Implementation-Backlog.md`

---

## 1. Product Boundary

### 1.1 Platform Boundary

`Darwin Lingua` is the platform and repository umbrella.

`Darwin Deutsch` is the first learner-facing product on that platform.

### 1.2 Product Boundary

Darwin Deutsch is a practical German-learning product focused on structured vocabulary learning for real-life usage.

It is not a generic social platform, not a broad AI tutor, and not a full migrant-service portal in Phase 1.

The long-term product boundary can expand into practical scenario learning, event discovery, safe conversation-partner discovery, and organizer tooling, but these must remain separate from the Phase 1 vocabulary boundary.

---

## 2. Phase 1 In Scope

Phase 1 must deliver a serious, usable, local-first vocabulary product.

### 2.1 User Features In Scope

- browse words by CEFR level
- browse words by topic
- search by German lemma
- view word details
- show multilingual meanings
- show example sentences
- play pronunciation through platform text-to-speech
- save favorite words
- store lightweight user word state
- support one or two selected meaning languages at the same time

### 2.2 Delivery Scope In Phase 1

- MAUI learner application
- local database initialization
- reference data seeding
- offline-capable core experience
- structured JSON content import through a separate import host/tool

### 2.3 Data Scope In Phase 1

- German source vocabulary
- controlled CEFR values
- controlled topic keys
- multilingual meaning translations
- example sentences and example translations
- user preferences and lightweight local user state

---

## 3. Phase 1 Out of Scope

The following must remain outside the Phase 1 implementation boundary:

- spaced repetition
- flashcard engine
- review sessions
- quiz engine
- grammar engine
- collocations and lexical relations
- support-resource directory
- social/community features
- public user-generated content
- cloud sync
- user accounts
- server-driven content updates
- web application
- public web API
- admin editing workflows
- import merge/update behavior

That boundary is deliberate and should not be blurred.

---

## 4. Later Product Expansion Scope

The following areas are valid later-phase expansion areas, but they are not Phase 1 requirements.

### 4.1 Practical Scenario Learning

Later phases may add structured scenario lessons for real German situations such as:

- doctor visits
- kindergarten and school communication
- workplace conversations
- job interviews
- government-office appointments
- housing and landlord conversations
- shopping and services
- conversation cafes and first meetings

### 4.2 Conversation Starters and Roleplay

Later phases may add:

- conversation starter packs
- likely questions and model answers
- useful fallback phrases
- politeness and tone notes
- scripted roleplay preparation
- future AI-assisted practice only after cost, privacy, and safety boundaries are explicit

### 4.3 Event and Resource Discovery

Later phases may add:

- conversation cafe listings
- language club listings
- online conversation events
- local integration and support resources
- event preparation packs linked to learning content

### 4.4 Organizer Tooling

Later phases may add B2B organizer tools for:

- conversation cafes
- teachers
- clubs
- libraries
- student associations
- integration organizations
- companies supporting foreign employees

Organizer tooling may include profiles, event publishing, RSVP, attendance lists, preparation packs, analytics, and plan-based feature flags.

### 4.5 Safe Social and Matching Features

Later phases may add learner profiles and request-based conversation-partner discovery.

These features must not ship publicly without:

- age and safety rules
- profile visibility controls
- report and block actions
- moderation workflows
- rate limits
- privacy review
- no unrestricted open chat in the first matching MVP

---

## 5. Platform Scope

### 5.1 Active Phase 1 Platforms

- Android
- iOS
- Windows

### 5.2 Planned Later Platforms

- Web

The architecture must support future web reuse, but the first production priority is the MAUI application.

---

## 6. Localization Scope

Localization is not optional and must start in Phase 1.

### 6.1 UI Language Scope

The user interface must support these UI languages from the start:

- English
- German

### 6.2 Localization Rules

- all user-visible UI strings must come from localization resources
- no hard-coded user-facing strings should remain in screens or view models
- the default UI language should follow the device language where supported
- the user must be able to override the UI language in settings

### 6.3 Meaning Language Scope

The content model must support multiple meaning languages.

Phase 1 may begin with a limited subset of meaning languages in real content packs, but the platform must support:

- one selected meaning language
- two selected meaning languages at the same time

Dual meaning-language learning is a strategic feature. For example, a learner may view German with English and Persian, English and Arabic, English and Turkish, or another supported combination.

---

## 7. Technical Scope

### 7.1 Storage Scope

For the MAUI/local-first Phase 1 product, SQLite is the correct storage choice.

If later web, API, admin, or shared cloud scenarios require a stronger server database, PostgreSQL is a good default candidate for that server-side part of the platform.

### 7.2 Import Scope

Phase 1 import is:

- JSON-based
- validation-driven
- insert-oriented
- duplicate-aware
- offline-capable

It is not:

- merge-capable
- admin-driven content management
- dynamic uncontrolled taxonomy creation

### 7.3 Quality Scope

Phase 1 must not be treated as a throwaway prototype.

The implementation is expected to follow the engineering standards defined in:

- `35-Engineering-Standards.md`

---

## 8. UX Scope

UI/UX is a first-class concern from the beginning.

Phase 1 must not ship as a purely technical demo.

Minimum UX expectations:

- clear browsing flow
- readable multilingual presentation
- consistent localization behavior
- responsive layouts
- accessible interaction patterns
- stable offline behavior

Later scenario, event, organizer, and matching features must keep the same quality bar and must not be added as disconnected feature islands.

---

## 9. Monetization Scope

Monetization should not block the core learning value.

Valid future monetization areas include:

- learner premium convenience features
- advanced scenario packs
- advanced review or roleplay features
- event preparation packs
- organizer profiles and self-service event management
- featured organizer or event listings
- organizer analytics and attendee management

Core catalog browse, basic word details, and enough practical learning content to make the product useful should remain available without a heavy paywall.

---

## 10. Scope Guardrails

When deciding whether a feature belongs in Phase 1, use these questions:

- does it directly improve vocabulary discovery or understanding?
- does it support local-first reliability?
- does it preserve architectural clarity?
- can it be implemented without dragging in future-phase complexity?

If the answer is no, it probably does not belong in Phase 1.

When deciding whether a later social or organizer feature is safe to release, use these questions:

- can the feature be abused?
- can users report or block abuse?
- can admins review public listings and user reports?
- does the feature expose private location or contact data?
- does the feature require unrestricted messaging?
- is the monetization fair and non-predatory?

If the safety answer is weak, the feature should not ship publicly.

---

## 11. Final Scope Summary

Phase 1 is a multilingual, local-first, vocabulary-first German learning product with:

- structured content
- stable import workflow
- multilingual UI support
- English and German UI localization
- clear architectural boundaries

It is intentionally not yet a full learning ecosystem.

Later phases can expand the product into real-life scenario learning, conversation practice, event discovery, organizer tooling, and safe partner matching, but those areas must be implemented with explicit safety, privacy, moderation, and monetization boundaries.
