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

### Core Product and Planning

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

### Domain Reference

- `22-Domain-Model.md`
- `23-Domain-Rules.md`
- `24-Entity-Relationships.md`
- `25-Phase-1-Domain-Cut.md`
- `26-Bounded-Contexts.md`

### Architecture and Storage

- `31-Solution-Architecture.md`
- `32-Storage-Strategy.md`
- `33-Offline-Strategy.md`
- `36-Server-Content-Distribution.md`
- `37-Shared-Content-Server-Domain.md`
- `35-Engineering-Standards.md`

### Runbooks and Handoffs

- `41-Phase-1-Use-Cases.md`
- `42-Continuation-Handoff.md`
- `43-Phase-1-Release-Checklist.md`
- `44-Phase-1-Manual-Validation-Worksheet.md`
- `45-Phase-1-Release-Notes-Template.md`
- `46-Phase-2-Practice-Validation-Worksheet.md`
- `47-Phase-3-Mobile-UX-Validation-Worksheet.md`
- `48-Mobile-Validation-Bundle-Runbook.md`
- `49-Local-Postgres-Setup.md`

Related release-execution helpers live under `tools/Phase1/`.

---

## Recommended Reading Order

If you are new to the project, read the documents in this order:

1. `01-Product-Vision.md`
2. `02-Product-Scope.md`
3. `03-Product-Phases.md`
4. `21-Early-Product-Decisions.md`
5. `31-Solution-Architecture.md`
6. `32-Storage-Strategy.md`
7. `33-Offline-Strategy.md`
8. `36-Server-Content-Distribution.md`
9. `37-Shared-Content-Server-Domain.md`
10. `11-Content-Strategy.md`
11. `12-Content-Package-Format.md`
12. `14-Import-Rules.md`
13. `22-Domain-Model.md`
14. `25-Phase-1-Domain-Cut.md`
15. `26-Bounded-Contexts.md`
16. `35-Engineering-Standards.md`
17. `34-Import-Workflow.md`
18. `49-Local-Postgres-Setup.md`
19. `04-Implementation-Backlog.md`
20. `42-Continuation-Handoff.md`

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
- `33-Offline-Strategy.md` defines the local-first runtime behavior.
- `36-Server-Content-Distribution.md` defines the future server-authored content-update architecture.
- `37-Shared-Content-Server-Domain.md` defines the multi-product server-side domain and publishing model.

### Reference vs Runbooks

- domain and architecture documents should stay stable and explanatory
- release worksheets and runbooks should stay short and operational
- the backlog remains the main execution checklist

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

The documentation set is intentionally split into:

- a smaller core reading path
- stable reference documents
- execution runbooks

If future cleanup is needed, prefer consolidating overlapping runbooks before touching the core architecture/domain docs.
