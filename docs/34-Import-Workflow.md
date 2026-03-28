# Import Workflow

## Purpose

This document defines the Phase 1 import workflow for Darwin Deutsch.

The import workflow is essential because Darwin Deutsch content is expected to grow gradually through structured packages, often generated or assisted by AI and then reviewed by a human before import.

This workflow must be:

- simple
- predictable
- traceable
- safe
- suitable for local/offline operation
- compatible with future expansion

This document builds on:

- `12-Content-Package-Format.md`
- `14-Import-Rules.md`
- `22-Domain-Model.md`
- `25-Phase-1-Domain-Cut.md`
- `31-Solution-Architecture.md`
- `32-Storage-Strategy.md`
- `33-Offline-Strategy.md`

---

# 1. Phase 1 Import Goal

The Phase 1 import pipeline should allow an operator to:

- provide a JSON content file
- validate its structure
- validate required fields
- validate controlled values
- detect duplicates
- insert valid data into the local database
- skip duplicates
- log invalid entries
- produce a clear import summary

The Phase 1 import pipeline should not try to do:

- merge/update existing content
- complex conflict resolution
- editorial approval workflows
- remote upload or remote publishing
- partial in-place entity patching

---

# 2. Import Philosophy

## 2.1 Core Rule

The Phase 1 import pipeline must be conservative and predictable.

That means:

- good entries should be imported
- malformed entries should be rejected
- duplicates should be skipped
- one bad record must not destroy the whole batch unless the file itself is fundamentally invalid

## 2.2 Human-AI Workflow

The intended workflow is:

1. AI or a human produces a JSON package
2. a human reviews it
3. the operator runs the import
4. the system validates and imports
5. the operator reviews the result summary

This keeps content production flexible but the data entry path disciplined.

---

# 3. Phase 1 Import Scope

## 3.1 Entities Created by Import

The import workflow may create:

- WordEntry
- WordSense
- SenseTranslation
- ExampleSentence
- ExampleTranslation
- WordTopic
- ContentPackage
- ContentPackageEntry

## 3.2 Entities Not Created by Import in Phase 1

The import workflow should not create or manage dynamically in Phase 1:

- Language
- Topic
- TopicLocalization
- user profile data
- favorites
- user word state
- practice entities
- support resources

### Rule

`Language` and `Topic` should already exist as controlled seed/reference data.

The import file should reference them, not invent them freely.

---

# 4. Import Input Model

## 4.1 Official Input Format

Phase 1 import input format:

- JSON

## 4.2 File-Level Input

The file should contain:

- packageVersion
- packageId
- packageName
- source
- defaultMeaningLanguages
- entries

## 4.3 Entry-Level Input

Each entry should contain at minimum:

- word
- language
- cefrLevel
- partOfSpeech
- topics
- meanings
- examples

Optional metadata may also be present.

### Current Implementation Note

The current Phase 1 implementation maps each imported entry to:

- one `WordEntry`
- one primary `WordSense`
- one set of sense translations
- one ordered set of example sentences under that sense

Multi-sense import packages can be introduced later, but they are not part of the current import contract.

---

# 5. Import Workflow Overview

## 5.1 High-Level Flow

The recommended high-level import flow is:

1. select content area
2. select folder
3. discover JSON files
4. confirm batch
5. read file
6. parse JSON
7. validate file structure
8. validate package metadata
9. validate each entry structure
10. validate reference values
11. normalize import values
12. detect duplicates
13. create content package record
14. process entries
15. write valid content
16. write package entry results
17. finalize package summary
18. return import report

### Current Tooling Note

The current console import tool is folder-oriented. The operator selects one content area, chooses a root folder, and the tool imports every `.json` file found under that folder.

The current tool writes to the host-machine SQLite database path used by the console host. Mobile app builds on Android and iOS keep their own sandbox databases, so importing on the host machine does not automatically populate an emulator or physical mobile-device database.

---

# 6. Detailed Import Stages

# 6.1 Stage 1 - File Acquisition

## Responsibility

Acquire the input JSON file from the operator.

## Source

The file may come from:

- a local filesystem path
- a batch-discovered folder of JSON files
- drag-and-drop in a future desktop tool
- command-line argument in a console tool

## Layer Responsibility

- ImportTool host
- Infrastructure for file reading

## Result

Raw file content is available.

---

# 6.2 Stage 2 - File Parsing

## Responsibility

Parse raw file content into a structured import DTO model.

## Layer Responsibility

- Infrastructure

## Notes

This stage is technical, not domain-level.

The parser should validate:

- valid JSON syntax
- root object presence
- entries array presence

## Failure Rule

If JSON parsing fails or the root structure is missing, the import should stop before database work begins.

This is a fatal file-level failure.

---

# 6.3 Stage 3 - File-Level Validation

## Responsibility

Validate package-level fields.

## Required Package Fields

- packageVersion
- packageId
- packageName
- entries

## Rules

- packageId must not be empty
- packageId must be unique in the local database
- entries must exist
- entries must not be empty
- packageVersion must be supported

## Layer Responsibility

- Application validation
- possibly shared validation helpers

## Failure Rule

If package-level validation fails, stop the import and return file-level errors.

---

# 6.4 Stage 4 - Entry-Level Structural Validation

## Responsibility

Validate the basic structure of each entry before domain mapping.

## Required Entry Fields

- word
- language
- cefrLevel
- partOfSpeech
- topics
- meanings
- examples

## Rules

- `word` must not be empty
- `language` must be `de` in Phase 1
- `cefrLevel` must be valid
- `partOfSpeech` must be valid
- `topics` must contain at least one item
- `meanings` must contain at least one item
- `examples` must contain at least one item

## Layer Responsibility

- Application validation

## Failure Rule

Invalid entries should be marked invalid and skipped from content creation.
The batch should continue.

---

# 6.5 Stage 5 - Reference Validation

## Responsibility

Validate that referenced controlled values exist and are usable.

## Checks

- topic keys exist
- meaning language codes exist
- meaning language codes are active
- meaning language codes are allowed for meaning support
- duplicate translation language codes inside one sense are not repeated
- duplicate translation language codes inside one example are not repeated

## Layer Responsibility

- Application + Infrastructure query access

## Failure Rule

If references are invalid for an entry, that entry should be marked invalid and skipped.

---

# 6.6 Stage 6 - Normalization

## Responsibility

Normalize values into a stable internal form before duplicate detection and persistence.

## Examples

- trim word text
- build `NormalizedLemma`
- normalize casing for language codes and topic keys
- trim translations
- trim example sentences

## Layer Responsibility

- Application

## Rule

Normalization logic must be centralized.
Do not repeat normalization behavior in multiple places.

---

# 6.7 Stage 7 - Duplicate Detection

## Responsibility

Determine whether the candidate word already exists based on Phase 1 identity rules.

## Phase 1 Duplicate Identity

- NormalizedLemma
- PartOfSpeech
- PrimaryCefrLevel

## Layer Responsibility

- Application orchestration
- Infrastructure query support

## Outcome Types

- not duplicate -> continue to create content
- duplicate -> skip entry and log warning

## Rule

A duplicate is not an error in Phase 1.
It is a handled skip outcome.

---

# 6.8 Stage 8 - Package Record Creation

## Responsibility

Create a `ContentPackage` record before or at the start of entry processing.

## Why

This gives the whole operation a traceable package identity.

## Recommended Stored Information

- package id
- package version
- package name
- source type
- input file name
- imported at
- totals initialized to zero or computed values

## Layer Responsibility

- Application orchestration
- Infrastructure persistence

---

# 6.9 Stage 9 - Entry Processing

## Responsibility

Process each entry one by one into final domain entities.

## Processing Logic

For each valid non-duplicate entry:

1. create WordEntry
2. create primary/default WordSense or mapped senses
3. create SenseTranslation records
4. create ExampleSentence records
5. create ExampleTranslation records
6. create WordTopic links
7. persist the aggregate
8. create ContentPackageEntry record linked to the imported word

For invalid or duplicate entries:

- create ContentPackageEntry with failure/skip status
- do not create content entities

---

# 6.10 Stage 10 - Persistence

## Responsibility

Write valid content and package tracking data into the local database.

## Layer Responsibility

- Infrastructure

## Rule

Persistence must be transactional at a sensible level.

### Recommendation

Use one transaction per package import operation if practical.

If that becomes too restrictive for large packages later, a chunked strategy may be introduced.
But Phase 1 can remain simple.

## Important Rule

Invalid or duplicate entries should not leave partial orphan data.

---

# 6.11 Stage 11 - Summary Finalization

## Responsibility

Finalize package totals and status.

## Summary Fields

- total entries
- inserted entries
- skipped duplicates
- invalid entries
- warning count
- final status

## Final Status Examples

- Completed
- CompletedWithWarnings
- Failed

---

# 6.12 Stage 12 - Import Report Return

## Responsibility

Return a result that the operator can inspect.

## Report Should Include

- package id
- file name
- totals
- warnings
- errors
- duplicate summary
- invalid entry summary

## Layer Responsibility

- Application returns import result DTO
- ImportTool presents it

---

# 7. Import Status Model

## 7.1 Package Status

Recommended package statuses for Phase 1:

- Pending
- Processing
- Completed
- CompletedWithWarnings
- Failed

## 7.2 Entry Processing Status

Recommended entry statuses:

- Pending
- Imported
- SkippedDuplicate
- Invalid
- Failed

---

# 8. Error Model

## 8.1 Fatal File-Level Errors

These should stop the import entirely:

- unreadable file
- invalid JSON
- missing root structure
- missing entries array
- unsupported package version
- database unavailable
- package record creation failure before processing begins

## 8.2 Entry-Level Errors

These should not stop the whole import:

- missing required entry field
- invalid CEFR value
- invalid part of speech
- unknown topic key
- unknown meaning language code
- duplicate meaning language in one sense
- duplicate example translation language in one example
- empty translation text
- empty example text

## 8.3 Duplicate Outcome

Duplicate detection is not a fatal error and not a hard failure.
It is a skip outcome with warning semantics.

---

# 9. Phase 1 Import Rules

## 9.1 No Merge Rule

Do not merge new data into an existing word in Phase 1.

If the word identity is already present, skip the entry.

## 9.2 No Partial Patch Rule

Do not patch existing translations or examples in Phase 1.

## 9.3 No Dynamic Topic Creation Rule

Do not create new topics automatically from import files in Phase 1.

If a topic key is unknown, the entry should be invalid.

## 9.4 No Dynamic Language Creation Rule

Do not create new language rows automatically from import files in Phase 1.

## 9.5 No Silent Coercion Rule

Do not silently coerce invalid values into “Other” or fallback values unless the rule is explicitly defined.

Bad data should be visible as bad data.

---

# 10. Import Use Case Structure

## 10.1 Main Application Use Case

Recommended main use case name:

- `ImportContentPackage`

## 10.2 Suggested Request Model

The request should contain at least:

- file path or file stream reference
- input file name
- source type
- operator identity if available

## 10.3 Suggested Response Model

The response should contain at least:

- package id
- package name
- package status
- total entries
- imported count
- skipped duplicate count
- invalid count
- warnings
- errors

---

# 11. Layer Responsibilities in Import

## 11.1 Domain Responsibilities

The Domain should provide:

- entity integrity
- relationship integrity
- invariant support
- controlled enum/value usage

The Domain should not parse JSON files.

---

## 11.2 Application Responsibilities

The Application should provide:

- import orchestration
- file-level validation rules
- entry-level validation rules
- normalization orchestration
- duplicate-check flow
- status/result creation
- package summary logic

This is the center of the import use case.

---

## 11.3 Infrastructure Responsibilities

The Infrastructure should provide:

- file reading
- JSON parsing
- EF Core persistence
- duplicate lookup queries
- transaction handling
- package and content saving

---

## 11.4 ImportTool Responsibilities

The ImportTool should provide:

- operator interaction
- content-area selection
- folder selection or argument handling
- batch file discovery
- command execution trigger
- result display

The ImportTool should not own the business rules.

---

# 12. Recommended Internal Service Split

## 12.1 Suggested Services / Components

The import pipeline may be implemented with components like:

- `IContentPackageFileReader`
- `IContentPackageParser`
- `IContentImportValidator`
- `IWordDuplicateChecker`
- `IContentImportService`

These do not all have to be separate from day one if the code stays clean.
But the responsibilities should remain clear.

## 12.2 Practical Recommendation

In Phase 1, keep the number of abstractions moderate.

Do not create ten interfaces for a simple workflow unless each one adds real clarity.

---

# 13. Suggested Processing Strategy

## 13.1 Processing Mode

Recommended Phase 1 mode:

- sequential per-entry processing inside one import operation

## 13.2 Why Sequential Is Fine

It is:

- easier to debug
- easier to report
- easier to keep deterministic
- fully sufficient for early expected content sizes

Do not parallelize the import pipeline in Phase 1.

---

# 14. Transaction Strategy

## 14.1 Recommended Strategy

Use one transaction for the overall package if the expected file size remains reasonable.

## 14.2 Why

This ensures:

- no partial aggregate corruption
- package and entry tracking consistency
- easier rollback on fatal persistence failure

## 14.3 Practical Note

If later content packages become large, the transaction strategy may evolve.
That is not a Phase 1 problem.

---

# 15. Import Reporting Model

## 15.1 Report Categories

The import result should distinguish clearly between:

- fatal errors
- entry validation errors
- duplicate skips
- successful inserts
- warnings

## 15.2 Operator-Facing Output

At minimum, the operator should see:

- package identity
- file name
- counts
- first set of warnings/errors
- completion state

## 15.3 Development Value

Detailed import reporting is not decoration.
It is operationally important because content quality will evolve over time.

---

# 16. Recommended ContentPackageEntry Usage

## 16.1 Purpose

A `ContentPackageEntry` should act as an audit line for one attempted entry.

## 16.2 Suggested Stored Values

It should capture things like:

- raw lemma
- normalized lemma
- CEFR level
- part of speech
- processing status
- error or warning message
- linked imported word id if successful

## 16.3 Why This Matters

This allows later inspection of:

- why an entry failed
- which entries were duplicates
- which entries were successfully imported

---

# 17. Import Workflow Example

## Example Entry Outcomes

### Entry A
- valid
- not duplicate
- imported successfully

Outcome:
- content rows created
- ContentPackageEntry status = Imported

### Entry B
- valid structure
- duplicate identity found

Outcome:
- content rows not created
- ContentPackageEntry status = SkippedDuplicate
- warning logged

### Entry C
- invalid topic key

Outcome:
- content rows not created
- ContentPackageEntry status = Invalid
- validation error logged

---

# 18. Phase 1 Import Sequence Summary

Recommended sequence:

1. operator selects file
2. tool reads file
3. parser parses JSON
4. package validation runs
5. package record is created
6. entries are validated and normalized
7. duplicates are checked
8. valid entries are inserted
9. package entry results are recorded
10. package summary is finalized
11. result is returned to operator

---

# 19. Explicit Non-Goals

The following are explicitly out of scope for the Phase 1 import workflow:

- online import
- remote publishing
- approval stages
- review queues for editors
- auto-fix logic for bad content
- import rollback UI
- content merge wizard
- update existing entry by package
- multi-tenant import logic

---

# 20. Final Import Summary

The Phase 1 import workflow should be:

- local
- JSON-based
- validation-driven
- duplicate-aware
- insert-oriented
- traceable
- operator-friendly

The most important rule is simple:

**Import must be predictable.**
