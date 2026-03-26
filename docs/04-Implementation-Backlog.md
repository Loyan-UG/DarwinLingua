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

### 3. Architecture and Project Wiring

- [-] decide and lock the canonical Phase 1 project/reference structure
- [x] add real project references between Domain, Application, Infrastructure, and hosts
- [x] create composition roots for MAUI and ImportTool
- [x] define dependency injection registration conventions
- [x] add centralized shared infrastructure for Phase 1 persistence
- [x] add shared abstractions only where they provide clear value

### 4. Localization Foundation

- [x] add English UI resource files
- [x] add German UI resource files
- [x] create localization access pattern for the MAUI app
- [-] ensure all user-facing strings come from resources
- [x] implement device-language-based default culture selection
- [x] implement user override of UI language in settings
- [x] persist selected UI language locally

### 5. UI and UX Foundation

- [x] define application navigation structure
- [ ] define design tokens and styling foundations
- [-] replace default template screens with real product screens
- [ ] establish reusable UI components for list items, filters, and detail sections
- [-] define empty/loading/error states
- [ ] define accessibility baseline for typography, contrast, and touch targets

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
- [-] enforce aggregate invariants
- [-] enforce uniqueness and relationship constraints

### 9. Learning Domain Implementation

- [x] implement `UserLearningProfile`
- [x] implement `UserFavoriteWord`
- [x] implement `UserWordState`
- [-] enforce separation between content and user state
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

- [-] implement home/browse screen
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
- [!] resolve local Windows application-control policy issue blocking some test assemblies and transitive dependencies; current default solution test run excludes `Learning.Domain.Tests`, `Catalog.Domain.Tests`, `Localization.Domain.Tests`, `ContentOps.Application.Tests`, and SQLite-backed infrastructure tests

### 15. Release Readiness

- [ ] validate offline behavior on target platforms
- [ ] validate English UI
- [ ] validate German UI
- [ ] validate data initialization from clean install
- [ ] validate import of sample content package
- [ ] validate performance on realistic starter datasets
- [ ] define release checklist for Phase 1 MVP

---

## Phase 2 Backlog Placeholder

Phase 2 focuses on turning content into repeatable learning behavior.

Planned areas:

- review engine
- flashcards
- quiz modes
- spaced repetition
- recent activity and progress

Detailed task planning should be added only after Phase 1 is stable.

---

## Phase 3 Backlog Placeholder

Phase 3 focuses on richer lexical intelligence.

Planned areas:

- word families
- collocations
- lexical relations
- grammar notes
- richer usage metadata

---

## Phase 4 Backlog Placeholder

Phase 4 focuses on migrant-support resource discovery.

Planned areas:

- support resources
- category and location search
- saved useful resources
- admin-managed resource updates

---

## Phase 5 Backlog Placeholder

Phase 5 focuses on server-backed platform expansion.

Planned areas:

- Web API
- web application
- admin application
- accounts and sync
- analytics
- monetization options

---

## Backlog Maintenance Rule

Before starting a new implementation slice:

- move the selected items to `[-]`
- keep unrelated items unchanged
- mark completed work with `[x]`
- add any newly discovered tasks in the correct workstream

This file should remain the main execution checklist for the project.
