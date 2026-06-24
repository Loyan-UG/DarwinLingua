# Implementation Backlog

## Purpose

This document is the working implementation backlog for the repository.

It is intended to:

- show what is done and what is still pending
- define the Phase 1 implementation order
- keep the next execution steps explicit
- provide high-level placeholders for later phases

This backlog should be updated continuously during implementation.

---

## Status Tracking Rule

Use these markers consistently:

- `[ ]` not started
- `[-]` in progress
- `[x]` completed
- `[!]` blocked / needs decision

Do not delete completed items. Mark them as completed so progress remains visible.

---

## Current Dialogue Follow-Ups

- [ ] Extend admin editing forms and MAUI list/detail screens to expose Dialogue exam metadata, useful word references, and speaking prompts with the same richness as the Web learner pages.
- [ ] Replace templated starter translations for non-English/non-Persian Dialogue meaning languages with human-reviewed localized translations before public launch.

---

## Phase 1 Target Outcome

Phase 1 is complete when the repository contains a usable local-first MAUI app and a working import pipeline that together support:

- database initialization
- reference data seeding
- content package import
- CEFR browsing
- topic browsing
- German lemma search
- word details
- English and German UI
- user-selectable UI language
- favorite words
- lightweight user word state
- platform text-to-speech

---

## Phase 1 Backlog

### 1. Repository and Tooling Foundation

- [x] add `global.json`
- [x] add `Directory.Build.props`
- [x] add `Directory.Packages.props`
- [x] add local EF Core tool manifest for migration commands
- [x] define package version management strategy
- [x] define solution-wide nullable and warnings policy
- [x] set up formatting and analysis defaults
- [x] add CI pipeline for restore, build, and tests

### 2. Documentation and Standards Adoption

- [x] clean up and normalize documentation file names
- [x] remove duplicate and obsolete documentation files
- [x] create a canonical documentation index
- [x] create a working implementation backlog
- [x] define engineering standards
- [-] keep backlog updated during implementation
- [x] add continuation handoff document and new-chat prompt template

### 3. Architecture and Project Wiring

- [x] decide and lock the canonical Phase 1 project/reference structure
- [x] add real project references between Domain, Application, Infrastructure, and hosts
- [x] create composition roots for MAUI and ImportTool
- [x] define dependency injection registration conventions
- [x] add centralized shared infrastructure for Phase 1 persistence
- [x] add shared abstractions only where they provide clear value

### 4. Localization Foundation

- [x] add English UI resource files
- [x] add German UI resource files
- [x] create localization access pattern for the MAUI app
- [x] ensure all user-facing strings come from resources
- [x] implement device-language-based default culture selection
- [x] implement user override of UI language in settings
- [x] persist selected UI language locally

### 5. UI and UX Foundation

- [x] define application navigation structure
- [x] define design tokens and styling foundations
- [x] replace default template screens with real product screens
- [x] establish reusable UI components for list items, filters, and detail sections
- [x] define empty/loading/error states
- [x] define accessibility baseline for typography, contrast, and touch targets

### 6. Storage Foundation

- [x] create EF Core database context
- [x] configure SQLite persistence for the MAUI/local application
- [x] define migration strategy in code
- [x] implement database initialization flow
- [x] create migration-based startup initialization
- [x] define database file location strategy per platform
- [x] add basic transactional support for write workflows

### 7. Reference Data and Seeding

- [x] define `Language` reference data
- [x] define supported UI languages for Phase 1
- [x] define supported meaning languages for initial rollout
- [x] define initial topic set
- [x] define topic localizations
- [x] implement stable reference-data seeding
- [x] ensure seeding is idempotent

### 8. Catalog Domain Implementation

- [x] implement `WordEntry`
- [x] implement `WordSense`
- [x] implement `SenseTranslation`
- [x] implement `ExampleSentence`
- [x] implement `ExampleTranslation`
- [x] implement `Topic`
- [x] implement `TopicLocalization`
- [x] implement `WordTopic`
- [x] enforce aggregate invariants
- [x] enforce uniqueness and relationship constraints

### 9. Learning Domain Implementation

- [x] implement `UserLearningProfile`
- [x] implement `UserFavoriteWord`
- [x] implement `UserWordState`
- [x] enforce separation between content and user state
- [x] persist user meaning-language preferences
- [x] persist user UI-language preference

### 10. Content Operations Implementation

- [x] implement `ContentPackage`
- [x] implement `ContentPackageEntry`
- [x] define package/result status model
- [x] implement content package file reader
- [x] implement JSON parser
- [x] implement file-level validation
- [x] implement entry-level validation
- [x] implement normalization pipeline
- [x] implement duplicate detection
- [x] implement import summary/report model
- [x] implement transactional persistence for imports

### 11. Application Use Cases

- [x] implement `InitializeDatabase`
- [x] implement `SeedReferenceData`
- [x] implement `EnsureUserLearningProfileExists`
- [x] implement `GetUserLearningProfile`
- [x] implement `GetTopics`
- [x] implement `GetWordsByCefr`
- [x] implement `GetWordsByTopic`
- [x] implement `SearchWords`
- [x] implement `GetWordDetails`
- [x] implement `UpdateMeaningLanguagePreferences`
- [x] implement `UpdateUiLanguagePreference`
- [x] implement `ToggleFavorite`
- [x] implement `GetFavoriteWords`
- [x] implement `TrackWordViewed`
- [x] implement `MarkWordKnown`
- [x] implement `MarkWordDifficult`
- [x] implement `ClearWordKnownState`
- [x] implement `ClearWordDifficultState`
- [x] implement `ImportContentPackage`
- [x] implement Talk Topic import parsing, validation, persistence, query, WebApi read endpoints, Web learner pages, and catalog package export
- [ ] add mobile Talk Topic parity after the Web flow is signed off
- [x] add first automated Talk Topic import validation, persistence, export, and endpoint smoke coverage
- [ ] expand automated Talk Topic coverage for Web rendering, localization, unresolved vocabulary links, and additional query filter combinations

### 12. MAUI Screens

- [x] implement home/browse screen
- [x] implement CEFR browsing flow
- [x] implement topic browsing flow
- [x] implement search flow
- [x] implement word detail screen
- [x] implement favorites screen
- [x] implement settings screen
- [x] implement localization switching in UI
- [x] implement meaning-language preferences in settings

### 13. Search and Audio

- [x] define lemma normalization/search behavior
- [x] implement local search queries
- [x] evaluate and implement SQLite search/index strategy
- [x] integrate platform TTS for words
- [x] integrate platform TTS for example sentences
- [x] define graceful failure behavior for missing TTS capability

### 14. Testing and Quality

- [x] add domain tests for aggregate invariants
- [x] add application tests for main use cases
- [x] add infrastructure tests for persistence mappings
- [x] add import workflow tests
- [x] cover successful package import with SQLite-backed integration testing
- [x] cover duplicate `packageId` rejection with SQLite-backed integration testing
- [x] cover SQLite search-index bootstrap and prefix-first ranking with integration tests
- [x] cover legacy `EnsureCreated` database baselining with integration tests
- [x] add seed-data tests
- [x] add localization coverage checks
- [x] add smoke tests for the MAUI startup path where practical
- [x] resolve local Windows application-control policy issue blocking some test assemblies and transitive dependencies; full local Windows solution restore/build/test now runs successfully

### 15. Release Readiness

- [ ] validate offline behavior on target platforms
  - Validation backlog: remaining Web/PWA validation is itemized in `71-Web-Test-Backlog.md` and the Web validation worksheets.
- [ ] validate English UI
  - Validation backlog: remaining Web UI validation is itemized in `71-Web-Test-Backlog.md` and the Web validation worksheets.
- [ ] validate German UI
  - Validation backlog: remaining Web UI validation is itemized in `71-Web-Test-Backlog.md` and the Web validation worksheets.
- [x] validate data initialization from clean install
- [x] validate import of sample content package
- [x] validate performance on realistic starter datasets
- [x] define release checklist for Phase 1 MVP

---

## Phase 2 Backlog

Phase 2 focuses on turning imported content into repeatable learning behavior while keeping the product local-first.

### 16. Phase 2 Planning and Module Foundation

- [x] define the ordered Phase 2 backlog and execution slices
- [x] add the `Practice` bounded-context projects to the solution structure
- [x] wire `Practice` module registration into the current MAUI and import-tool hosts
- [x] implement the first `GetPracticeOverview` use case for progress and review-entry visibility
- [x] add integration coverage for practice-overview queries over imported sample content

### 17. Practice Scheduling and Review Rules

- [x] define deterministic review-candidate prioritization rules
- [x] define the persistent practice-attempt and review-scheduling model
- [x] implement spaced-repetition scheduling updates after answers
- [x] persist wrong-answer counters and recent-attempt history
- [x] add tests for scheduling transitions and prioritization behavior

### 18. Practice Application Use Cases

- [x] implement `GetPracticeOverview`
- [x] implement `GetReviewQueue`
- [x] implement `StartReviewSession`
- [x] implement `SubmitFlashcardAnswer`
- [x] implement `SubmitQuizAnswer`
- [x] implement `GetRecentActivity`
- [x] implement `GetLearningProgressSnapshot`

### 19. Practice UI

- [x] add a localized practice entry point to the MAUI navigation flow
- [x] implement the practice overview screen
- [x] implement the flashcard session flow
- [x] implement the quiz session flow
- [x] implement answer feedback and session-summary states
- [x] keep all new learner-facing text localized in `AppStrings.resx` and `AppStrings.de.resx`

### 20. Phase 2 Quality and Release Readiness

- [x] add application tests for review and quiz use cases
- [x] add infrastructure tests for practice queries and persistence
- [x] add MAUI smoke tests for practice navigation
- [ ] validate practice flows on target devices
  - Validation backlog: remaining target-device validation is itemized in `71-Web-Test-Backlog.md` and the phase validation worksheets.
- [x] validate performance on realistic early-learning datasets

---

## Phase 3 Backlog

Phase 3 focuses on richer lexical intelligence while continuing to prioritize the mobile learner experience.

### 21. Phase 3 Planning and Content-Contract Evolution

- [x] define the ordered Phase 3 backlog and mobile execution slices
- [x] extend the content package contract for lexical usage/context labels
- [x] add sample content coverage for the first lexical-intelligence metadata slice

### 22. Lexical Intelligence Domain Foundation

- [x] implement lexical usage/context labels on `WordEntry`
- [x] persist lexical labels with uniqueness and ordering constraints
- [x] implement grammar notes
- [x] implement word families
- [x] implement collocations
- [x] implement lexical relations such as synonyms and antonyms

### 23. Lexical Intelligence Import and Query Workflows

- [x] parse and validate `usageLabels` and `contextLabels` during import
- [x] persist imported lexical labels through the current content pipeline
- [x] expose lexical labels from `GetWordDetails`
- [x] expose grammar notes from `GetWordDetails`
- [x] expose collocations from `GetWordDetails`
- [x] expose word families from `GetWordDetails`
- [x] expose richer lexical relations from detail queries

### 24. Mobile Word Detail UX Evolution

- [x] redesign the word-detail hero and metadata layout for richer content
- [x] render usage/context metadata as wrapped chips with localized display text
- [x] surface grammar notes in the word-detail flow
- [x] surface collocations in the word-detail flow
- [x] surface word families in the word-detail flow
- [x] surface lexical relations in the word-detail flow
- [x] review and polish the main learner-facing screens for stronger visual consistency

### 25. Phase 3 Quality and Release Readiness

- [x] add domain/application/infrastructure coverage for lexical-label behavior
- [x] add localization smoke coverage for lexical-label display helpers
- [ ] validate enhanced word-detail UX on target devices
  - Validation backlog: remaining target-device validation is itemized in `71-Web-Test-Backlog.md` and the phase validation worksheets.

---

## Phase 4 Backlog Placeholder

Phase 4 focuses on migrant-support resource discovery.

Planned areas:

- support resources
- category and location search
- saved useful resources
- admin-managed resource updates

---

## Phase 5 Backlog

Phase 5 focuses on server-backed content distribution and platform expansion.

### 26. Shared Content Source-of-Truth Architecture

- [x] lock the server-authored content distribution architecture
- [x] define the shared-content source-of-truth boundary vs local user-state boundary
- [x] define the multi-product shared-backend partition strategy for future learner apps
- [x] define the mobile update scopes:
  - full database update
  - content-area update
  - CEFR-level word update
- [x] define server package/version metadata and checksum rules
- [x] define local package-receipt tracking for mobile clients

### 27. Web API Content Distribution Foundation

- [x] add the Web API host to the solution structure
- [x] add baseline server configuration files for local PostgreSQL and Web API development
- [x] define the mobile content manifest contract
- [x] implement read-only manifest endpoints for mobile clients
- [x] add PostgreSQL-backed persistence for client products, content streams, and published packages
- [x] implement package-download endpoints for full, area, and CEFR-slice updates
- [x] define schema-version compatibility checks between server packages and mobile clients

### 28. Server-Side Content Operations

- [x] move the canonical import pipeline from local-only seed generation to server-side shared-content import
- [x] persist imported content into the central PostgreSQL catalog
- [x] persist package receipts and publishing/version metadata on the server
- [x] define publishing workflow for draft vs published content packages
- [x] add lightweight admin endpoints to inspect staged and published package batches
- [x] add cleanup flow for superseded package batches and payload files
- [x] add publish-history and retention-summary endpoints for admin operations
- [x] add rollback workflow for superseded package batches
- [x] add audit trail for publish, rollback, and cleanup events
- [x] add a local bootstrap flow for first server import and publish

### 29. Mobile Content Update Client

- [x] add a mobile update client that can fetch manifests and packages from the Web API
- [x] implement transactional application of downloaded packages into local SQLite
- [x] preserve favorites, preferences, word state, and practice state during shared-content updates
- [x] add diagnostics for local version, remote version, last update time, and last update failure
- [x] add recent local update history visibility for remote update attempts

### 30. Settings Update UX

- [x] add a primary `Update All Content` action in settings
- [x] add per-area update actions in settings
- [x] add `A1`-`C2` per-level update actions for the word catalog
- [x] expose update counts and version summaries before apply
- [x] define non-blocking offline/error behavior for update actions

### 31. Future Platform Expansion

- [x] add admin application planning slices
- [x] add web application planning slices
- [x] define account and sync boundaries
- [x] define analytics boundaries
- [x] define monetization boundaries

### 32. Web Platform Planning and Architecture

- [x] define the ASP.NET Core MVC web-platform architecture
- [x] define the learner-facing root site vs `Areas/Admin` split
- [x] define the Tailwind + `htmx` UI direction
- [x] define the installable PWA baseline
- [x] define the first user-account boundary for web learners
- [x] define the shared-backend reuse rules between Web API, MVC web, and mobile
- [x] define the first ordered implementation slices for the future web project

### 33. Web Platform Implementation Placeholder

- [x] add `DarwinLingua.Web` MVC host to the solution
- [x] add the first learner-facing shared layout and navigation shell
- [x] add `manifest.webmanifest`, icons, and service worker baseline
- [x] add the first `Areas/Admin` dashboard shell
- [x] add `htmx` and Tailwind-ready frontend baseline files
- [x] add ASP.NET Core Identity with learner registration and sign-in
- [x] add learner-facing browse/search/detail pages
- [x] add learner favorites and recent-activity persistence on the web
- [x] add `Areas/Admin` with authorization boundary and operator shell
- [x] add first admin pages for content batch inspection and publish history

### 34. Shared Account, Role, And Seed Foundation

- [x] define the shared account boundary for web and mobile
- [x] keep web on cookies while defining token-based mobile sign-in against the same identity system
- [x] assign the default `Learner` role on successful learner registration
- [x] seed a non-production system admin account
- [x] seed a non-production learner test account
- [x] move seed-account credentials to environment-backed local secrets
- [x] validate clean-start role creation and seed-account bootstrap

### 35. Monetization And Entitlement Foundation

- [x] define entitlements as a separate model from roles
- [x] define baseline entitlement states for `Free`, `Trial`, and `Premium`
- [x] define the first server-owned premium feature gating boundary shared by web and mobile
- [x] keep core catalog browse and word viewing permanently outside premium enforcement unless product policy changes later
- [x] add backlog placeholders for premium-capable features such as favorites, dual-meaning-language mode, and future advanced practice flows
- [x] define audit and diagnostics requirements for entitlement changes and trial expiration
- [x] add an admin surface for entitlement inspection and tier changes
- [x] add Web API admin endpoints for entitlement inspection and tier changes

## Phase 6 Backlog

This phase is the merged execution backlog for the practical conversation, learner-profile, event-directory, and B2B organizer direction. It preserves the full content that previously lived in 64-Conversation-And-Organizer-Implementation-Backlog.md.

### Purpose

This document is the executable backlog for the practical conversation, learner-profile, event-directory, and B2B organizer direction.

It expands the product beyond vocabulary and review into:

- scenario-based practical German learning
- conversation starters and roleplay preparation
- multilingual and dual-meaning-language learning flows
- local conversation clubs, cafes, and events
- organizer profiles and B2B organizer tooling
- safe learner profiles and request-based conversation matching
- premium and organizer entitlement boundaries
- moderation and safety workflows

Use this document together with:

- `04-Implementation-Backlog.md`
- `63-Market-Product-And-Organizer-Strategy.md`
- `52-Future-Platform-Expansion.md`
- `38-Web-Platform-Architecture.md`

---

### Status Tracking Rule

Use these markers consistently:

- `[ ]` not started
- `[-]` in progress
- `[x]` completed
- `[!]` blocked / needs decision

Do not delete completed items. Mark them as completed so the implementation history remains visible.

---

### Target Outcome

This backlog is complete when Darwin Lingua supports a safe and useful practical conversation layer:

- learners can prepare for real German situations using dialogue practice lessons
- learners can see German content with one or two meaning languages at the same time
- learners can use conversation starters for specific contexts
- learners can find local and online conversation events
- organizers can publish and manage language-practice events
- learners can prepare for events using event-specific preparation packs
- safe profile and partner-request flows exist without unrestricted open chat in the MVP
- moderation, report, block, and organizer verification workflows exist
- premium learner features and organizer plan flags are technically possible

---

### 1. Product and Documentation Foundation

- [x] document market, product, and organizer strategy
- [x] document the conversation and organizer implementation backlog
- [x] update product phases to explicitly include scenario learning, event discovery, organizer tooling, and safe partner matching
- [x] update product scope with MVP/non-MVP boundaries for the social and organizer layer
- [x] update domain model documentation with planned `Scenario`, `Event`, `Organizer`, `Profile`, `PartnerRequest`, and `Moderation` concepts
- [x] update bounded-context documentation with `Scenarios`, `Events`, `Organizers`, `Profiles`, `Matching`, and `Moderation`
- [x] update monetization documentation with learner and organizer entitlement boundaries
- [x] add a safety and moderation requirements document before implementing public profiles or matching

---

### 2. Dual Meaning-Language Learning

#### Goal

Make dual meaning-language learning a first-class product feature.

Examples:

- German + English + Persian
- German + English + Arabic
- German + English + Turkish
- German + English + Ukrainian
- German + English + Russian

#### Backlog

- [x] review the existing meaning-language preference model and confirm it supports exactly one or two active meaning languages cleanly
- [x] define UI rules for primary and secondary meaning languages
- [x] add compact and expanded translation display modes where needed
- [x] ensure word details, examples, dialogue practice lessons, and conversation starters can display two meaning languages consistently
- [x] add validation rules for missing translations in one of the selected meaning languages
- [x] add fallback rules when the secondary meaning language is unavailable
- [x] add tests for dual-language selection and rendering decisions
- [x] add content-quality checks that identify incomplete dual-language coverage

---

### 3. Dialogue Practice

#### Goal

Add practical real-life dialogue practice lessons that help learners use German in concrete situations.

#### Initial Dialogue Categories

- doctor and healthcare
- school and kindergarten
- workplace and colleagues
- job interviews
- government offices and appointments
- housing and landlords
- shopping and services
- transport and travel
- social small talk
- conversation cafes and first meetings

#### Dialogue Practice Content Model

Each dialogue practice lesson should support:

- title
- description
- learner goal
- CEFR range
- topic/category
- context type: formal, informal, phone, office, workplace, social
- supported meaning languages
- dialogue turns
- key vocabulary
- likely questions
- useful answers
- politeness notes
- common mistakes
- roleplay prompts
- practice links

#### Backlog

- [x] define `DialogueLesson` content contract
- [x] define `DialogueTurn` content contract
- [x] define `DialoguePhrase` or `UsefulPhrase` content contract
- [x] define `DialogueQuestion` and `DialogueAnswer` content contract
- [x] define dialogue CEFR-level and topic mapping rules
- [x] define import validation rules for dialogue content
- [x] implement dialogue import pipeline support
- [x] persist dialogue lessons in the shared content model
- [x] expose dialogue lessons through application queries
- [x] add dialogue list and detail screens in MAUI
- [x] add dialogue list and detail pages in the web app when appropriate
- [ ] add tests for dialogue import, persistence, query, and rendering
  - Progress: import, persistence, application query, mobile package export, Web API build, and web controller language-selection tests are covered; keep open until Razor/page rendering is explicitly tested.
  - Test backlog: remaining Web coverage is itemized in `71-Web-Test-Backlog.md` and is owned by the separate test-development workflow.
- [x] add initial sample dialogues for A1/A2 learners

#### Talk Topics

`Talk Topics` are separate from dialogue practice. They contain a topic, article-style German text, warm-up and discussion questions for group conversation, speaking goals, optional sensitivity metadata, and linked vocabulary that points to canonical word detail URLs.

---

### 4. Conversation Starters and Topic Packs

#### Goal

Help learners start and continue real conversations.

#### Conversation Starter Dimensions

- situation: work, cafe, class, neighbor, school, event, online meeting
- CEFR level: A1, A2, B1, B2
- tone: formal, friendly, very simple, casual
- goal: introduction, asking questions, continuing conversation, ending politely, arranging next contact
- meaning languages: selected one or two languages

#### Backlog

- [x] define `ConversationStarterPack` content contract
- [x] define `ConversationStarterPhrase` content contract
- [x] define starter categories and filters
- [x] support starter packs inside dialogue practice lessons
- [x] support standalone starter packs
  - Progress: content contract, parser model support, import-boundary validation, persistence, catalog queries, WebApi endpoints, web/mobile UI, and mobile package export for `conversationStarterPacks` are implemented.
- [x] add mobile browsing and detail UI for starter packs
- [x] connect starter packs to event preparation packs
  - Progress: starter packs accept/persist `linkedEventPreparationPackSlugs`; event-preparation packs accept/persist/query reciprocal `linkedConversationStarterPackSlugs`; scenario detail surfaces related preparation packs in web and mobile.
- [ ] add tests for starter filtering and dual-language rendering
  - Progress: parser contract, invalid-contract validation, persistence, query filtering, scenario-link query coverage, dual-language query coverage, and web/mobile rendering implementation exist; keep open until explicit UI rendering tests are implemented.
  - Test backlog: remaining Web coverage is itemized in `71-Web-Test-Backlog.md` and is owned by the separate test-development workflow.
- [x] add sample starter packs for conversation cafes, workplace small talk, and first meetings

---

### 5. Roleplay Preparation

#### Goal

Add structured roleplay practice without requiring AI in the first implementation.

#### MVP Direction

Start with scripted roleplay flows:

- system displays the other person's line
- learner selects or types a response later
- system gives static feedback or shows model answers
- learner can replay the roleplay

AI-assisted feedback can be added later behind a premium or pro tier.

#### Backlog

- [x] define `RoleplayScenario` content contract
  - Progress: `70-Roleplay-Content-Package-Contract.md` defines the scripted roleplay package and allows Web MVP fallback from scenario dialogue turns.
- [x] define role labels such as learner, doctor, teacher, colleague, organizer, partner
  - Progress: the roleplay contract now defines the supported starter role label set.
- [x] define scripted turn sequence model
  - Progress: the roleplay contract defines ordered prompt/learner-response turns and validation rules.
- [x] define answer-choice model for early MVP
  - Progress: the roleplay contract defines optional authored answer choices with correctness and static feedback.
- [x] define static feedback model
  - Progress: the roleplay contract defines deterministic feedback types and the no-AI MVP boundary.
- [x] add roleplay launch from scenario detail
  - Progress: Web scenario detail now links to a stateless scripted roleplay page derived from scenario dialogue turns.
- [x] persist basic roleplay attempts if useful
  - Progress: not persisted for the Web MVP because no scoring, scheduling, or learner-history behavior depends on attempts yet.
- [ ] add tests for roleplay sequence behavior
  - Test backlog: remaining Web coverage is itemized in `71-Web-Test-Backlog.md` and is owned by the separate test-development workflow.
- [x] keep AI roleplay feedback out of MVP unless cost and safety boundaries are decided
  - Progress: the contract documents the AI boundary, and the Web MVP uses only scripted model answers and static feedback.

---

### 6. Event and Club Directory

#### Goal

Let learners find conversation cafes, clubs, online meetings, language events, and support resources.

#### MVP Directory Fields

- name
- description
- city or online flag
- country/region
- approximate location
- event or resource category
- supported learner levels
- supported helper languages
- organizer name
- external link
- email or contact method if public
- schedule text
- price type: free, donation, paid, unknown
- verification status
- source/update metadata

#### Backlog

- [x] define `ResourceDirectory` and `ConversationEvent` boundaries
  - Progress: Web MVP uses `ConversationEvent` catalog records with separate supported-level, helper-language, and preparation-pack link collections; a future ResourceDirectory module can split this boundary when organizer self-service starts.
- [x] define event category list
  - Progress: event taxonomy now constrains categories, price types, and verification states in the domain model.
- [x] define city/location model without exposing precise private user locations
  - Progress: events store `City`, `CountryRegion`, `ApproximateLocation`, and `IsOnline`; no precise user or attendee location is stored.
- [x] implement read-only event directory model
  - Progress: `ConversationEvent` domain/EF mapping/migration is implemented for reviewed published listings.
- [x] implement event list query with filters by city, level, language, online/offline, and price type
- [x] implement event detail query
- [ ] add mobile event directory screens
- [x] add web event directory pages
- [x] support external registration links
- [x] support manual admin-managed event creation first
  - Progress: Web Admin can create/replace reviewed conversation events through WebApi while organizer self-service stays deferred.
- [x] add validation for stale event data
  - Progress: reviewed/verified admin-created events require a last-verified timestamp within the last 180 days.
- [x] add tests for filtering and visibility rules
  - Progress: repository tests cover published visibility, filters, detail materialization, and preparation-pack links.

---

### 7. Event Preparation Packs

#### Goal

Differentiate Darwin Lingua from generic event platforms by helping learners prepare before attending.

#### Preparation Pack Content

Each event can have:

- useful words
- opening sentences
- fallback phrases
- short dialogues
- likely questions
- topic suggestions
- roleplay exercises
- post-event review prompts

#### Backlog

- [x] define `EventPreparationPack` content package contract and parsed import model
- [x] allow preparation packs to link to existing dialogue practice lessons at the content-contract/parser/validation boundary
- [x] allow preparation packs to link to vocabulary entries at the content-contract/parser/validation boundary
- [x] allow preparation packs to link to conversation starter packs at the content-contract/parser/validation boundary
- [x] persist `EventPreparationPack` catalog records
- [x] query `EventPreparationPack` catalog records
- [x] expose read-only WebApi endpoints for event preparation packs
- [x] show preparation packs on scenario detail screens
- [x] add web/mobile preparation-pack detail screens
- [x] export event preparation packs in full and CEFR mobile catalog packages
- [x] show preparation packs on web event detail screens
  - Progress: Web event detail now surfaces entitled linked preparation packs; mobile parity is intentionally deferred until the Web backlog is complete.
- [x] add `Prepare for this event` action
  - Progress: scenario detail now presents entitled preparation-pack links as `Prepare for this event`.
- [x] track preparation-pack completion if useful
  - Progress: Web preparation-pack detail now exposes a `Mark prepared` action that records aggregate completion KPI events without adding learner-history persistence.
- [ ] add tests for event-to-pack mapping
  - Progress: parser coverage, invalid-contract application validation, EF persistence, migration coverage, positive import persistence tests, query filtering, scenario-link query, and detail query tests exist. UI mapping tests remain open until web/mobile screens are added.
  - Test backlog: remaining Web coverage is itemized in `71-Web-Test-Backlog.md` and is owned by the separate test-development workflow.

---

### 8. Organizer Profiles

#### Goal

Create public pages for clubs, teachers, cafes, organizations, and other language-practice organizers.

#### Organizer Profile Fields

- organizer name
- organization type
- description
- city/region
- online availability
- supported levels
- supported helper languages
- website
- public contact method
- verified status
- active events
- historical event count
- optional logo/image later

#### Backlog

- [x] define `OrganizerProfile` domain model
  - Progress: Web MVP stores organizer profiles with supported levels, helper languages, verification status, plan key, public contact fields, and historical event counts.
- [x] define organizer type list: teacher, cafe, club, association, school, company, library, other
- [x] implement read-only organizer profile pages
  - Progress: Web-only public organizer list/detail pages are implemented; mobile parity is intentionally deferred until the Web backlog is complete.
- [x] link events to organizer profiles
  - Progress: conversation events now support an optional `OrganizerProfileSlug`; public event detail and admin event tables link to the organizer page when present.
- [x] add admin-managed organizer creation first
  - Progress: Web Admin can create/replace reviewed organizer profiles through WebApi before organizer self-service is enabled.
- [x] add organizer claim workflow placeholder
  - Progress: Web organizer pages now expose a claim-request form; WebApi persists submitted claim requests for admin review, and Web Admin lists recent requests without granting self-service ownership yet.
- [x] add verified organizer badge rules
  - Progress: reviewed/verified organizer statuses are displayed as public organizer badges; stricter verification workflow remains in the moderation backlog.
- [x] add tests for organizer visibility and event linking
  - Progress: repository tests cover published organizer visibility, active event counts, and event linking; WebApi tests cover admin creation and invalid organizer type rejection.

---

### 9. Organizer Dashboard

#### Goal

Allow approved organizers to manage their own profile and events.

#### MVP Dashboard Features

- edit organizer profile
- create event
- edit event
- disable event
- define recurring schedule text or simple recurrence rules
- define capacity
- view RSVP list when RSVP is implemented
- see basic analytics

#### Backlog

- [x] define organizer account ownership rules
  - Progress: Web MVP uses explicit `OrganizerProfileOwner` assignments from reviewed organizer profile slug to owner email; admins assign owners after claim review.
- [x] define organizer admin role or entitlement boundary
  - Progress: Identity now includes an `Organizer` role; Web Admin can add/remove roles for users, and operator/admin roles keep override access.
- [x] implement organizer dashboard authorization
  - Progress: Web organizer dashboard is protected by the `Organizer` policy and only displays profiles assigned to the signed-in email.
- [x] implement profile edit workflow
  - Progress: assigned organizers can edit their owned public profile fields from the Web organizer dashboard while verification status, plan, and historical counts stay admin-controlled.
- [x] implement event create/edit workflow
  - Progress: assigned organizers can create new events and edit linked active events for owned profiles from the Web organizer dashboard; events remain tied to the owned `OrganizerProfileSlug`.
- [x] implement event activation/deactivation workflow
  - Progress: assigned organizers can archive and reactivate owned events from the Web organizer dashboard; archived events are hidden from the public directory but remain visible in the managed event list.
- [x] implement recurring-event support after one-off events work
  - Progress: Web/Admin and organizer event forms now support an optional recurrence rule and capacity field; public event detail displays those operational details without enabling RSVP yet.
- [x] implement basic organizer analytics
  - Progress: the Web organizer dashboard shows per-profile active/archived event counts, online/in-person counts, and total configured capacity from existing event data.
- [ ] add tests for authorization and ownership rules
  - Test backlog: remaining Web coverage is itemized in `71-Web-Test-Backlog.md` and is owned by the separate test-development workflow.

---

### 10. RSVP and Attendance

#### Goal

Let learners express interest or reserve a spot without building a full event-ticketing system.

#### Backlog

- [x] define `EventRsvp` model
- [x] support RSVP states: interested, going, cancelled, attended, no-show
- [x] define capacity and waitlist rules
  - Progress: Web MVP uses configured event capacity and computes remaining capacity from `going` RSVPs; waitlist behavior remains deferred until capacity overflow needs an explicit user workflow.
- [x] add `I am interested` or `Join` action on event detail
  - Progress: public event detail supports saving `interested`, `going`, and `cancelled` RSVP states.
- [x] show remaining capacity where appropriate
- [x] expose attendee count without exposing private attendee details publicly
  - Progress: public event detail shows aggregate interested/going counts and remaining capacity only.
- [x] show attendee list only to authorized organizers/admins
  - Progress: assigned organizer dashboard shows RSVP participant details for owned events; public event pages do not expose attendee identities.
- [x] add post-event attendance confirmation
  - Progress: assigned organizers can update RSVP rows for owned events to `attended` or `no-show` from the Web organizer dashboard.
- [ ] add tests for capacity, cancellation, and organizer visibility
  - Test backlog: remaining Web coverage is itemized in `71-Web-Test-Backlog.md` and is owned by the separate test-development workflow.

---

### 11. Learner Profiles

#### Goal

Create a minimal learner profile suitable for conversation practice and event participation.

#### MVP Profile Fields

- display name
- city or region
- online/in-person preference
- target-language learning level
- selected helper languages
- conversation goals
- availability notes
- profile visibility
- age confirmation for social features

#### Safety Boundaries

- no exact home address
- no public email or phone by default
- no full profile visibility for minors unless a separate safe model is designed
- allow user to hide or delete profile

#### Backlog

- [x] define learner profile model
  - Progress: WebApi now persists `LearnerConversationProfile` with display name, city/region, interaction preference, target-language learning level, helper languages, goals, availability, visibility, and adult confirmation.
- [x] define profile visibility states
  - Progress: Web MVP supports `private`, `request-only`, `public`, and `disabled`; re-enabling a disabled profile returns it to `private`.
- [x] define public profile projection separate from private profile data
  - Progress: WebApi exposes a public projection that omits owner email, availability notes, timestamps, and internal identifiers.
- [x] implement profile create/edit screen
  - Progress: signed-in Web learners can create and edit their conversation profile from the account area.
- [x] implement profile enable/disable toggle
  - Progress: signed-in Web learners can disable their profile or re-enable it as private.
- [x] implement profile deletion or anonymization flow
  - Progress: signed-in Web learners can delete their profile through an anonymization flow that clears public/private profile content and marks the profile disabled.
- [ ] add tests for public/private projection
  - Test backlog: remaining Web coverage is itemized in `71-Web-Test-Backlog.md` and is owned by the separate test-development workflow.
- [x] add privacy review before release
  - Progress: `65-Safety-And-Moderation-Requirements.md` now includes a Phase 6 privacy review checklist covering public exposure, admin exposure, retention/removal, and release decision states.

---

### 12. Safe Partner Matching

#### Goal

Let learners find conversation partners without creating an unsafe open social network.

#### MVP Direction

Use request-based matching, not unrestricted open chat.

#### Matching Filters

- city/region
- online/in-person
- target-language learning level
- helper languages
- native language
- learning goal
- availability

#### Backlog

- [x] define `PartnerRequest` model
  - Progress: WebApi now persists request-based partner introductions between learner profiles without creating an open chat model.
- [x] define match search query model
  - Progress: Web matching search supports city/region, interaction preference, target-language learning level, helper language, and goal keyword filters.
- [x] define request states: pending, accepted, declined, cancelled, blocked, expired
  - Progress: WebApi partner requests support the full MVP state set and expire pending requests after their expiry timestamp.
- [x] add rate limits for new users
  - Progress: WebApi limits learners to five new partner requests per 24-hour window and blocks duplicate pending requests to the same profile.
- [x] add predefined opener templates
  - Progress: Web request forms use predefined opener templates: practice goals, same city, online practice, and event follow-up.
- [x] add accept/decline flow
  - Progress: Web learners can accept, decline, block, or cancel partner requests from the Partners page.
- [x] avoid unrestricted direct messaging in MVP
  - Progress: Web MVP has no direct messaging; requests carry only a short bounded note and a predefined opener template.
- [x] reveal contact details only after mutual consent if this feature is added
  - Progress: WebApi returns the other learner email only when a partner request has been accepted.
- [ ] add tests for request-state transitions and rate limits
  - Test backlog: remaining Web coverage is itemized in `71-Web-Test-Backlog.md` and is owned by the separate test-development workflow.

---

### 13. Moderation, Abuse Handling, and Trust

#### Goal

Make social and organizer features safe enough to release publicly.

#### Required Before Public Profiles or Matching

- report user
- block user
- hide profile
- admin report queue
- organizer verification workflow
- listing approval workflow
- rate limits
- audit logs for moderation decisions

#### Backlog

- [x] define `UserReport` model
  - Progress: WebApi now persists user reports with target type/key, optional reported user email, reason, details, status, and decision metadata.
- [x] define `UserBlock` model
  - Progress: WebApi now persists learner blocks and suppresses blocked users from partner search and partner request creation in both directions.
- [x] define `OrganizerVerification` model
  - Progress: domain and EF persistence model exist for organizer verification status tracking.
- [x] define `ListingReview` workflow
  - Progress: domain and EF persistence model exist for listing review status tracking for events and organizer profiles.
- [x] implement report action from profile, event, and organizer pages
  - Progress: Web learners can report learner profiles from partner matching, conversation events from event detail, and organizer profiles from organizer detail.
- [x] implement block action from profile and partner-request flows
  - Progress: Web learners can block learner profiles without exposing email, and blocking from partner-request flows creates a block record.
- [x] implement admin moderation queue
  - Progress: Web Admin includes a moderation queue for report review and status filtering.
- [x] implement moderation decision audit log
  - Progress: WebApi records moderation decision audits whenever an admin decision is saved.
- [ ] add tests for block visibility and request suppression
  - Test backlog: remaining Web coverage is itemized in `71-Web-Test-Backlog.md` and is owned by the separate test-development workflow.
- [x] add operational runbook for moderation
  - Progress: `65-Safety-And-Moderation-Requirements.md` now includes a Web moderation runbook for report triage, blocks, event/organizer reports, decision logging, and escalation.

---

### 14. Entitlements and Monetization

#### Learner Plans

| Plan | Direction |
|---|---|
| Free | Useful core learning, basic directory, limited saves/requests |
| Plus | More saved phrases, more scenario packs, advanced dual-language layout, event preparation packs |
| Pro | Future AI-assisted practice, writing help, advanced roleplay, interview preparation |

#### Organizer Plans

| Plan | Direction |
|---|---|
| Free Organizer | Limited public page and limited active events |
| Organizer Lite | More events, RSVP, basic analytics |
| Organizer Standard | Recurring events, attendee export, featured city placement, verified badge |
| Organizer Pro | Multiple admins, multiple locations, richer analytics, branded profile |

#### Backlog

- [x] define entitlement keys for scenario and conversation features
  - Progress: Identity feature keys now explicitly cover dialogue practice lessons, conversation starters, conversation events, event RSVP, learner profiles, partner matching, advanced scenario packs, and event preparation packs.
- [x] define entitlement keys for organizer features
  - Progress: Identity feature keys now explicitly cover organizer dashboard, profile management, event management, RSVP management, analytics, recurring events, featured placement, multiple admins, and branded profiles.
- [x] keep core catalog browse and basic scenario access outside strict premium enforcement
  - Progress: scenario detail remains accessible while event-preparation pack links are hidden when the learner lacks the paid feature.
- [x] add feature gates for advanced scenario packs if needed
  - Progress: Web entitlement access service now exposes an advanced-scenario-pack gate for future advanced scenario surfaces; no current Web route consumes it yet.
- [x] add feature gates for preparation packs if needed
  - Progress: `event-preparation-packs` is a paid entitlement feature for Trial/Premium users; Web and MAUI hide scenario-linked packs for users without access and block direct preparation-pack detail access.
- [x] add organizer plan flags without payment integration first
  - Progress: Organizer plan keys are resolved into plan feature flags and active-event limits without payment integration; the Web organizer dashboard displays enabled plan features and enforces active-event limits.
- [x] add admin entitlement management for organizer plans
  - Progress: Admin organizer profile management already controls organizer `PlanKey`; the Web organizer dashboard now consumes that plan key through the organizer plan policy.
- [ ] add tests for feature-gate behavior
  - Progress: entitlement seed coverage plus web scenario and direct detail gate coverage exist for preparation packs; keep open until broader WebApi/Web/MAUI feature-gate behavior is covered.
  - Test backlog: remaining Web coverage is itemized in `71-Web-Test-Backlog.md` and is owned by the separate test-development workflow.

---

### 15. Analytics and KPIs

#### MVP Metrics

- scenario views
- scenario completion
- conversation-starter usage
- saved phrases
- event views
- event RSVP rate
- preparation-pack usage
- organizer profile views
- partner request sent/accepted rate
- report/block rate
- premium feature usage

#### Backlog

- [x] define analytics event names and payloads
  - Progress: Web product analytics now records named aggregate counters for scenario views, starter usage, saved favorites, event views/RSVPs, preparation-pack usage, organizer profile views, partner requests, report/block actions, and premium feature denials.
- [x] avoid collecting unnecessary personal data
  - Progress: counters store only event name, sanitized scope key, count, and first/last seen timestamps; no email, IP address, free-text note, contact detail, or user identifier is stored.
- [x] implement privacy-safe analytics boundaries
  - Progress: scope keys are normalized to a short ASCII allow-list and Web instrumentation uses content/feature scopes instead of learner-owned identifiers.
- [x] add admin summary views for organizer/event analytics
  - Progress: Web Admin includes an aggregated analytics summary grouped by event and product area with detailed counters for operator review.
- [x] add export only where operationally necessary
  - Progress: no export was added for the MVP because the operational need is summary review only.

---

### 16. Content Seed Priorities

#### First Scenario Packs

- [x] First language-practice meeting
- [x] Introducing yourself in simple German
- [x] Asking someone to speak slowly
- [x] Asking for correction politely
- [x] Talking to a doctor
- [x] Writing to kindergarten or school
- [x] Calling an office for an appointment
- [x] Talking to a landlord
- [x] Workplace small talk
- [x] Job interview basics
  - Progress: `de-a1-a2-practical-scenarios-001.json` now contains 14 A1/A2 practical scenarios, including all first scenario priorities, with German base text plus English and Persian translations.

#### First Conversation Starter Packs

- [x] Conversation cafe starters
- [x] Workplace lunch starters
- [x] School/kindergarten parent starters
- [x] Neighbor small talk starters
- [x] Online practice meeting starters
  - Progress: `de-a1-a2-practical-scenarios-001.json` now contains 5 conversation starter packs with linked scenarios and linked preparation packs.

#### First Event Directory Seeds

- [x] manually add a small set of public conversation cafes in selected cities
- [x] manually add online German practice groups where allowed
- [x] manually add integration-oriented support resources where appropriate
- [x] define update/verification date per listing
  - Progress: `de-event-directory-seeds-001.json` now provides operational Web/Admin seed data for 7 organizer profiles and 7 reviewed directory listings across Berlin, Hamburg, Munich, online German practice, and official integration-course information. Each event listing includes source URL and `lastVerifiedAtUtc`.

---

### 17. Explicit Non-MVP Items

Do not implement these in the first release unless there is a specific decision to expand scope:

- unrestricted user-to-user chat
- ticket payments inside the app
- live audio or video calls
- dating-style matching
- public comments under profiles or events
- map-heavy real-time location tracking
- AI feedback without cost, safety, and privacy boundaries
- organizer self-service billing
- complex recommendation engine

---

### Recommended Execution Order

1. Product/documentation updates
2. Dialogue content contract
3. Dialogue import and query support
4. Dialogue mobile UI
5. Conversation starter packs
6. Event and club directory
7. Event preparation packs
8. Organizer profiles
9. Basic organizer dashboard
10. RSVP
11. Learner profiles
12. Partner requests
13. Moderation and report/block flows
14. Entitlement flags
15. Analytics and KPI tracking

---

### Acceptance Criteria

The first public version of this expansion should be considered acceptable only when:

- dialogue practice lessons work in at least German + one meaning language
- dual-language display works consistently where content exists
- at least 10 high-value practical scenarios exist
- event directory can be browsed and filtered
- event detail can show a preparation pack
- organizer profiles can be displayed
- no public matching feature is released without report/block/moderation controls
- no unrestricted chat exists in the MVP
- premium features do not block the core learning value
- all user-facing strings are localized through the existing localization system
- tests cover the main query, state-transition, and visibility rules

---

### Maintenance Rule

Before starting a new implementation slice:

- move selected items to `[-]`
- keep unrelated items unchanged
- mark completed work with `[x]`
- add new tasks in the correct section
- update `04-Implementation-Backlog.md` if the work changes the main project phase plan
- update `63-Market-Product-And-Organizer-Strategy.md` only when the strategy changes, not for every implementation detail


---

## Phase 7 Backlog - Complete Learning Portal

`76-Learning-Portal-Roadmap-And-Backlog.md` is the source of truth for Phase 7 planning. This section keeps the project-wide backlog aligned without duplicating the full roadmap.

Content generation for new modules must not start until the corresponding implementation, validation rules, and Web rendering are stable.

### 7.0 Current Web Readiness Blockers

Status as of 2026-06-24: Web feature/content implementation for the current Learning Portal baseline is complete enough for controlled testing, but tester invitation is still blocked by one external Brevo gate and five human evidence gates. Current operator packet: `artifacts/validation/web-external-action-packet/web-external-action-packet-20260624-000221.md`.

- [ ] add the current host IP shown in the latest external action packet to Brevo `Security -> Authorised IPs`
  - Verification: rerun `tools/Web/Invoke-BrevoProductionReadinessCheck.ps1 -VerifyBrevoApi -RequireRealDelivery -SenderVerified -DnsAuthenticated -WebhookConfigured -DpaAccepted` and require `Blockers=0`, `Warnings=0`.
- [ ] close the real mailbox rendering review gate
  - Verification: record safe evidence through `docs/91-Web-Manual-External-Review-Checklist.md` and rerun `tools/Web/Test-WebManualExternalEvidence.ps1 -FailOnIssue`.
- [ ] close the desktop PWA install evidence gate
  - Verification: record the desktop Chrome/Edge install result using `docs/56-Web-Pwa-Install-Validation-Worksheet.md` or the manual external review report.
- [ ] close the Android PWA install evidence gate or explicitly mark it out of scope for this controlled tester pass
  - Verification: record Android Chrome install evidence or a deliberate `not-in-scope-for-this-pass` decision; do not treat that as broad production sign-off.
- [ ] close the tester-pass start-status gate
  - Verification: confirm the current tester bundle, tester-facing quick start, operator-only manual checklist, intended tester count, helper-language coverage, and Premium grant approach before invitations.
- [ ] run the controlled tester pass and triage feedback
  - Verification: collect `WebTesterFeedback.csv`, run `tools/Web/Convert-WebTesterFeedbackToReport.ps1`, fix blocker/major issues, and rerun `tools/Web/New-WebControlledTesterReadinessAudit.ps1 -FailOnAutomatedFailure -FailOnOpenHumanGates`.
- [ ] create the next restore-ready external backup after the controlled tester readiness gates are closed
  - Verification: backup under `X:\Projects\DarwinLingua.Backup` includes PostgreSQL dump, globals, restore list, dry-run restore evidence, repo overlay, separate secret bundle, manifest, checksums, and current readiness evidence.

### 7.1 Learning Portal Foundation

- [x] define shared learning portal navigation
  - Progress: Web learner navigation groups existing routes under Learn, Practice, Speak, Prepare, and Resources without exposing future dead routes.
- [ ] define cross-content linking model
- [ ] define reusable linked-word reference model
- [x] define unified CEFR/category/topic filter conventions
  - Progress: CEFR values use a shared Web convention helper; category/topic conventions stay module-specific until implementation starts.
- [x] define Web-first/mobile-later parity rule
- [ ] add admin/system report placeholders
  - Note: keep placeholders documentation-only until real Phase 7 module content exists.
- [x] add test backlog entries

### 7.2 Grammar Guide

- [x] implement GrammarTopic model
- [x] implement import/validation
- [x] implement Web API list/detail
- [x] implement Web pages
- [x] add admin visibility
- [x] add tests
- [x] add content contract when implementation starts
- [x] add rich localized grammar blocks, localized title/description, content revision, image-slot references, and pilot package import support
- [x] import and render the first reviewed pilot grammar package
- [x] archive temporary pilot/validation Grammar packages outside the official package path
- [x] start official Grammar content generation with `grammar-a1-core-v1.json`
  - Progress: the official A1 core package now includes personal pronouns, sein, haben, regular verbs, verb position, yes/no questions, W-questions, definite articles, indefinite articles, noun gender basics, plural basics, nominative case, simple accusative introduction, kein vs nicht basics, possessive pronouns, basic adjective position, basic prepositions, numbers/grammar use, time expressions, word order with time/place, simple modal verbs, polite requests with möchte, imperative basics, separable verbs introduction, simple conjunctions und/aber, pronoun and verb agreement, formal Sie, du versus Sie grammar basics, basic sentence negation, question-answer sentence patterns, articles with food, drinks, and shopping nouns, basic location phrases, basic appointment phrases, common A1 grammar mistakes, and the final A1 grammar review map.
- [x] start official A2 Grammar content generation with `grammar-a2-core-v1.json`
  - Progress: the official A2 core package now starts with Perfekt with haben, Perfekt with sein, common irregular participles, Präteritum of sein/haben, modal verbs in more detail, dative case basics, accusative vs dative basics, dative pronouns, accusative pronouns, possessive pronouns in cases, Wechselpräpositionen introduction, prepositions with dative, prepositions with accusative, separable verbs in Perfekt, reflexive verbs introduction, dass clauses, weil clauses, wenn for conditions, denn vs weil, sentence order in subordinate clauses, comparative forms, superlative basics, adjective endings introduction, indirect questions introduction, imperative formal/informal, time clauses with bevor/nachdem, zu + infinitive introduction, man as general subject, es gibt, polite forms with würde, simple email grammar, grammar for phone calls, grammar for appointments, grammar for doctor visits, grammar for school/kindergarten communication, common A2 mistakes, the A2 connectors overview, the A2 case review, the A2 verb review, and the final A2 grammar review map, including 10 learner languages, rich sections, examples, rules, common mistakes, and linked word references.
- [x] start official B1 Grammar content generation with `grammar-b1-core-v1.json`
  - Progress: the official B1 core package now starts with relative clauses basics, relative pronouns in nominative and accusative, relative pronouns in dative, Konjunktiv II for polite requests, Konjunktiv II with wäre/hätte/würde, passive voice introduction, werden as auxiliary, infinitive with zu, um ... zu, damit vs um ... zu, weil/obwohl/trotzdem, als vs wenn, nachdem/bevor/während, indirect questions, reported requests/polite questions, adjective declension after definite article, adjective declension after indefinite article, adjective declension without article, genitive introduction, prepositional verbs introduction, verb + preposition combinations, noun-verb phrases, connectors for opinion, connectors for contrast, connectors for cause/effect, sentence order with multiple clauses, formal email sentence structure, complaint sentence patterns, giving reasons clearly, agreeing/disagreeing grammatically, describing experiences in the past, and talking about plans and conditions, including 10 learner languages, rich sections, examples, rules, common mistakes, and linked word references.
- [ ] add broader validation coverage for every rich grammar block type after more reviewed pilot packages are available
- [ ] add mobile package export TODO after Web sign-off

### 7.3 Everyday Expressions

- [x] implement ExpressionEntry model
- [x] implement idiom/proverb/colloquial metadata
- [x] implement import/validation
- [x] implement Web API list/detail
- [x] implement Web pages
- [x] add safety/tone warnings
- [x] add tests
- [x] add content contract when implementation starts
- [ ] add broader query/WebApi rendering coverage after first real expression content package is available
- [ ] add mobile package export TODO after Web sign-off

### 7.4 Exercise Engine

- [x] implement reusable Exercise and ExerciseSet models
- [x] support initial exercise types
- [x] support deterministic feedback
- [x] support links to grammar, words, expressions, dialogues, Talk Topics, and lessons
- [x] add Web exercise runner
- [x] add initial tests
- [x] add content contract when implementation starts
- [x] separate stateless public exercise evaluation from authenticated persisted attempts
- [x] reject missing authenticated user ids for persisted attempts instead of storing anonymous attempts
- [x] bound and validate submitted-answer JSON before evaluation or persistence
- [x] rate-limit exercise evaluation and attempt endpoints
- [x] broaden runner UI beyond generic JSON submission for initial choice, single-answer, error-correction, sentence-ordering, and matching inputs
- [ ] add tests for every initial exercise type after first real exercise package is available

### 7.5 Course Lessons / Learning Paths

- [x] implement CoursePath, CourseModule, CourseLesson
- [x] link lessons to grammar, words, expressions, dialogues, Talk Topics, and exercises
- [x] add Web course/lesson pages
- [ ] add lesson progress tracking
- [x] add initial parser/Web/navigation tests
- [x] add content contract in `80-Course-Content-Package-Contract.md`
- [ ] add broader linked-content projection and WebApi rendering coverage after the first real course package is available

### 7.6 Exam Preparation

- [x] implement exam profile taxonomy
- [x] implement exam prep unit model
- [x] link to dialogues, exercises, grammar, expressions, writing templates, Talk Topics, and course lessons
- [x] add Web exam prep pages
- [x] add initial parser/import/navigation/localization tests
- [x] add content contract in `83-Exam-Prep-Content-Package-Contract.md`
- [ ] add broader list/detail query, Web API, Web rendering, and linked-content coverage after first real exam-prep package is available

### 7.7 Writing Templates

- [x] implement WritingTemplate model
- [x] support variables and filled examples
- [x] link to grammar, words, expressions, and exercises
- [x] add Web pages
- [x] add initial parser/navigation/localization tests
- [x] add content contract in `81-Writing-Template-Content-Package-Contract.md`
- [ ] add broader validation, query, Web API, and Web rendering coverage after first real writing-template package is available

### 7.8 Country Guidance

- [x] implement CountryGuidanceNote model
- [x] link to dialogues, expressions, writing templates, Talk Topics, and course lessons
- [x] add Web pages
- [x] add initial parser/navigation/localization tests
- [x] add content contract in `82-Country-Guidance-Content-Package-Contract.md`
- [ ] add broader validation, query, Web API, and Web rendering coverage after first real country-guidance package is available

### 7.9 Unified Learning Search

- [x] define unified search result model
- [x] implement cross-content search endpoint
- [x] extend Web search experience without replacing existing word search
- [x] support CEFR/content type/category/topic filters at the API level
- [x] add application query validation for empty, short, long, and unsupported result-type queries
- [x] add PostgreSQL trigram/filter index migration for the bulk-content search path
- [x] apply PostgreSQL trigram/filter indexes during shared database startup for existing search tables
- [x] rate-limit the public unified search endpoint
- [x] add initial application and structural tests for query validation, result projection, route hardening, and search indexes
- [ ] add broader repository/WebApi/Web rendering coverage with seeded learning content
- [ ] add seeded performance coverage before bulk Phase 7 content generation

### 7.10 Progress And Personalization

- [x] implement `UserContentProgress` as user state separated from content entities
- [x] track viewed/completed/needs-review/skipped states for controlled Learning Portal owner types
- [x] add WebApi progress summary/update endpoints
- [x] extend Web recent activity with a basic Learning Portal progress summary
- [x] show course-lesson viewed state where lessons exist
- [x] add deterministic recommendation service for next course lessons and incomplete grammar topics
- [x] add domain/application tests for state transitions, owner validation, summaries, and deterministic recommendations
- [ ] add WebApi endpoint tests for authenticated progress workflows
- [ ] add Web rendering tests for course lesson progress and dashboard summary
- [ ] add broader personalization signals from weak exercises and difficult words

### 7.11 Admin And Operations

- [x] add admin content overview
- [x] add content quality reports
- [x] add unresolved-link reports
- [x] add missing-translation reports
- [x] add CEFR/module seed coverage reports
- [ ] add import validation summaries per Phase 7 package type
- [ ] add filtered drill-down/export for Learning Portal quality issues

### 7.12 Mobile Parity

- [x] update full/all mobile package export after Web sign-off for grammar, expressions, exercises, courses, exam prep, writing templates, country guidance notes, linked references, and unified search inputs
  - Progress: full database and catalog-full packages now include Phase 7 arrays; CEFR slice packages remain word/current-conversation scoped.
- [x] publish module-scoped mobile packages for selective first-run downloads
  - Progress: WebApi now exposes `catalog-module` manifests/downloads for `words`, `dialogues`, `talk-topics`, `grammar`, `expressions`, `exercises`, `courses`, `exam-prep`, `writing-templates`, `country-guidance`, `events`, `organizers`, and `conversation-starters`.
- [x] remove Web runtime dependency on local SQLite learning/content databases
  - Progress: DarwinLingua.Web requires PostgreSQL/Npgsql for `WebIdentityDbContext` and no longer registers the shared SQLite infrastructure initializer.
- [x] make packaged seed optional legacy fallback instead of mandatory MAUI first-start initialization
  - Progress: first startup initializes the local schema and then offers module selection from WebApi packages; skipping content leaves an empty but usable local database.
- [x] refactor MAUI navigation after Web model is stable
  - Progress: shell navigation now exposes Learn, Practice, Speak, Prepare, and Resources surfaces with a Learning Portal hub.
- [x] add MAUI dynamic list/detail/search surfaces backed by local Catalog query services
  - Progress: generic list/detail pages cover Grammar, Expressions, Exercise Sets, Courses, Exam Prep, Writing Templates, Country Guidance, and Talk Topics; unified learning search has a mobile route.
- [x] update full-replacement local package apply path for Phase 7 content tables
- [x] add module-replacement local package apply path that preserves unrelated modules and user state
- [ ] implement mobile exercise runner for individual exercise attempts
- [ ] integrate cross-content progress/sync on mobile when account sync is ready
- [ ] add full manual validation worksheet bundle support for Phase 7 mobile parity

### 7.13 Web Release Hardening

- [x] run WebApi and Web builds for Phase 7 Web release hardening
- [x] run current WebApi, ContentOps parser, Catalog application, Learning application/domain, and Localization test suites
- [x] add structural route checks for Phase 7 Web learner routes and WebApi route registrations
- [x] add structural localization checks for Phase 7 English/German release-surface keys
- [x] update Web release checklist with Phase 7 module readiness gates
- [ ] complete manual browser smoke against a running Web/WebApi environment with validated sample content
- [ ] complete seeded WebApi filter/detail tests for every Phase 7 module
- [ ] keep MAUI parity deferred until Web sign-off is recorded

---

## Phase 8 Backlog - Multi-Target Learning Language Platform

Phase 8 converts Darwin Lingua from a German-first learning portal with multilingual helper text into a platform that can teach multiple target languages such as German, English, Spanish, French, and later additional languages. This phase is not a content-generation phase first; it is a product, schema, routing, import, search, progress, admin, and content-contract refactor.

Status as of 2026-06-24: German baseline architecture gate complete; first non-German pilot planning ready. Architecture/reference-data/user-profile foundation, target-language schema/index scope for Web learning roots including Conversation Starter Packs, Event Preparation Packs, Conversation Events, Event RSVPs, and organizer supported-level metadata, package-level target-language import metadata, German package JSON backfill, canonical `/learn/{targetLearningLanguageCode}/...` routing, Country Guidance, German baseline regression, admin smoke, and restore-ready backup gates are documented. English is the recommended first non-German pilot, but it remains inactive until a separate reviewed content-generation goal starts.

### 8.0 Locked Product Decisions

- [x] treat `targetLearningLanguage` as a first-class concept separate from UI language and helper/meaning languages
  - Decision: `targetLearningLanguage` is the language the learner wants to learn, for example `de`, `en`, `es`, or `fr`.
  - Decision: `uiLanguage` remains the language of site chrome and account/admin UI.
  - Decision: `primaryMeaningLanguage` and `secondaryMeaningLanguage` remain helper explanation languages and can expand independently from the currently active set of ten languages.
- [x] keep German as the default active target language until another target language has real reviewed content
  - Decision: current users and testers should still see German content by default.
  - Decision: all existing German content must be migrated/backfilled as `targetLearningLanguageCode = de`.
- [x] allow clean route, schema, and contract changes now instead of preserving legacy behavior that would create technical debt
  - Decision: the product is still pre-customer and under active development, so a correct clean migration is preferred over compatibility wrappers.
  - Decision: old public route compatibility is not required if the new route model is clearer and easier to test.
  - Decision: temporary redirects may be used only as short-lived development aids; they must not become permanent hidden compatibility behavior.
- [x] model country/culture/civic guidance as language-aware country-context content
  - Decision: the current Life in Germany feature should belong to German learning by default because Germany is a German-speaking country.
  - Decision: future German target-language content can include Germany, Austria, Switzerland, and other relevant German-speaking regions.
  - Decision: future English target-language content can include United States, United Kingdom, Australia, and other English-speaking regions.
  - Decision: the same country may appear under multiple target languages when appropriate, such as Switzerland under German, French, and Italian, with separate original content per target language rather than a single reused translation.
  - Decision: helper translations still work as learner support for each target-language/country-context note.
  - Decision: the stable model name is `Country Guidance`; `Life in Germany` is a German/Germany-facing product label, not the permanent global module name.
- [x] add a level-system abstraction instead of hard-coding CEFR as the only possible scale
  - Decision: CEFR remains the active level system for German and for European languages where it fits.
  - Decision: each level must have a code, localized display title, learner-friendly description, sort order, and optional mapped standard level.
  - Decision: A1-C2 cards should show both the compact code and a human-readable label so learners who do not know CEFR can still choose a level.
- [x] require a target-language capability profile before activating any new learning language
  - Decision: each target language must define script/direction, level system, search normalization, morphology/grammar assumptions, punctuation/capitalization behavior, input/keyboard guidance, TTS expectations, country contexts, exam ecosystem, writing conventions, and initial module coverage.
  - Decision: English, Spanish, French, and later languages must be planned as native learning products, not as translated copies of German.

### 8.0A No-Debt Execution Plan

- [x] record the latest product decisions for Phase 8 before continuing implementation
  - Decision: do not keep legacy public routes, model names, or package roots when they would hide missing target-language or country-context scope.
  - Decision: because the system is still pre-customer, broken old URLs during the migration are acceptable if the canonical routes, tests, and admin diagnostics catch missing references.
  - Decision: `Life in Germany` remains visible inside the German target-language experience because Germany is a German-speaking country, but the stable platform model is `Country Guidance`.
  - Decision: Country Guidance must support multiple countries for one target language and the same country under multiple target languages. Each target-language/country-context pair is a separate source-content stream with its own helper translations.
  - Decision: helper-language expansion is independent from target-language activation. German may later support more helper languages without changing German source content identity.
  - Decision: every learner-facing level selector must show a compact code plus learner-friendly label and explanation from level-system reference data.
- [x] close the remaining target-language architecture gaps before any non-German content generation
  - Scope: remaining admin diagnostics, route helper consolidation, settings/onboarding selector, helper-language expansion readiness, smoke tests, and backup.
- [x] implement the remaining work in ordered implementation slices
  - Slice 1: finish admin reporting and diagnostics target-language/country-context scope.
    - Progress: Learning Portal admin reports and issue drilldown now accept explicit `targetLearningLanguageCode`, expose report scope metadata, and filter Learning Portal counts and quality diagnostics inside the selected target language. Global social, moderation, identity, and email sections remain explicitly global.
  - Slice 2: refactor `Life in Germany` / `CountryGuidanceNote` into clean `Country Guidance` model, routes, package roots, admin labels, import metadata, and tests.
    - Progress: the first Country Guidance foundation adds real `countryContextCode` persistence, default Germany context, import validation, target/country-scoped replacement, package JSON backfill, repository projection, and contract wording. Public Web/API routes, Web shell links, learner content link resolution, Unified Search result type, admin dashboard link, admin route, import summary grouping, source package root, source package IDs, and JSON root array now use canonical `country-guidance` wording under `/learn/{targetLearningLanguageCode}/country-guidance/{countryContextCode}`, `/api/catalog/country-guidance/{countryContextCode}`, `/admin/country-guidance`, `content/learning-portal/country-guidance/packages`, and `countryGuidanceNotes`. Internal entity/table naming is now clean: `CountryGuidanceNote` maps to `CountryGuidanceNotes`, with a migration from the old table name.
  - Slice 3: finish canonical `/learn/{targetLearningLanguageCode}/...` routes and remove permanent old public route compatibility for learner content.
    - Progress: Conversation Events and Organizer Profiles moved from legacy top-level learner routes to canonical `/learn/{targetLearningLanguageCode}/conversation-events` and `/learn/{targetLearningLanguageCode}/organizers` routes. Web shell, mobile nav, learner views, admin preview links, organizer dashboard links, and redirect paths now pass explicit target-learning-language route values.
  - Slice 4: finish target-aware cross-content link resolution, breadcrumbs, search result links, activity targets, and admin preview links.
    - Progress: Conversation Starter Packs and Event Preparation Packs now persist `TargetLearningLanguageCode`, use target-scoped slug/filter indexes, import/replace by package target language, and Web/API query paths pass `targetLearningLanguageCode`. Dialogue-related starter/preparation lookups are target-scoped.
    - Progress: Conversation Events now persist `TargetLearningLanguageCode`, Event RSVPs are keyed by target language plus event slug plus participant email, and public/admin Web/API event and RSVP queries pass `targetLearningLanguageCode`. Organizer profiles remain global identity/directory records, while `OrganizerProfileSupportedLevel` and active-event views are target-scoped so the same organizer can later support different learning languages without profile duplication.
    - Evidence: `dotnet build src\Apps\DarwinLingua.WebApi\DarwinLingua.WebApi.csproj --no-restore /p:UseSharedCompilation=false`, `dotnet build src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj --no-restore /p:UseSharedCompilation=false`, `dotnet test tests\Apps\DarwinLingua.WebApi.Tests\DarwinLingua.WebApi.Tests.csproj --no-restore --filter "ConversationEvent|OrganizerProfile|EventRsvp|OrganizerDashboard|TargetLearningLanguage" /p:UseSharedCompilation=false` (28 tests), `dotnet test tests\Modules\Catalog\DarwinLingua.Catalog.Infrastructure.Tests\DarwinLingua.Catalog.Infrastructure.Tests.csproj --no-restore --filter "ConversationEvent|OrganizerProfile|DatabaseInitializationUseCaseTests" /p:UseSharedCompilation=false` (8 tests), and `.\tools\Web\Start-WebPublicDevStack.ps1 -StopExisting` passed on 2026-06-24.
    - Evidence: PostgreSQL `darwinlingua_shared` now has target-language columns and short target-aware indexes for `ConversationEvents`, `EventRsvps`, and `OrganizerProfileSupportedLevels`; legacy slug/email/level unique indexes were removed. Current German counts are `ConversationEvents(de)=9`, `EventRsvps(de)=10`, and `OrganizerProfileSupportedLevels(de)=39`.
    - Progress: Admin Conversation Events and Organizer Profiles pages now expose an explicit active target-language scope selector, reject inactive target languages on save, preserve target scope through save/owner/claim actions, and generate target-aware learner preview links. Organizer identity remains global; organizer supported levels, active-event views, and event management use the selected target scope.
    - Evidence: `dotnet build src\Apps\DarwinLingua.WebApi\DarwinLingua.WebApi.csproj --no-restore /p:UseSharedCompilation=false`, `dotnet build src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj --no-restore /p:UseSharedCompilation=false`, `dotnet test tests\Apps\DarwinLingua.WebApi.Tests\DarwinLingua.WebApi.Tests.csproj --no-restore --filter "AdminOrganizerEventManagementStructuralTests|ConversationEventDateFilterStructuralTests|OrganizerProfileAdminServiceTests|OrganizerDashboardControllerTests|TargetLearningLanguageApiStructureTests" /p:UseSharedCompilation=false` (21 tests), `dotnet test tests\Modules\Localization\DarwinLingua.Localization.Application.Tests\DarwinLingua.Localization.Application.Tests.csproj --no-restore --filter WebLearnerShellStructureTests /p:UseSharedCompilation=false` (8 tests), and `dotnet test tests\Modules\Catalog\DarwinLingua.Catalog.Infrastructure.Tests\DarwinLingua.Catalog.Infrastructure.Tests.csproj --no-restore --filter "ConversationEventRepositoryTests|OrganizerProfileRepositoryTests" /p:UseSharedCompilation=false` (4 tests) passed on 2026-06-24.
    - Evidence: `.\tools\Web\Start-WebPublicDevStack.ps1 -StopExisting` produced `artifacts\validation\web-public-stack\web-public-stack-20260624-150054.md` with smoke passed. Runtime checks returned 200 for `https://darwinlingua.com/learn/de/conversation-events` and `https://darwinlingua.com/learn/de/organizers`, and 404 for the removed legacy `https://darwinlingua.com/conversation-events` and `https://darwinlingua.com/organizers` routes.
  - Slice 5: finish Web target-language selection UX and level-card labels/descriptions.
    - Scope: Course level cards, onboarding/settings target-language selector, helper-language selector separation, country-context selector where relevant, and empty states for planned inactive target languages.
    - Requirement: level surfaces must use level-system reference data and show a prominent compact code plus learner-friendly label/description. For German CEFR this means `A1 Einstieg`, `A2 Grundlagen`, `B1 Selbststaendig`, `B2 Kompetent`, `C1 Souveraen`, and `C2 Meisterschaft` or their localized equivalents.
    - Requirement: the learner must be able to understand the difference between "I want to learn German", "show the UI in English/German", and "explain German in Persian/English/etc.".
    - Progress: Course index level cards now use `LearningLevelSystemCatalog` definitions, show the compact level code prominently, and render learner-friendly level label/description before the course title and description. Settings now exposes a separate target-learning-language selector with inactive planned languages disabled, while UI language and helper/meaning languages stay in a separate section.
    - Evidence: `npm run build:css`, `dotnet build src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj --no-restore /p:UseSharedCompilation=false`, `dotnet test tests\Apps\DarwinLingua.WebApi.Tests\DarwinLingua.WebApi.Tests.csproj --no-restore --filter "SettingsControllerLanguagePreferenceTests|CourseRouteStructuralTests|TargetLearningLanguageApiStructureTests" /p:UseSharedCompilation=false` (42 tests), and `dotnet test tests\Modules\Localization\DarwinLingua.Localization.Application.Tests\DarwinLingua.Localization.Application.Tests.csproj --no-restore --filter WebLearnerShellStructureTests /p:UseSharedCompilation=false` (8 tests) passed on 2026-06-24.
    - Evidence: `.\tools\Web\Start-WebPublicDevStack.ps1 -StopExisting` produced `artifacts\validation\web-public-stack\web-public-stack-20260624-151540.md` with smoke passed. Runtime checks returned 200 for `https://darwinlingua.com/learn/de/courses` with `Einstieg` and `course-level-card__label`, 200 for `https://darwinlingua.com/settings` with `Target learning language` and `I want to learn`, and 404 for inactive `https://darwinlingua.com/learn/en/courses`.
  - Slice 6: finish German-specific wording cleanup and level-reference reuse outside Course index.
    - Scope: conversation profile forms, partner matching, account/profile summaries, filters, onboarding/settings, and any learner-facing view that still says `German level` when the field represents the selected target-language level.
    - Requirement: keep German-specific wording only when the content itself is German-specific, such as German source lesson text or German/Germany Country Guidance display copy.
    - Requirement: level options should reuse target-language/level-system reference data where practical rather than duplicating A1-C2 labels in each view.
    - Progress: Learner conversation profile and partner matching models, APIs, Web forms, filters, seed fixtures, EF configuration, database initializer, and model snapshot now use `LearningLevel`. The Web level pickers reuse `LearningLevelSystemCatalog` labels instead of duplicating raw A1-C2 options.
    - Evidence: `dotnet build src\Apps\DarwinLingua.WebApi\DarwinLingua.WebApi.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 warnings/errors on 2026-06-24 after stopping the running WebApi/Web processes that held DLL locks.
    - Evidence: `dotnet test tests\Apps\DarwinLingua.WebApi.Tests\DarwinLingua.WebApi.Tests.csproj --no-restore --filter "LearnerPartnerMatchingServiceTests|WebReadinessSeedFixtureManifestTests|ModerationServiceTests|SettingsControllerLanguagePreferenceTests|TargetLearningLanguageApiStructureTests" /p:UseSharedCompilation=false` passed 26/26, `dotnet test tests\Modules\Localization\DarwinLingua.Localization.Application.Tests\DarwinLingua.Localization.Application.Tests.csproj --no-restore --filter WebLearnerShellStructureTests /p:UseSharedCompilation=false` passed 8/8, and `dotnet test tests\Modules\Catalog\DarwinLingua.Catalog.Infrastructure.Tests\DarwinLingua.Catalog.Infrastructure.Tests.csproj --no-restore --filter "ConversationEventRepositoryTests|OrganizerProfileRepositoryTests|DatabaseInitializationUseCaseTests" /p:UseSharedCompilation=false` passed 8/8 on 2026-06-24.
    - Evidence: `.\tools\Web\Start-WebPublicDevStack.ps1 -StopExisting` produced `artifacts\validation\web-public-stack\web-public-stack-20260624-153544.md` with smoke passed. `artifacts\validation\phase8-learning-level\runtime-smoke.json` verified PostgreSQL has `LearningLevel` and no `GermanLevel` column on `LearnerConversationProfiles`, `https://darwinlingua.com/learn/de/courses` returned 200 with `Einstieg`, `https://darwinlingua.com/settings` returned 200 with `Target learning language`, and inactive `https://darwinlingua.com/learn/en/courses` returned 404.
  - Slice 7: run full German baseline regression, wrong-language isolation checks, admin diagnostics checks, and a restore-ready backup.
    - Scope: German `/learn/de/...` Web browsing, API details, helper-language projections, Unified Search, progress/recommendations, admin report drilldowns, content imports, and Country Guidance `de|DE`.
    - Requirement: unsupported or inactive target-language routes/API calls must not fall back to German content invisibly.
    - Requirement: backup manifest must record counts by target language, country context, module, and helper-language coverage.
    - Progress: German baseline regression, inactive target-language isolation, admin authenticated smoke, PostgreSQL schema verification, and restore-ready phase backup are complete.
    - Evidence: `dotnet test tests\Apps\DarwinLingua.WebApi.Tests\DarwinLingua.WebApi.Tests.csproj --no-restore --filter "TargetLearningLanguageApiStructureTests|CourseRouteStructuralTests|ExerciseAttemptAndSearchHardeningStructuralTests|LearningProgressRouteStructuralTests|WebsiteAdminQueryServiceLearningPortalReportTests|AdminReportsSummaryStructuralTests|AdminReportsLearningPortalIssueDrilldownStructuralTests|CountryGuidanceNoteRouteStructuralTests|ConversationStarterRouteStructuralTests|EventPreparationRouteStructuralTests|EventRsvpServiceTests|OrganizerDashboardControllerTests|OrganizerProfileAdminServiceTests|SettingsControllerLanguagePreferenceTests" /p:UseSharedCompilation=false` passed 75/75, `dotnet test tests\Modules\Catalog\DarwinLingua.Catalog.Application.Tests\DarwinLingua.Catalog.Application.Tests.csproj --no-restore --filter "UnifiedLearningSearchServiceTests|ExerciseAttemptServiceTests" /p:UseSharedCompilation=false` passed 17/17, `dotnet test tests\Modules\Catalog\DarwinLingua.Catalog.Infrastructure.Tests\DarwinLingua.Catalog.Infrastructure.Tests.csproj --no-restore --filter "UnifiedLearningSearchPostgresRepositoryTests|CountryGuidanceNotePostgresRepositoryTests|CourseRepositoryTests|ExercisePostgresRepositoryTests|WritingTemplatePostgresRepositoryTests|ExamPrepPostgresRepositoryTests" /p:UseSharedCompilation=false` passed 6/6, and `dotnet test tests\Modules\ContentOps\DarwinLingua.ContentOps.Infrastructure.Tests\DarwinLingua.ContentOps.Infrastructure.Tests.csproj --no-restore --filter "ContentImportParserCourseTests|ContentImportParserCountryGuidanceNoteTests|ContentImportParserExerciseTests|ContentImportParserWritingTemplateTests|ContentImportParserExamPrepTests|ContentImportServiceTests" /p:UseSharedCompilation=false` passed 21/21 on 2026-06-24.
    - Evidence: `.\tools\Web\Invoke-WebAdminAuthenticatedSmoke.ps1 -UseLocalDevelopmentSeed` passed admin dashboard, reports, diagnostics, publishing, imports, words, topics, users, moderation, billing diagnostics, and email diagnostics on 2026-06-24; report: `artifacts\validation\web-admin-smoke\web-admin-authenticated-smoke-20260624-154122.json`.
    - Evidence: restore-ready backup created at `X:\Projects\DarwinLingua.Backup\20260624-160737-phase8-complete-pre-english-pilot`. It includes `darwinlingua_shared_20260624-160737.dump` (182.33 MB), globals, restore list, repo overlay, secret bundle, docker inspect metadata, checksums (2009 entries), source inventory, and restore dry-run inventory. Source/restored inventories matched exactly and record `CourseLessons=560`, `WritingTemplates=120`, `ExamPrepUnits=246`, `CountryGuidanceNotes=30`, all under target language `de`, with Country Guidance `de|DE=30`.
  - Slice 8: only after Slice 7, create the first non-German target-language capability profile and pilot plan.
    - Recommendation: English first, then Spanish/French later unless tester/business priority changes.
    - Requirement: no new target-language content generation starts without a reviewed capability profile that covers script, levels, search, grammar, pronunciation, writing conventions, country contexts, exams, pilot module coverage, and safety/legal refresh needs.
    - Requirement: the pilot plan must include the complete content list for the first small native pilot before package generation starts.
    - Progress: English is documented as the first recommended non-German target-language pilot, but remains inactive. The profile defines source-content rules, country-context scope (`US`, `GB`, `AU`), CEFR-friendly labels, English-specific learning requirements, first A1 course candidates, grammar/expression/exercise/writing-template candidates, validation gates, and explicit non-goals.
    - Evidence: `artifacts\planning\target-language-en-capability-profile-and-pilot.md`.
- [x] keep this phase non-content-generation focused
  - Requirement: no English, Spanish, French, or other non-German learning content is generated until the architecture is green.
  - Requirement: any content touched during migration must be repair/backfill only, not bulk generation.
  - Progress: Phase 8 produced architecture/backlog/planning, cleanup, tests, and backup only. No non-German content packages were generated, and English remains inactive.

### 8.1 Architecture And Terminology Contract

- [x] create or update the architecture contract that defines `targetLearningLanguage`, `uiLanguage`, `meaningLanguage`, `countryContext`, and `levelSystem`
  - Progress: `94-Multi-Target-Language-Architecture.md` defines stable terminology, product rules, route direction, content partitioning, package-contract rules, module-specific rules, testing gates, and the first non-German pilot rule.
- [x] update product-scope and vision docs so Darwin Lingua is described as a multilingual language-learning platform, not only a German-first portal
  - Progress: `01-Product-Vision.md`, `02-Product-Scope.md`, `03-Product-Phases.md`, and `76-Learning-Portal-Roadmap-And-Backlog.md` now describe German as the first target-language baseline inside a multilingual platform direction.
- [x] audit and replace ambiguous wording where older docs meant helper/meaning languages but sounded like target-learning languages
  - Progress: learner-facing Search copy and current roadmap wording now distinguish selected learning language from helper/meaning languages. Historical package notes keep content-package evidence only where they describe already imported German packages.
- [x] document the rule that target-language source content is canonical and helper translations are explanatory support, not replacements
  - Progress: `94-Multi-Target-Language-Architecture.md` records this as a product rule and package-contract rule.
- [x] document that new target-language content must be authored natively for that target language and culture; it must not be bulk-translated from German packages
  - Progress: `94-Multi-Target-Language-Architecture.md` defines native target-language authoring as a hard product rule and calls out idioms, country guidance, exams, writing templates, and courses.
- [x] define a support matrix for active target languages, planned target languages, active helper languages, planned helper languages, and country contexts
  - Progress: `94-Multi-Target-Language-Architecture.md` now defines the first support matrix: German active, English recommended as the first non-German pilot, Spanish/French planned later, Italian as a future candidate, and helper languages explicitly independent from target languages.
- [x] define naming conventions for code, docs, JSON packages, routes, feature labels, and admin labels
  - Progress: `Country Guidance` is the stable platform/module concept; `Life in Germany` remains a country-specific German/Germany-facing label. The Country Guidance package contract now requires target-language and country-context metadata. The internal naming cleanup is complete: the code model is `CountryGuidanceNote` and the table is `CountryGuidanceNotes`.
  - Requirement: final learner-facing routes and package roots should use `country-guidance`; `Life in Germany` should be display text for `targetLearningLanguageCode = de` and `countryContextCode = DE`.
  - Requirement: admin labels should expose both target language and country context rather than using a globally German-specific module name.
  - Progress: learner-facing routes, search result links, admin navigation, import grouping, package root, package IDs, and package root array now use `country-guidance`; `Life in Germany` remains display copy in the German/Germany learner navigation. The internal naming debt is closed: `CountryGuidanceNote` maps to `CountryGuidanceNotes`, with old names retained only in historical migrations and explicit rename SQL.

### 8.2 Reference Data And Level System

- [x] introduce target-learning-language reference data with activation status, display order, native name, localized names, text direction, and optional default country contexts
  - Progress: `TargetLearningLanguageCatalog` introduces the active German target-language definition with activation status, display order, native name, localized names, text direction, default CEFR level system, and default Germany country context.
- [x] introduce a level-system model that supports CEFR and future non-CEFR scales
  - Progress: `LearningLevelSystemCatalog` introduces a level-system abstraction with the active `cefr` system, leaving room for future non-CEFR systems.
- [x] introduce target-language level definitions for current German levels
  - Proposed German baseline labels: `A1 - Einstieg`, `A2 - Grundlagen`, `B1 - Selbststaendig`, `B2 - Kompetent`, `C1 - Souveraen`, `C2 - Meisterschaft`.
  - Requirement: labels must be localized for UI languages and written plainly enough for learners who do not know CEFR.
  - Progress: German CEFR definitions now include learner-friendly labels and descriptions for A1-C2 in `LearningLevelSystemCatalog`.
- [x] define the first future target-language pilot order
  - Recommendation: start with English after German because demand and tester availability are likely strongest.
  - Alternatives: Spanish or French can be chosen first if business/testing priorities change.
  - Decision: English is the first recommended non-German pilot after the German baseline gate.
  - Evidence: `artifacts\planning\target-language-en-capability-profile-and-pilot.md`.
- [x] add level labels/descriptions to all learner-facing level selection surfaces
  - Requirement: German target-language level cards must show a large compact code such as `A1`, then a friendly label such as `Einstieg`, then the title and description.
  - Requirement: future target languages may use CEFR, a mapped CEFR variant, or a different level system, but must still expose friendly labels.
  - Requirement: level labels should be stored/read from level-system reference data, not hardcoded per Course page.
  - Progress: Course index level cards, course CEFR filter options, learner conversation profile forms, and partner matching level filters now read learner-friendly labels/descriptions from `LearningLevelSystemCatalog` instead of duplicating raw A1-C2 metadata in Razor.
- [x] define country-context reference data with country code, region grouping, target-language availability, localized labels, and display rules
  - Progress: `CountryContextCatalog` introduces the active Germany country context for German with localized labels and display order. Future country contexts stay blocked until target-language content scoping exists.

### 8.3 User Preferences And Onboarding

- [x] add `TargetLearningLanguageCode` to Web user preferences and learning profile state with default `de`
  - Progress: Web preferences, shared learning profile state, EF configuration, PostgreSQL idempotent schema update, and focused tests now carry the default target language `de`.
- [x] update onboarding/settings so the learner can choose the language they want to learn
  - Progress: Settings now has a separate `I want to learn` target-language selector. German is active/selectable; planned languages are visible but disabled until native reviewed content exists.
- [x] keep UI language and helper languages visually and conceptually separate from target-learning-language selection
  - Progress: Settings separates the target-learning-language selector from the UI/helper-language controls, and `WebUserPreferenceService.UpdatePreferencesAsync` now persists `TargetLearningLanguageCode` explicitly instead of silently preserving the previous value.
- [x] support helper-language expansion beyond the current ten languages without changing the target-language model
  - Requirement: helper-language expansion must be managed as explanation-language coverage, not as new target-learning-language activation.
  - Requirement: validation and admin diagnostics must report helper-language coverage by target language, module, and helper language.
  - Progress: target-learning-language reference data, helper-language validation, and backup/admin inventory treat helper/meaning languages separately from target-language activation. Detailed per-helper coverage dashboards remain tracked under Admin/Diagnostics rather than blocking the target-language model.
- [x] support target-language specific country-context selection where relevant
  - Example: German can show Germany, Austria, and Switzerland; English can show United States, United Kingdom, and Australia.
  - Example: Switzerland can appear under German, French, and Italian later, but each target-language/country-context pair must have separate original source content.
  - Requirement: country-context selection must not be confused with UI language or helper language.
  - Requirement: German learners should see Germany guidance by default; Austria and Switzerland can be added later as additional German country contexts.
  - Requirement: country-context UI must not imply that one country note can be shared as source content across multiple target languages.
  - Progress: Country Guidance persists `CountryContextCode`, German uses Germany (`DE`) as the active default, and canonical routes/API carry country context under the selected target language. Additional countries remain content work, not schema redesign.
- [x] define behavior for authenticated, anonymous, and first-visit users
  - Progress: anonymous and first-visit learners default to `de`; authenticated users persist `TargetLearningLanguageCode`; explicit learner routes remain authoritative for page context.
- [x] define empty states for a selected target language that is enabled in the platform but has no public content yet
  - Progress: planned languages are visible as inactive/disabled in settings and the learner switcher; inactive `/learn/en/...` routes return 404 rather than showing German content.
- [x] define Premium/entitlement implications per target language without blocking the core target-language selection
  - Decision: target-language selection is a core learning-context setting. Premium entitlements remain feature/module gates and must not be used to hide the active target-language identity.
- [x] update settings/profile UI copy so learners understand the difference between "I want to learn German" and "explain German in Persian/English/etc."
  - Progress: Settings separates `I want to learn`, UI language, and helper/meaning language controls; profile/matching copy now uses `Learning level` rather than `German level`.

### 8.4 Database And Domain Refactor

- [x] add `TargetLearningLanguageCode` to content root aggregates and their PostgreSQL tables
  - Scope: words, grammar topics, expressions, dialogues, Talk Topics, exercises, exercise sets, roleplays, course paths, course modules, course lessons, exam profiles, exam-prep units, writing templates, Life/Country guidance notes, conversation starter packs, event preparation packs, events/organizers where language learning scope matters, and package metadata.
  - Progress: implementation slices added `TargetLearningLanguageCode` to the main Web learning roots and `ContentPackage` metadata: Course paths/modules/lessons, Grammar topics, Expressions, Dialogues, Talk Topics, Exercises/Sets, Roleplays, Exam Profiles/Units, Writing Templates, Country Guidance, Conversation Starter Packs, Event Preparation Packs, Conversation Events, Event RSVPs, and package receipts. `OrganizerProfile` remains a global organizer identity record by product decision, while `OrganizerProfileSupportedLevel.TargetLearningLanguageCode` scopes the organizer's language-level coverage. `WordEntry.LanguageCode` is documented and indexed as the vocabulary target-language scope. User-state partitioning is implemented.
- [x] migrate existing content to `TargetLearningLanguageCode = de`
  - Progress: PostgreSQL `darwinlingua_shared` has the new target-language column on the main Web learning roots, Conversation Starter Packs, Event Preparation Packs, Conversation Events, and Event RSVPs; current rows are backfilled to `de`; package receipts are also backfilled to `de`. Organizer supported-level rows are backfilled to `de`. All official German package JSON files under `content/learning-portal/**/packages` now declare `targetLearningLanguageCode = de`.
- [x] update uniqueness constraints from slug-only to target-language-aware keys, for example `(TargetLearningLanguageCode, Slug)`
  - Progress: main Web learning root slug indexes, Conversation Starter Pack/Event Preparation Pack slug indexes, Conversation Event slug/filter indexes, Event RSVP event/email indexes, OrganizerProfile supported-level indexes, `ContentPackages(TargetLearningLanguageCode, PackageId)`, vocabulary language/lemma indexes, `UserContentProgress(UserId, TargetLearningLanguageCode, ContentOwnerType, ContentOwnerSlug)`, and user exercise attempt lookup indexes have been replaced with target-language-scoped indexes in EF configuration, EF migrations/retrofits, and PostgreSQL `darwinlingua_shared`. Remaining cross-content link work is route/helper consolidation and diagnostics, not hidden slug-only uniqueness debt.
- [x] update cross-content links so target language is part of link resolution
  - Requirement: link resolution must treat `(targetLearningLanguageCode, targetType, targetSlug)` as the lookup identity for target-scoped content.
  - Requirement: Country Guidance links must also include `countryContextCode` where the target type is country guidance.
  - Requirement: wrong-language links should be admin/report issues, not silently resolved by a same-slug item from another target language.
  - Progress: learner routes, Web API client calls, Unified Search links, Country Guidance links, Conversation Starter/Event Preparation lookups, event/organizer links, and admin unresolved-link diagnostics now carry target-language scope. Country Guidance links also include country context.
- [x] update progress/user-state owner identity so the same slug in two target languages cannot share progress accidentally
  - Progress: `UserContentProgress` and `UserExerciseAttempt` now persist `TargetLearningLanguageCode`; progress summary/update, recommendations, Recent word activity, favorite word lookup, and exercise attempt persistence pass explicit target-language context. PostgreSQL `darwinlingua_shared` has the new columns and short target-aware indexes.
  - Evidence: `dotnet build src/Apps/DarwinLingua.WebApi/DarwinLingua.WebApi.csproj --no-restore`, `dotnet build src/Apps/DarwinLingua.Web/DarwinLingua.Web.csproj --no-restore /p:UseSharedCompilation=false`, `dotnet test tests/Modules/Learning/DarwinLingua.Learning.Domain.Tests/DarwinLingua.Learning.Domain.Tests.csproj --no-restore --filter UserContentProgressTests`, `dotnet test tests/Modules/Learning/DarwinLingua.Learning.Application.Tests/DarwinLingua.Learning.Application.Tests.csproj --no-restore --filter UserContentProgressServiceTests`, `dotnet test tests/Apps/DarwinLingua.WebApi.Tests/DarwinLingua.WebApi.Tests.csproj --no-restore --filter "LearningProgressRouteStructuralTests|TargetLearningLanguageApiStructureTests|ExerciseAttemptAndSearchHardeningStructuralTests"`, and `dotnet test tests/Modules/Catalog/DarwinLingua.Catalog.Application.Tests/DarwinLingua.Catalog.Application.Tests.csproj --no-restore --filter ExerciseAttemptServiceTests` passed on 2026-06-24.
- [x] update package receipt/version metadata so published packages are partitioned by target language and module
  - Progress: `ContentPackage.TargetLearningLanguageCode` is persisted, indexed with `PackageId`, validated during import, and used by duplicate package checks.
- [x] remove or replace hard-coded German-only constants such as a single global learning-language code
  - Requirement: remaining German-specific property names, UI labels, route helpers, tests, and docs should be classified as either valid German content/source text or target-language debt.
  - Progress: generic learner UI now uses target-language wording and level-system reference data. Remaining German wording is either German source content, German/Germany display copy, or historical migration/package evidence.
- [x] keep mobile/SQLite changes deferred unless an API or package contract requires forward-compatible fields; do not do direct mobile UX work in this phase

### 8.5 Content Package Contracts And Import Pipeline

- [x] add required package-level `targetLearningLanguageCode` to every current content package contract
  - Progress: the shared package format and all active module package contracts now document `targetLearningLanguageCode` as the language being taught, separate from helper/meaning-language translations.
- [x] add optional package-level `levelSystemCode` where the package uses levels
  - Progress: parser/model/import validation accept `levelSystemCode` and contracts document current German CEFR usage. Future target-language capability profiles decide whether CEFR or another level system applies before package generation.
- [x] add package-level country-context metadata for Country Guidance packages
  - Progress: parser/model/import validation accept `countryContextCode`; Country Guidance packages with `countryGuidanceNotes` now must declare an active country context for the target language; the current Germany packages declare `countryContextCode = DE`.
- [x] update import validation so target language must be active or explicitly allowed for staging
  - Progress: import now rejects missing or inactive `targetLearningLanguageCode`; current active target language is `de`.
- [x] update import validation so helper translation languages must come from the active helper-language set, not from target-language definitions
  - Progress: package target-language validation is separate from existing helper/meaning-language validation, preserving the current active helper-language set.
- [x] update package replacement behavior so replacing a course path or module only replaces content for the same target language
  - Progress: imported root entities are persisted with target-language scope and package duplicate/replacement checks are target-language aware for the implemented Web learning roots, including Conversation Starter Packs and Event Preparation Packs.
- [-] update validation reports to include target language, level system, country context, package path, and helper-language coverage
  - Requirement: helper-language coverage must be reported separately from target-language coverage so future helper-language expansion does not look like target-language activation.
- [x] enforce country-context metadata for Country Guidance packages
  - Requirement: Germany guidance must import as `targetLearningLanguageCode = de` and `countryContextCode = DE`.
  - Requirement: future Austria/Switzerland guidance under German must be separate packages with their own country context.
  - Requirement: the same country under another target language must be a separate original source stream, not a helper translation.
  - Requirement: package roots and contracts should use `country-guidance` naming after the migration, not the older `culturalNotes` wording.
  - Progress: import validation rejects Country Guidance packages that omit `countryContextCode` or use a country context inactive for the package target language; the active Germany source packages now live under `content/learning-portal/country-guidance/packages` and use the `countryGuidanceNotes` root array.
- [x] backfill all official German package JSON files with `targetLearningLanguageCode = de`
  - Evidence: 507 package JSON files under `content/learning-portal/**/packages` were checked and all include `targetLearningLanguageCode = de`.
- [x] keep current helper translations intact while renaming docs/contracts away from confusing `targetLanguages` wording where it means meaning/helper languages
  - Progress: the new architecture and package-contract sections distinguish target-learning language from helper/meaning languages. The current learner-facing wording audit is closed; remaining historical content-package notes are evidence records, not active UI copy.

### 8.6 Web Routes And UX

- [x] design a clean canonical route model that includes target language instead of relying only on implicit profile state
  - Recommended route shape: `/learn/{targetLanguageCode}/courses`, `/learn/{targetLanguageCode}/grammar`, `/learn/{targetLanguageCode}/exercises`, and equivalent detail routes.
  - Decision: because there are no active customers, final learner routes should be explicit and clean. Old top-level learner routes should not be preserved permanently if they hide missing target-language context.
  - Progress: learner-facing route constants and route validation have started for `/learn/{targetLearningLanguageCode}/...`. Web learner navigation, public content links, forms, and admin preview links for learner-facing modules now pass target-language route values explicitly.
  - Evidence: `LearningRouteConventions`, `TargetLearningLanguageRouteFilter`, Web route attributes, shell links, content view links, admin preview links, `dotnet build src/Apps/DarwinLingua.Web/DarwinLingua.Web.csproj --no-restore`, and `dotnet test tests/Modules/Localization/DarwinLingua.Localization.Application.Tests/DarwinLingua.Localization.Application.Tests.csproj --no-restore --filter WebLearnerShellStructureTests` passed on 2026-06-24.
  - Progress: community learner routes for Conversation Events and Organizer Profiles now use `LearningRouteConventions.ConversationEvents` and `LearningRouteConventions.OrganizerProfiles`; structural tests assert the old top-level `[Route("conversation-events")]` and `[Route("organizers")]` routes are not retained.
- [x] update navigation so the selected target language is visible and switchable
  - Requirement: the selected target language must be visually separate from UI language and helper/meaning languages.
  - Requirement: until non-German content exists, the selector can show German as the active/default option and keep planned languages disabled or marked as coming later.
  - Progress: the Web shell now renders a shared target-learning-language switcher in desktop and mobile navigation. German is the only active option; English, Spanish, and French are present as planned/inactive options and cannot leak German content into another target route.
  - Evidence: `TargetLearningLanguageCatalog` lists active/planned target languages separately, `_TargetLearningLanguageSwitcher.cshtml` separates target language from UI/helper-language settings, `npm run build:css`, `dotnet build src/Apps/DarwinLingua.Web/DarwinLingua.Web.csproj --no-restore /p:UseSharedCompilation=false`, and `dotnet test tests/Modules/Localization/DarwinLingua.Localization.Application.Tests/DarwinLingua.Localization.Application.Tests.csproj --no-restore --filter WebLearnerShellStructureTests /p:UseSharedCompilation=false` passed on 2026-06-24.
- [x] update Course level cards to show large level code, learner-friendly level label, title, and description
  - Requirement: the large code should be the first visual signal, centered and prominent, followed by the friendly label and then the level title/description.
- [x] update all module list/detail pages to read target language from the canonical route or explicit request context
  - Progress: Conversation Events and Organizer Profiles now read target language from the canonical learner route; admin pages use an explicit operator-selected target-language scope.
- [x] update link helpers so cross-content links preserve target language and do not point to content from another language
  - Progress: Razor links/forms for current learner-facing modules, Web API calls, search result URLs, Country Guidance routes, and admin preview links carry target-language scope. Remaining reusable-helper cleanup is no longer a hidden blocker because structural tests cover the final route shape.
- [x] update breadcrumbs, page titles, search links, and admin preview links to include the target language context
  - Requirement: old top-level learner links should be removed from final navigation/tests once the matching canonical `/learn/{targetLearningLanguageCode}/...` route is green.
  - Requirement: temporary development redirects are acceptable only during migration; they must not be required for the final Web acceptance gate.
- [x] update UI copy from German-specific wording to target-language-aware wording where the feature is no longer German-only
- [x] keep German-specific labels only inside German target-language content and German country-context guidance

### 8.7 API And Query Layer

- [x] add target-language filtering to all public list/detail endpoints
  - Progress: Course, Grammar, Expressions, Dialogues, Talk Topics, Roleplays, Exercises/Sets/evaluate, Writing Templates, Country Guidance, and Exam Prep profile/unit public endpoints now accept `targetLearningLanguageCode`, validate it through the active target-language catalog, default anonymous/empty requests to `de`, and filter PostgreSQL rows by `TargetLearningLanguageCode`.
  - Evidence: `Program.cs`, the corresponding Catalog query services/repositories, `WebCatalogApiClient`, and Web/Admin controllers were updated; `dotnet build src/Apps/DarwinLingua.WebApi/DarwinLingua.WebApi.csproj --no-restore`, `dotnet build src/Apps/DarwinLingua.Web/DarwinLingua.Web.csproj --no-restore`, `dotnet test tests/Apps/DarwinLingua.WebApi.Tests/DarwinLingua.WebApi.Tests.csproj --no-restore --filter "TargetLearningLanguageApiStructureTests|CourseRouteStructuralTests"`, and `dotnet test tests/Modules/Localization/DarwinLingua.Localization.Application.Tests/DarwinLingua.Localization.Application.Tests.csproj --no-restore --filter WebLearnerShellStructureTests` passed on 2026-06-24.
  - Progress: Unified Search now accepts and normalizes `targetLearningLanguageCode`, defaults missing requests to `de`, filters target-scoped learning result types by target language, filters vocabulary by `WordEntry.LanguageCode`, and returns learner URLs under `/learn/{targetLearningLanguageCode}/...`.
  - Evidence: `dotnet test tests/Modules/Catalog/DarwinLingua.Catalog.Application.Tests/DarwinLingua.Catalog.Application.Tests.csproj --no-restore --filter UnifiedLearningSearchServiceTests`, `dotnet test tests/Apps/DarwinLingua.WebApi.Tests/DarwinLingua.WebApi.Tests.csproj --no-restore --filter "TargetLearningLanguageApiStructureTests|CourseRouteStructuralTests|ExerciseAttemptAndSearchHardeningStructuralTests"`, `dotnet test tests/Modules/Localization/DarwinLingua.Localization.Application.Tests/DarwinLingua.Localization.Application.Tests.csproj --no-restore --filter WebLearnerShellStructureTests`, and `dotnet test tests/Modules/Catalog/DarwinLingua.Catalog.Infrastructure.Tests/DarwinLingua.Catalog.Infrastructure.Tests.csproj --no-restore --filter UnifiedLearningSearchPostgresRepositoryTests` passed on 2026-06-24.
  - Progress: Conversation Starter Packs, Event Preparation Packs, Conversation Events, Event RSVP summaries/submission/admin status updates, and Organizer Profile list/detail views now accept/pass `targetLearningLanguageCode`. OrganizerProfile identity data is documented as global; supported levels and active-event views are target-scoped.
  - Progress: Admin Conversation Events and Organizer Profiles pages now have explicit target-language scope selectors and reject inactive target-language saves.
- [x] add target-language filtering to Unified Search, recommendations, recent activity, progress summary, and admin reporting endpoints
  - Progress: Unified Search is target-language scoped for vocabulary and the main learning modules. Search result URLs for learning content now use `/learn/de/...` for the current German target language.
  - Progress: progress summary/update, deterministic recommendations, Recent word activity, difficult-word recommendation URLs, favorite word lookup, and secondary word lookup now receive explicit `targetLearningLanguageCode` and are scoped to the current target language.
  - Progress: admin system report and Learning Portal issue drilldown now accept explicit `targetLearningLanguageCode`, return report scope metadata, filter Learning Portal counts/quality checks/unresolved-link diagnostics inside the target language, and list social/moderation/email sections as global resources.
  - Evidence: `dotnet build src/Apps/DarwinLingua.WebApi/DarwinLingua.WebApi.csproj --no-restore`, `dotnet build src/Apps/DarwinLingua.Web/DarwinLingua.Web.csproj --no-restore /p:UseSharedCompilation=false`, `dotnet test tests/Apps/DarwinLingua.WebApi.Tests/DarwinLingua.WebApi.Tests.csproj --no-restore --filter "WebsiteAdminQueryServiceLearningPortalReportTests|AdminReportsSummaryStructuralTests|AdminReportsLearningPortalIssueDrilldownStructuralTests|TargetLearningLanguageApiStructureTests"`, and `dotnet test tests/Modules/Localization/DarwinLingua.Localization.Application.Tests/DarwinLingua.Localization.Application.Tests.csproj --no-restore --filter WebLearnerShellStructureTests` passed on 2026-06-24.
  - Evidence: Slice 7 regression and admin smoke verified the target-scoped German baseline, including current community resources under canonical German target context.
- [x] update API contracts so target language is explicit and not inferred from helper-language parameters
- [x] add API validation for unsupported target-language codes and inactive target languages
  - Progress: the shared API resolver rejects unsupported/inactive target-language codes for the covered core learning endpoints, Unified Search, conversation support endpoints, Conversation Events, Event RSVPs, and Organizer Profile views through `TargetLearningLanguageCatalog.TryFindActive`.
- [x] ensure anonymous requests default to `de` until a target-language selector is implemented
  - Progress: the API resolver defaults missing or blank `targetLearningLanguageCode` to `ContentLanguageRequirements.DefaultTargetLearningLanguageCode` (`de`) for the covered core learning endpoints.
  - Progress: the learner shell now exposes the current target language explicitly, so anonymous users can see the active target-language context instead of relying on hidden German-only assumptions.
- [x] ensure authenticated requests use the stored target-language preference unless the route explicitly overrides it
  - Requirement: explicit route/request target language wins over stored preference; stored preference is only a default when the user enters a target-neutral page.
  - Progress: `WebUserPreferenceService` persists the selected target language; learner routes resolve explicit route context; missing/blank API requests default to `de` until additional active target languages exist.
- [ ] update generated/mobile-facing package manifest contracts so target language is part of package identity
  - Note: this is a forward-compatible API/package-contract task only. Direct MAUI/mobile UX work remains deferred.

### 8.8 Module-Specific Refactor Rules

- [x] Vocabulary: make lexical entries target-language scoped; meanings remain helper-language translations
- [x] Grammar: make grammar categories and grammar block validation target-language scoped because grammar concepts differ by language
- [x] Expressions: require native idiom/pragmatic expression authoring per target language; do not translate German idioms into English/Spanish/French packages
- [x] Dialogues and Talk Topics: make scenarios target-language scoped and keep linked words/expressions inside the same target language
- [x] Exercises: keep reusable exercise engine shapes, but make answer validation and target grammar assumptions target-language aware
- [x] Course Lessons: make course paths/modules/lessons target-language scoped and use the level-system abstraction for browsing
- [x] Exam Prep: make exam profiles target-language and country/provider scoped; Goethe/telc German content must not appear under English or Spanish
- [x] Writing Templates: make register, template conventions, and sample outputs target-language scoped because email/form norms differ by language and country
- [x] Life/Country Guidance: refactor the current `CountryGuidanceNote`/Life in Germany concept into target-language-aware country guidance
  - Requirement: German target language initially shows Germany content and later can show Austria and Switzerland.
  - Requirement: future English target language can show country contexts such as United States, United Kingdom, and Australia.
  - Requirement: Switzerland can have separate German, French, and Italian target-language content if those target languages are active.
  - Requirement: country guidance content is authored separately for each target-language/country-context pair; helper translations do not turn one source stream into another target-language stream.
  - Requirement: route and admin labels should move toward `Country Guidance`; `Life in Germany` can remain the German/Germany display label.
  - Requirement: since the product is pre-customer, migrate the internal model, route names, package roots, admin labels, tests, and docs cleanly instead of carrying `CountryGuidanceNote` as permanent legacy naming.
  - Requirement: remove old public routes after canonical routes and imports are green; broken old URLs during development are acceptable if tests catch missing references quickly.
  - Requirement: Germany guidance remains visible inside the German learning experience by default; it must not become a global module disconnected from the selected learning language.
  - Requirement: future Country Guidance must support multiple countries for the same target language and the same country under multiple target languages.
  - Requirement: source content for each target-language/country-context pair must be authored separately; helper translations are not a substitute for source content in another target language.
  - Requirement: Country Guidance is broader than official exam preparation. For German/Germany it should cover everyday life, civic/legal basics, orientation-course concepts, bureaucracy, social norms, and integration topics; it must not become only a fixed-question trainer.
  - Requirement: legal-adjacent and official-process guidance must be refreshed from current sources before generation when facts are likely to change.
  - Progress: `CountryContextCode` is now a persisted partition on Country Guidance content, with Germany as the default active country context for German. Import replacement is scoped by both target language and country context so future Austria/Switzerland packages cannot overwrite Germany notes with matching slugs.
- [x] Unified Search: keep result-type labels generic, but rank/filter only inside the active target language unless the user intentionally searches cross-language administrative views
  - Progress: public Unified Search filters word, grammar, expression, dialogue, talk-topic, roleplay, exercise, course lesson, exam prep, writing template, and Country Guidance results inside the active target language. Conversation events and organizer profile views are now target-scoped where they represent language-practice inventory; generic organizer identity remains global.

### 8.9 Admin, Diagnostics, And Operations

- [-] add admin filters for target language, level system, and country context across content reports
  - Progress: Learning Portal admin report and issue drilldown accept `targetLearningLanguageCode`, default to `de`, preserve the scope through Web links/forms/CSV exports, and expose current `CountryContextCode = DE` metadata for the German baseline. Level-system and real country-context partition filters remain open.
- [x] add counts by target language for every module
  - Progress: Learning Portal report counts and quality counters are filtered by target language for the implemented Web learning modules; social/moderation/email sections are intentionally global. Admin Reports now also expose `CountsByTargetLanguage` rows such as `course-lesson:de`, `grammar-topic:de`, and `country-guidance:de`.
  - Evidence: `dotnet test tests\Apps\DarwinLingua.WebApi.Tests\DarwinLingua.WebApi.Tests.csproj --no-restore --filter "WebsiteAdminQueryServiceLearningPortalReportTests|AdminReportsSummaryStructuralTests" /p:UseSharedCompilation=false` passed 3/3 on 2026-06-24.
- [x] add missing-helper-translation reports by target language and helper language
  - Progress: Admin Reports now expose missing helper translation coverage by helper language and by module, using the centralized `ContentLanguageRequirements.RequiredMeaningLanguageCodes` list rather than a report-local hard-coded language set.
  - Evidence: `WebsiteAdminQueryServiceLearningPortalReportTests` asserts helper-language and module coverage rows, and `AdminReportsSummaryStructuralTests` asserts the Web report renders the new coverage tables.
- [-] add unresolved-link reports that detect links crossing target-language boundaries incorrectly
  - Progress: Learning Portal unresolved-link diagnostics now build valid slug sets only inside the selected target language, so same-slug content in another future target language cannot mask a broken link.
- [x] add duplicate-slug diagnostics within each target-language namespace and across namespaces for operator review
  - Progress: Admin Reports now expose duplicate slug diagnostics for same target-language/module namespace collisions and cross-module same-slug collisions, with issue drilldown samples under `Slug namespace`.
  - Evidence: `WebsiteAdminQueryServiceLearningPortalReportTests` seeds a cross-module `de:a1-articles` collision and asserts `DuplicateSlugsByType` plus `DuplicateSlugCount`.
- [x] add content coverage dashboards for target language, level, module, activity flow, exercise availability, writing templates, exam prep, and country guidance
  - Requirement: Country Guidance dashboard must break counts down by target language plus country context, for example `de|DE`, `de|AT`, `de|CH`, `en|US`, `en|GB`, and `en|AU` when those streams exist.
  - Progress: Admin Reports now show target-language counts, CEFR/module/category/register/profile tables, course activity quality counters, exercise/exam/writing/Country Guidance coverage, Country Guidance by country context, helper-language gaps, duplicate slug diagnostics, and target-language activation gate rows.
- [-] add target-language activation gate diagnostics
  - Requirement: admin diagnostics must show whether a planned target language has reference data, level labels, country contexts, helper-language coverage, route isolation, search isolation, progress isolation, and package-import readiness.
  - Requirement: inactive target-language routes must be visible in diagnostics as intentionally unavailable, not as missing content errors.
  - Progress: Admin Reports now expose activation gate rows for active/planned target-language reference data, selected target status, selected level definitions, selected active country contexts, selected content items, and selected Country Guidance stream count. Route/search/progress/package readiness still need explicit pass/fail rows before first non-German activation.
- [-] add Country Guidance stream diagnostics by target-language/country-context pair
  - Requirement: report each stream as `targetLearningLanguageCode|countryContextCode`, for example `de|DE`, `de|AT`, `de|CH`, `en|US`, `en|GB`, `en|AU`, `fr|CH`, or `it|CH`.
  - Requirement: Germany guidance remains under German by default and may be displayed as `Life in Germany`; the stable module remains `Country Guidance`.
  - Requirement: a country appearing under multiple target languages must be diagnosed as separate source-content streams, not merged or treated as translations of one stream.
- [x] add helper-language expansion readiness checks
  - Requirement: helper-language coverage reports must not assume the active set is permanently limited to the current ten helper languages.
  - Requirement: adding new helper languages later must be visible as missing-helper coverage work, not require schema or target-language redesign.
  - Progress: Admin helper coverage now reads the centralized required helper-language list. Expanding helper languages later changes one shared requirement source and appears as missing coverage in the admin report.
- [x] update backup manifests to record counts by target language and country context
  - Progress: `tools\Web\New-WebReadinessPhaseBackup.ps1` now writes `verification\database-inventory.txt` with content counts by target language, Country Guidance by target/country context, and helper-translation JSON coverage.
- [x] update restore verification to check target-language partition counts after migration
  - Progress: `tools\Web\New-WebReadinessPhaseBackup.ps1` now runs a temporary PostgreSQL restore dry-run and writes `verification\restore-dry-run-database-inventory.txt`; the final Phase 8 backup at `X:\Projects\DarwinLingua.Backup\20260624-160737-phase8-complete-pre-english-pilot` has matching source/restored inventories with `DifferenceCount=0`.
  - Requirement: global/community resources that are intentionally not target-scoped must be explicitly listed as exemptions in admin diagnostics and docs.
  - Current exemption: `OrganizerProfile` identity/directory data is global; `OrganizerProfileSupportedLevels`, active event lists, and event management are target-scoped.

### 8.10 Testing And Migration Gates

- [x] add migration tests proving all existing German content is backfilled to `de`
- [x] add regression tests proving current German browsing still works after the target-language refactor
- [x] add isolation tests proving content for one target language is not returned under another target language
- [x] add route tests for the new canonical `/learn/{targetLearningLanguageCode}/...` shape
  - Progress: structural tests now assert route conventions, route filter registration, missing old top-level content routes for covered controllers, learner-facing Razor links carrying `asp-route-targetLearningLanguageCode`, Web client forwarding of `targetLearningLanguageCode`, API/repository target-language filtering for the covered core learning modules, and Unified Search URLs under `/learn/de/...`. Conversation Event, Event RSVP, and Organizer Profile Web/API tests cover target-language query propagation for current community resources. Remaining work: integration/smoke route tests against running Web/WebApi and explicit wrong-language isolation fixtures once a second active target language exists.
- [x] add import tests for missing, unsupported, inactive, and mismatched target-language codes
- [x] add search tests for target-language filtering, ranking stability, empty states, and cross-module result links
  - Progress: application, WebApi structural, Web shell structural, and PostgreSQL Unified Search tests cover target-language normalization/filtering and learn-aware cross-module result links for the current German target language.
  - Remaining: add explicit wrong-language isolation fixtures once a second active target language is available.
- [x] add progress tests proving the same slug in two target languages creates separate progress records
- [x] add admin report tests for target-language counts, country-context counts, missing translations, unresolved links, and wrong-language leakage
- [x] run targeted Web/WebApi/Catalog/ContentOps/Learning tests after each implementation slice
- [x] run a restore-ready backup after the German baseline is migrated and verified under the new target-language architecture

### 8.11 First Non-German Content Pilot

- [x] choose the first non-German target language after the architecture is green
  - Recommendation: English first.
  - Rationale: English has broad demand, obvious country-context expansion (`US`, `GB`, `AU`), and enough tester availability to validate the multi-target model early.
  - Alternatives: Spanish first if regional/community demand is stronger; French first if Switzerland/Belgium/Canada context testing becomes the priority.
  - Decision: English is the recommended first non-German target-language pilot.
- [x] create a small target-language pilot plan before generating content
  - Requirement: the pilot plan must include the target-language capability profile and a module-by-module content list before any package is generated.
  - Evidence: `artifacts\planning\target-language-en-capability-profile-and-pilot.md`.
- [ ] complete target-language activation gate before generating the English pilot
  - Requirement: English must remain `planned/inactive` until reference data, level labels, country contexts, route behavior, API behavior, search, progress, admin reports, imports, helper translations, and backup evidence are green.
  - Requirement: planned English routes must not silently show German content.
  - Requirement: activation is a product/data decision, not only a route or database flag change.
- [ ] define the English learner-friendly level labels before content generation
  - Requirement: even if English initially uses CEFR, the UI must show compact code plus plain learner-friendly label and explanation.
  - Requirement: labels must be reviewed for English-learning needs and not copied blindly from German labels.
- [ ] define first English country-context scope before content generation
  - Requirement: pick the first Country Guidance stream deliberately, recommended `en|US` for the first small pilot unless product demand points elsewhere.
  - Requirement: keep `en|GB` and `en|AU` planned until their own native source-content plans are reviewed.
- [ ] generate native target-language content, not translated German content
- [ ] start with a small A1/basic pilot across Course, Grammar, Expressions, Exercises, Writing Templates, and a minimal country-context guidance slice
- [ ] import the pilot into PostgreSQL with target-language partitioning
- [ ] smoke `/learn/{targetLanguageCode}/...`, search, progress, admin counts, and helper-language projection
- [ ] review content quality with native-language expectations before expanding to bulk generation

### 8.11A Target-Language Activation Framework

- [x] create a reusable activation checklist for each new target language
  - Requirement: checklist must cover script/direction, level system, search normalization, morphology/grammar assumptions, punctuation/capitalization, input/keyboard guidance, TTS/pronunciation expectations, country contexts, exam ecosystem, writing conventions, first-pilot modules, helper-language launch set, and legal/current-source refresh needs.
  - Requirement: this checklist must be completed before any broad content generation for English, Spanish, French, or later languages.
  - Progress: `94-Multi-Target-Language-Architecture.md` now defines the activation gate and language-specific requirements. The checklist is architectural; each target language still needs its own completed profile before activation.
- [x] make target-language capability profiles first-class planning artifacts
  - Requirement: every target language gets an artifact under `artifacts/planning/target-language-{code}-capability-profile-and-pilot.md` or equivalent before package creation.
  - Requirement: the profile must describe what is different from German instead of inheriting German defaults silently.
  - Progress: the first English profile artifact exists at `artifacts/planning/target-language-en-capability-profile-and-pilot.md`; future Spanish, French, Italian, and other targets must follow the same pattern before package creation.
- [x] define target-language status transitions
  - Requirement: allowed states should distinguish at least `planned`, `pilot`, and `active`.
  - Requirement: `planned` languages may be shown as disabled options only with a clear empty state.
  - Requirement: `pilot` languages may be visible only to admin/tester flows if content is incomplete.
  - Requirement: `active` languages must pass Web/API/search/progress/admin/import/backup gates.
  - Progress: status semantics are now locked in the architecture document; implementation still has to enforce every gate in route/API/search/progress/admin/package tests before English can move beyond planned/inactive.
- [x] define clean no-compatibility migration rules for future target-language refactors
  - Requirement: because the product is still pre-customer, do not preserve old ambiguous routes, package roots, or model names when they would create permanent target-language or country-context debt.
  - Requirement: temporary development redirects are allowed only inside an active migration slice and must be removed or tested as non-canonical before the slice is closed.
  - Progress: old public route compatibility is not a product requirement for this phase. Canonical target-language routes and scoped package/model names are preferred even if old development URLs break.
- [x] define country-context source-stream rules for all target languages
  - Requirement: Country Guidance for `de|CH`, `fr|CH`, and `it|CH` are separate authored source streams if those target languages become active.
  - Requirement: helper translations can adapt explanations to the learner's culture when useful, but they do not turn one country stream into another target-language source stream.
  - Progress: Country Guidance is defined as `(targetLearningLanguageCode, countryContextCode)`. German shows `de|DE` as Life in Germany now, and can later add `de|AT` and `de|CH`; English can add `en|US`, `en|GB`, and `en|AU`; Switzerland can appear as separate source streams under German, French, and Italian.
- [x] define helper-language expansion rules
  - Requirement: the system must allow adding helper languages later, for example expanding German from ten helper languages to fifteen, without changing target-language source identity.
  - Requirement: content reports and import validation must make missing helper-language coverage explicit per module, target language, and helper language.
  - Progress: helper/meaning languages are independent from target-learning-language. A target language can launch with one reviewed helper-language set and expand later; admin reports/import validation must expose missing coverage rather than forcing schema redesign.

### 8.11B No-Debt Multi-Target Completion Backlog

- [x] implement German CEFR level metadata as real reference data everywhere it is displayed
  - Requirement: learner-facing level surfaces must show compact code plus friendly label and short explanation, for example `A1 - Einstieg`, `A2 - Grundlagen`, `B1 - Selbststaendig`, `B2 - Kompetent`, `C1 - Souveraen`, and `C2 - Meisterschaft`.
  - Requirement: labels must come from level-system/reference data, not hard-coded Course page markup, so future English, Spanish, French, and other target languages can define their own level labels.
  - Surfaces: onboarding/settings target-language selection, Course level cards, search filters, progress summaries, event/profile matching, admin preview, and package manifests where level metadata is exposed.
  - Progress: `LearningLevelSystemCatalog.GermanCefrLevels` now uses the current German CEFR learner labels and short descriptions, and Web English/German localization resources include all level label/description keys used by Course cards and level pickers.
  - Evidence: `dotnet test tests\Apps\DarwinLingua.WebApi.Tests\DarwinLingua.WebApi.Tests.csproj --no-restore --filter CourseRouteStructuralTests /p:UseSharedCompilation=false` passed 28/28; `dotnet test tests\Modules\Localization\DarwinLingua.Localization.Application.Tests\DarwinLingua.Localization.Application.Tests.csproj --no-restore --filter WebLearnerShellStructureTests /p:UseSharedCompilation=false` passed 8/8; `dotnet build src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 warnings/errors on 2026-06-24.
- [x] finish target-language route and link cleanup with no permanent legacy compatibility
  - Requirement: learner routes must use `/learn/{targetLearningLanguageCode}/...` where the content is target-language scoped.
  - Requirement: ambiguous old top-level learner routes should be removed or fail after their canonical replacement is green; temporary redirects are allowed only inside an active migration slice.
  - Requirement: breadcrumbs, search result links, activity-block links, admin preview links, and generated package URLs must carry target language.
  - Progress: reusable linked-content URL generation no longer hard-codes `/learn/de`; `LearningContentLinkResolver` and `CourseActivityTargetLinkResolver` require explicit target-language context, and linked practice partials pass the current route target language. Country Guidance linked-content partials also pass the current country context.
  - Evidence: runtime source audit found no remaining hard-coded `/learn/de` or old top-level target-scoped learner routes outside route-mapping tests; `dotnet test tests\Apps\DarwinLingua.WebApi.Tests\DarwinLingua.WebApi.Tests.csproj --no-restore --filter "CourseRouteStructuralTests|CountryGuidanceNoteRouteStructuralTests|ExamPrepRouteStructuralTests|WritingTemplateRouteStructuralTests" /p:UseSharedCompilation=false` passed 35/35; `dotnet build src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 warnings/errors on 2026-06-24.
- [x] finish API request contract cleanup for target language
  - Requirement: every public endpoint serving target-scoped learning content must accept or derive explicit target language and reject unsupported/inactive targets consistently.
  - Requirement: stored authenticated preference is a default only when the route/request has no explicit target-language context.
  - Requirement: unsupported or inactive target APIs must not silently return German content.
  - Progress: WebApi target-language resolution now treats unsupported target languages and unsupported country contexts as request/domain-rule errors instead of internal operation failures. Missing target language still defaults to the active German baseline for current compatibility, while explicit inactive targets such as future `en` do not pass `TryFindActive`.
  - Evidence: `dotnet test tests\Apps\DarwinLingua.WebApi.Tests\DarwinLingua.WebApi.Tests.csproj --no-restore --filter TargetLearningLanguageApiStructureTests /p:UseSharedCompilation=false` passed 14/14; `dotnet build src\Apps\DarwinLingua.WebApi\DarwinLingua.WebApi.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 warnings/errors on 2026-06-24.
- [x] finish Country Guidance expansion readiness
  - Requirement: `de|DE` remains visible inside German learning as Life in Germany.
  - Requirement: Country Guidance must support future streams such as `de|AT`, `de|CH`, `en|US`, `en|GB`, `en|AU`, `fr|CH`, and `it|CH` without schema redesign.
  - Requirement: each target/country pair is original source content; helper translations are learner support, not source-content reuse.
  - Requirement: official-process, legal-adjacent, residence/civic, and public-service topics must have current-source refresh discipline before new batches are generated.
  - Progress: `CountryContextCatalog` now distinguishes all planned country contexts from active contexts. Germany (`DE`) remains active for German; Austria (`AT`), Switzerland (`CH` for German/French/Italian readiness), United States (`US`), United Kingdom (`GB`), and Australia (`AU`) are present but inactive until reviewed content is generated. Country Guidance URL generation, Web navigation defaults, Web/API country validation, domain validation, import validation, repository filtering, and Unified Search links now use centralized target/country reference data instead of hard-coded Germany fallback.
  - Evidence: `dotnet test tests\Apps\DarwinLingua.WebApi.Tests\DarwinLingua.WebApi.Tests.csproj --no-restore --filter "CountryGuidanceNoteRouteStructuralTests|TargetLearningLanguageApiStructureTests|CourseRouteStructuralTests|AdminReportsSummaryStructuralTests|AdminReportsLearningPortalIssueDrilldownStructuralTests" /p:UseSharedCompilation=false` passed 51/51; `dotnet test tests\Modules\ContentOps\DarwinLingua.ContentOps.Application.Tests\DarwinLingua.ContentOps.Application.Tests.csproj --no-restore --filter CountryGuidance /p:UseSharedCompilation=false` passed 3/3; `dotnet test tests\Modules\Catalog\DarwinLingua.Catalog.Infrastructure.Tests\DarwinLingua.Catalog.Infrastructure.Tests.csproj --no-restore --filter UnifiedLearningSearchPostgresRepositoryTests /p:UseSharedCompilation=false` passed 2/2; `dotnet build src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build src\Apps\DarwinLingua.WebApi\DarwinLingua.WebApi.csproj --no-restore /p:UseSharedCompilation=false` both passed with 0 warnings/errors on 2026-06-24.
- [x] finish target-language isolation for search, progress, recommendations, and recent activity
  - Requirement: same slug in different target languages must create separate progress records and separate search/recommendation results.
  - Requirement: planned/inactive target languages must show explicit unavailable/empty state rather than German fallback.
  - Requirement: deterministic recommendations must never cross target-language boundaries unless a future explicit cross-language learning feature is designed.
  - Progress: Unified Search and deterministic recommendations already filter target-scoped content and attempts by `TargetLearningLanguageCode`; progress records are keyed and queried by target language. The remaining recent-activity gap is closed: Web word states now persist `TargetLearningLanguageCode`, use target-aware unique/recent indexes, bootstrap existing databases with a default German target, and query recent words before `Take` inside the current target language. Word detail interactions now store viewed/known/difficult state in the current learner route target language.
  - Evidence: `dotnet test tests\Apps\DarwinLingua.WebApi.Tests\DarwinLingua.WebApi.Tests.csproj --no-restore --filter "LearningProgressRouteStructuralTests|WordsControllerDualMeaningLanguageTests|TargetLearningLanguageApiStructureTests" /p:UseSharedCompilation=false` passed 22/22; `dotnet build src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 warnings/errors on 2026-06-24.
- [x] finish admin diagnostics and activation-gate reporting
  - Requirement: reports must show counts by target language, level system, country context, module, helper language, and Country Guidance stream.
  - Requirement: diagnostics must include route isolation, API isolation, search isolation, progress isolation, package/import readiness, unresolved links crossing target-language boundaries, duplicate slug collisions, and helper-language gaps.
  - Requirement: global exceptions such as organizer identity must be explicitly listed so they are not mistaken for target-language leakage.
  - Progress: Admin system reports now expose target-language content counts, Country Guidance country-context counts, and an activation gate with active/planned target rows, per-target content totals, level-definition availability, active country contexts, and planned country contexts. Country Guidance issue labels no longer use the old `Cultural linked ...` wording in Web/API report diagnostics.
  - Evidence: `dotnet test tests\Apps\DarwinLingua.WebApi.Tests\DarwinLingua.WebApi.Tests.csproj --no-restore --filter WebsiteAdminQueryServiceLearningPortalReportTests /p:UseSharedCompilation=false` passed 2/2; `dotnet test tests\Apps\DarwinLingua.WebApi.Tests\DarwinLingua.WebApi.Tests.csproj --no-restore --filter "AdminReportsSummaryStructuralTests|AdminReportsLearningPortalIssueDrilldownStructuralTests" /p:UseSharedCompilation=false` passed 4/4 on 2026-06-24.
- [x] finish generated/package manifest identity
  - Requirement: every generated or mobile-facing package manifest must include `targetLearningLanguageCode`.
  - Requirement: level-system metadata must be included where package slices are level-scoped.
  - Requirement: `countryContextCode` must be included for Country Guidance and any future country-specific package.
  - Requirement: direct MAUI/mobile UX remains deferred, but exported contracts must not be German-only.
  - Progress: `ContentPackage` now stores `TargetLearningLanguageCode`, `LevelSystemCode`, and `CountryContextCode`; import parsing and validation require `levelSystemCode = cefr` for current levelled packages and require active country context for Country Guidance packages.
  - Progress: all active package JSON files under `content/learning-portal/**/packages` now declare `targetLearningLanguageCode = de` and `levelSystemCode = cefr`; Country Guidance packages also declare `countryContextCode = DE`.
  - Progress: PostgreSQL `darwinlingua_shared.ContentPackages` now has `LevelSystemCode` and `CountryContextCode`; existing package receipt rows are backfilled so package audit history has no missing target or level metadata, and German Country Guidance receipts have `DE`.
  - Evidence: package audit on 2026-06-24 reported `packageFiles=505`, `parseErrors=0`, `missingTarget=0`, `missingLevel=0`, and `countryGuidanceMissingCountry=0`.
  - Evidence: `dotnet test tests\Modules\ContentOps\DarwinLingua.ContentOps.Application.Tests\DarwinLingua.ContentOps.Application.Tests.csproj --no-restore --filter FullyQualifiedName~ContentPackageDomainTests /p:UseSharedCompilation=false` passed 24/24; `dotnet test tests\Modules\ContentOps\DarwinLingua.ContentOps.Application.Tests\DarwinLingua.ContentOps.Application.Tests.csproj --no-restore --filter FullyQualifiedName~ContentImportServiceApplicationTests /p:UseSharedCompilation=false` passed 92/92; `dotnet test tests\Modules\ContentOps\DarwinLingua.ContentOps.Infrastructure.Tests\DarwinLingua.ContentOps.Infrastructure.Tests.csproj --no-restore --filter "FullyQualifiedName~ContentImportServiceTests|FullyQualifiedName~ContentImportParser" /p:UseSharedCompilation=false` passed 38/38.
  - Evidence: targeted builds for `DarwinLingua.ContentOps.Application.csproj` and `DarwinLingua.Infrastructure.csproj` succeeded with 0 warnings and 0 errors.
- [x] add wrong-language and inactive-target regression fixtures
  - Requirement: tests must prove inactive `en`, `es`, or `fr` learner routes and API calls do not display German content.
  - Requirement: tests must prove `de|DE` Country Guidance is under German and not a global module.
  - Requirement: tests must prove the same country can be represented under multiple target languages without schema changes.
  - Progress: Unified Search PostgreSQL tests now inject a wrong-language `en` row directly into the test database and verify German search remains bounded to `/learn/de/...` results. Application-level search validation rejects inactive `en` before repository access. WebApi structure tests verify inactive/planned target resolver behavior and Country Context validation against the resolved active target language.
  - Evidence: `dotnet test tests\Modules\Catalog\DarwinLingua.Catalog.Infrastructure.Tests\DarwinLingua.Catalog.Infrastructure.Tests.csproj --no-restore --filter FullyQualifiedName~UnifiedLearningSearchPostgresRepositoryTests /p:UseSharedCompilation=false` passed 2/2; `dotnet test tests\Modules\Catalog\DarwinLingua.Catalog.Application.Tests\DarwinLingua.Catalog.Application.Tests.csproj --no-restore --filter FullyQualifiedName~UnifiedLearningSearchServiceTests /p:UseSharedCompilation=false` passed 6/6; `dotnet test tests\Apps\DarwinLingua.WebApi.Tests\DarwinLingua.WebApi.Tests.csproj --no-restore --filter FullyQualifiedName~TargetLearningLanguageApiStructureTests /p:UseSharedCompilation=false` passed 14/14.
- [ ] close the German baseline regression and backup before first non-German pilot content
  - Requirement: Web browsing, API detail, helper translations, search, progress, recommendations, admin reports, imports, and Country Guidance under `/learn/de/...` must be green.
  - Requirement: backup under `X:\Projects\DarwinLingua.Backup` must include PostgreSQL dump, globals, restore list, dry-run restore, repo overlay, separate secret bundle, manifest, checksums, target-language counts, country-context counts, and helper-language coverage.
- [ ] start first non-German pilot only after the activation gate is green
  - Recommendation: English first.
  - Requirement: first English pilot must be small and native, not translated German: Course, Grammar, Expressions, Exercises, Writing Templates, and one minimal Country Guidance stream such as `en|US`.
  - Requirement: `en|GB` and `en|AU` remain planned until their own reviewed source-content plans exist.

### 8.12 Execution Order

1. Freeze the German baseline snapshot for this refactor.
   - Record current counts by module, target language, country context, and helper-language coverage.
   - Keep Web as the active product surface and keep direct MAUI/mobile UX work deferred.
   - Do not generate non-German content during this architecture refactor.
2. Complete package/import scoping.
   - Add `targetLearningLanguageCode`, `levelSystemCode`, and `countryContextCode` where required.
   - Backfill all German package JSON files.
   - Make replacement/import behavior target-language scoped.
   - Enforce country-context metadata for Country Guidance packages.
3. Finish database/domain scoping.
   - Cover package metadata, conversation/event resources where language scoped, link tables, progress state, recommendations, and admin diagnostics.
   - Keep direct MAUI UX out of scope, but preserve forward-compatible API/package fields.
4. Implement canonical Web/API route context.
   - Move learner routes to `/learn/{targetLanguageCode}/...`.
   - Use explicit route context in link helpers, breadcrumbs, search links, and detail links.
   - Remove ambiguous legacy top-level route behavior from final tests and docs; temporary development redirects may exist only while a slice is actively being migrated.
5. Refactor Life in Germany into Country Guidance.
   - Rename public/platform surfaces toward `Country Guidance`.
   - Keep `Life in Germany` only as the German/Germany display label inside the German target-language experience.
   - Replace permanent internal/public `CountryGuidanceNote` naming if keeping it would create technical debt.
   - Add `countryContextCode` as a real content partition.
   - Move package roots and contracts toward `country-guidance` naming.
   - Use canonical routes such as `/learn/de/country-guidance/de`.
   - Remove old public learner routes after the canonical routes, imports, tests, and links are green.
   - Verify Germany content is under `targetLearningLanguageCode = de` and `countryContextCode = DE`.
   - Verify future Austria, Switzerland, United States, United Kingdom, and Australia contexts can be represented without schema redesign.
6. Add target-language isolation to search, progress, recommendations, recent activity, and admin reports.
   - Every count, filter, unresolved-link check, and recommendation query must include target language.
   - Country Guidance diagnostics must also include country context.
7. Update Web target-language selection UX.
   - Show active target language clearly.
   - Keep target language, UI language, and helper/meaning languages conceptually separate.
   - Show learner-friendly level labels on Course level cards.
   - Allow helper-language expansion without changing target-language selection.
   - Show planned target languages only when the empty state is clear and no German content leaks into them.
8. Remove remaining German-only wording from generic learner surfaces.
   - Keep German-specific labels only inside German source content or German/Germany display names such as Life in Germany.
   - Use target-language-neutral labels in account/profile/matching/settings surfaces.
   - Prefer level-system reference data for all level pickers and summaries.
9. Run German baseline regression.
   - Current German Web browsing, API detail, helper translations, search, progress, recommendations, admin reports, and imports must stay green under `/learn/de/...`.
   - Confirm there is no wrong-language leakage by querying unsupported target-language routes and APIs.
10. Produce a restore-ready backup under `X:\Projects\DarwinLingua.Backup`.
   - Backup manifest must include counts by target language and country context.
11. Create the first non-German pilot plan.
   - Recommended target: English.
   - Planning must include the target-language capability profile before any content is generated.
12. Generate only a small native non-German pilot after the architecture is green.
   - Do not bulk-translate German content.

---

## Backlog Maintenance Rule

Before starting a new implementation slice:

- move the selected items to `[-]`
- keep unrelated items unchanged
- mark completed work with `[x]`
- add any newly discovered tasks in the correct workstream

This file should remain the main execution checklist for the project.

