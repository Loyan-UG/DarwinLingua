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
- [ ] validate English UI
- [ ] validate German UI
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

- learners can prepare for real German situations using scenario lessons
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
- [x] ensure word details, examples, scenario lessons, and conversation starters can display two meaning languages consistently
- [x] add validation rules for missing translations in one of the selected meaning languages
- [x] add fallback rules when the secondary meaning language is unavailable
- [x] add tests for dual-language selection and rendering decisions
- [x] add content-quality checks that identify incomplete dual-language coverage

---

### 3. Scenario-Based Learning

#### Goal

Add practical real-life scenario lessons that help learners use German in concrete situations.

#### Initial Scenario Categories

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

#### Scenario Content Model

Each scenario should support:

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

- [x] define `ScenarioLesson` content contract
- [x] define `ScenarioDialogueTurn` content contract
- [x] define `ScenarioPhrase` or `UsefulPhrase` content contract
- [x] define `ScenarioQuestion` and `ScenarioAnswer` content contract
- [x] define scenario CEFR-level and topic mapping rules
- [x] define import validation rules for scenario content
- [x] implement scenario import pipeline support
- [x] persist scenario lessons in the shared content model
- [x] expose scenario lessons through application queries
- [x] add scenario list and detail screens in MAUI
- [x] add scenario list and detail pages in the web app when appropriate
- [ ] add tests for scenario import, persistence, query, and rendering
  - Progress: import, persistence, application query, mobile package export, Web API build, and web controller language-selection tests are covered; keep open until Razor/page rendering is explicitly tested.
- [x] add initial sample scenarios for A1/A2 learners

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
- [x] support starter packs inside scenario lessons
- [x] support standalone starter packs
  - Progress: content contract, parser model support, import-boundary validation, persistence, catalog queries, WebApi endpoints, web/mobile UI, and mobile package export for `conversationStarterPacks` are implemented.
- [x] add mobile browsing and detail UI for starter packs
- [ ] connect starter packs to event preparation packs
  - Progress: starter packs already accept and persist `linkedEventPreparationPackSlugs`; event-preparation packs now accept reciprocal `linkedConversationStarterPackSlugs` at the content-contract/parser/validation boundary. Keep open until event-preparation packs are persisted and queryable.
- [ ] add tests for starter filtering and dual-language rendering
  - Progress: parser contract, invalid-contract validation, persistence, query filtering, scenario-link query coverage, dual-language query coverage, and web/mobile rendering implementation exist; keep open until explicit UI rendering tests are implemented.
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

- [ ] define `RoleplayScenario` content contract
- [ ] define role labels such as learner, doctor, teacher, colleague, organizer, partner
- [ ] define scripted turn sequence model
- [ ] define answer-choice model for early MVP
- [ ] define static feedback model
- [ ] add roleplay launch from scenario detail
- [ ] persist basic roleplay attempts if useful
- [ ] add tests for roleplay sequence behavior
- [ ] keep AI roleplay feedback out of MVP unless cost and safety boundaries are decided

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

- [ ] define `ResourceDirectory` and `ConversationEvent` boundaries
- [ ] define event category list
- [ ] define city/location model without exposing precise private user locations
- [ ] implement read-only event directory model
- [ ] implement event list query with filters by city, level, language, online/offline, and price type
- [ ] implement event detail query
- [ ] add mobile event directory screens
- [ ] add web event directory pages
- [ ] support external registration links
- [ ] support manual admin-managed event creation first
- [ ] add validation for stale event data
- [ ] add tests for filtering and visibility rules

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
- [x] allow preparation packs to link to existing scenario lessons at the content-contract/parser/validation boundary
- [x] allow preparation packs to link to vocabulary entries at the content-contract/parser/validation boundary
- [x] allow preparation packs to link to conversation starter packs at the content-contract/parser/validation boundary
- [x] persist `EventPreparationPack` catalog records
- [x] query `EventPreparationPack` catalog records
- [x] expose read-only WebApi endpoints for event preparation packs
- [ ] show preparation packs on event detail screens
- [ ] add `Prepare for this event` action
- [ ] track preparation-pack completion if useful
- [ ] add tests for event-to-pack mapping
  - Progress: parser coverage, invalid-contract application validation, EF persistence, migration coverage, positive import persistence tests, query filtering, scenario-link query, and detail query tests exist. UI mapping tests remain open until web/mobile screens are added.

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

- [ ] define `OrganizerProfile` domain model
- [ ] define organizer type list: teacher, cafe, club, association, school, company, library, other
- [ ] implement read-only organizer profile pages
- [ ] link events to organizer profiles
- [ ] add admin-managed organizer creation first
- [ ] add organizer claim workflow placeholder
- [ ] add verified organizer badge rules
- [ ] add tests for organizer visibility and event linking

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

- [ ] define organizer account ownership rules
- [ ] define organizer admin role or entitlement boundary
- [ ] implement organizer dashboard authorization
- [ ] implement profile edit workflow
- [ ] implement event create/edit workflow
- [ ] implement event activation/deactivation workflow
- [ ] implement recurring-event support after one-off events work
- [ ] implement basic organizer analytics
- [ ] add tests for authorization and ownership rules

---

### 10. RSVP and Attendance

#### Goal

Let learners express interest or reserve a spot without building a full event-ticketing system.

#### Backlog

- [ ] define `EventRsvp` model
- [ ] support RSVP states: interested, going, cancelled, attended, no-show
- [ ] define capacity and waitlist rules
- [ ] add `I am interested` or `Join` action on event detail
- [ ] show remaining capacity where appropriate
- [ ] expose attendee count without exposing private attendee details publicly
- [ ] show attendee list only to authorized organizers/admins
- [ ] add post-event attendance confirmation
- [ ] add tests for capacity, cancellation, and organizer visibility

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

- [ ] define learner profile model
- [ ] define profile visibility states
- [ ] define public profile projection separate from private profile data
- [ ] implement profile create/edit screen
- [ ] implement profile enable/disable toggle
- [ ] implement profile deletion or anonymization flow
- [ ] add tests for public/private projection
- [ ] add privacy review before release

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

- [ ] define `PartnerRequest` model
- [ ] define match search query model
- [ ] define request states: pending, accepted, declined, cancelled, blocked, expired
- [ ] add rate limits for new users
- [ ] add predefined opener templates
- [ ] add accept/decline flow
- [ ] avoid unrestricted direct messaging in MVP
- [ ] reveal contact details only after mutual consent if this feature is added
- [ ] add tests for request-state transitions and rate limits

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

- [ ] define `UserReport` model
- [ ] define `UserBlock` model
- [ ] define `OrganizerVerification` model
- [ ] define `ListingReview` workflow
- [ ] implement report action from profile, event, and organizer pages
- [ ] implement block action from profile and partner-request flows
- [ ] implement admin moderation queue
- [ ] implement moderation decision audit log
- [ ] add tests for block visibility and request suppression
- [ ] add operational runbook for moderation

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

- [ ] define entitlement keys for scenario and conversation features
- [ ] define entitlement keys for organizer features
- [ ] keep core catalog browse and basic scenario access outside strict premium enforcement
- [ ] add feature gates for advanced scenario packs if needed
- [ ] add feature gates for preparation packs if needed
- [ ] add organizer plan flags without payment integration first
- [ ] add admin entitlement management for organizer plans
- [ ] add tests for feature-gate behavior

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

- [ ] define analytics event names and payloads
- [ ] avoid collecting unnecessary personal data
- [ ] implement privacy-safe analytics boundaries
- [ ] add admin summary views for organizer/event analytics
- [ ] add export only where operationally necessary

---

### 16. Content Seed Priorities

#### First Scenario Packs

- [ ] First language-practice meeting
- [ ] Introducing yourself in simple German
- [ ] Asking someone to speak slowly
- [ ] Asking for correction politely
- [ ] Talking to a doctor
- [ ] Writing to kindergarten or school
- [ ] Calling an office for an appointment
- [ ] Talking to a landlord
- [ ] Workplace small talk
- [ ] Job interview basics

#### First Conversation Starter Packs

- [ ] Conversation cafe starters
- [ ] Workplace lunch starters
- [ ] School/kindergarten parent starters
- [ ] Neighbor small talk starters
- [ ] Online practice meeting starters

#### First Event Directory Seeds

- [ ] manually add a small set of public conversation cafes in selected cities
- [ ] manually add online German practice groups where allowed
- [ ] manually add integration-oriented support resources where appropriate
- [ ] define update/verification date per listing

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
2. Scenario content contract
3. Scenario import and query support
4. Scenario mobile UI
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

- scenario lessons work in at least German + one meaning language
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

## Backlog Maintenance Rule

Before starting a new implementation slice:

- move the selected items to `[-]`
- keep unrelated items unchanged
- mark completed work with `[x]`
- add any newly discovered tasks in the correct workstream

This file should remain the main execution checklist for the project.
