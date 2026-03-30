# Phase 5 Remote Update Validation Worksheet

## Purpose

This worksheet captures the manual device validation needed for the Phase 5 remote content-update flow.

Use it when validating a build that can:

- talk to the shared `DarwinLingua.WebApi`
- fetch manifests for full, area, and CEFR scopes
- download published packages
- apply those packages into local SQLite without losing user state

This worksheet is not complete until every executed check has a result and notes.

---

## Build Under Test

- Build label:
- Build commit:
- Device / emulator:
- OS version:
- Web API base URL:
- Server environment:
- Tester:
- Validation date:

---

## Preconditions

Before starting, confirm:

- the Web API is running and reachable from the test device
- at least one published package exists for:
  - full database
  - catalog full area
  - one CEFR slice such as `A1`
- the mobile build is configured to use the intended Web API base URL
- the device already contains some local user state:
  - at least one favorite
  - at least one known/difficult word
  - at least one practice attempt if Practice is enabled in the build

Record the currently published package IDs:

- Full package:
- Catalog full package:
- A1 package:
- A2 package:
- B1 package:
- B2 package:
- C1 package:
- C2 package:

---

## Validation Matrix

Mark each row:

- `Pass`
- `Fail`
- `Not Run`

| Area | Scenario | Result | Notes |
| --- | --- | --- | --- |
| Setup | App opens Settings without error |  |  |
| Setup | Remote diagnostics section renders without blank labels or crashes |  |  |
| Setup | Scope/checksum/schema diagnostics are visible for full update |  |  |
| Setup | Scope/checksum/schema diagnostics are visible for catalog area update |  |  |
| Setup | Scope/checksum/schema diagnostics are visible for CEFR update actions |  |  |

---

## Full Database Update

### Scenario

Apply `Update All Content` against a server that has a newer published full package.

### Steps

1. Open `Settings`.
2. Confirm the full-update diagnostics show:
   - local package/version
   - remote package/version
   - checksum
   - schema version
   - manifest generated timestamp
3. Tap `Update All Content`.
4. Wait for completion.
5. Return to `Home`, `Browse`, and `Search`.

### Expected Result

- update finishes without crash
- success dialog appears
- diagnostics change to the new package/version/checksum
- word content reflects the newly published package
- favorites remain intact
- known/difficult state remains intact
- practice state remains intact

### Result

- Result:
- Notes:

---

## Catalog Area Update

### Scenario

Apply the catalog-area update when a newer published catalog package exists.

### Steps

1. Open `Settings`.
2. Inspect the `Word catalog` remote diagnostics.
3. Tap the catalog area update button.
4. Wait for completion.
5. Re-open the main browse surfaces.

### Expected Result

- update finishes without crash
- only catalog content is refreshed
- learner state remains intact
- diagnostics move to the newly applied package/version/checksum

### Result

- Result:
- Notes:

---

## CEFR Slice Update

### Scenario

Apply a single CEFR update, such as `A1`, when that slice has a newer published package.

### Steps

1. Open `Settings`.
2. Inspect the `A1` diagnostics and note package/version/checksum.
3. Tap the `A1` update button.
4. After completion, open the `A1` browse flow.
5. Open another level such as `B1` or `C1` and compare behavior.

### Expected Result

- the selected CEFR level refreshes successfully
- other CEFR levels remain available
- the selected level shows updated content
- favorites/known/difficult/practice state remains intact
- diagnostics for that CEFR slice move to the new package/version/checksum

### Result

- Result:
- Notes:

Repeat as needed:

- `A2` result:
- `B1` result:
- `B2` result:
- `C1` result:
- `C2` result:

---

## Offline and Failure Behavior

### Scenario

Validate that update actions fail safely when the server is unavailable.

### Steps

1. Disconnect the test device from the API or stop the Web API.
2. Open `Settings`.
3. Trigger:
   - full update
   - catalog area update
   - one CEFR update

### Expected Result

- no crash
- error dialog is shown
- last failure text updates
- existing local content remains usable
- existing user state remains intact

### Result

- Result:
- Notes:

---

## Publish-Lifecycle Guard

### Scenario

Validate that draft packages are not offered to the mobile client.

### Steps

1. Stage a new draft package batch on the server without publishing it.
2. Open `Settings` in the app.
3. Refresh the remote diagnostics or reopen the page.
4. Compare the visible remote package/version with server-side draft metadata.

### Expected Result

- mobile diagnostics still show the latest published package
- draft package ID/version is not exposed to the client
- no update button points to the unpublished draft batch

### Result

- Result:
- Notes:

---

## Regression Check

After at least one successful remote update, confirm:

- `Home` loads correctly
- CEFR browse still opens correctly
- `Search` returns results
- `Word detail` opens correctly
- `Favorites` still shows favorited words
- `Practice` still loads if the build includes Practice data

| Surface | Result | Notes |
| --- | --- | --- |
| Home |  |  |
| CEFR browse |  |  |
| Search |  |  |
| Word detail |  |  |
| Favorites |  |  |
| Practice |  |  |

---

## Sign-Off

- Overall recommendation:
  - `Ready`
  - `Ready with known issues`
  - `Not ready`
- Known issues:
- Follow-up work:
- Tester sign-off:
- Date:
