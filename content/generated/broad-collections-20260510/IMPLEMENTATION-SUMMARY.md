# Broad Collections Implementation Summary

Generated on 2026-05-10 for DarwinLingua / Darwin Deutsch.

## Content Package Output

- Collections created or updated: 40
- Target words per collection: 100
- Actual words per collection: 100
- Total collection assignments: 4000
- Generated word batch files: 72
- Generated collection package files: 1
- Total JSON package files: 73
- Unique package IDs: 73
- Unique generated word keys: 2134
- Word batch size range: 29 to 30 entries

The collection package is intentionally collection-only with `entries: []`, so it does not create a duplicate anchor word. Word batches are imported first, then the final `zz` collection package assigns the resolved words to collections.

## Corrected WebApi Shared Import Result

- Files processed: 73
- Files succeeded: 73
- Files failed: 0
- Entries total: 2134
- Entries imported in the checked shared database: 5
- Existing duplicate entries reused by import in the checked shared database: 2129
- Invalid entries: 0
- Collections imported or updated after fix package: 40
- Collection assignments after fix package: 4000

The import target is the WebApi shared PostgreSQL catalog (`--target shared`), not the MAUI seed database.

## WebApi Endpoint Verification

- Visible collections from `/api/catalog/collections?meaningLanguageCode=en`: 41
- Existing `erp` collection retained: yes, 439 words
- Removed legacy starter collections absent: yes
- Target broad collections found: 40
- Target collection assignments: 4000
- Collections with a count other than 100: 0
- Duplicate assignments inside a collection: 0
- Required composite word index present: yes
- Required normalized lemma search index present: yes

## Collection Definition Audit

- Expected collection slugs: 40
- Actual collection slugs: 40
- Missing slugs: 0
- Unexpected slugs: 0
- Sort order mismatches: 0
- Image URL issues: 0
- Sort order range: 10 to 400

## Validation Commands Run

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File D:\_Projects\DarwinLingua\tools\ContentUtilities\TestBroadCollectionPackages.ps1 -ContentPath D:\_Projects\DarwinLingua\content\generated\broad-collections-20260510
dotnet build D:\_Projects\DarwinLingua\DarwinLingua.slnx --no-restore
dotnet test D:\_Projects\DarwinLingua\DarwinLingua.slnx --no-build
git diff --check
```

All commands passed. `git diff --check` reports only normal CRLF line-ending normalization warnings.

## Supporting Reports

- `de-broad-collections-20260510-report.txt`: source selection and per-collection generated package counts.
- `WEBAPI-IMPORT-REPORT.md`: corrected shared WebApi import result and cleanup notes.
- `README.md`: import and validation instructions for this package set.

## Notes

- The existing `erp` collection was retained.
- The legacy starter collections `crm-sales-playlist`, `warehouse-procurement`, and `project-meetings-b2` were deleted from the WebApi shared database and removed from automatic collection seeding.
- The MAUI seed database was restored so this broad collection content is not shipped through mobile seed.
- Existing words were reused when the importer detected duplicate normalized lemmas.
- Collection references include word, part of speech, and CEFR level.
- All package IDs are present and unique.
- Only existing topic keys were used.
- Labels are lowercase kebab-case.
- No words were intentionally skipped.
