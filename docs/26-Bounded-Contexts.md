# Bounded Contexts

## Purpose

This document defines the bounded contexts of Darwin Deutsch.

The goal is to establish clear conceptual and architectural boundaries early, so that:

- the domain remains understandable
- implementation remains maintainable
- future modules can be added safely
- code sharing across platforms stays clean
- the first release does not become polluted by future concerns

This document complements:

- `22-Domain-Model.md`
- `23-Domain-Rules.md`
- `24-Entity-Relationships.md`
- `25-Phase-1-Domain-Cut.md`

---

# 1. Why Bounded Contexts Matter Here

Darwin Deutsch is not only a vocabulary app.

Even though Phase 1 is intentionally focused, the full product vision includes multiple conceptual areas:

- learning content
- user-specific learning state
- practice and review
- content import and operations
- local support resource discovery
- localization support
- future administration and publishing

These concerns should not be modeled as one large flat domain.

If they are mixed too early, the project will become harder to evolve, harder to test, and harder to share across MAUI, API, admin tools, and future web applications.

---

# 2. Context Map Overview

The product should be divided into the following bounded contexts:

- Content Catalog
- Learning Profile
- Practice
- Content Operations
- Publishing and Distribution
- Resource Discovery
- Localization Support

These are conceptual domain boundaries.

They do not necessarily mean that Phase 1 must implement a separate deployable service for each context.

In Phase 1, these contexts can remain inside a modular monolith while still preserving clear internal boundaries.

---

# 3. Content Catalog Context

## 3.1 Purpose

The Content Catalog context is the core business context of Phase 1.

It defines the shared German learning content that all users consume.

## 3.2 Responsibilities

This context is responsible for:

- German word entries
- meanings by language
- example sentences
- CEFR-based structure
- topic associations
- lexical metadata
- controlled content lifecycle

## 3.3 Core Concepts

Main concepts inside this context:

- WordEntry
- WordSense
- SenseTranslation
- ExampleSentence
- ExampleTranslation
- Topic
- TopicLocalization
- WordTopic
- Language reference usage for meaning/localization links

## 3.4 What Belongs Here

Belongs here:

- content structure
- content relationships
- semantic meaning structure
- topic classification
- multilingual meaning storage
- content publication state

## 3.5 What Must Not Belong Here

Must not belong here:

- user favorites
- user known/difficult state
- review scheduling
- quiz sessions
- import job execution logic
- admin UI concerns
- support center directory data

## 3.6 Why This Context Is Central

This is the foundation of the entire product.

Everything else depends on shared learning content.

If this context is weak, every future module becomes unstable.

---

# 4. Learning Profile Context

## 4.1 Purpose

The Learning Profile context manages per-user preferences and lightweight personal learning state.

This context exists because user-specific data must remain separate from global learning content.

## 4.2 Responsibilities

This context is responsible for:

- meaning language preferences
- UI language preference
- optional native language
- favorites
- lightweight per-word state
- simple personalization

## 4.3 Core Concepts

Main concepts inside this context:

- UserLearningProfile
- UserFavoriteWord
- UserWordState

## 4.4 What Belongs Here

Belongs here:

- user-selected meaning language 1
- user-selected meaning language 2
- UI language selection
- favorite words
- view count
- first viewed / last viewed
- is known / is difficult markers

## 4.5 What Must Not Belong Here

Must not belong here:

- shared content structure
- word senses
- example translations
- quiz result logic
- review scheduling
- import pipeline state

## 4.6 Dependency Rule

This context may reference content entities such as `WordEntry`, but content must never depend on user profile state.

### Direction

- Learning Profile -> Content Catalog
- Content Catalog -X-> Learning Profile

This direction must remain one-way.

---

# 5. Practice Context

## 5.1 Purpose

The Practice context is responsible for active learning mechanics rather than passive browsing.

It is intentionally deferred from Phase 1, but must be modeled conceptually now.

## 5.2 Responsibilities

This context is responsible for:

- flashcards
- quizzes
- review sessions
- spaced repetition scheduling
- mistake tracking
- practice history

## 5.3 Core Concepts

Main concepts expected later:

- ReviewQueueItem
- PracticeSession
- PracticeSessionItem
- UserMistakePattern

## 5.4 What Belongs Here

Belongs here:

- review cadence
- correct/incorrect answer tracking
- practice session lifecycle
- mistake analysis
- scheduling of future review

## 5.5 What Must Not Belong Here

Must not belong here:

- word meaning storage
- topic metadata
- import logging
- user UI language preference
- support resource directory data

## 5.6 Dependency Rule

Practice depends on content and may also depend on learning profile preferences.

### Direction

- Practice -> Content Catalog
- Practice -> Learning Profile

But:

- Content Catalog -X-> Practice
- Learning Profile -X-> Practice

## 5.7 Design Warning

Do not start leaking practice fields into `UserWordState` in Phase 1.

Examples of what should not be prematurely added there:

- spaced repetition interval
- ease factor
- failure streak
- next review date

Those belong to the Practice context.

---

# 6. Content Operations Context

## 6.1 Purpose

The Content Operations context handles structured content import and related operational tracking.

This context is especially important in Darwin Deutsch because content will grow gradually and AI-assisted generation is part of the real workflow.

## 6.2 Responsibilities

This context is responsible for:

- content package metadata
- package entry tracking
- import validation flow
- duplicate detection flow
- import summary and reporting
- source traceability

## 6.3 Core Concepts

Main concepts inside this context:

- ContentPackage
- ContentPackageEntry
- optional ImportJob

## 6.4 What Belongs Here

Belongs here:

- package identity
- source type
- file-level tracking
- per-entry processing result
- warning/error tracking
- package summary data

## 6.5 What Must Not Belong Here

Must not belong here:

- word detail viewing logic
- favorite management
- UI rendering rules
- practice scheduling
- support resource search

## 6.6 Dependency Rule

Content Operations depends on Content Catalog because successful package entries create content records.

### Direction

- Content Operations -> Content Catalog

But:

- Content Catalog -X-> Content Operations

### Important Interpretation

The content model must not know or care whether a word was inserted manually, from a package, or from an AI-assisted file beyond minimal traceability fields.

The import process is operational behavior, not the heart of the learning content domain.

---

# 6.7 Publishing and Distribution Context

## 6.7.1 Purpose

This future context owns how shared server-side content is published to clients.

It is distinct from both raw lexical authoring and raw import execution.

## 6.7.2 Responsibilities

This context is responsible for:

- client product registration
- content stream definitions
- manifest generation
- package versioning
- package artifact metadata
- schema compatibility checks

## 6.7.3 Core Concepts

Main concepts expected later:

- ClientProduct
- ContentStream
- PublishedPackage
- PublishedManifest
- PackageArtifact

## 6.7.4 Dependency Rule

Publishing and Distribution depends on Content Catalog and may consume Content Operations outputs.

### Direction

- Publishing and Distribution -> Content Catalog
- Publishing and Distribution -> Content Operations

But:

- Content Catalog -X-> Publishing and Distribution
- Learning Profile -X-> Publishing and Distribution

## 6.7.5 Why It Must Stay Separate

This keeps multi-app package delivery clean:

- Darwin Deutsch today
- future language apps later

without mixing package publication logic into lexical entities.

---

# 7. Resource Discovery Context

## 7.1 Purpose

The Resource Discovery context handles helpful real-world resources for immigrants and foreign learners.

This context is important for the long-term product identity, but it is intentionally out of Phase 1 implementation.

## 7.2 Responsibilities

This context is responsible for:

- support resource records
- categories
- addresses
- discoverability by location and topic
- metadata for practical usefulness

## 7.3 Core Concepts

Main concepts expected later:

- SupportResource
- SupportResourceCategory
- SupportResourceCategoryLocalization
- SupportResourceCategoryLink
- Address
- SupportResourceAddress
- SupportResourceTag

## 7.4 What Belongs Here

Belongs here:

- language schools
- speaking cafés
- counseling centers
- support offices
- city/area-linked resources
- resource categories and tags

## 7.5 What Must Not Belong Here

Must not belong here:

- word meanings
- example translations
- user review scheduling
- content import package flow

## 7.6 Dependency Rule

This context may later reuse shared language/reference concepts and possibly topic-like tagging concepts, but it should remain separate from the vocabulary domain.

### Direction

- Resource Discovery may reference Localization Support
- Resource Discovery should remain independent from Learning Profile and Practice

### Important Warning

Do not model support resources as if they are just another type of “word topic”.
That would be conceptually wrong.

---

# 8. Localization Support Context

## 8.1 Purpose

Localization Support is a supporting context, not the primary business context.

It provides the stable language-related reference concepts needed by other contexts.

## 8.2 Responsibilities

This context is responsible for:

- supported languages
- language activation state
- language classification for UI and meaning support
- reference support for localized display content

## 8.3 Core Concepts

Main concept:

- Language

Related localized entities remain inside their owning contexts, such as:

- TopicLocalization inside Content Catalog
- SupportResourceCategoryLocalization inside Resource Discovery

## 8.4 Why This Is a Supporting Context

This context does not define the product by itself.
It supports other contexts by providing a stable language model.

## 8.5 Dependency Direction

Allowed directions:

- Content Catalog -> Localization Support
- Learning Profile -> Localization Support
- Resource Discovery -> Localization Support

Not allowed:

- Localization Support -> Content Catalog
- Localization Support -> Learning Profile
- Localization Support -> Resource Discovery

---

# 9. Context Dependency Summary

## 9.1 Main Dependency Graph

Recommended dependency direction:

- Content Catalog -> Localization Support
- Learning Profile -> Content Catalog
- Learning Profile -> Localization Support
- Practice -> Content Catalog
- Practice -> Learning Profile
- Content Operations -> Content Catalog
- Content Operations -> Localization Support
- Resource Discovery -> Localization Support

## 9.2 Dependency Rule

Dependencies should generally point inward toward more stable shared concepts.

The most stable conceptual elements are:

- Localization Support
- Content Catalog

The most changeable are:

- Practice
- Content Operations
- Publishing and Distribution
- Resource Discovery UI behavior
- Admin workflows

---

# 10. Phase 1 Active Contexts

Phase 1 should actively implement only these contexts:

- Content Catalog
- Learning Profile
- Content Operations
- Localization Support

## 10.1 Phase 1 Context Roles

### Content Catalog
Fully active.

### Learning Profile
Lightly active.

### Content Operations
Active, but operationally simple.

### Localization Support
Active as controlled reference support.

---

# 11. Phase 1 Deferred Contexts

The following contexts should remain design-level only in Phase 1:

- Practice
- Resource Discovery

### Rule

Their concepts may appear in documentation, but they should not appear in the production implementation for Phase 1 except where minimal forward-compatibility naming decisions are needed.

---

# 12. Cross-Context Interaction Rules

## 12.1 Content Catalog and Learning Profile

Learning Profile may store references to content.

Examples:

- favorite word
- user word state

But Content Catalog must remain purely shared content.

## 12.2 Content Operations and Content Catalog

Import logic may create or skip content, but imported words become normal content entries afterward.

The content domain must not behave differently at runtime simply because an entry was imported.

## 12.3 Practice and Content

Practice consumes content.
It does not define content.

## 12.4 Resource Discovery and Content

These contexts may both use topics or categories conceptually, but they should not share the same domain entities blindly.

A future mapping strategy may exist, but direct unification too early would create confusion.

---

# 13. Context Boundaries and Application Layer Design

Bounded contexts are domain boundaries, but they also strongly influence application-layer organization.

Recommended application-layer grouping later:

- Content features
- Learning profile features
- Import/content operations features
- publishing/distribution features
- Practice features
- Resource discovery features

### Rule

Application handlers, services, and use cases should be organized by context and feature, not by technical layer only.

---

# 14. Context Boundaries and Database Design

In Phase 1, all active contexts may share the same physical database.

That is acceptable.

Bounded contexts do not require separate databases in the first version.

### Important Rule

Even if the physical database is shared, conceptual and code-level boundaries must still be preserved.

Do not use a shared database as an excuse to collapse everything into one giant model.

---

# 15. Context Boundaries and API Design

Future APIs should reflect context boundaries.

Examples:

- content endpoints
- profile endpoints
- import/admin endpoints
- practice endpoints later
- resource endpoints later

### Rule

Do not expose one giant “everything controller” model.

---

# 16. Ubiquitous Language by Context

Different contexts may use different language for the same broad product.

This is normal and healthy.

## 16.1 Content Catalog Language

- word
- sense
- translation
- example
- topic
- CEFR

## 16.2 Learning Profile Language

- favorite
- known
- difficult
- preference
- last viewed

## 16.3 Practice Language

- review
- session
- answer
- interval
- mistake

## 16.4 Content Operations Language

- package
- import
- validation
- duplicate
- warning
- job

## 16.5 Resource Discovery Language

- resource
- category
- address
- city
- service
- verified

### Rule

Avoid mixing these vocabularies carelessly in entity names and use case names.

---

# 17. Anti-Corruption Rules for Future Growth

As the product grows, some contexts may need to consume data from others through a simplified or mapped model rather than direct deep coupling.

Examples:

- Practice should use a simplified content projection, not necessarily the entire content aggregate graph
- Resource Discovery should not directly reuse vocabulary entities just because both have categories
- Web and mobile UI should consume application DTOs, not domain entities

This is especially important if the solution later moves toward modular APIs or services.

---

# 18. Recommended Phase 1 Context-to-Project Alignment

This is only a conceptual preview. Detailed solution structure will come later.

## Suggested alignment for Phase 1

### Domain

- Content Catalog domain model
- Learning Profile domain model
- Localization Support reference model
- Content Operations domain-support model

### Application

- content queries and commands
- profile commands and queries
- import commands and validation

### Infrastructure

- persistence for active contexts
- import file parsing support
- platform integrations later

### MAUI App

- UI for content browsing
- UI for favorites and preferences
- import tool may be separate, depending on implementation choice

---

# 19. Key Decisions from This Context Map

## Decision 1

Content is shared and global.

## Decision 2

User learning state is separate and per-user.

## Decision 3

Practice is a future behavior context, not a field collection inside current entities.

## Decision 4

Import is an operational context, not a temporary script concern.

## Decision 5

Publishing and distribution is a future bounded context, not just an API controller wrapper around raw tables.

## Decision 6

Resource discovery is a future independent domain area, not an extension of vocabulary topics.

## Decision 7

Language support is a supporting context used by multiple primary contexts.

---

# 20. Final Bounded Context Summary

Darwin Deutsch should be treated as a modular monolith with clear bounded contexts.

### Active in Phase 1

- Content Catalog
- Learning Profile
- Content Operations
- Localization Support

### Deferred but Designed

- Practice
- Resource Discovery

This boundary is critical.

It allows the first version to remain clean while keeping the long-term direction intact.
