# Storage Strategy

## Purpose

This document defines the storage strategy for Darwin Deutsch.

The storage design must support:

- Phase 1 local-first delivery
- clean support for multilingual content
- structured content import
- user preferences and lightweight user state
- future expansion to sync and server-backed scenarios
- safe evolution without major redesign

This document builds on:

- `22-Domain-Model.md`
- `25-Phase-1-Domain-Cut.md`
- `26-Bounded-Contexts.md`
- `31-Solution-Architecture.md`

---

# 1. Storage Principles

## 1.1 Main Principles

The storage strategy should follow these principles:

- simple in Phase 1
- relational by default
- local-first for the first release
- compatible with MAUI across Android, iOS, and Windows
- migration-friendly
- future-ready for server expansion
- separate shared content from per-user state logically
- avoid premature distributed data complexity

---

## 1.2 Phase 1 Storage Goal

Phase 1 should provide a stable local database that can:

- store vocabulary content
- store translations and examples
- store topics and languages
- store user preferences
- store favorites
- store basic user word state
- receive imported content through a dedicated import tool

This is enough for a serious first release.

---

# 2. Recommended Database Choice

## 2.1 Phase 1 Recommendation

Use **SQLite** as the primary database for Phase 1.

## 2.2 Why SQLite Is the Right Choice

SQLite is the correct Phase 1 choice because it offers:

- strong .NET and EF Core support
- excellent compatibility with MAUI platforms
- low setup cost
- easy local development
- simple deployment
- portable database files
- good fit for mostly local read-heavy usage
- no server infrastructure requirement for early validation

## 2.3 Why Not Start with Server Database

Do not start Phase 1 with PostgreSQL or SQL Server unless there is a hard requirement for:

- multi-user shared backend
- live cloud sync
- remote admin editing
- centralized analytics
- remote content publishing

Those are future concerns, not Phase 1 needs.

---

# 3. Storage Topology for Phase 1

## 3.1 Recommended Topology

Phase 1 should use a **single local relational database** per app installation.

This database should contain:

- shared learning content
- reference data
- local user-specific app state

## 3.2 Why One Database Is Fine in Phase 1

A single database is the best Phase 1 choice because it is:

- simpler
- faster to build
- easier to migrate
- easier to debug
- enough for offline/local usage

### Important Note

Even if one physical database is used, the logical separation between contexts must remain clear.

---

# 4. Logical Storage Areas

Within the single database, the data should be treated as belonging to logical storage areas.

## 4.1 Content Area

Contains:

- WordEntry
- WordSense
- SenseTranslation
- ExampleSentence
- ExampleTranslation
- Topic
- TopicLocalization
- WordTopic
- Language

This is shared app content.

## 4.2 Learning Profile Area

Contains:

- UserLearningProfile
- UserFavoriteWord
- UserWordState

This is per-user local state.

## 4.3 Content Operations Area

Contains:

- ContentPackage
- ContentPackageEntry
- optional ImportJob later

This supports traceability of imported content.

---

# 5. Shared Content vs User State

## 5.1 Conceptual Separation

Even in one local database, shared content and user state must remain conceptually separate.

### Shared Content

Shared content is:

- language-independent core learning data
- imported packages
- topics
- meanings
- examples

### User State

User state is:

- preferences
- favorites
- viewed history/state
- difficult/known markers

## 5.2 Reason

This separation matters because later you may want to:

- replace or update content packages
- preserve user favorites and progress
- sync user state differently from content
- download content packs separately

If content and user state are mixed carelessly, future sync becomes painful.

---

# 6. Recommended Table Groups for Phase 1

## 6.1 Content Tables

Recommended Phase 1 content tables:

- WordEntries
- WordSenses
- SenseTranslations
- ExampleSentences
- ExampleTranslations
- Topics
- TopicLocalizations
- WordTopics
- Languages

## 6.2 User Tables

Recommended Phase 1 user tables:

- UserLearningProfiles
- UserFavoriteWords
- UserWordStates

## 6.3 Import/Operations Tables

Recommended Phase 1 operations tables:

- ContentPackages
- ContentPackageEntries

---

# 7. Database Schema Strategy

## 7.1 Naming Strategy

Use explicit and predictable table names.

Examples:

- WordEntries
- WordSenses
- SenseTranslations
- ExampleSentences
- ExampleTranslations
- Topics
- TopicLocalizations
- WordTopics

Avoid overly abbreviated or cryptic names.

## 7.2 Key Strategy

Recommended Phase 1 key strategy:

- internal numeric keys or GUID/ULID keys
- stable public IDs only where externally useful

### Direct Recommendation

For Phase 1 relational simplicity, numeric primary keys are acceptable and practical.

Use `PublicId` on content roots where you want stable external exposure later.

Examples:

- WordEntry.PublicId
- ContentPackage.PackageId

## 7.3 Foreign Keys

Use real relational foreign keys in the database.

Do not fake relational consistency at application level if the database can enforce it safely.

---

# 8. EF Core Persistence Strategy

## 8.1 ORM Recommendation

Use **EF Core** as the primary persistence technology.

## 8.2 Why EF Core

It gives you:

- strong SQLite support
- clean code-first migrations
- cross-platform development compatibility
- good mapping support for the current domain shape
- future migration path to server databases

## 8.3 Configuration Rule

All entity configuration should be in Infrastructure via dedicated configuration classes.

Do not rely on scattered conventions alone for important constraints.

---

# 9. Migration Strategy

## 9.1 Use Code-First Migrations

Phase 1 should use EF Core code-first migrations.

This is the right choice because the domain is still evolving and migrations should remain versioned in source control.

## 9.2 Migration Discipline

Every schema change should be represented as a migration.

Do not allow silent manual schema drift.

## 9.3 Migration Storage

Migrations should live in:

- `DarwinDeutsch.Infrastructure`

## 9.4 Database Initialization Rule

On first app launch, the database should be created and migrations applied in a controlled way.

### Current Direction

Use migration-based startup initialization.

For local developer databases that were created earlier with `EnsureCreated`, the startup flow should baseline the initial migration history before continuing with normal migrations so existing local files do not break during the transition.

---

# 10. Seed Data Strategy

## 10.1 Seed Data Scope

The following should be seeded in Phase 1:

- Languages
- Topics
- TopicLocalizations
- possibly initial reference enum mappings if needed
- possibly minimal starter content only if explicitly intended

## 10.2 What Should Not Be Seeded Blindly

Do not hardcode a large vocabulary dataset into migrations or code-based seed blocks.

Vocabulary content should come through the content import pipeline, not through giant migration seed blobs.

## 10.3 Why

This keeps content growth manageable and consistent with your AI-assisted import workflow.

## 10.4 Recommended Seed Types

### Stable Seed Data

These are safe to seed structurally:

- Language
- Topic
- TopicLocalization

### Imported Content Data

These should come from import packages:

- WordEntry
- WordSense
- translations
- examples

---

# 11. Import Storage Strategy

## 11.1 Content Import Storage

Imported content should be written into the same database model used by the app.

There should not be a separate temporary vocabulary storage model in Phase 1.

## 11.2 Import Traceability

Every import should create a `ContentPackage` record.

Every processed item should create a `ContentPackageEntry` record.

## 11.3 Why This Matters

This gives you:

- traceability
- error reporting
- future re-import support
- future admin visibility
- better debugging

---

# 12. Duplicate Handling in Storage

## 12.1 Phase 1 Duplicate Rule

Duplicates should be detected before insert using the Phase 1 uniqueness identity:

- NormalizedLemma
- PartOfSpeech
- PrimaryCefrLevel

## 12.2 Storage Enforcement

This rule should be supported by:

- application/import duplicate detection
- database unique index where appropriate

## 12.3 Recommendation

Use both:

- application-level detection for friendly reporting
- database-level unique constraint for safety

---

# 13. Indexing Strategy

## 13.1 Phase 1 Indexes

At minimum, create indexes for:

- WordEntry.NormalizedLemma
- WordEntry.PrimaryCefrLevel
- SenseTranslation.LanguageCode
- ExampleTranslation.LanguageCode
- Topic.Key
- WordTopic.WordEntryId
- WordTopic.TopicId
- UserFavoriteWord.UserId
- UserWordState.UserId
- ContentPackage.PackageId

## 13.2 Unique Indexes

Create unique indexes for:

- WordEntry(NormalizedLemma, PartOfSpeech, PrimaryCefrLevel)
- WordSense(WordEntryId, SenseOrder)
- SenseTranslation(WordSenseId, LanguageCode)
- ExampleSentence(WordSenseId, SentenceOrder)
- ExampleTranslation(ExampleSentenceId, LanguageCode)
- Topic(Key)
- TopicLocalization(TopicId, LanguageCode)
- WordTopic(WordEntryId, TopicId)
- Language(Code)
- UserFavoriteWord(UserId, WordEntryId)
- UserWordState(UserId, WordEntryId)

## 13.3 Search Reality

Phase 1 search does not need advanced full-text search.

The current preferred strategy is:

- normalized German lemma search
- prefix-first ranking for the best learner-facing results
- contains fallback for broader recall
- explicit SQLite indexes for the active normalized-lemma path

This keeps search simple, predictable, and fast enough for the Phase 1 local dataset shape.

---

# 14. Content Pack Thinking

## 14.1 Future-Oriented View

Even though Phase 1 uses a single local database, the storage strategy should conceptually support future content pack behavior.

That means the system should already be comfortable with:

- starter content
- additional imported packs
- category/topic-based expansion
- selective dataset growth

## 14.2 Practical Implication

Do not design the schema as if one fixed monolithic content set is the only future.

Imported content should feel like modular packages even if they land in one database.

---

# 15. User Identity Storage in Phase 1

## 15.1 Local Identity Assumption

In early Phase 1, user-related tables may use a local app-level user identifier.

If the app is single-user per installation, this may be simple.

## 15.2 Recommendation

Still keep `UserId` explicit in the schema.

Do not assume forever that the device equals the user.

This keeps the path open for later:

- account-based sync
- remote login
- profile transfer

---

# 16. Content Update Strategy

## 16.1 Phase 1 Rule

Phase 1 should not implement full content update/merge logic.

Imported content is primarily insert-based with duplicate skip behavior.

## 16.2 Why

This keeps import behavior predictable and prevents accidental content corruption.

## 16.3 Future Direction

Later, you may support:

- package update
- merge existing entry
- archive replaced content
- content diff review

But not in Phase 1.

---

# 17. Delete and Retention Strategy

## 17.1 Content Retention

Prefer keeping content rows and controlling visibility through status rather than deleting them routinely.

### Recommendation

Use `PublicationStatus` for user-visible content lifecycle.

## 17.2 Import Retention

Keep import package history and entry logs at least in early development.

This is useful for debugging and content audit.

## 17.3 User State Retention

User preferences, favorites, and word states should survive app restarts and normal content growth.

---

# 18. Encryption and Sensitive Data

## 18.1 Phase 1 Reality

Phase 1 does not store highly sensitive personal data.

Typical stored user data is lightweight:

- language preferences
- favorites
- basic usage state

## 18.2 Recommendation

Do not overcomplicate the initial storage model with unnecessary encryption layers unless platform policy or app-store requirements demand it.

However:

- secure platform storage may later be used for auth/session tokens
- that is separate from the content database itself

---

# 19. Multi-Platform File Location Strategy

## 19.1 Database File Location

Each platform should store the SQLite database in its appropriate application data directory.

This is a platform/infrastructure concern, not a domain concern.

## 19.2 Rule

The path resolution logic belongs in the host/infrastructure layer.

Do not hardcode platform-specific file paths into domain or application logic.

---

# 20. Backup and Portability Thinking

## 20.1 Early Portability Value

SQLite gives you a portable database file, which is useful for:

- development testing
- debugging
- possible manual inspection
- future backup/export workflows

## 20.2 Recommendation

Keep this advantage in mind when designing content import and local storage paths.

---

# 21. Server Expansion Path

## 21.1 Future Server Scenario

If later you add:

- Web API
- admin editing
- cloud sync
- shared user accounts
- centralized resource discovery

then the storage model can evolve to a server-backed architecture.

For that future shared backend scenario, PostgreSQL is the preferred default server database unless a concrete requirement leads to another choice.

## 21.2 Recommended Future Server Split

A likely future split would be:

### Server Content Storage
- shared content catalog
- topics
- languages
- support resources
- import history
- admin-managed data
- likely hosted in PostgreSQL

### User Cloud Storage
- favorites
- preferences
- progress
- review state

### Local Device Storage
- cache
- offline content
- pending sync operations

## 21.3 Why Phase 1 Still Works

Because the current model already distinguishes shared content from user state conceptually, the future split remains feasible.

---

# 22. Local Cache vs Source-of-Truth in Phase 1

## 22.1 Phase 1 Source-of-Truth

In Phase 1 local-only mode, the local SQLite database is the source of truth.

There is no server source of truth yet.

## 22.2 Future Adjustment

Later, when a backend exists:

- server may become source of truth for shared content
- user cloud state may become source of truth for account-linked preferences
- local database may become cache + offline state layer

The current storage strategy should not block this evolution.

---

# 23. Performance Expectations

## 23.1 Expected Workload

Phase 1 workload is mostly:

- reading words
- filtering by CEFR
- filtering by topic
- searching by lemma
- reading translations and examples
- writing lightweight user state
- occasional imports

This is well within SQLite capability.

## 23.2 Conclusion

Do not prematurely optimize around storage scale that Phase 1 does not have.

---

# 24. Recommended Phase 1 Storage Decisions

## Decision 1

Use SQLite.

## Decision 2

Use one local relational database per app installation.

## Decision 3

Use EF Core with code-first migrations.

## Decision 4

Seed only stable reference data through structured seeding.

## Decision 5

Import vocabulary content through the import pipeline, not through giant migration seed data.

## Decision 6

Keep shared content and user state logically separate even if physically stored together.

## Decision 7

Use database constraints and indexes to protect integrity.

---

# 25. Final Storage Summary

Phase 1 storage should be simple, local, relational, and migration-driven.

The correct Phase 1 approach is:

- SQLite
- EF Core
- one local database
- stable reference seeds
- imported content packages
- lightweight local user state
- future-ready separation of shared content and user data

This is the right balance between pragmatism and forward compatibility.
