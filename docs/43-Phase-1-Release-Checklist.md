# Phase 1 Release Checklist

## Purpose

This checklist defines the minimum release-readiness gate for the first public Darwin Deutsch Phase 1 MVP build.

It is intentionally practical.

Use it before publishing any installer, test build, or internal milestone candidate that may be treated as a release.

Pair it with `docs/44-Phase-1-Manual-Validation-Worksheet.md` for the device-bound checks.
Use `docs/45-Phase-1-Release-Notes-Template.md` to record the final release decision and accepted known issues.
Use `tools/Phase1/Invoke-Phase1ReleasePrep.ps1` to collect the automated Windows restore/build/test evidence before the manual worksheet begins.

---

## Preconditions

The following must already be true before running the checklist:

- current branch is merged or otherwise approved for release
- `docs/04-Implementation-Backlog.md` reflects the actual implementation state
- the app builds locally on the release workstation
- the intended content package for the release is available

---

## Build And Test Gate

- [ ] run `dotnet restore DarwinLingua.slnx`
- [ ] run `dotnet build DarwinLingua.slnx -c Debug`
- [ ] run `dotnet test DarwinLingua.slnx -c Debug --no-restore -m:1`
- [ ] confirm there are no unexpected build warnings or test failures
- [ ] confirm the MAUI app starts on the Windows target machine

---

## Data And Initialization Gate

- [ ] validate first-run database initialization on a clean local app data directory
- [ ] confirm reference-data seeding creates supported languages and initial topics
- [ ] confirm existing local databases migrate forward without data-loss regressions
- [ ] validate sample content package import completes successfully
- [ ] confirm duplicate `packageId` rejection still works as expected

---

## UX And Localization Gate

- [ ] validate core navigation: home, browse, search, topic words, CEFR words, word details, favorites, settings
- [ ] validate English UI copy on the target build
- [ ] validate German UI copy on the target build
- [ ] validate language switching updates visible UI labels without restart regressions
- [ ] validate user-facing empty/loading/error states on core data-driven pages
- [ ] validate lexical metadata, favorites actions, and learning-state actions show localized UI text

---

## Offline And Device Capability Gate

- [ ] validate core browse and search flows with network access disabled
- [ ] validate TTS success path on a supported device
- [ ] validate graceful TTS failure messaging on a device without a compatible German voice
- [ ] validate app behavior after restart with previously imported data present

---

## Performance Gate

- [ ] import a realistic starter dataset and record import duration
- [ ] validate browse/search responsiveness on that dataset
- [ ] confirm no unacceptable startup regression after initialization and seeding

---

## Release Notes Gate

- [ ] capture commit hash used for the release build
- [ ] record the sample content package version used in validation
- [ ] list known issues accepted for the release
- [ ] record the validation date and responsible reviewer

---

## Exit Rule

Do not call the build release-ready until every required checkbox above is complete or an explicit waiver is recorded for the skipped item.
