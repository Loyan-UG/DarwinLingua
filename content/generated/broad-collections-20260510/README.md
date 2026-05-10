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

## WebApi Shared Database Import

```powershell
dotnet run --project D:\_Projects\DarwinLingua\src\Apps\DarwinLingua.ImportTool\DarwinLingua.ImportTool.csproj -- --target shared --yes D:\_Projects\DarwinLingua\content\generated\broad-collections-20260510
```

Corrected WebApi shared-database import result:

- Files processed: 73
- Files succeeded: 73
- Files failed: 0
- Entries total: 2134
- Entries imported in the checked shared database: 5
- Duplicate skips in the checked shared database: 2129
- Invalid entries: 0
- Collections imported/updated after fix package: 40
- Collection assignments after fix package: 4000

The WebApi collection endpoint verification at `http://localhost:53945/api/catalog/collections?meaningLanguageCode=en` returned:

- Visible collections: 41 (`erp` plus 40 broad collections)
- `erp` retained with 439 words
- Removed starter collections absent: `crm-sales-playlist`, `warehouse-procurement`, `project-meetings-b2`
- Target broad collection slugs found: 40/40
- Collections with a word count other than 100: 0
- Total collection assignments: 4000

`de-broad-collections-20260510-report.txt` reports reuse from existing generated content files in the repository. The older seed verification files are superseded by `WEBAPI-IMPORT-REPORT.md`.

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

- These packages are intended for the WebApi shared catalog. They should not be imported into the MAUI seed database unless mobile seed content is explicitly requested.
- Existing words are reused by normalized lemma where the importer skips duplicates.
- Collection references include `word`, `partOfSpeech`, and `cefrLevel`.
- Only existing topic keys are used.
- Labels are lowercase kebab-case and defined inside each word batch package.
- The final collections package is collection-only (`entries: []`) so it does not duplicate a vocabulary entry as an anchor.
