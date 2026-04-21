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

## UX Telemetry Direction

When explicit telemetry is introduced, it should focus on:

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
