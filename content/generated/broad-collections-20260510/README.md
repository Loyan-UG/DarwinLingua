# Broad Vocabulary Collections 2026-05-10

Importable content package set for the DarwinLingua / Darwin Deutsch broad vocabulary collections.

## Contents

- `de-broad-collections-20260510-words-001.json` through `de-broad-collections-20260510-words-072.json`
- `de-broad-collections-20260510-zz-collections.json`
- `de-broad-collections-20260510-report.txt`
- `de-broad-collections-20260510-import-report.txt`
- `de-broad-collections-20260510-db-verification.txt`
- `IMPLEMENTATION-SUMMARY.md`
- `FILES-CHANGED.md`
- `PER-COLLECTION-SUMMARY.csv`

The collections file is intentionally prefixed with `zz` so folder imports process all word batches first.
Report and summary files do not use `.json`, so the import tool does not treat them as content packages.

## Seed Import

```powershell
dotnet run --project D:\_Projects\DarwinLingua\src\Apps\DarwinLingua.ImportTool\DarwinLingua.ImportTool.csproj -- --yes D:\_Projects\DarwinLingua\content\generated\broad-collections-20260510
```

Clean seed import result:

- Files processed: 73
- Files succeeded: 73
- Files failed: 0
- Entries total: 2134
- Entries imported: 2092
- Duplicate skips: 42
- Invalid entries: 0
- Collections imported/updated: 40
- Collection assignments: 4000

Direct seed database verification:

- Target collection slugs found: 40/40
- Collections with a word count other than 100: 0
- Duplicate assignments inside a collection: 0
- Total collection assignments: 4000

`de-broad-collections-20260510-report.txt` reports reuse from existing generated content files in the repository. `de-broad-collections-20260510-db-verification.txt` reports reuse from the pre-import seed database versus words imported by this batch.

## Validation

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File D:\_Projects\DarwinLingua\tools\ContentUtilities\TestBroadCollectionPackages.ps1 -ContentPath D:\_Projects\DarwinLingua\content\generated\broad-collections-20260510
```

Latest validation results:

- Content package audit: passed
- Required meaning/example translation language coverage: passed
- Collection localization and metadata coverage: passed
- Priority collection word counts: 100 each
- `dotnet build DarwinLingua.slnx --no-restore`: passed with 0 warnings and 0 errors
- `dotnet test DarwinLingua.slnx --no-build`: passed
- `git diff --check`: passed, with only line-ending normalization warnings

## Notes

- Existing words are reused by normalized lemma where the importer skips duplicates.
- Collection references include `word`, `partOfSpeech`, and `cefrLevel`.
- Only existing topic keys are used.
- Labels are lowercase kebab-case and defined inside each word batch package.
- The final collections package is collection-only (`entries: []`) so it does not duplicate a vocabulary entry as an anchor.
