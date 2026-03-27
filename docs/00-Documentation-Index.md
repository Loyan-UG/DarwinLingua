# Documentation Index

## Purpose

This document is the canonical index for the project documentation.

It exists to:

- define the official documentation set
- provide a recommended reading order
- reduce duplicate documents
- keep file naming and numbering consistent
- make future maintenance easier

All project documentation should remain in English.

---

## Official Documentation Set

### Product

- `01-Product-Vision.md`
- `02-Product-Scope.md`
- `03-Product-Phases.md`
- `04-Implementation-Backlog.md`
- `21-Early-Product-Decisions.md`

### Content and Import

- `11-Content-Strategy.md`
- `12-Content-Package-Format.md`
- `13-Entry-Structure.json`
- `14-Import-Rules.md`
- `15-Topic-Seed-Ideas.md`
- `34-Import-Workflow.md`

### Domain

- `22-Domain-Model.md`
- `23-Domain-Rules.md`
- `24-Entity-Relationships.md`
- `25-Phase-1-Domain-Cut.md`
- `26-Bounded-Contexts.md`

### Architecture and Delivery

- `31-Solution-Architecture.md`
- `32-Storage-Strategy.md`
- `33-Offline-Strategy.md`
- `35-Engineering-Standards.md`
- `41-Phase-1-Use-Cases.md`
- `42-Continuation-Handoff.md`
- `43-Phase-1-Release-Checklist.md`
- `44-Phase-1-Manual-Validation-Worksheet.md`

---

## Recommended Reading Order

If you are new to the project, read the documents in this order:

1. `01-Product-Vision.md`
2. `02-Product-Scope.md`
3. `03-Product-Phases.md`
4. `21-Early-Product-Decisions.md`
5. `11-Content-Strategy.md`
6. `12-Content-Package-Format.md`
7. `14-Import-Rules.md`
8. `22-Domain-Model.md`
9. `23-Domain-Rules.md`
10. `25-Phase-1-Domain-Cut.md`
11. `26-Bounded-Contexts.md`
12. `31-Solution-Architecture.md`
13. `32-Storage-Strategy.md`
14. `33-Offline-Strategy.md`
15. `35-Engineering-Standards.md`
16. `34-Import-Workflow.md`
17. `41-Phase-1-Use-Cases.md`
18. `04-Implementation-Backlog.md`
19. `42-Continuation-Handoff.md`
20. `43-Phase-1-Release-Checklist.md`
21. `44-Phase-1-Manual-Validation-Worksheet.md`

---

## Document Roles

### Vision vs Scope

- `01-Product-Vision.md` defines why the product exists and what it should become.
- `02-Product-Scope.md` defines what is in scope and out of scope, especially for Phase 1.

### Phases vs Backlog

- `03-Product-Phases.md` defines the high-level product phases.
- `04-Implementation-Backlog.md` defines executable work planning, with detailed Phase 1 tasks.

### Rules vs Workflow

- `14-Import-Rules.md` defines stable import rules and constraints.
- `34-Import-Workflow.md` defines the end-to-end processing flow and responsibilities.

### Domain vs Architecture

- `22-Domain-Model.md` explains the domain concepts.
- `23-Domain-Rules.md` defines invariant and lifecycle rules.
- `31-Solution-Architecture.md` defines project/layer structure and dependency direction.
- `32-Storage-Strategy.md` defines the SQLite, migration, and indexing direction for Phase 1.

---

## File Naming Rule

Documentation file names must follow this pattern:

- numeric prefix for reading order
- short stable English title
- words separated with hyphens
- no spaces
- no draft-only names unless the file is truly temporary

Examples:

- `01-Product-Vision.md`
- `32-Storage-Strategy.md`
- `35-Engineering-Standards.md`

---

## Maintenance Rules

- Do not keep duplicate documents that describe the same concern.
- If a document becomes obsolete, remove it instead of leaving conflicting copies.
- Keep README as the repository entry point, not as the place for every detail.
- Keep detailed implementation rules in the dedicated docs, not buried in issue text or chat history.
- Update internal references when files are renamed.

---

## Current Cleanup Result

The following draft/redundant files were removed during documentation cleanup:

- `00- Solution Folders.md`
- `00- Stracture.md`

Their useful content was folded into this index, the README, and the backlog/architecture documents.
