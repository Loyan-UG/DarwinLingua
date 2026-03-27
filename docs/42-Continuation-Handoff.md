# Continuation Handoff (Codex / Chat Reset)

## Purpose

This document captures the **minimum context needed to continue implementation in a new chat** without losing execution state.

Use it when:

- the active coding chat becomes too long
- tooling context is truncated
- you need to hand work to a new agent run

---

## Current Snapshot (as of 2026-03-27)

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
- CI (`.github/workflows/ci.yml`) runs restore/build/test on non-MAUI projects and test projects.

---

## Recommended Next Implementation Slice

Focus next on Phase 2 practice quality and release-readiness work while keeping the remaining Phase 1 manual release checks visible.

Suggested scope:

1. Add direct application-level tests for review and quiz use cases, not only infrastructure-backed integration coverage.
2. Validate Phase 2 performance against a realistic early-learning dataset and document the bound used.
3. Keep the remaining manual device worksheet items for offline behavior, English UI, German UI, and TTS queued for final Phase 1 sign-off, then extend that validation to Practice flows.

---

## New Chat Prompt Template

Use the following prompt to resume implementation in a fresh chat:

```text
Continue DarwinLingua implementation from the latest commit.

Context:
- Read and follow docs/04-Implementation-Backlog.md and docs/42-Continuation-Handoff.md first.
- Phase 1 release validation still has manual device-bound checks open, but Phase 2 implementation has now started.
- Prioritize the next ordered Phase 2 quality items under the Practice backlog, continuing from the implemented MAUI practice tab, overview screen, and session UI.
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
