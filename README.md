# Darwin Lingua

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-Cross--Platform-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/dotnet/maui/)
[![SQLite](https://img.shields.io/badge/SQLite-Local%20Storage-003B57?logo=sqlite&logoColor=white)](https://www.sqlite.org/)
[![EF Core](https://img.shields.io/badge/EF%20Core-ORM-6DB33F?logo=.net&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![Architecture](https://img.shields.io/badge/Architecture-Modular%20Monolith-0A7EA4)](#architecture)
[![Approach](https://img.shields.io/badge/Approach-Local--First-1F8A70)](#phase-1-focus)
[![Status](https://img.shields.io/badge/Status-Docs%20%26%20Scaffolding-F39C12)](#current-status)

Darwin Lingua is a modular language-learning platform built with `.NET 10` and `.NET MAUI`.

It is designed as a multi-product ecosystem where each learner-facing product targets a specific language while reusing a shared architectural foundation. The first active product is **Darwin Deutsch**, a German-learning app focused on practical vocabulary for immigrants, newcomers, foreign workers, and other real-world learners.

## Current Status

This repository is currently **documentation-first and scaffold-first**.

- The product, domain, storage, offline, and import strategy are documented in depth.
- The solution structure is scaffolded under `src/`.
- The MAUI app is still close to the default template.
- The import tool is currently a placeholder.
- Test projects exist, but they are still placeholder tests.

In other words: the architectural direction is strong, but the business implementation is still at an early stage.

## Why This Project Exists

Most language apps optimize for generic drills. Darwin Deutsch is intended to optimize for **practical language use in real life**.

Phase 1 starts with a disciplined vocabulary-first product that helps users:

- browse German vocabulary by `CEFR` level
- browse vocabulary by real-life topic
- read meanings in one or two selected meaning languages
- read example sentences
- mark favorites
- keep lightweight local learning state
- use the app offline

The long-term direction expands beyond vocabulary into practice, richer lexical intelligence, and eventually newcomer-support resource discovery.

## Phase 1 Focus

### Included in Phase 1

- German vocabulary entries
- CEFR-based browsing
- topic-based browsing
- multilingual meanings
- example sentences
- one or two selected meaning languages
- platform text-to-speech
- favorites
- lightweight user word state
- local SQLite storage
- JSON-based content import
- offline-capable core experience

### Explicitly Deferred

- spaced repetition
- flashcards and review sessions
- grammar engine
- collocations and lexical relations
- cloud sync
- admin editing workflows
- web app and public API
- migrant support resource directory
- merge/update import behavior

## Product Direction

Darwin Lingua is not intended to remain a single-language app. The platform is designed so future products can reuse the same core foundation, for example:

- Darwin Deutsch
- Darwin English
- Darwin Arabic
- Darwin Turkish
- Darwin Persian

The key design rule is simple: **do not hardcode the platform as German-only even though Phase 1 is German-focused**.

## Architecture

The documented architectural direction is:

- modular monolith
- clean architecture
- bounded contexts
- local-first Phase 1 delivery
- shared business logic across delivery applications
- structured content import instead of ad hoc content editing

### Active Phase 1 Contexts

- `Catalog`
- `Learning`
- `ContentOps`
- `Localization`

### Deferred but Designed

- `Practice`
- `Resource Discovery`

### Important Architectural Notes

- Shared content and user state should remain logically separate.
- Import is an operational context, not a UI shortcut or temporary script concern.
- Phase 1 should stay small on purpose.
- A modular monolith is the right choice here; microservices would be premature.

## Repository Structure

Current repository layout:

```text
DarwinLingua/
├─ docs/
├─ src/
│  ├─ Apps/
│  │  ├─ DarwinDeutsch.Maui/
│  │  └─ DarwinLingua.ImportTool/
│  ├─ BuildingBlocks/
│  │  ├─ DarwinLingua.Contracts/
│  │  └─ DarwinLingua.SharedKernel/
│  └─ Modules/
│     ├─ Catalog/
│     ├─ ContentOps/
│     ├─ Learning/
│     └─ Localization/
├─ tests/
├─ tools/
├─ assets/
└─ DarwinLingua.slnx
```

Note: the documentation sometimes discusses a leaner five-project Phase 1 cut (`Domain`, `Application`, `Infrastructure`, `Maui`, `ImportTool`). The current codebase is organized as **modular context-based scaffolding**, which is acceptable, but the implementation should stay disciplined so the internal boundaries remain clear.

## Documentation Map

The documentation is the strongest part of the repository right now. Start here:

### Overview

- [Product Vision](docs/01-%20Product-Vision.md)
- [Product Scope](docs/02-%20Product-Scope.md)
- [Product Phases](docs/03-%20Product-Phases.md)
- [Early Product Decisions](docs/21-%20Early%20Product%20Decisions.md)

### Content and Import

- [Content Strategy](docs/11-%20Content-Strategy.md)
- [AI Content Format](docs/12-%20AI-Content-Format.md)
- [Entry Structure Example](docs/13-%20Entry%20Structure.json)
- [Import Rules](docs/14-%20Import-Rules.md)
- [Import Workflow](docs/34-%20Import-Workflow.md)
- [Topic Seed Ideas](docs/15-%20Topic-Seed-Ideas.md)

### Domain

- [Domain Model](docs/22-%20Domain-Model.md)
- [Domain Rules](docs/23-%20Domain-Rules.md)
- [Entity Relationships](docs/24-%20Entity-Relationships.md)
- [Phase 1 Domain Cut](docs/25-%20Phase-1-Domain-Cut.md)
- [Bounded Contexts](docs/26-%20Bounded-Contexts.md)

### Architecture and Runtime

- [Solution Architecture](docs/31-%20Solution-Architecture.md)
- [Storage Strategy](docs/32-%20Storage-Strategy.md)
- [Offline Strategy](docs/33-%20Offline-Strategy.md)
- [Phase 1 Use Cases](docs/41-%20Phase-1-Use-Cases.md)

### Repository Reference

- [Documentation Index Draft](docs/00-%20Docs.md)
- [Solution Folder Draft](docs/00-%20Solution%20Folders.md)
- [Structure Draft](docs/00-%20Stracture.md)

## Recommended Reading Order

1. [Product Vision](docs/01-%20Product-Vision.md)
2. [Product Phases](docs/03-%20Product-Phases.md)
3. [Content Strategy](docs/11-%20Content-Strategy.md)
4. [Domain Model](docs/22-%20Domain-Model.md)
5. [Phase 1 Domain Cut](docs/25-%20Phase-1-Domain-Cut.md)
6. [Bounded Contexts](docs/26-%20Bounded-Contexts.md)
7. [Solution Architecture](docs/31-%20Solution-Architecture.md)
8. [Storage Strategy](docs/32-%20Storage-Strategy.md)
9. [Offline Strategy](docs/33-%20Offline-Strategy.md)
10. [Import Workflow](docs/34-%20Import-Workflow.md)
11. [Phase 1 Use Cases](docs/41-%20Phase-1-Use-Cases.md)

## Technology Stack

- `.NET 10`
- `.NET MAUI`
- `EF Core` (architectural target)
- `SQLite` for Phase 1 local storage
- `xUnit` for test projects
- `Visual Studio` solution via `DarwinLingua.slnx`

## Getting Started

### Prerequisites

- `.NET 10 SDK`
- `.NET MAUI` workloads
- `Visual Studio 2026` or another environment that supports `.slnx` and MAUI

### Open the Solution

Open:

```text
DarwinLingua.slnx
```

### Current Reality

You can explore and extend the solution structure now, but you should not expect a feature-complete learner product yet. Most domain, application, infrastructure, import, and test projects are still scaffolds and need real implementation.

## What Is Strong About This Project

- clear product positioning
- disciplined Phase 1 scope
- strong bounded-context thinking
- pragmatic local-first strategy
- import pipeline treated as a first-class workflow
- good separation between shared content and user state

## Known Repository Gaps

At the moment, the repository still has a few documentation and implementation mismatches:

- `docs/02- Product-Scope.md` currently duplicates `Product Vision`
- several docs still reference older file names or an older folder taxonomy
- `docs/14- Import-Rules.md` has Markdown formatting issues
- the source code does not yet implement the documented architecture in depth

## Guiding Principle

The project should remain:

**useful, stable, local-first, and expandable.**
