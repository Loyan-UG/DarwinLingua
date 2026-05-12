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
- German level
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
  - Progress: WebApi now persists `LearnerConversationProfile` with display name, city/region, interaction preference, German level, helper languages, goals, availability, visibility, and adult confirmation.
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
- German level
- helper languages
- native language
- learning goal
- availability

#### Backlog

- [x] define `PartnerRequest` model
  - Progress: WebApi now persists request-based partner introductions between learner profiles without creating an open chat model.
- [x] define match search query model
  - Progress: Web matching search supports city/region, interaction preference, German level, helper language, and goal keyword filters.
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
- [ ] add broader query/WebApi rendering coverage after first real grammar content package is available
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
- [ ] broaden type-specific runner UI beyond generic JSON submission
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

### 7.8 Cultural Notes

- [x] implement CulturalNote model
- [x] link to dialogues, expressions, writing templates, Talk Topics, and course lessons
- [x] add Web pages
- [x] add initial parser/navigation/localization tests
- [x] add content contract in `82-Cultural-Note-Content-Package-Contract.md`
- [ ] add broader validation, query, Web API, and Web rendering coverage after first real cultural-note package is available

### 7.9 Unified Learning Search

- [x] define unified search result model
- [x] implement cross-content search endpoint
- [x] extend Web search experience without replacing existing word search
- [x] support CEFR/content type/category/topic filters at the API level
- [x] add initial application tests for empty query and result projection
- [ ] add broader repository/WebApi/Web rendering coverage with seeded learning content

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

- [x] update full/all mobile package export after Web sign-off for grammar, expressions, exercises, courses, exam prep, writing templates, cultural notes, linked references, and unified search inputs
  - Progress: full database and catalog-full packages now include Phase 7 arrays; CEFR slice packages remain word/current-conversation scoped until per-module slice validation is added.
- [x] refactor MAUI navigation after Web model is stable
  - Progress: shell navigation now exposes Learn, Practice, Speak, Prepare, and Resources surfaces with a Learning Portal hub.
- [x] add MAUI dynamic list/detail/search surfaces backed by local Catalog query services
  - Progress: generic list/detail pages cover Grammar, Expressions, Exercise Sets, Courses, Exam Prep, Writing Templates, Cultural Notes, and Talk Topics; unified learning search has a mobile route.
- [x] update full-replacement local package apply path for Phase 7 content tables
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

## Backlog Maintenance Rule

Before starting a new implementation slice:

- move the selected items to `[-]`
- keep unrelated items unchanged
- mark completed work with `[x]`
- add any newly discovered tasks in the correct workstream

This file should remain the main execution checklist for the project.
