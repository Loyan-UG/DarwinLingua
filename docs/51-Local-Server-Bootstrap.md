# Local Server Bootstrap

## Purpose

This runbook initializes the local shared PostgreSQL databases, imports the first catalog content, and publishes the first package batch for mobile updates.

Use it after:

- Docker Desktop PostgreSQL is running
- local Web API secrets are configured
- you have one or more content JSON files ready to import

---

## Prerequisites

1. Complete `docs/49-Local-Postgres-Setup.md`.
2. Create `src/Apps/DarwinLingua.WebApi/appsettings.Development.Local.json` from:
   - `src/Apps/DarwinLingua.WebApi/appsettings.Development.Local.example.json`
3. Put the real PostgreSQL passwords only in:
   - `src/Apps/DarwinLingua.WebApi/appsettings.Development.Local.json`
   - `tools/Server/Postgres/.env`
4. Ensure your content files exist, for example:
   - `D:\_Projects\DarwinLingua.Content\A1.json`
   - or a folder such as `D:\_Projects\DarwinLingua.Content`

---

## What The Script Does

`tools/Server/Initialize-LocalServerContent.ps1` performs this flow:

1. verifies that local Web API settings exist
2. optionally starts the Web API
3. waits for `/health`
4. imports one JSON file or every JSON file in a folder
5. publishes the latest staged draft batch
6. prints a short summary with imported counts and published package IDs

Starting the Web API is enough to create the required PostgreSQL tables because the host already runs startup initialization for:

- shared catalog tables
- server-content metadata tables
- publication audit tables

---

## Recommended Command

If the Web API is not already running:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -StartWebApi -ContentPath "D:\_Projects\DarwinLingua.Content"
```

If the Web API is already running:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -ContentPath "D:\_Projects\DarwinLingua.Content"
```

You can also import a single file:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -StartWebApi -ContentPath "D:\_Projects\DarwinLingua.Content\A1.json"
```

---

## Expected Result

At the end you should have:

- PostgreSQL tables created
- imported catalog content inside the shared server database
- one published batch available through the Web API
- package payload files under `assets/ServerContent/PublishedPackages`

You can then verify:

- `GET http://localhost:5099/health`
- `GET http://localhost:5099/api/mobile/content/manifest?clientProductKey=darwin-deutsch`
- `GET http://localhost:5099/api/admin/content/catalog/history?clientProductKey=darwin-deutsch`

---

## Remaining Manual Work

After this bootstrap, the main remaining local tasks are:

1. run the mobile app against the live Web API
2. execute `docs/50-Phase-5-Remote-Update-Validation-Worksheet.md`
3. decide whether you want stronger retention rules or operator-search/notes on the admin side
