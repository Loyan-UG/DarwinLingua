# Darwin Lingua

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-Cross--Platform-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/dotnet/maui/)
[![SQLite](https://img.shields.io/badge/SQLite-Phase%201%20Local%20Storage-003B57?logo=sqlite&logoColor=white)](https://www.sqlite.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Future%20Server%20Default-336791?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Architecture](https://img.shields.io/badge/Architecture-Modular%20Monolith-0A7EA4)](#architecture)
[![Status](https://img.shields.io/badge/Status-Foundation%20In%20Progress-F39C12)](#current-status)

Darwin Lingua is a modular language-learning platform built with `.NET 10` and `.NET MAUI`.

The first learner-facing product is **Darwin Deutsch**, a German-learning application focused on practical vocabulary and real-life language support for immigrants, newcomers, foreign workers, and other serious learners.

## Current Status

- `Phase 1 MVP implementation`: `Completed`
  - local-first MAUI app, import pipeline, browse/search/detail flows, favorites, lightweight user state, localization, and TTS
- `Phase 1 release validation`: `In Progress`
  - automated validation is complete; manual device validation for offline behavior, English UI, German UI, and TTS remains open
- `Phase 2 practice and review`: `In Progress`
  - practice tab/navigation, overview screen, flashcard and quiz session UI, answer feedback, session summary, due-aware review queue, recent activity, learning progress snapshot, answer submission, and persisted scheduling/attempt history are implemented
- `Phase 3 enhanced lexical intelligence`: `In Progress`
  - lexical usage/context labels, grammar notes, collocations, word families, and synonym/antonym relations now flow from imported content into a richer word-detail screen, and the main learner-facing mobile screens now share a cleaner visual hierarchy
- `Phase 5 server content distribution`: `Planned`
  - the first backend foundation now includes a Web API host, PostgreSQL-backed manifest persistence for client products/content streams/published packages, and local PostgreSQL/Web API development templates; package payload delivery and publishing workflows are next
- `Phase 4 support resources`: `Planned`
  - resource discovery and non-lexical support content remain future work

## Product Direction

Darwin Lingua is designed as a multi-product platform, not as a one-off single-language app.

Current product:

- Darwin Deutsch

Possible future products:

- Darwin English
- Darwin Arabic
- Darwin Turkish
- Darwin Persian

Phase 1 remains intentionally focused on a local-first, vocabulary-first German product.

## Phase 1 Focus

### Included

- CEFR-based browsing
- topic-based browsing
- German lemma search
- multilingual meanings
- example sentences
- one or two selected meaning languages
- favorites
- lightweight user word state
- platform text-to-speech
- SQLite local storage
- JSON-based content import
- offline-capable core experience
- English and German UI localization

### Deferred

- spaced repetition
- quiz/review engine
- grammar engine
- collocations and lexical relations
- cloud sync
- web application
- public Web API
- admin editing workflows
- support-resource directory
- import merge/update behavior

## Architecture

The intended architecture is:

- modular monolith
- clean architecture
- bounded contexts
- shared business core
- local-first Phase 1 delivery

Active contexts:

- `Catalog`
- `Learning`
- `ContentOps`
- `Localization`
- `Practice` (early Phase 2 foundation)

Deferred but designed:

- `Resource Discovery`
- `Publishing` / `Distribution` (Phase 5 foundation direction)

## Storage Direction

For the Phase 1 MAUI product, `SQLite` is the correct primary database.

For the planned server-backed content model:

- `PostgreSQL` is the preferred shared-content source of truth
- the `Web API` becomes the mobile-facing content-distribution layer
- mobile `SQLite` remains the runtime and offline database

## Engineering Standards

The project has explicit implementation standards in:

- [Engineering Standards](docs/35-Engineering-Standards.md)
- [Phase 1 Release Checklist](docs/43-Phase-1-Release-Checklist.md)
- [Phase 1 Manual Validation Worksheet](docs/44-Phase-1-Manual-Validation-Worksheet.md)
- [Phase 1 Release Notes Template](docs/45-Phase-1-Release-Notes-Template.md)
- [Phase 2 Practice Validation Worksheet](docs/46-Phase-2-Practice-Validation-Worksheet.md)
- [Phase 3 Mobile UX Validation Worksheet](docs/47-Phase-3-Mobile-UX-Validation-Worksheet.md)
- [Mobile Validation Bundle Runbook](docs/48-Mobile-Validation-Bundle-Runbook.md)

Important rules include:

- null handling is mandatory
- code should be production quality, not throwaway prototype code
- classes, methods, and important logic blocks require English comments
- all user-facing strings must come from localization resources
- the UI must support English and German from day one
- the default UI language should follow the device language where supported
- the user must be able to override UI language in settings
- UI/UX quality is a core requirement, not a polish-only concern

## Documentation Map

### Start Here

- [Documentation Index](docs/00-Documentation-Index.md)
- [Product Vision](docs/01-Product-Vision.md)
- [Product Scope](docs/02-Product-Scope.md)
- [Product Phases](docs/03-Product-Phases.md)
- [Implementation Backlog](docs/04-Implementation-Backlog.md)

### Product and Planning

- [Early Product Decisions](docs/21-Early-Product-Decisions.md)
- [Phase 1 Use Cases](docs/41-Phase-1-Use-Cases.md)
- [Continuation Handoff (for new chats)](docs/42-Continuation-Handoff.md)
- [Phase 1 Release Checklist](docs/43-Phase-1-Release-Checklist.md)
- [Phase 1 Manual Validation Worksheet](docs/44-Phase-1-Manual-Validation-Worksheet.md)
- [Phase 1 Release Notes Template](docs/45-Phase-1-Release-Notes-Template.md)
- [Phase 2 Practice Validation Worksheet](docs/46-Phase-2-Practice-Validation-Worksheet.md)
- [Phase 3 Mobile UX Validation Worksheet](docs/47-Phase-3-Mobile-UX-Validation-Worksheet.md)
- [Mobile Validation Bundle Runbook](docs/48-Mobile-Validation-Bundle-Runbook.md)

### Content and Import

- [Content Strategy](docs/11-Content-Strategy.md)
- [Content Package Format](docs/12-Content-Package-Format.md)
- [Entry Structure Example](docs/13-Entry-Structure.json)
- [Import Rules](docs/14-Import-Rules.md)
- [Topic Seed Ideas](docs/15-Topic-Seed-Ideas.md)
- [Import Workflow](docs/34-Import-Workflow.md)

### Domain and Architecture

- [Domain Model](docs/22-Domain-Model.md)
- [Domain Rules](docs/23-Domain-Rules.md)
- [Entity Relationships](docs/24-Entity-Relationships.md)
- [Phase 1 Domain Cut](docs/25-Phase-1-Domain-Cut.md)
- [Bounded Contexts](docs/26-Bounded-Contexts.md)
- [Solution Architecture](docs/31-Solution-Architecture.md)
- [Storage Strategy](docs/32-Storage-Strategy.md)
- [Offline Strategy](docs/33-Offline-Strategy.md)
- [Server Content Distribution](docs/36-Server-Content-Distribution.md)
- [Shared Content Server Domain](docs/37-Shared-Content-Server-Domain.md)

## Recommended Reading Order

1. [Product Vision](docs/01-Product-Vision.md)
2. [Product Scope](docs/02-Product-Scope.md)
3. [Product Phases](docs/03-Product-Phases.md)
4. [Early Product Decisions](docs/21-Early-Product-Decisions.md)
5. [Content Strategy](docs/11-Content-Strategy.md)
6. [Content Package Format](docs/12-Content-Package-Format.md)
7. [Import Rules](docs/14-Import-Rules.md)
8. [Domain Model](docs/22-Domain-Model.md)
9. [Phase 1 Domain Cut](docs/25-Phase-1-Domain-Cut.md)
10. [Bounded Contexts](docs/26-Bounded-Contexts.md)
11. [Solution Architecture](docs/31-Solution-Architecture.md)
12. [Storage Strategy](docs/32-Storage-Strategy.md)
13. [Offline Strategy](docs/33-Offline-Strategy.md)
14. [Server Content Distribution](docs/36-Server-Content-Distribution.md)
15. [Import Workflow](docs/34-Import-Workflow.md)
16. [Phase 1 Use Cases](docs/41-Phase-1-Use-Cases.md)
17. [Implementation Backlog](docs/04-Implementation-Backlog.md)
18. [Phase 1 Release Checklist](docs/43-Phase-1-Release-Checklist.md)
19. [Phase 1 Manual Validation Worksheet](docs/44-Phase-1-Manual-Validation-Worksheet.md)
20. [Phase 1 Release Notes Template](docs/45-Phase-1-Release-Notes-Template.md)
21. [Phase 2 Practice Validation Worksheet](docs/46-Phase-2-Practice-Validation-Worksheet.md)
22. [Phase 3 Mobile UX Validation Worksheet](docs/47-Phase-3-Mobile-UX-Validation-Worksheet.md)
23. [Mobile Validation Bundle Runbook](docs/48-Mobile-Validation-Bundle-Runbook.md)

## Repository Structure

```text
DarwinLingua/
├─ docs/
├─ src/
│  ├─ Apps/
│  ├─ BuildingBlocks/
│  └─ Modules/
├─ tests/
├─ tools/
├─ assets/
└─ DarwinLingua.slnx
```

## Getting Started

Prerequisites:

- `.NET 10 SDK`
- `.NET MAUI` workloads
- Visual Studio version with `.slnx` and MAUI support

Optional developer tooling:

- `dotnet tool restore` for local EF Core migration commands

Common local migration commands:

```text
dotnet tool restore
dotnet tool run dotnet-ef migrations add <MigrationName> --project src/BuildingBlocks/DarwinLingua.Infrastructure --context DarwinLinguaDbContext --output-dir Persistence/Migrations
```

Open:

```text
DarwinLingua.slnx
```

## Working Rule

Implementation should follow the backlog:

- [Implementation Backlog](docs/04-Implementation-Backlog.md)

That file is the execution checklist for Phase 1 and should be updated as work progresses.
