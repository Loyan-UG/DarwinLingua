# Engineering Standards

## Purpose

This document defines the implementation standards that must be followed across the solution.

These rules exist to keep the codebase:

- safe
- understandable
- maintainable
- localized
- ready for future expansion

All source comments and technical documentation must be written in English.

---

## 1. General Principles

- write production-quality code from the start
- prefer clarity over cleverness
- avoid hidden technical debt
- keep architecture boundaries explicit
- keep implementation consistent across modules

If a compromise is ever accepted, it must be documented explicitly rather than left as an invisible shortcut.

---

## 2. Null-Safety Standard

Null handling is mandatory.

### Rules

- nullable reference types must remain enabled
- all external input must be validated
- all infrastructure results that may be missing must be handled explicitly
- all optional values must be modeled intentionally
- null-forgiving operators should be avoided unless there is a documented and justified reason

The codebase must not rely on optimistic assumptions that can lead to runtime null failures.

---

## 3. Technical Debt Standard

The default rule is to avoid introducing technical debt in the main implementation path.

### Rules

- do not leave known broken paths behind temporary TODOs
- do not ship placeholder logic as if it were complete behavior
- do not accept duplicated logic when a clean shared implementation is practical
- do not postpone core correctness work that will obviously be required immediately after

If debt is deliberately accepted, it must be:

- small
- isolated
- documented
- scheduled in the backlog

---

## 4. Commenting Standard

Commenting is required, but comments must stay useful.

### Required Comment Coverage

- classes must have English comments describing responsibility
- methods must have English comments describing behavior, inputs, outputs, and important constraints
- private or internal methods may use brief comments if they are truly small and obvious, but they still should not be left undocumented by default
- important logic blocks inside methods must have English comments when they implement non-obvious rules

### Comment Quality Rule

Comments must explain intent and important decisions, not merely repeat the code line-by-line.

---

## 5. Localization Standard

Localization is a hard requirement from the beginning.

### UI Languages

The first supported UI languages are:

- English
- German

### Rules

- all user-visible strings must come from resource files
- screens must not hard-code UI text
- validation messages shown to users must be localizable
- empty states, errors, labels, and settings text must also be localized
- the default UI language should follow the device language where supported
- the user must be able to change the UI language in settings

---

## 6. UI and UX Standard

UI/UX is part of product quality, not a cosmetic afterthought.

### Rules

- screens must be designed intentionally, not left as template-level placeholders
- multilingual text must remain readable and well structured
- layout must behave correctly on supported device sizes
- accessibility basics must be respected
- error, loading, and empty states must be designed explicitly
- offline behavior must feel consistent and honest to the user

---

## 7. Resource Usage Standard

The solution must separate internal identifiers from localized display strings.

### Rules

- use stable keys for topics, languages, and internal values
- use resource files or localization tables for display text
- do not use display text as business identifiers

---

## 8. Architecture Boundary Standard

The solution must respect the documented layer and context boundaries.

### Rules

- domain code must not depend on UI or infrastructure concerns
- application code must not depend on MAUI or web frameworks
- import-specific parsing DTOs must not be treated as domain entities
- user state must remain separate from shared content

---

## 9. Testing Standard

Testing is required for the important rules and workflows.

### Minimum Expectations

- domain invariants should have tests
- import validation and duplicate handling should have tests
- persistence mapping should have tests where the risk justifies it
- major application use cases should have tests

---

## 10. Documentation Standard

- README must remain aligned with the real repository state
- backlog must remain aligned with the real work state
- implementation decisions with long-term impact should be documented
- new modules and important workflows should update the relevant docs

---

## 11. Migration Standard

Database schema changes must use EF Core migrations.

### Rules

- do not introduce schema drift by editing local SQLite files manually
- do not add new tables, columns, or indexes without a migration
- use the local tool manifest and run `dotnet tool restore` before migration commands when needed
- keep startup initialization migration-based
- treat `EnsureCreated` as legacy compatibility only, not as the normal forward path

### Current Practical Workflow

Typical workflow for schema changes:

1. update entity/configuration code
2. create a migration
3. build and test the solution
4. keep README and backlog aligned if the storage workflow changed materially

---

## 12. Final Rule

The project standard is:

**clean, explicit, localized, null-safe, and production-ready code.**
