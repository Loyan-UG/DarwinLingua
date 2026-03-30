# Shared Content Server Domain

## Purpose

This document defines the **server-side domain shape** for Darwin Lingua's future shared backend.

It extends the product domain beyond the current local-only mobile implementation and answers these questions:

- how one server database should support multiple learner apps
- how shared content should be partitioned without creating one database per app
- how package publication should work for mobile update flows
- how local mobile user state remains separate from shared content

This document builds on:

- `22-Domain-Model.md`
- `26-Bounded-Contexts.md`
- `31-Solution-Architecture.md`
- `36-Server-Content-Distribution.md`

---

## 1. Design Goal

The server domain must support:

- `DarwinDeutsch` today
- future learner apps such as `DarwinEnglish`, `DarwinArabic`, or other product variants later
- one shared backend and one shared content database
- multiple mobile clients with different content scopes

The design should avoid:

- one database per app
- one schema per language
- raw table sync to mobile devices
- mixing shared content with learner-specific local state

---

## 2. Core Principle

The backend should treat learner apps as **clients of shared content streams**, not as isolated systems.

That means:

- the database is shared
- the content model is shared
- publishing is scoped by product and content stream
- mobile apps download only the packages relevant to their stream

---

## 3. Recommended Server-Side Bounded Contexts

The future server side should keep the modular-monolith approach, but add two server-facing bounded contexts on top of the current model.

### 3.1 Shared Catalog

Owns the canonical shared learning content:

- languages
- lexical entries
- senses
- translations
- examples
- topics
- lexical metadata
- support resources later

This is the server-side source of truth for content.

### 3.2 Publishing and Distribution

Owns how shared content is prepared for clients:

- client app registration
- content stream definitions
- package publication
- manifest generation
- package artifact metadata
- schema/version compatibility

This context should not own lexical authoring itself.

### 3.3 Content Operations

Owns the operational side of importing and publishing:

- import jobs
- validation reports
- draft packages
- publish history

### 3.4 Learning and Practice

For now, mobile learner state stays local-first and should not be moved into the first shared-content server slice.

---

## 4. Multi-Product Shared Model

### 4.1 Why Product Scoping Must Exist

The backend must support multiple learner apps later.

Examples:

- `DarwinDeutsch` teaches German
- `DarwinEnglish` may teach English
- `DarwinArabic` may teach Arabic

These apps may share:

- topics
- language references
- package publication logic
- Web API contracts
- operational tooling

But they must still receive only the content intended for them.

### 4.2 Recommended Product Concept

Introduce a stable `ClientProduct` concept.

Examples:

- `darwin-deutsch`
- `darwin-english`
- `darwin-arabic`

This does **not** mean content must be duplicated per product.

Instead, product identity is used to determine:

- which content streams a client can request
- which manifests it should see
- which package versions are compatible

---

## 5. Recommended Shared Content Partition

### 5.1 Use Learning Language as the Primary Content Axis

The most stable content axis is the **language being learned**.

Examples:

- German-learning content -> `learningLanguageCode = de`
- English-learning content -> `learningLanguageCode = en`
- Arabic-learning content -> `learningLanguageCode = ar`

This is better than creating one schema or table set per app.

### 5.2 Product vs Content

Recommended interpretation:

- `learningLanguageCode` partitions the actual educational content
- `clientProductKey` scopes which packages and manifests a client app can consume

This lets multiple apps potentially share the same content language later if needed.

### 5.3 Content Area

Shared content should also be partitioned by `contentAreaKey`.

Examples:

- `catalog`
- `support-resources`
- `practice-content` later

### 5.4 Slice Key

Within a content area, distribution can be further sliced.

Examples:

- `cefr:a1`
- `cefr:a2`
- `topic:housing`
- `topic:doctor`

---

## 6. Recommended Aggregate Model

### 6.1 Aggregate: ClientProduct

Represents one mobile or future web/admin client family.

Recommended fields:

- `Id`
- `Key`
- `DisplayName`
- `LearningLanguageCode`
- `DefaultUiLanguageCode`
- `IsActive`
- `CreatedAtUtc`
- `UpdatedAtUtc`

Examples:

- `darwin-deutsch`
- `darwin-english`

### 6.2 Aggregate: ContentStream

Represents one publishable stream of shared content.

Recommended fields:

- `Id`
- `ClientProductId`
- `ContentAreaKey`
- `SliceKey`
- `LearningLanguageCode`
- `SchemaVersion`
- `IsActive`
- `CreatedAtUtc`
- `UpdatedAtUtc`

Examples:

- `darwin-deutsch / catalog / full / de`
- `darwin-deutsch / catalog / cefr:a1 / de`
- `darwin-deutsch / support-resources / full / de`

### 6.3 Aggregate: PublishedPackage

Represents one versioned unit distributed to clients.

Recommended fields:

- `Id`
- `PackageId`
- `ContentStreamId`
- `Version`
- `PackageType`
- `Checksum`
- `EntryCount`
- `WordCount`
- `SchemaVersion`
- `PublishedAtUtc`
- `Status`
- `CreatedAtUtc`
- `UpdatedAtUtc`

### 6.4 Entity: PackageArtifact

Represents how a package is physically stored.

Recommended fields:

- `Id`
- `PublishedPackageId`
- `StorageKind`
- `RelativePath`
- `ContentType`
- `SizeBytes`
- `Checksum`
- `CreatedAtUtc`

### 6.5 Entity: PublishedManifest

Represents the generated view used by mobile clients.

Recommended fields:

- `Id`
- `ClientProductId`
- `ManifestKey`
- `SchemaVersion`
- `GeneratedAtUtc`
- `Checksum`
- `CreatedAtUtc`

### 6.6 Entity: ManifestPackageLink

Links a manifest to the packages it contains.

Recommended fields:

- `Id`
- `PublishedManifestId`
- `PublishedPackageId`
- `SortOrder`

### 6.7 Aggregate: ServerImportBatch

Represents one server-side import batch into shared content.

Recommended fields:

- `Id`
- `SourceReference`
- `InputFileName`
- `SourceType`
- `Status`
- `StartedAtUtc`
- `FinishedAtUtc`
- `InsertedEntries`
- `UpdatedEntries`
- `ArchivedEntries`
- `InvalidEntries`
- `WarningCount`

---

## 7. Shared Catalog Additions for Multi-Language Growth

The existing lexical domain already supports translations and meanings.

For the shared server database, the catalog should additionally make sure that core shared content rows always carry or imply:

- `learningLanguageCode`
- `publicationStatus`
- `published package lineage` where useful

### 7.1 WordEntry Rule

Each `WordEntry` in the shared database should belong to one learning language.

For now, that is German.

Later:

- German words for `DarwinDeutsch`
- English words for `DarwinEnglish`
- Arabic words for `DarwinArabic`

### 7.2 Topic Rule

Topics can remain globally shared when they are language-neutral by key.

Display names remain localized separately.

### 7.3 Language Rule

The `Language` reference table remains shared across the whole platform.

It should support:

- UI languages
- meaning languages
- learning languages

---

## 8. Manifest Contract Direction

The first manifest contract should be shaped around:

- `clientProductKey`
- `learningLanguageCode`
- `contentAreaKey`
- `sliceKey`
- `schemaVersion`
- `latestPackageVersion`
- `checksum`
- `entryCount`
- `downloadUrl`

This keeps the API neutral enough for multiple apps.

---

## 9. What Must Stay Out of the First Shared Server Slice

The first server-backed content slice should not include:

- user favorites in PostgreSQL
- cloud practice history
- account login domain
- subscription/paywall domain
- analytics event warehouse

Those would blur the shared-content boundary too early.

---

## 10. Physical Database Recommendation

The recommended local and early production setup is:

- one PostgreSQL instance
- one main application database for shared content and publishing metadata
- one application role for runtime Web API access
- one higher-privilege operational role for import/migration work

Do not create one database per app language unless a real operational constraint appears.

---

## 11. Final Recommendation

The right server-side domain is:

- one shared PostgreSQL database
- shared catalog content partitioned primarily by `learningLanguageCode`
- client-specific distribution partitioned by `clientProductKey`, `contentAreaKey`, and `sliceKey`
- published packages and manifests as first-class domain concepts
- local learner state still outside the first server-backed slice

This is the cleanest path for supporting Darwin Deutsch now and additional learner apps later without redesigning the backend again.
