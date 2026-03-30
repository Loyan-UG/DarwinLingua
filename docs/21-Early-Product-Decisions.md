# Early Product Decisions

## Purpose

This document records the early decisions that should be treated as active project direction unless they are replaced intentionally.

---

## Decision 1 - Official Import Format

The official Phase 1 import format is JSON.

---

## Decision 2 - Content Workflow

Content may be authored manually, generated with AI assistance, or produced through a hybrid workflow, but imported content must always conform to the approved package format.

---

## Decision 3 - Duplicate Import Behavior

Phase 1 import must skip duplicates and continue, while reporting the outcome clearly.

---

## Decision 4 - Controlled Growth

The product will grow in phases. Phase 1 must stay focused on a strong vocabulary-first release instead of trying to implement the full ecosystem immediately.

---

## Decision 5 - Architectural Style

The solution should begin as a clean modular monolith with disciplined internal boundaries.

---

## Decision 6 - Shared Core

The solution must be designed from the beginning for shared business logic across MAUI and future web/API/admin applications.

---

## Decision 7 - UI Localization From Day One

The application UI must be localizable from the start. Localization is not a later enhancement.

---

## Decision 8 - Initial UI Languages

The first UI languages are:

- English
- German

Additional UI languages may be introduced later.

---

## Decision 9 - UI Language Selection Behavior

The app should use the device language by default where supported, but the user must be able to change the UI language in settings.

---

## Decision 10 - User-Facing Strings

All user-facing strings must come from localization resources rather than hard-coded screen text.

---

## Decision 11 - Phase 1 Storage Choice

For the local-first MAUI application in Phase 1, SQLite is the primary storage choice.

---

## Decision 12 - Future Server Database Direction

If a future web/API/admin/backend part of the platform needs a stronger shared server database, PostgreSQL is the preferred default candidate unless a concrete requirement justifies another option.

---

## Decision 13 - Import Host Separation

The import workflow should be hosted separately from the learner-facing MAUI app, even if both reuse the same application core.

---

## Decision 14 - Engineering Quality Bar

Phase 1 code is not throwaway code. It must be written to production quality standards, including null-safety, localization discipline, testability, and documentation comments in English.

---

## Decision 15 - Local Profile Strategy for Phase 1

Phase 1 should use a single local learning profile per app installation.

This keeps the first release aligned with the local-first requirement and avoids introducing account or multi-user complexity before the core product is stable.

---

## Decision 16 - Phase 1 Audio Behavior

Phase 1 should use platform text-to-speech for German words and German example sentences.

If the platform does not support text-to-speech or does not have a compatible German voice available, the UI must fail gracefully with a localized non-blocking message instead of crashing or pretending playback succeeded.

---

## Decision 17 - Future Shared Content Source

When Darwin Lingua moves to a server-backed content model, the server becomes the source of truth for shared content and the mobile SQLite database becomes the local runtime/offline copy.

---

## Decision 18 - Content Update Unit

Mobile apps should receive shared content through versioned content packages exposed by the Web API, not through raw table synchronization.

---

## Decision 19 - Update Controls

The mobile app should eventually support three update scopes in Settings:

- update all shared content
- update one content area
- update one CEFR slice for the word catalog

---

## Decision 20 - Shared Backend for Multiple Learner Apps

Future learner apps should share the same backend infrastructure and PostgreSQL database, rather than using one isolated backend per app.

Shared content should be partitioned logically by product and learning-language scope, not by separate schemas or separate databases by default.
