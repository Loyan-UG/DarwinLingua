# WebApi Import Report

Date: 2026-05-10

## Target

Imported into the WebApi shared PostgreSQL catalog with `--target shared`.

The MAUI seed database is not the target for these broad collections and was restored so the broad collection content is not shipped through mobile seed.

## Actions

- Kept the existing `erp` collection.
- Deleted `crm-sales-playlist`, `warehouse-procurement`, and `project-meetings-b2` from the WebApi shared database.
- Removed those three legacy starter collections from `CatalogWordCollectionSeeder` so they are not recreated by database initialization.
- Imported the 40 broad collections and 72 word batches into the shared catalog.
- Added a one-file fix package for `time-calendar-planning` after correcting two collection references:
  - `achtzehn`: `Numeral`, `A1`
  - `an`: `Preposition`, `A1`

## Import Commands

```powershell
dotnet run --project D:\_Projects\DarwinLingua\src\Apps\DarwinLingua.ImportTool\DarwinLingua.ImportTool.csproj -- --target shared --yes D:\_Projects\DarwinLingua\content\generated\broad-collections-20260510
dotnet run --project D:\_Projects\DarwinLingua\src\Apps\DarwinLingua.ImportTool\DarwinLingua.ImportTool.csproj -- --target shared --yes D:\_Projects\DarwinLingua\content\generated\broad-collections-20260510-fixes\de-broad-collections-20260510-time-calendar-fix.json
```

## Final Verification

- Public WebApi collection count: 41
- Existing `erp` collection: present, 439 words
- Broad collections: 40/40 present
- Broad collection word counts: 100 each
- Broad collection assignments: 4000
- Removed legacy starter collections visible: 0

No words were intentionally skipped.
