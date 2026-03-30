# Server Content Distribution

## Purpose

This document defines the target architecture for moving Darwin Lingua from a purely local content model to a **server-authored content distribution model**.

The goal is to keep the mobile apps local-first at runtime while allowing a central backend to become the canonical source of shared content updates.

The same backend must also stay reusable for future learner apps beyond Darwin Deutsch.

This document builds on:

- `31-Solution-Architecture.md`
- `32-Storage-Strategy.md`
- `33-Offline-Strategy.md`
- `34-Import-Workflow.md`
- `37-Shared-Content-Server-Domain.md`

---

## 1. Target Outcome

The long-term content model should work like this:

1. the team updates one central server database
2. the Web API exposes content manifests and downloadable content packages
3. each mobile app keeps a local SQLite database for runtime reads and offline use
4. the mobile app can update:
   - the full local content database
   - one content area
   - one CEFR slice such as `A1`
5. learner-specific local state remains separate and must survive content updates
6. future learner apps can reuse the same backend and database without a redesign

This is a **hybrid local-first** model, not a thin-client model.

---

## 2. Source-of-Truth Model

### 2.1 Shared Content

The server database becomes the source of truth for shared content:

- languages
- topics
- lexical entries
- senses
- translations
- examples
- lexical metadata
- future support-resource content

The recommended default server database remains **PostgreSQL**.

### 2.2 Local Mobile Database

Each mobile installation keeps a local SQLite database that acts as:

- the runtime read model
- the offline cache
- the local persistence layer for learner state

The app must continue to read from SQLite for normal UX performance and offline stability.

### 2.3 User State

User state must remain logically separate from shared content:

- favorites
- meaning-language preferences
- UI-language preference
- word state
- practice/review state

Content updates must never wipe or recreate user-state tables.

---

## 3. Recommended Topology

### 3.1 Backend Components

The recommended backend shape is:

- one shared content database
- one Web API for mobile update and future web/admin clients
- one admin/import workflow that writes to the server database
- one publishing model that can serve multiple learner products

### 3.2 Mobile Components

The recommended mobile shape is:

- bundled starter seed for cold install
- local SQLite runtime database
- update client inside the MAUI app
- settings surface for manual update actions

### 3.3 Import Direction

The current import workflow should evolve from:

- `JSON -> local seed database`

to:

- `JSON -> server-side import workflow -> server database -> mobile update packages`

The current local seed flow still remains useful for development, testing, and fallback packaging.

---

## 4. Web API Responsibilities

The first server-facing API responsibilities should be:

- expose the latest content manifest
- expose available content areas and slices
- expose package metadata and package versioning
- expose downloadable package payloads
- support client checks for full update vs partial update

The first mobile-update API should stay read-only.

Admin editing, user accounts, analytics, and monetization should remain separate concerns.

### 4.1 Recommended First Endpoints

Recommended initial endpoint groups:

- `GET /api/mobile/content/manifest`
- `GET /api/mobile/content/areas`
- `GET /api/mobile/content/areas/{areaKey}/manifest`
- `GET /api/mobile/content/areas/catalog/cefr/{level}/manifest`
- `GET /api/mobile/content/packages/{packageId}`

Optional later endpoints:

- `POST /api/mobile/content/update-plan`
- `GET /api/mobile/content/full-snapshot`

---

## 5. Distribution Unit

### 5.1 Use Packages, Not Direct Table Sync

The mobile app should not sync raw tables over HTTP.

The correct unit of distribution is a **versioned content package**.

That package can represent:

- full catalog snapshot
- one content area
- one CEFR slice
- one topic slice later

### 5.2 Why Packages Are Better

Packages provide:

- stable import/update behavior
- resumable delivery model
- better auditability
- easier testing
- one shared mental model between import, backend, and mobile update

### 5.3 Package Metadata

Each package should have stable metadata such as:

- `packageId`
- `packageType`
- `areaKey`
- `sliceKey`
- `version`
- `createdAtUtc`
- `entryCount`
- `checksum`
- `minimumAppSchemaVersion`

---

## 6. Mobile Update Flow

### 6.1 Full Update

The `Update All Content` button in Settings should:

1. fetch the global manifest
2. compare local package receipts with server package versions
3. download missing or newer packages
4. apply them transactionally to SQLite
5. refresh the local update status

### 6.2 Area Update

Area-specific buttons should follow the same flow but target one logical area:

- `Catalog`
- `Practice content` later
- `Support resources` later

### 6.3 CEFR Update

For the word catalog, CEFR-specific buttons should target slices such as:

- `A1`
- `A2`
- `B1`
- `B2`
- `C1`
- `C2`

This lets the user or operator pull only the level they need.

---

## 7. Settings UX Direction

The Settings page should eventually expose three update tiers:

### Tier 1 - Full Database Update

- one primary button for updating all shared content

### Tier 2 - Section Update

- one button per logical content area

### Tier 3 - Slice Update

- for the word catalog, one button per CEFR level

Each action should show:

- current local version
- latest remote version
- pending package count
- pending word count where available
- last successful update time
- last failed update message if relevant

---

## 8. Update Semantics

### 8.1 Shared Content Is Server-Authored

For shared content, the server is authoritative.

### 8.2 Local User State Must Be Preserved

The update process must preserve:

- favorites
- word states
- practice history
- learning preferences

### 8.3 Transaction Rule

Each package application must be transactional.

Partial package application is not acceptable.

### 8.4 Package Receipt Rule

The local app should store package receipts so it knows:

- what has already been applied
- what version is currently local
- what changed during the last update

---

## 9. Offline Behavior

This server-backed model must still keep the app offline-first:

- startup must not block on the network
- browsing must continue to use local SQLite
- failed update checks must not break normal learning flows
- manual updates should fail gracefully when there is no connection

Auto-checking for updates can be added later, but auto-apply must remain careful and non-blocking.

---

## 10. Recommended Implementation Order

The recommended execution order is:

1. define server-side content package/version model
2. define mobile update manifest contract
3. add Web API host and read-only mobile content endpoints
4. add server-side import path into the central database
5. add mobile update client and update-status model
6. add Settings UI for:
   - update all
   - update by area
   - update by CEFR
7. add background-safe refresh and diagnostics

This order keeps the first slice focused on content distribution, not accounts or sync.

---

## 11. Non-Goals for the First Server Slice

The first server-backed content slice should not yet include:

- multi-user login
- user-state cloud sync
- social or collaborative features
- analytics dashboards
- monetization
- bi-directional conflict resolution

Those concerns should come later.

---

## 12. Final Recommendation

The correct architecture is:

- **PostgreSQL** as the central shared-content database
- **Web API** as the mobile-facing content-distribution layer
- **SQLite** as the mobile runtime/offline database
- **versioned content packages** as the update unit
- **settings-driven update controls** for full, area, and CEFR-slice updates
- **strict preservation of local user state** during content updates

This keeps Darwin Lingua local-first for learners while giving the team one canonical server-side content source.
