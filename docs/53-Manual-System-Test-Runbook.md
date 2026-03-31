# Manual System Test Runbook

## Purpose

This runbook is the single end-to-end manual test path for the current Darwin Lingua system.

Use it when you want to:

- start the local PostgreSQL server
- start the local Web API
- import reviewed content into the shared server database
- publish that content for mobile delivery
- run the mobile app against the live Web API
- validate remote content sync and the main learner-facing app flows

This document intentionally combines the previously split bootstrap and validation flows into one executable manual path.

---

## Scope

This runbook covers:

- local PostgreSQL with Docker Desktop
- local `DarwinLingua.WebApi`
- server-side content import and publish
- mobile remote update flow
- core mobile regression checks after content sync

This runbook does not replace the detailed phase worksheets. It gives you one master execution path and points to the deeper worksheets when needed.

---

## Prerequisites

Before starting, make sure you have:

- Docker Desktop installed and running
- WSL 2 enabled
- `.NET 10 SDK` installed
- MAUI workloads installed
- an Android emulator or target device available
- reviewed JSON content files ready, for example under:
  - `D:\_Projects\DarwinLingua.Content`

Recommended:

- Visual Studio or a working MAUI deployment flow
- pgAdmin if you want to inspect PostgreSQL visually

---

## Files You Must Prepare

### 1. PostgreSQL Docker env file

Create:

- `tools/Server/Postgres/.env`

From:

- `tools/Server/Postgres/.env.example`

Replace all template passwords.

### 2. Web API local config

The local bootstrap flow accepts either of these files:

- preferred local-only file: `src/Apps/DarwinLingua.WebApi/appsettings.Development.Local.json`
- accepted fallback file: `src/Apps/DarwinLingua.WebApi/appsettings.Development.json`

If you want the safer local-only pattern, create:

- `src/Apps/DarwinLingua.WebApi/appsettings.Development.Local.json`

From:

- `src/Apps/DarwinLingua.WebApi/appsettings.Development.Local.example.json`

Put the real PostgreSQL passwords only in your active local config file.

### 3. Content folder

Prepare at least one reviewed JSON file, for example:

- `D:\_Projects\DarwinLingua.Content\A1.json`

Or a whole folder such as:

- `D:\_Projects\DarwinLingua.Content`

---

## Test Flow Overview

Recommended execution order:

1. start PostgreSQL
2. verify PostgreSQL is reachable
3. bootstrap the Web API and server database
4. import content into the main server database
5. publish the server package batch
6. verify manifest and package endpoints
7. run the mobile app
8. sync content from the server into the app
9. run mobile regression checks
10. optionally run the detailed worksheets for final sign-off

---

## Step 1. Start PostgreSQL

From the repository root run:

```powershell
docker compose --env-file tools/Server/Postgres/.env -f tools/Server/Postgres/docker-compose.yml up -d
```

### Expected Result

- Docker starts PostgreSQL successfully
- pgAdmin also starts if it is included in your compose setup
- no port conflict error appears for `5432`

### Verify

Run:

```powershell
docker compose --env-file tools/Server/Postgres/.env -f tools/Server/Postgres/docker-compose.yml ps
```

### Expected Result

- the PostgreSQL container is `running`
- the pgAdmin container is `running` if enabled

---

## Step 2. Verify PostgreSQL

Run:

```powershell
docker exec -it darwinlingua-postgres psql -U postgres -d darwinlingua_shared -c "\dt"
```

### Expected Result

- the command connects successfully
- if this is the very first run, the table list may still be empty
- no authentication or connection error appears

If authentication fails, fix:

- `tools/Server/Postgres/.env`
- the active Web API appsettings file

Make sure the passwords match.

---

## Step 3. Bootstrap the Local Server

If the Web API is not already running, use:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -StartWebApi -ContentPath "D:\_Projects\DarwinLingua.Content"
```

If the Web API is already running, use:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -ContentPath "D:\_Projects\DarwinLingua.Content"
```

You can also test with a single file:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -StartWebApi -ContentPath "D:\_Projects\DarwinLingua.Content\A1.json"
```

### What This Step Does

The script:

1. validates that a usable Web API settings file exists
2. starts the Web API if requested
3. waits for `/health`
4. imports one JSON file or all JSON files in the folder
5. stages a draft batch
6. publishes the latest staged batch
7. prints a short summary

### Expected Result

- PostgreSQL tables are created automatically if they do not exist
- content is imported into the main shared PostgreSQL database
- a published server package batch is created
- package payload files exist under:
  - `assets/ServerContent/PublishedPackages`

### If This Step Fails

Check:

- your active Web API appsettings file exists
- the PostgreSQL password in that file matches `tools/Server/Postgres/.env`
- no other process is already locking `DarwinLingua.WebApi.exe`

---

## Step 4. Verify Server Data Exists

### 4.1 Verify Health

Open:

- [http://localhost:5099/health](http://localhost:5099/health)

### Expected Result

- HTTP `200`
- a healthy response body

### 4.2 Verify Mobile Manifest

Open:

- [http://localhost:5099/api/mobile/content/manifest?clientProductKey=darwin-deutsch](http://localhost:5099/api/mobile/content/manifest?clientProductKey=darwin-deutsch)

### Expected Result

- HTTP `200`
- manifest contains published package metadata
- only published packages are visible

### 4.3 Verify Admin Drafts

Open:

- [http://localhost:5099/api/admin/content/catalog/drafts?clientProductKey=darwin-deutsch](http://localhost:5099/api/admin/content/catalog/drafts?clientProductKey=darwin-deutsch)

### Expected Result

- HTTP `200`
- at least one batch is listed
- the latest imported-and-published batch appears

### 4.4 Verify Admin History

Open:

- [http://localhost:5099/api/admin/content/catalog/history?clientProductKey=darwin-deutsch](http://localhost:5099/api/admin/content/catalog/history?clientProductKey=darwin-deutsch)

### Expected Result

- HTTP `200`
- the latest batch appears as `Published`

### 4.5 Verify Full Package Download

Open:

- [http://localhost:5099/api/mobile/content/download/full?clientProductKey=darwin-deutsch&clientSchemaVersion=1](http://localhost:5099/api/mobile/content/download/full?clientProductKey=darwin-deutsch&clientSchemaVersion=1)

### Expected Result

- HTTP `200`
- JSON package content is returned

---

## Step 5. Optional Database Inspection

If you want to inspect the main server database directly, use pgAdmin or run SQL through `psql`.

Suggested checks:

```sql
select count(*) from "WordEntries";
select count(*) from "ContentPackages";
select count(*) from "PublishedPackages";
select count(*) from "ClientProducts";
```

### Expected Result

- `WordEntries` is greater than `0`
- `ContentPackages` is greater than `0`
- `PublishedPackages` is greater than `0`
- `ClientProducts` contains at least `darwin-deutsch`

---

## Step 6. Start the Mobile App Against the Live Web API

### 6.1 Make Sure the Web API Is Still Running

If needed, run:

```powershell
dotnet run --project .\src\Apps\DarwinLingua.WebApi\DarwinLingua.WebApi.csproj
```

### Expected Result

- the API starts cleanly
- no database connection error appears

### 6.2 Deploy the Mobile App

Run the MAUI app on your Android emulator or target device.

Important local dev behavior:

- Android emulator uses `http://10.0.2.2:5099`
- other local targets generally use `http://localhost:5099`

### Expected Result

- the app opens successfully
- no startup crash occurs
- Settings opens successfully

---

## Step 7. Test Server-to-App Sync

Go to:

- `Settings`

You should see the remote update section with:

- `Update All Content`
- `Word catalog`
- separate CEFR update actions:
  - `A1`
  - `A2`
  - `B1`
  - `B2`
  - `C1`
  - `C2`

### 7.1 Test Full Update

Steps:

1. open `Settings`
2. inspect the full-update diagnostics
3. tap `Update All Content`
4. wait for completion

### Expected Result

- no crash
- success message appears
- diagnostics update to the latest published package
- local content becomes available in browse/search/detail flows

### 7.2 Test Catalog Update

Steps:

1. in `Settings`, find the catalog update action
2. tap the catalog update action

### Expected Result

- no crash
- update completes successfully
- catalog diagnostics update

### 7.3 Test One CEFR Slice Update

Steps:

1. in `Settings`, find `A1`
2. tap the `A1` update action

### Expected Result

- no crash
- `A1` diagnostics update
- `A1` content is available in CEFR browsing

---

## Step 8. Test Main Mobile Content Flows

After at least one successful update, test these screens.

### 8.1 Home

Steps:

1. open `Home`
2. verify the header and CEFR entry points render

### Expected Result

- page opens quickly
- no blank state caused by missing content

### 8.2 CEFR Browse

Steps:

1. tap `A1`
2. wait for the first word detail to open
3. use `Next`
4. use `Previous`
5. use `List`

### Expected Result

- the first word opens faster than the old heavy list-first behavior
- navigation works
- returning to the same level restores the last viewed word
- the list page opens without a long freeze

### 8.3 Search

Steps:

1. open `Search`
2. search for a known imported word
3. open the result

### Expected Result

- results appear
- tapping a result opens word detail
- no error dialog appears

### 8.4 Topic Browse

Steps:

1. open `Browse`
2. open one topic
3. scroll through the list

### Expected Result

- topic list loads
- words continue loading as you scroll
- no visible freeze or crash

### 8.5 Word Detail

Steps:

1. open one word
2. inspect meanings, examples, usage, grammar notes, collocations, family, and relations if available
3. tap favorite
4. tap known/difficult
5. tap speech buttons

### Expected Result

- detail page opens without crashing
- metadata sections render cleanly if content exists
- action buttons change state correctly
- speech works if the device has an available TTS engine and German voice

### 8.6 Favorites

Steps:

1. favorite one word
2. open `Favorites`

### Expected Result

- favorited word appears
- it remains after content updates

### 8.7 Practice

Steps:

1. open `Practice`
2. start a flashcard session
3. answer at least one item
4. start a quiz session

### Expected Result

- practice surfaces still load after remote content updates
- no content-update regression removes user learning state unexpectedly

---

## Step 9. Test Content Change Round Trip

This step validates the real editorial loop.

### Steps

1. modify or add content in one reviewed JSON file under `D:\_Projects\DarwinLingua.Content`
2. rerun:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -ContentPath "D:\_Projects\DarwinLingua.Content"
```

3. verify the server manifest or history endpoint shows a newer published batch
4. open the app
5. go to `Settings`
6. run:
   - `Update All Content`
   - or the specific catalog/CEFR update that matches your changed content
7. navigate to the changed word in the app

### Expected Result

- the server imports the changed content successfully
- a new published batch is created
- the mobile app downloads and applies the newer package
- the changed word or new word becomes visible in the app
- favorites, known/difficult state, and practice history remain intact

---

## Step 10. Test Failure Behavior

### 10.1 Web API Unavailable

Steps:

1. stop the Web API
2. open `Settings`
3. trigger a remote update action

### Expected Result

- no crash
- a clear update failure is shown
- the last failure message updates
- existing local content remains usable

### 10.2 Draft Package Guard

Steps:

1. import content into the server without publishing a new batch
2. inspect the app diagnostics again

### Expected Result

- the app still sees only the latest published batch
- draft package metadata is not offered to the mobile client

---

## Recommended Sign-Off Pass

After finishing the main runbook, execute these detailed worksheets as needed:

- `docs/44-Phase-1-Manual-Validation-Worksheet.md`
- `docs/46-Phase-2-Practice-Validation-Worksheet.md`
- `docs/47-Phase-3-Mobile-UX-Validation-Worksheet.md`
- `docs/50-Phase-5-Remote-Update-Validation-Worksheet.md`

If you want one evidence folder for the whole test run, first create a bundle with:

```powershell
pwsh ./tools/Mobile/Start-MobileValidationBundle.ps1
```

---

## Common Problems and Likely Causes

### The bootstrap script says the local Web API settings file does not exist

Check that one of these files exists:

- `src/Apps/DarwinLingua.WebApi/appsettings.Development.Local.json`
- `src/Apps/DarwinLingua.WebApi/appsettings.Development.json`

### PostgreSQL starts but import fails

Usually one of these is wrong:

- the database password in `tools/Server/Postgres/.env`
- the password in the active Web API appsettings file
- the database host or port

### Mobile app cannot reach the local Web API on Android emulator

Use the emulator bridge address:

- `http://10.0.2.2:5099`

Do not use `localhost` from inside the Android emulator for the host machine.

### Remote update fails but local content still exists

This is the correct safe behavior.

The app should preserve:

- local SQLite content already applied
- favorites
- user word state
- practice state

---

## Final Expected Outcome

At the end of a successful full manual system test:

- PostgreSQL is running locally
- the shared server database contains imported catalog content
- the Web API exposes published manifest and package endpoints
- the mobile app can download and apply published content
- the app can browse and search the imported words
- user state survives content updates
- error cases fail safely without wiping local data

If all of those are true, the current local end-to-end system is functioning correctly.
