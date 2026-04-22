# Web Operations Notes

## Purpose

This document captures the production-focused logging and diagnostics expectations for `DarwinLingua.Web`.

---

## Logging Baseline

- keep structured ASP.NET Core request logging enabled
- keep application/service errors visible at `Error`
- avoid noisy framework logging at `Information` in production unless actively diagnosing an issue
- log startup failures, database initialization failures, and configuration faults clearly

---

## Diagnostics Focus Areas

Prioritize visibility into:

- startup initialization success/failure
- shared database connectivity
- browse/search/detail errors
- htmx POST failures for learner state changes
- service worker registration or manifest delivery failures
- admin-route authorization failures after Identity is added

---

## Compression and Asset Caching Review

Current baseline:

- response compression is enabled for HTTPS responses
- static assets are referenced with `asp-append-version` where applicable
- PWA shell assets are explicitly versioned by the service worker
- output caching is already applied to landing and catalog browse slices

Review rule before release:

1. confirm text responses are compressed in the deployed environment
2. confirm versioned CSS and JS assets receive long-lived cache headers at the edge or host
3. confirm manifest and service worker updates are not pinned behind stale caching rules
4. confirm htmx partial endpoints are not cached in a way that leaks user-specific state
5. confirm any future image set uses appropriately sized assets instead of oversized originals

---

## UX Telemetry Direction

Current lightweight telemetry baseline now covers:

- slow server-side learner and admin requests through request-duration aggregation
- failed `htmx` request paths through request and client-event logging
- slow client page-load events above the current threshold
- install prompt availability and install completion events

When telemetry is expanded further, it should continue to focus on:

- slow browse pages
- slow search responses
- word-detail load latency
- failed favorite/known/difficult state updates
- install prompt display vs install completion

This telemetry should stay proportional and privacy-aware. Avoid collecting unnecessary learner content data.

---

## Incident Triage Checklist

When the web host misbehaves:

1. confirm the deployment commit and configuration
2. confirm database connectivity
3. confirm learner routes vs admin routes separately
4. confirm CSP/security headers are not blocking first-party assets
5. confirm static asset versions match the deployed build
6. confirm service worker cache is not serving stale shell assets
