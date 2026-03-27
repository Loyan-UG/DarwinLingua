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
- Automated release-readiness coverage now includes clean-install database initialization validation and sample content-package import validation.
- Automated release-readiness coverage now also validates import and browse/search responsiveness on a realistic starter dataset size.
- MAUI smoke coverage now also guards localized shell/page wiring and ensures core learner flows stay free of direct network dependencies.
- The canonical Phase 1 project/reference structure is now locked to the current modular-monolith layout documented in `docs/31-Solution-Architecture.md`.
- CI (`.github/workflows/ci.yml`) runs restore/build/test on non-MAUI projects and test projects.

---

## Recommended Next Implementation Slice

Focus next on the remaining Phase 1 release-execution work. The main engineering implementation slices are effectively complete; the remaining gaps are manual validation and final sign-off capture.

Suggested scope:

1. Execute the manual device worksheet for offline behavior, English UI, German UI, and TTS validation.
2. Capture accepted known issues and the final release recommendation in the release-notes template.
3. If manual findings require code changes, keep all new user-facing labels localized through `AppStrings` and rerun the Windows checks.

---

## New Chat Prompt Template

Use the following prompt to resume implementation in a fresh chat:

```text
Continue DarwinLingua Phase 1 implementation from the latest commit.

Context:
- Read and follow docs/04-Implementation-Backlog.md and docs/42-Continuation-Handoff.md first.
- Prioritize executing the remaining manual Phase 1 validation worksheet and capturing release sign-off results.
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
