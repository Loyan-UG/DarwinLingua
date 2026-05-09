# Web Seed Coverage And Admin Reports

## Purpose

This document records the Web-focused seed coverage and the admin reporting surface required for local development, manual QA, and future automated test work.

## Seed Coverage Status

The Web seed set is split by responsibility:

- `content/generated/de-a1-a2-practical-dialogues-001.json`
  - dialogue lessons: 16 across A1, A2, B1, and B2
  - conversation starter packs: 7 across A1, A2, B1, and B2
  - event preparation packs: 7 across A1, A2, B1, and B2
- `content/generated/de-event-directory-seeds-001.json`
  - source references: 7
  - organizer profiles: 7
  - conversation events: 7
  - organizer profile owners: 4
  - organizer claim requests: 4
  - event RSVPs: 8
  - learner conversation profiles: 6
  - partner requests: 5
  - user reports: 4
  - user blocks: 3
  - moderation decision audits: 3

The dialogue, starter, and preparation-pack file follows the normal content-import package path.

The event-directory file is an operational seed file. Its records mirror the current Web/Admin request and response shapes and can be applied through:

```powershell
.\tools\Server\Initialize-LocalWebOperationalSeeds.ps1 -ApiBaseUrl "http://localhost:5099"
```

Add `-StartWebApi` when the local Web API is not already running.

Admin and private operational WebApi routes are protected by the internal admin API key header:

- header: `X-DarwinLingua-Admin-Key`
- local development default: `local-dev-admin-api-key-change-me`
- override for scripts: `-AdminApiKey "..."` or `DARWINLINGUA_ADMIN_API_KEY`

The Web project sends this header through `WebApi:AdminApiKey` when calling WebApi server-to-server. Public catalog read routes remain callable without the internal key.

## Required Seed Coverage Rule

Each new Web area should keep at least three representative records unless the area is binary or singleton by design.

Required areas:

- dialogue lessons and roleplay-ready dialogue turns
- conversation starter packs
- event preparation packs
- organizer profiles
- conversation events
- event RSVPs
- organizer profile claims
- organizer profile owner assignments
- learner conversation profiles
- partner requests across all major states
- moderation reports
- user blocks
- moderation decision audit rows
- admin/identity users and entitlement tiers

Identity users are environment-backed bootstrap records rather than content seed records. Local seed credentials must stay out of committed JSON files.

## Admin Report Surface

The Web Admin area includes `/admin/reports`, and the `/admin` overview now surfaces the highest-signal catalog, social-learning, and moderation counts.

The page summarizes:

- Identity user count from the Web Identity store
- catalog counts for active words, draft words, topics, dialogues, starter packs, and preparation packs
- social-learning counts for organizers, events, online events, RSVPs, claims, owners, learner profiles, public profiles, and partner requests
- moderation counts for reports, pending reports, blocks, and decision audits
- operations counts for imported packages, failed imports, and last import time
- top in-memory Web product analytics counters

The report data comes from `/api/admin/catalog/system-report` plus the Web Identity store and Web product analytics service.

The `/admin` overview uses the same system-report source for its operational snapshot so the first admin screen is not limited to catalog/import counts.

## Known Follow-Up

The operational seed script uses public/admin Web API routes, so it preserves normal validation behavior. A lower-level database seed path is still optional future work if fresh PostgreSQL environments need to be prepared without running WebApi.
