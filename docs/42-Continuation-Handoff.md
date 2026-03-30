# Continuation Handoff (Codex / Chat Reset)

## Purpose

This document captures the **minimum context needed to continue implementation in a new chat** without losing execution state.

Use it when:

- the active coding chat becomes too long
- tooling context is truncated
- you need to hand work to a new agent run

---

## Current Snapshot (as of 2026-03-30)

- Phase 1 is still in progress.
- Core app flows exist: home, CEFR browse, topic browse, search, word details, favorites, settings.
- Home now includes dashboard quick actions for search, topic browse, and favorites in addition to CEFR shortcuts.
- The browse tab now acts as a real browse hub with CEFR shortcuts, topic browsing, and direct paths to search and favorites.
- Catalog domain hardening is complete for Phase 1 aggregate invariants, uniqueness constraints, and topic-link relationship constraints.
- Learning-domain separation is now enforced at the persistence boundary: local user-state rows stay decoupled from catalog-content foreign keys and survive content-row deletion.
- Current MAUI user-facing labels and lexical metadata text are now routed through `AppStrings` resource catalogs for both English and German.
- Shared MAUI reusable controls currently include:
  - `WordListItemView`
  - `TopicListItemView`
  - `DetailSectionView`
  - `CefrQuickFilterView`
  - `ActionBlockView`
- Database initialization, seed workflows, localization setup, and transactional write service are implemented.
- Full local Windows checks now succeed: `dotnet restore`, `dotnet build`, and `dotnet test` on `DarwinLingua.slnx`.
- On Windows, prefer `dotnet test DarwinLingua.slnx -c Debug --no-restore -m:1` to avoid transient MAUI Android file-lock failures inside `obj\Debug\net10.0-android`.
- Phase 1 release validation now has a dedicated checklist in `docs/43-Phase-1-Release-Checklist.md`.
- Manual device validation now has a dedicated worksheet in `docs/44-Phase-1-Manual-Validation-Worksheet.md`.
- Release sign-off and accepted-known-issues capture now have a template in `docs/45-Phase-1-Release-Notes-Template.md`.
- Windows release-prep automation now has a dedicated script in `tools/Phase1/Invoke-Phase1ReleasePrep.ps1` to capture restore/build/test evidence and prefill release metadata.
- Phase 1 release execution now also has a wrapper in `tools/Phase1/Start-Phase1ReleaseValidation.ps1` that creates a per-run validation bundle with the automated summary, a worksheet copy, and a release-notes draft.
- Automated release-readiness coverage now includes clean-install database initialization validation and sample content-package import validation.
- Automated release-readiness coverage now also validates import and browse/search responsiveness on a realistic starter dataset size.
- MAUI smoke coverage now also guards localized shell/page wiring and ensures core learner flows stay free of direct network dependencies.
- The canonical Phase 1 project/reference structure is now locked to the current modular-monolith layout documented in `docs/31-Solution-Architecture.md`.
- Starter localization reference data now seeds meaning-language support for `en`, `fa`, `ru`, `ar`, `pl`, `tr`, `ro`, `sq`, `ckb`, and `kmr`, with `en`/`de` still serving as the initial UI-language pair.
- The canonical Phase 1 sample content package now contains twelve German seed words across CEFR `A1`-`C2`, each carrying meanings in the seeded starter language set.
- Phase 2 now includes the new `Practice` bounded context, `GetPracticeOverview`, a due-aware deterministic `GetReviewQueue`, `StartReviewSession`, `GetRecentActivity`, `GetLearningProgressSnapshot`, `SubmitFlashcardAnswer`, and `SubmitQuizAnswer` with persisted attempt history and spaced-repetition scheduling updates.
- The MAUI app now exposes a localized `Practice` tab and home-screen entry point, plus a real practice overview screen with progress metrics, review-session preview, and recent activity backed by the Practice application services.
- The Practice UI now supports end-to-end flashcard and quiz sessions, including answer reveal, answer submission, per-answer feedback, and a session summary state, all localized through `AppStrings`.
- Practice now also has a dedicated `DarwinLingua.Practice.Application.Tests` project that covers review-queue/session delegation and quiz-answer submission behavior at the application-service layer.
- Practice infrastructure coverage now also includes query/persistence behavior for missing meanings and inactive content filtering, plus a release-readiness performance test over a realistic early-learning practice dataset.
- Manual/device-bound Practice validation now has a dedicated worksheet in `docs/46-Phase-2-Practice-Validation-Worksheet.md`.
- Phase 3 mobile lexical-intelligence slices now include imported usage/context labels, learner-facing grammar notes, collocations, word families, and synonym/antonym relations on `WordEntry`, all flowing through `GetWordDetails` into the upgraded word-detail screen, plus broader visual-consistency polish across the main learner-facing mobile screens.
- Manual/device-bound Phase 3 mobile UX validation now has a dedicated worksheet in `docs/47-Phase-3-Mobile-UX-Validation-Worksheet.md`.
- A single mobile validation bundle can now be prepared with `tools/Mobile/Start-MobileValidationBundle.ps1`, using the runbook in `docs/48-Mobile-Validation-Bundle-Runbook.md`.
- The next architectural direction is now explicitly defined as a server-authored content-distribution model with PostgreSQL as the shared-content source of truth, a Web API for package delivery, and mobile SQLite remaining the runtime/offline store; see `docs/36-Server-Content-Distribution.md`.
- The server-side domain and multi-product partitioning direction are now explicitly defined in `docs/37-Shared-Content-Server-Domain.md`.
- Local Docker Desktop setup guidance for PostgreSQL now exists in `docs/49-Local-Postgres-Setup.md` with matching config templates under `tools/Server`.
- Phase 5 planning now explicitly includes full, area, and CEFR-slice mobile content update flows in `docs/04-Implementation-Backlog.md`.
- CI (`.github/workflows/ci.yml`) runs restore/build/test on non-MAUI projects and test projects.

---

## Recommended Next Implementation Slice

Focus next on the first executable Phase 5 slice: the server-backed shared-content foundation.

Suggested scope:

1. Add the `DarwinLingua.WebApi` host and define the first mobile content manifest contract.
2. Define the first server-side package/version schema for shared content in PostgreSQL-friendly terms.
3. Keep the remaining manual mobile validation worksheets visible, but treat them as parallel release work rather than the next architecture slice.

---

## New Chat Prompt Template

Use the following prompt to resume implementation in a fresh chat:

```text
Continue DarwinLingua implementation from the latest commit.

Context:
- Read and follow docs/04-Implementation-Backlog.md and docs/42-Continuation-Handoff.md first.
- The next architecture slice is the server-authored content-distribution model documented in docs/36-Server-Content-Distribution.md.
- Prioritize the first Phase 5 foundation work: Web API host, mobile content manifest contract, and package/version model for shared content updates.
- Keep the mobile app local-first: local SQLite remains the runtime store and user state must survive content updates.
- Keep all user-facing text localized via AppStrings resources for any newly added UI.
- After code changes, update backlog/docs status accurately.
- Run the full local Windows .NET checks after changes.

Delivery requirements:
- Make focused, production-quality changes.
- Run available checks.
- Commit changes.
- Provide a concise summary with exact file references and next step suggestions.
```

---

## Handoff Checklist (Before Ending a Chat)

- [ ] Backlog status markers are updated to match reality.
- [ ] README “Current Status” reflects newly completed capabilities.
- [ ] This handoff file is updated with date + latest next-step recommendation.
- [ ] A ready-to-paste “new chat prompt” is present and current.
