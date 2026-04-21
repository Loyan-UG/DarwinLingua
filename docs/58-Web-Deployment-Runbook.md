# Web Deployment Runbook

## Purpose

This runbook describes the minimum deployment shape for `DarwinLingua.Web`.

It assumes one ASP.NET Core MVC host, one PostgreSQL database for shared catalog data, and a reverse proxy or platform edge terminating HTTPS.

---

## Recommended Hosting Shape

- runtime: `ASP.NET Core`
- app type: server-rendered MVC host with progressive enhancement via `htmx`
- database: `PostgreSQL` for shared catalog/admin data
- HTTPS: required
- static assets: served by the ASP.NET Core host or fronted by the platform edge

---

## Required Configuration

Set these values per environment:

- `ConnectionStrings__SharedCatalog`
- `ConnectionStrings__SharedCatalogAdmin` when admin and learner read paths intentionally diverge
- `ASPNETCORE_ENVIRONMENT`
- logging overrides as needed for production

When Identity is introduced, add the required identity/auth-secret configuration in the same environment-specific system instead of hard-coding it in repository files.

---

## Deployment Steps

1. Build the solution on the intended release commit.
2. Run `dotnet test DarwinLingua.slnx -c Debug -m:1` or the release-appropriate equivalent in CI.
3. Publish `src/Apps/DarwinLingua.Web`.
4. Provide production connection strings through the deployment platform.
5. Start the host and verify it completes startup initialization.
6. Verify learner routes and admin routes respond successfully.
7. Run the learner-web and PWA validation worksheets.

---

## Post-Deploy Checks

- verify `Home`, `Browse`, `Search`, `Favorites`, `Recent`, and `Settings`
- verify admin routes render with the admin shell
- verify recent activity and word-state interactions persist
- verify CSP/security headers are present
- verify service worker registration and manifest delivery

---

## Rollback Rule

Rollback the web deployment when:

- the host fails startup
- browse/search/detail flows return errors
- admin shell routes are broken
- CSP/security headers block first-party functionality unexpectedly
- the deployed build serves stale or invalid content against the intended shared database
