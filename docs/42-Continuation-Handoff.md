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
- The first executable Phase 5 foundation slice now exists in `src/Apps/DarwinLingua.WebApi` with a local-only `appsettings.Development.Local.json` override pattern and read-only mobile manifest endpoints.
- The second Phase 5 foundation slice now persists `ClientProduct`, `ContentStream`, and `PublishedPackage` metadata in PostgreSQL-backed storage and serves manifests from that persistence layer.
- The third Phase 5 foundation slice now exposes package-download endpoints for package ID, full, area, and CEFR scopes, backed by payload files under `assets/ServerContent/PublishedPackages`, with compatibility checks via `clientSchemaVersion`.
- The fourth Phase 5 slice now runs the canonical content import pipeline on the server side, writes imported content into the shared PostgreSQL-backed catalog, generates versioned full/catalog/CEFR package payloads, and records server import receipts through `POST /api/admin/content/catalog/import`.
- The fifth Phase 5 slice now lets the MAUI client fetch manifests and full packages from the Web API, apply downloaded catalog packages transactionally into local SQLite, preserve favorites/preferences/word state/practice state during content replacement, and expose a primary `Update All Content` action with remote-version diagnostics in Settings.
- The sixth Phase 5 slice now adds local scope-receipt tracking plus granular remote update controls in Settings: one catalog-area action, separate `A1`-`C2` actions, and pre-apply version/word-count summaries for each scope.
- The seventh Phase 5 slice now makes server package lifecycle explicit: catalog import stages draft package batches, `POST /api/admin/content/catalog/publish` promotes one draft batch to published status, older published batches are superseded, and mobile manifest/download endpoints now expose published packages only.
- The eighth Phase 5 slice now enriches mobile remote-update diagnostics with scope keys, content area and slice metadata, package type, checksum visibility, schema-version visibility, and manifest-generation timestamps for each Settings update surface.
- The ninth Phase 5 slice now adds lightweight admin draft-management visibility: `GET /api/admin/content/catalog/drafts` lists staged and published package batches, and `GET /api/admin/content/catalog/drafts/{publicationBatchId}` exposes one batch with package-level metadata.
- The tenth Phase 5 slice now adds a cleanup flow for superseded server package batches: `DELETE /api/admin/content/catalog/drafts/{publicationBatchId}` removes only `Superseded` batches and deletes their payload files from package storage.
- The eleventh Phase 5 slice now adds publish-history visibility for admin operations: `GET /api/admin/content/catalog/history` returns draft/published/superseded batch history, and `GET /api/admin/content/catalog/history/summary` exposes lifecycle and retention counts per product.
- The twelfth Phase 5 slice now records recent remote update attempts on-device and shows the last few `full`, `catalog`, and `CEFR` update runs in Settings, including applied/current/failed outcomes.
- The thirteenth Phase 5 slice now adds admin rollback support: `POST /api/admin/content/catalog/rollback` re-activates one `Superseded` batch and supersedes the currently `Published` batch for the same product.
- The fourteenth Phase 5 slice now adds a publication audit trail: publish, rollback, and cleanup operations write `ContentPublicationEvents`, and `GET /api/admin/content/catalog/events` returns recent audited events per product.
- Manual/device-bound Phase 5 remote-update validation now has a dedicated worksheet in `docs/50-Phase-5-Remote-Update-Validation-Worksheet.md`.
- The shared mobile validation bundle now also includes the Phase 5 remote-update worksheet through `tools/Mobile/Start-MobileValidationBundle.ps1`.
- Phase 5 planning now explicitly includes full, area, and CEFR-slice mobile content update flows in `docs/04-Implementation-Backlog.md`.
- CI (`.github/workflows/ci.yml`) runs restore/build/test on non-MAUI projects and test projects.

---

## Recommended Next Implementation Slice

Focus next on the fifteenth executable Phase 5 slice: execute device validation and decide the remaining retention/rollback support after the publication audit trail landed.

Suggested scope:

1. Execute the Phase 5 remote-update worksheet on a target device or emulator against a live Web API.
2. Decide whether update history or rollback affordances are needed once device validation is complete.
3. Decide whether the admin side now needs a stronger retention policy or operator notes/search after history, cleanup, rollback, and audit support exist.

---

## New Chat Prompt Template

Use the following prompt to resume implementation in a fresh chat:

```text
Continue DarwinLingua implementation from the latest commit.

Context:
- Read and follow docs/04-Implementation-Backlog.md and docs/42-Continuation-Handoff.md first.
- The next architecture slice is the server-authored content-distribution model documented in docs/36-Server-Content-Distribution.md.
- Prioritize the next Phase 5 work: executing remote-update device validation, support-level polish for update diagnostics/history, and any needed admin-side retention or operator-audit refinement.
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
