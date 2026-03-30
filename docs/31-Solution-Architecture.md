# Solution Architecture

## Purpose

This document defines the canonical Phase 1 solution architecture for Darwin Lingua.

It records the structure that is already implemented in the repository and should now be treated as the locked baseline for Phase 1 work.

The architecture must continue to satisfy these goals:

- support Phase 1 delivery without overengineering
- preserve clean domain boundaries
- keep the solution modular inside a single deployable codebase
- maximize reuse across MAUI and future hosts
- support structured content import and local-first storage
- keep infrastructure concerns out of the domain model
- keep UI concerns out of the application core
- keep a clean path from local seed delivery to future server-authored content distribution

This document builds on:

- `22-Domain-Model.md`
- `23-Domain-Rules.md`
- `24-Entity-Relationships.md`
- `25-Phase-1-Domain-Cut.md`
- `26-Bounded-Contexts.md`

---

# 1. Architectural Style

## 1.1 Recommended Style

The solution uses:

- modular monolith
- clean architecture principles
- bounded-context-oriented project slices
- shared building blocks for cross-cutting concerns
- multiple delivery hosts

This means Darwin Lingua remains one product solution, but internal boundaries are enforced with separate projects and controlled references.

---

## 1.2 Why This Is the Right Choice

Phase 1 does not need microservices or a server-first topology.

The current modular monolith is the correct shape because it provides:

- simpler development
- simpler local deployment
- shared domain and application logic across hosts
- easier debugging and refactoring
- lower operational cost
- a clean path to future API/web/admin expansion if needed

---

## 1.3 Core Rule

The structure should stay modular for clarity, not for ceremony.

Add a new project only when it protects a real boundary, removes a real coupling problem, or provides a clear host/runtime separation.

---

# 2. Architectural Layers

The solution follows these logical layers:

- Domain
- Application
- Infrastructure
- Presentation / Delivery

---

## 2.1 Domain Layer

The Domain layer contains:

- entities
- value objects where they add real clarity
- enums
- aggregate rules
- domain services where genuinely needed
- business invariants

The Domain layer must not contain:

- EF Core configuration
- database access code
- file-system access
- import file parsing infrastructure
- MAUI UI logic
- platform APIs
- resource files
- host-specific orchestration

---

## 2.2 Application Layer

The Application layer contains:

- commands
- queries
- handlers
- DTOs
- validation
- use-case orchestration
- service contracts
- transaction boundaries where applicable

Application may depend on Domain and shared building blocks.

Application must not depend on MAUI, platform SDKs, or persistence implementations.

---

## 2.3 Infrastructure Layer

Infrastructure contains:

- EF Core persistence
- database context
- migrations
- entity configurations
- seed workflows
- import parser implementations
- storage/file services
- time and environment adapters
- technical service implementations

Infrastructure depends on Application, Domain, and shared building blocks.

---

## 2.4 Presentation / Delivery Layer

Phase 1 delivery hosts are:

- `DarwinDeutsch.Maui`
- `DarwinLingua.ImportTool`

Future server/web hosts are planned but not yet implemented:

- `DarwinLingua.WebApi`
- `DarwinLingua.Admin`
- `DarwinLingua.Web`

These hosts compose application and infrastructure services for a specific runtime and UX.

---

# 3. Canonical Phase 1 Solution Structure

## 3.1 Solution-Level Layout

The canonical repository structure for Phase 1 is:

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

---

## 3.2 Host Projects

Phase 1 host projects are:

- `src/Apps/DarwinDeutsch.Maui`
- `src/Apps/DarwinLingua.ImportTool`

### `DarwinDeutsch.Maui`

Responsibility:

- learner-facing UI
- navigation and shell
- page/view-model orchestration
- MAUI resource integration
- platform text-to-speech adapters
- startup composition root

### `DarwinLingua.ImportTool`

Responsibility:

- operator-facing content import workflow
- import execution host
- package summary/report output
- startup composition root for import scenarios

Import remains a separate host because it is an operational workflow, not a normal learner-app feature.

---

## 3.3 Shared Building Blocks

Phase 1 shared building blocks are:

- `src/BuildingBlocks/DarwinLingua.SharedKernel`
- `src/BuildingBlocks/DarwinLingua.Contracts`
- `src/BuildingBlocks/DarwinLingua.Infrastructure`

### `DarwinLingua.SharedKernel`

Contains minimal cross-cutting primitives that are safe to share across bounded contexts, such as base abstractions and common low-level domain/application helpers.

### `DarwinLingua.Contracts`

Contains cross-module contracts that are intentionally shared and stable enough to avoid host-to-module coupling through implementation details.

### `DarwinLingua.Infrastructure`

Contains shared infrastructure building blocks that are not owned by a single bounded context, including the shared EF Core database foundation and common persistence/runtime services.

This project is not a place for domain rules or feature logic.

Operational support files for the future server path may live under:

- `tools/Server/Postgres`
- `tools/Server/Config`

---

## 3.4 Bounded-Context Modules

The currently active bounded contexts are:

- `Catalog`
- `Learning`
- `ContentOps`
- `Localization`
- `Practice`

Each active context uses a three-project split:

- `*.Domain`
- `*.Application`
- `*.Infrastructure`

Current module projects:

- `src/Modules/Catalog/DarwinLingua.Catalog.Domain`
- `src/Modules/Catalog/DarwinLingua.Catalog.Application`
- `src/Modules/Catalog/DarwinLingua.Catalog.Infrastructure`
- `src/Modules/Learning/DarwinLingua.Learning.Domain`
- `src/Modules/Learning/DarwinLingua.Learning.Application`
- `src/Modules/Learning/DarwinLingua.Learning.Infrastructure`
- `src/Modules/ContentOps/DarwinLingua.ContentOps.Domain`
- `src/Modules/ContentOps/DarwinLingua.ContentOps.Application`
- `src/Modules/ContentOps/DarwinLingua.ContentOps.Infrastructure`
- `src/Modules/Localization/DarwinLingua.Localization.Domain`
- `src/Modules/Localization/DarwinLingua.Localization.Application`
- `src/Modules/Localization/DarwinLingua.Localization.Infrastructure`
- `src/Modules/Practice/DarwinLingua.Practice.Domain`
- `src/Modules/Practice/DarwinLingua.Practice.Application`
- `src/Modules/Practice/DarwinLingua.Practice.Infrastructure`

Deferred contexts already reserved in the solution structure but not implemented are:

- `ResourceDirectory`

Future server-side bounded contexts expected to appear when Phase 5 starts are:

- `Publishing`
- `Distribution`

Those placeholders should stay deferred until real scope justifies them.

---

# 4. Module Responsibilities

## 4.1 Catalog

Owns lexical content and browse/search details, including:

- word entries
- senses
- translations
- examples
- topics
- topic localizations
- CEFR/topic/search/detail query models

Catalog must not own learner-specific mutable progress state.

---

## 4.2 Learning

Owns learner-specific local state, including:

- favorite words
- user word state
- learning preferences

Learning must stay persistence-separated from catalog-content ownership boundaries.

---

## 4.3 ContentOps

Owns structured content import workflows, including:

- package parsing
- import orchestration
- duplicate/package validation
- import reporting

It is an operational context, not a learner-facing UI context.

---

## 4.4 Localization

Owns application-language preferences and localization-related workflows that belong in the application core.

UI resource files still live in the MAUI host.

---

# 5. Dependency Direction

## 5.1 Canonical Rules

The dependency direction for each active bounded context is:

- `Module.Domain` -> `DarwinLingua.SharedKernel` only when needed
- `Module.Application` -> `Module.Domain`, `DarwinLingua.SharedKernel`, `DarwinLingua.Contracts`
- `Module.Infrastructure` -> `Module.Application`, `Module.Domain`, `DarwinLingua.SharedKernel`, `DarwinLingua.Contracts`, `DarwinLingua.Infrastructure`

The hosts currently reference the module application and infrastructure projects they compose:

- `DarwinDeutsch.Maui` -> active module `*.Application` and `*.Infrastructure` projects plus shared infrastructure
- `DarwinLingua.ImportTool` -> active module `*.Application` and `*.Infrastructure` projects plus shared infrastructure

This host-level direct infrastructure reference is acceptable because composition roots live in the hosts.

---

## 5.2 Forbidden Dependencies

The following remain forbidden:

- Domain -> Application
- Domain -> Infrastructure
- Domain -> host/UI projects
- Application -> host/UI projects
- shared building blocks -> host/UI projects
- one bounded context reaching into another context's infrastructure implementation

Cross-context collaboration should happen through contracts, shared abstractions, or deliberate domain/application references that reflect real business coupling.

---

# 6. Internal Organization Guidance

## 6.1 Domain Projects

Domain projects should organize around aggregates and rules, not technical patterns.

Typical contents:

- entities
- enums
- value objects
- domain exceptions
- invariant logic

---

## 6.2 Application Projects

Application projects should organize around use cases.

Typical contents:

- commands
- queries
- handlers
- DTOs
- validators
- service abstractions

Feature-first folders are preferred over generic utility buckets.

---

## 6.3 Infrastructure Projects

Infrastructure projects should organize around technical capability areas.

Typical contents:

- persistence
- configurations
- migrations
- seeding
- import services
- system adapters
- dependency injection

---

## 6.4 Host Projects

Host projects should contain only host-specific UI/runtime composition concerns.

Typical MAUI contents:

- pages
- controls
- resources
- services
- navigation
- startup wiring
- platform integrations

Typical ImportTool contents:

- command-line or operator workflow entrypoints
- reporting/output formatting
- host startup wiring

---

# 7. Persistence Strategy

## 7.1 Database Direction

Phase 1 uses SQLite for the local-first application experience.

The architecture remains ready for future server-side expansion, where PostgreSQL is still the preferred default direction for shared backend workloads if those are introduced later.

For the planned server-backed content model:

- PostgreSQL is the shared-content source of truth
- the Web API exposes manifests and content packages
- mobile SQLite remains the runtime/offline store

---

## 7.2 DbContext Strategy

Phase 1 uses a shared application database foundation with one EF Core database context hosted in shared infrastructure and extended through module configurations.

This keeps local persistence centralized without collapsing bounded-context ownership.

Startup initialization must continue to use EF Core migrations.

---

## 7.3 Mapping Strategy

Entity configuration stays in infrastructure, not in domain entities.

One configuration class per entity or tightly related small entity group is still the preferred default.

---

# 8. Import Architecture

The Phase 1 import flow remains:

1. read content package input
2. parse JSON into import models
3. validate package structure and identifiers
4. apply domain/application rules
5. detect duplicates and package conflicts
6. persist valid package and content rows
7. return an operator-facing summary

Ownership split:

- parsing and file access -> Infrastructure
- orchestration and use-case flow -> Application
- import execution host -> `DarwinLingua.ImportTool`

When the server-backed content path is introduced, the same content-package model should evolve into:

1. server-side import into the shared PostgreSQL catalog
2. Web API distribution of versioned content packages
3. mobile application of those packages into local SQLite

---

# 9. MAUI Architecture Direction

The MAUI host should continue to consume application queries, commands, DTOs, and UI-safe mapped models.

It should not manipulate persistence internals or domain graphs directly.

Platform text-to-speech, shell/navigation, and UI resource localization remain host concerns.

For future content updates, the MAUI host should add:

- one mobile-facing update client
- update diagnostics and settings actions
- local package-application orchestration over SQLite

It should still avoid talking to raw database internals directly.

---

# 10. Naming Conventions

The canonical solution and project naming pattern is now:

- solution: `DarwinLingua`
- platform product app: `DarwinDeutsch.Maui`
- cross-product/platform projects: `DarwinLingua.*`

This naming split is intentional:

- `DarwinLingua` names the platform/repository
- `DarwinDeutsch` names the first learner-facing product

Future hosts should follow the same pattern unless there is a strong reason not to.

---

# 11. Testing Alignment

The architecture should continue to support:

- domain tests
- application tests
- infrastructure integration tests
- pragmatic MAUI smoke coverage

Tests should remain aligned to module boundaries rather than collapsing into a single giant test surface.

---

# 12. Anti-Patterns to Avoid

- letting MAUI absorb business logic that belongs in Application or Domain
- treating shared infrastructure as a dumping ground for feature code
- bypassing module boundaries by referencing another module's infrastructure directly
- moving all validation into handlers and leaving aggregates anemic
- introducing deferred contexts or extra projects before real scope exists

---

# 13. Final Architecture Summary

Darwin Lingua is currently implemented as a modular monolith with:

- two host projects
- three shared building-block projects
- five active bounded contexts, each split into Domain/Application/Infrastructure projects
- deferred placeholders for later contexts that are intentionally not implemented yet

This is the locked project/reference structure and should be used as the baseline for the remaining Phase 1 release work and ongoing Phase 2 implementation.

The next architectural expansion should preserve this modular-monolith core while adding a server content-distribution edge:

- server database for shared content
- Web API for package delivery
- local SQLite for mobile runtime and offline access
