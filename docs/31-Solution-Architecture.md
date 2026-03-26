# Solution Architecture

## Purpose

This document defines the recommended solution architecture for Darwin Deutsch.

The architecture must satisfy these goals:

- support Phase 1 delivery without overengineering
- preserve clean domain boundaries
- maximize code sharing across MAUI and future web applications
- support structured content import
- remain ready for later API, web, admin, and sync expansion
- keep infrastructure concerns out of the domain model
- keep UI concerns out of the application core

This document builds on:

- `22-Domain-Model.md`
- `23-Domain-Rules.md`
- `24-Entity-Relationships.md`
- `25-Phase-1-Domain-Cut.md`
- `26-Bounded-Contexts.md`

---

# 1. Architectural Style

## 1.1 Recommended Style

The recommended architecture is:

- modular monolith
- clean architecture principles
- domain-centered design
- shared application core
- multiple delivery applications

This means the solution should remain one product solution, but internally modular and disciplined.

---

## 1.2 Why Modular Monolith Is the Right Choice

At this stage, Darwin Deutsch does not need microservices.

A modular monolith is the correct choice because it provides:

- simpler development
- simpler deployment
- easier debugging
- easier refactoring
- lower operational cost
- faster early delivery

At the same time, if the internal boundaries are respected, future extraction remains possible if ever needed.

---

## 1.3 Core Rule

The solution must be designed for modular clarity, not for artificial technical complexity.

Do not create distributed architecture for a product that is still validating its first serious release.

---

# 2. Architectural Layers

The solution should follow these logical layers:

- Domain
- Application
- Infrastructure
- Presentation / Delivery

---

## 2.1 Domain Layer

The Domain layer contains:

- entities
- value objects
- enums
- domain rules
- domain services where genuinely needed
- aggregate logic
- business invariants

The Domain layer must not contain:

- database code
- EF Core configuration
- HTTP logic
- MAUI UI logic
- file system logic
- JSON parsing infrastructure
- platform APIs
- view models

---

## 2.2 Application Layer

The Application layer contains:

- use cases
- commands
- queries
- handlers
- DTOs
- validation
- mapping contracts
- service abstractions
- orchestration logic
- transaction boundaries where applicable

The Application layer may depend on Domain.

The Application layer must not depend on MAUI, Web, or API frameworks.

---

## 2.3 Infrastructure Layer

The Infrastructure layer contains:

- EF Core persistence
- repository implementations if used
- database context
- migrations
- file import parser implementations
- time providers
- storage providers
- logging adapters where needed
- future API client integrations
- future sync implementations

Infrastructure depends on Application and Domain contracts.

---

## 2.4 Presentation / Delivery Layer

This includes:

- MAUI app
- future Web app
- future API host
- future Admin host
- import utility host if separated

These layers depend on Application, not directly on persistence internals.

---

# 3. Solution-Level Design Goals

The solution should be organized so that:

- the Domain model is shared
- use cases are shared
- persistence is centralized
- UI-specific code stays in UI projects
- platform-specific integrations are isolated
- import workflows are not buried inside the app UI
- later expansion to web and API is natural

---

# 4. Recommended Solution Structure

## 4.1 Recommended High-Level Project Set

Recommended initial long-term project structure:

- DarwinDeutsch.Domain
- DarwinDeutsch.Application
- DarwinDeutsch.Infrastructure
- DarwinDeutsch.Maui
- DarwinDeutsch.ImportTool
- DarwinDeutsch.SharedKernel (optional, only if truly needed later)
- DarwinDeutsch.WebApi (later)
- DarwinDeutsch.Web (later)
- DarwinDeutsch.Admin (later)

---

## 4.2 Phase 1 Actual Project Cut

For Phase 1, the practical project set should be:

- DarwinDeutsch.Domain
- DarwinDeutsch.Application
- DarwinDeutsch.Infrastructure
- DarwinDeutsch.Maui
- DarwinDeutsch.ImportTool

### Optional in Phase 1

- DarwinDeutsch.Contracts

Only add this if you truly need a separate contracts project.
Do not add it by default unless there is a real reason.

---

# 5. Project Responsibilities

## 5.1 DarwinDeutsch.Domain

### Responsibility

Contains the core domain model and business rules.

### Should Contain

- WordEntry
- WordSense
- SenseTranslation
- ExampleSentence
- ExampleTranslation
- Topic
- TopicLocalization
- WordTopic
- Language
- UserLearningProfile
- UserFavoriteWord
- UserWordState
- ContentPackage
- ContentPackageEntry
- enums
- value objects where useful
- domain exceptions if needed

### Must Not Contain

- DbContext
- repositories backed by EF
- DTOs for UI
- JSON file parsing
- MAUI services
- localization resource files
- platform TTS code

---

## 5.2 DarwinDeutsch.Application

### Responsibility

Contains use cases and orchestration.

### Should Contain

- commands
- queries
- handlers
- request/response DTOs
- validators
- interfaces for persistence/service dependencies
- mapping definitions or mapping profiles if appropriate
- import workflow orchestration contracts
- query models for screens
- feature organization by bounded context

### Example Feature Areas

- Content/Queries
- Content/Commands
- LearningProfile/Commands
- LearningProfile/Queries
- Favorites/Commands
- UserWordState/Commands
- Import/Commands
- Import/Services

### Must Not Contain

- EF Core implementation
- UI controls
- HTTP controllers
- MAUI page logic

---

## 5.3 DarwinDeutsch.Infrastructure

### Responsibility

Implements technical concerns.

### Should Contain

- AppDbContext
- EF Core entity configurations
- migrations
- repository/query implementations
- import JSON parser
- duplicate detection persistence helpers
- file reading services
- time/date provider implementations
- future storage implementations
- future sync infrastructure

### Recommended Internal Areas

- Persistence
- Persistence/Configurations
- Persistence/Seed
- Services
- Importing
- Options

### Must Not Contain

- page view models
- MAUI-specific UI behavior
- business rules that belong in Domain or Application

---

## 5.4 DarwinDeutsch.Maui

### Responsibility

Main end-user application for Android, iOS, and Windows.

### Should Contain

- App.xaml and startup
- pages/views
- MAUI view models
- UI state handling
- platform TTS adapters
- localization resource integration
- navigation
- shell
- dependency registration for UI concerns
- device/platform-specific code

### Must Not Contain

- EF Core entity design
- domain rules
- duplicated business logic
- import parsing logic beyond UI triggering/orchestration

---

## 5.5 DarwinDeutsch.ImportTool

### Responsibility

A small dedicated host/application for importing structured content files.

### Why Separate

This is the correct choice because import is an operational workflow, not a normal end-user mobile feature.

It keeps the MAUI app cleaner and gives you a better place for:

- manual imports
- validation-only runs
- package reporting
- admin/operator usage

### Possible Forms

This can be one of the following:

- console app
- desktop utility
- minimal internal tool
- later admin module

### Recommendation

For Phase 1, use a simple console or small desktop-oriented utility.
Do not bury content import inside the end-user MAUI app unless you have a very strong reason.

---

# 6. Project Dependency Direction

## 6.1 Allowed Dependency Flow

Recommended dependency flow:

- DarwinDeutsch.Maui -> DarwinDeutsch.Application
- DarwinDeutsch.Maui -> DarwinDeutsch.Infrastructure (only through startup composition root if needed)
- DarwinDeutsch.ImportTool -> DarwinDeutsch.Application
- DarwinDeutsch.ImportTool -> DarwinDeutsch.Infrastructure
- DarwinDeutsch.Infrastructure -> DarwinDeutsch.Application
- DarwinDeutsch.Application -> DarwinDeutsch.Domain

### Optional

- DarwinDeutsch.Infrastructure -> DarwinDeutsch.Domain

This is normal because persistence implements the domain model.

---

## 6.2 Forbidden Dependencies

The following should be forbidden:

- Domain -> Application
- Domain -> Infrastructure
- Domain -> Maui
- Application -> Maui
- Application -> Web
- Application -> ImportTool UI
- Infrastructure -> Maui
- Infrastructure -> Web UI

---

# 7. Composition Root Strategy

## 7.1 Principle

Dependency injection composition should happen in the host projects.

For Phase 1, the main composition roots are:

- DarwinDeutsch.Maui
- DarwinDeutsch.ImportTool

### Rule

Do not register DI directly inside Domain.

Application may define interfaces.
Infrastructure provides implementations.
Host wires them together.

---

# 8. Bounded Context to Project Mapping

Bounded contexts do not require one project each in Phase 1.

That would be premature.

Instead, use logical modularity inside Domain/Application/Infrastructure.

---

## 8.1 Domain Project Internal Organization

Recommended folder structure inside `DarwinDeutsch.Domain`:

- Content
- LearningProfile
- ContentOperations
- Localization
- Common

### Example

- Content/Entities
- Content/Enums
- LearningProfile/Entities
- ContentOperations/Entities
- Localization/Entities
- Common/Abstractions
- Common/Enums
- Common/Exceptions
- Common/ValueObjects

---

## 8.2 Application Project Internal Organization

Recommended folder structure inside `DarwinDeutsch.Application`:

- Content
- LearningProfile
- Favorites
- UserWordStates
- Importing
- Common

### Under Each Feature

- Commands
- Queries
- DTOs
- Validators
- Handlers

### Example

- Content/Queries/GetWordDetails
- Content/Queries/SearchWords
- Content/Queries/GetWordsByTopic
- Content/Queries/GetWordsByCefr
- LearningProfile/Commands/UpdatePreferences
- Favorites/Commands/ToggleFavorite
- UserWordStates/Commands/MarkWordKnown
- Importing/Commands/ImportContentPackage

---

## 8.3 Infrastructure Project Internal Organization

Recommended folder structure inside `DarwinDeutsch.Infrastructure`:

- Persistence
- Persistence/Configurations
- Persistence/Seed
- Importing
- Services
- DependencyInjection

### Example

- Persistence/AppDbContext.cs
- Persistence/Configurations/WordEntryConfiguration.cs
- Persistence/Configurations/TopicConfiguration.cs
- Importing/JsonContentPackageReader.cs
- Importing/ContentImportService.cs
- Services/SystemClock.cs
- DependencyInjection/ServiceCollectionExtensions.cs

---

## 8.4 Maui Project Internal Organization

Recommended folder structure inside `DarwinDeutsch.Maui`:

- Views
- ViewModels
- Resources
- Services
- Navigation
- Extensions
- Platforms
- DependencyInjection

### Example

- Views/Content
- Views/Settings
- Views/Favorites
- ViewModels/Content
- ViewModels/Settings
- Services/TextToSpeech
- Resources/Localization
- DependencyInjection/MauiServiceCollectionExtensions.cs

---

# 9. Domain Modeling Strategy in Code

## 9.1 Domain Entities Should Be Persistence-Aware but Not Persistence-Owned

Entities should be modeled cleanly and practically.

Do not force bizarre purity if it makes EF mapping painful for no benefit.

### Recommendation

Use domain entities that EF Core can map cleanly, while keeping EF configuration in Infrastructure.

---

## 9.2 Value Objects

Use value objects selectively.

Good candidates in the future:

- PublicId
- LanguageCode
- CefrLevel
- TopicKey

But do not create value objects everywhere in Phase 1 if they slow delivery.

### Rule

Prefer clarity over decorative abstraction.

---

## 9.3 Enums vs Lookup Tables

For Phase 1:

- CEFR level -> enum or strongly controlled value type
- part of speech -> enum
- publication status -> enum
- content source type -> enum

This is simpler than lookup tables.

---

# 10. Persistence Strategy

## 10.1 Database Choice

For Phase 1, a relational database is the correct choice.

### Recommendation

Use SQLite first for local/desktop/mobile-friendly Phase 1.

This gives you:

- easy local development
- easy MAUI compatibility
- simple early persistence
- portable content database

### Later Options

If later you introduce cloud sync, a shared Web API, or admin workflows, you may add a server database for those server-side workloads.

Recommended default:

- PostgreSQL

Other server databases may still be used if a concrete requirement justifies them, but PostgreSQL is the preferred default direction for future shared backend scenarios.

Do not start there unless Phase 1 genuinely requires it.

---

## 10.2 DbContext Strategy

Use a single application database context in Phase 1.

### Recommendation

One `AppDbContext` is enough initially.

Internal organization inside the DbContext should still respect context grouping.

Example groups:

- content sets
- learning profile sets
- content operations sets
- localization sets

Startup initialization should use EF Core migrations.

Legacy local databases created earlier with `EnsureCreated` may be baselined during startup compatibility handling, but new schema evolution should always move through migrations.

---

## 10.3 EF Core Configuration Strategy

All entity configuration should be in Infrastructure using separate configuration classes.

Do not use bloated inline configuration in DbContext.

### Recommendation

One configuration class per entity or per closely related entity if small.

---

# 11. Import Architecture

## 11.1 Import Flow

Recommended flow:

1. read JSON file
2. validate file structure
3. map to import DTO/model
4. validate domain-relevant rules
5. detect duplicates
6. create ContentPackage record
7. create ContentPackageEntry records
8. insert valid content
9. update package summary
10. return import report

---

## 11.2 Import Placement

### File parsing belongs to:
- Infrastructure

### Import orchestration belongs to:
- Application

### Import execution host belongs to:
- ImportTool

### Domain validation belongs to:
- Domain + Application validation

---

## 11.3 Important Rule

Do not put import-specific parsing DTOs inside the Domain project.

They are not domain entities.

---

# 12. MAUI Architecture Direction

## 12.1 UI Pattern

For MAUI, use a clean MVVM-style structure.

### Recommended

- Views
- ViewModels
- Application service access
- DTO/view-model mapping at UI boundary

---

## 12.2 What the MAUI App Should Consume

The MAUI app should consume:

- application queries
- application commands
- response DTOs
- UI-specific mapped models where needed

It should not manipulate domain entity graphs directly.

---

## 12.3 Audio Strategy

Platform text-to-speech should be implemented in the MAUI layer or a MAUI platform adapter layer.

It should not live in Domain or Application.

---

## 12.4 Localization Strategy

UI localization resources belong in the MAUI project.

Domain data localization belongs in the data/domain model.

These are different concerns and must stay separate.

---

# 13. Future Web and API Expansion

## 13.1 Why the Current Architecture Supports Future Expansion

Because the Domain and Application layers are shared and independent, future delivery applications can be added without redesigning the business core.

### Later additions

- DarwinDeutsch.WebApi
- DarwinDeutsch.Web
- DarwinDeutsch.Admin

---

## 13.2 WebApi Project Role

Future `DarwinDeutsch.WebApi` should contain:

- API endpoints/controllers
- auth configuration
- dependency injection
- API-specific request/response contracts if needed
- host startup

It should depend on Application and Infrastructure.

It must not contain core business rules.

---

## 13.3 Web Project Role

Future `DarwinDeutsch.Web` should contain:

- web UI
- pages/components
- navigation
- localization resources for web
- client-side interaction with Application or API

---

## 13.4 Admin Project Role

Future `DarwinDeutsch.Admin` should contain:

- content management UI
- import history viewing
- content package review
- content editing workflows
- resource management later

This should not be mixed into the public learner-facing app.

---

# 14. Recommended Naming Convention

## 14.1 Solution Name

Recommended solution name:

- DarwinDeutsch

## 14.2 Project Names

Recommended project names:

- DarwinDeutsch.Domain
- DarwinDeutsch.Application
- DarwinDeutsch.Infrastructure
- DarwinDeutsch.Maui
- DarwinDeutsch.ImportTool

Future:

- DarwinDeutsch.WebApi
- DarwinDeutsch.Web
- DarwinDeutsch.Admin

### Rule

Keep names direct and predictable.
Do not create overly creative project names.

---

# 15. Recommended Initial Phase 1 Feature Set by Project

## 15.1 Domain

Implement:

- core phase-1 entities
- enums
- minimal value objects if truly useful

## 15.2 Application

Implement feature groups:

- SearchWords
- GetWordDetails
- GetWordsByTopic
- GetWordsByCefr
- GetFavorites
- ToggleFavorite
- GetUserPreferences
- UpdateUserPreferences
- TrackWordViewed
- MarkWordKnown
- MarkWordDifficult
- ImportContentPackage

## 15.3 Infrastructure

Implement:

- AppDbContext
- migrations
- entity configurations
- seed data
- JSON import reader
- import persistence service
- duplicate checking queries

## 15.4 Maui

Implement screens:

- home
- browse by level
- browse by topic
- word details
- favorites
- settings/preferences

## 15.5 ImportTool

Implement:

- select/load file
- validate/import file
- display summary
- show warnings/errors

---

# 16. Recommended Cross-Cutting Concerns

## 16.1 Logging

Logging should be host/infrastructure concern.

Do not bake logging into domain entities.

---

## 16.2 Validation

Validation may happen at multiple levels:

- input validation in Application
- invariant validation in Domain
- technical format validation in Infrastructure

### Rule

Do not overload one layer with all validation responsibility.

---

## 16.3 Mapping

Mapping between layers should be explicit and predictable.

### Recommendation

Use mapping carefully.
Do not create deep magic mapping that hides important transformations.

---

## 16.4 Time Handling

Use UTC in core persistence fields.

Local presentation formatting belongs to UI.

---

# 17. Testing Strategy Alignment

The architecture should support:

- domain tests
- application use case tests
- infrastructure integration tests
- MAUI UI tests later if needed

### Recommendation

The cleanest early test split is:

- Domain.Tests
- Application.Tests
- Infrastructure.Tests

These can be added once the implementation starts.

Do not create empty test projects too early if no real tests are ready, but the architecture should anticipate them.

---

# 18. Anti-Patterns to Avoid

## 18.1 Fat MAUI App

Do not let the MAUI project become the place where all logic lives.

## 18.2 Anemic Domain by Accident

Do not move every rule into handlers if some rules naturally belong in entities.

## 18.3 Generic Repository Everywhere

Do not build a giant generic repository abstraction just because many tutorials do it.

Use abstractions only where they help.

## 18.4 Import Logic Hidden in UI

Do not turn content import into a page-click workflow buried inside the learner app.

## 18.5 Future-Feature Pollution

Do not add fields, interfaces, and abstractions only for imagined future features with no current use.

---

# 19. Final Architecture Summary

Darwin Deutsch should begin as a clean modular monolith with five main Phase 1 projects:

- DarwinDeutsch.Domain
- DarwinDeutsch.Application
- DarwinDeutsch.Infrastructure
- DarwinDeutsch.Maui
- DarwinDeutsch.ImportTool

The architecture should keep:

- domain rules in Domain
- use cases in Application
- persistence and import parsing in Infrastructure
- user-facing UI in MAUI
- operational import hosting in ImportTool

This is the right architecture for:

- Phase 1 speed
- future web/API/admin expansion
- code reuse
- long-term maintainability

It is intentionally disciplined, not oversized.
