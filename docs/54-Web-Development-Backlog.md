# Web Development Backlog

## Purpose

This backlog is the execution checklist for `DarwinLingua.Web`.

The approved web stack is:

- `ASP.NET Core MVC`
- `Razor Views`
- `Tailwind CSS`
- `htmx`
- `ASP.NET Core Identity`
- `Entity Framework Core`
- `PostgreSQL`
- `PWA manifest + service worker`

This backlog is intentionally ordered so the project can grow without collapsing into a SPA rewrite or an admin-first design.

---

## 1. Foundation

- [x] create `DarwinLingua.Web` MVC host
- [x] add the web project to the solution structure
- [x] add root layout, navigation shell, and learner-first landing page
- [x] add `Areas/Admin` baseline route and operator dashboard placeholder
- [x] add PWA manifest, service worker, and install prompt baseline
- [x] add `htmx` baseline script integration
- [x] add Tailwind-ready project structure and config placeholders
- [ ] replace temporary CSS fallback with compiled Tailwind output once Node tooling is available in the active environment

## 2. Frontend System

- [x] define shared design tokens for color, spacing, radius, elevation, and typography
- [x] define learner shell vs admin shell visual separation
- [ ] add reusable Razor partials / view components for:
- [ ] app header
- [ ] mobile nav
- [ ] section heading
- [ ] metric card
- [ ] word card
- [ ] CEFR badge
- [ ] favorite toggle
- [ ] async status / empty state / failure state
- [ ] add accessibility review checklist for focus order, landmarks, contrast, and keyboard interaction

## 3. Routing and Host Composition

- [x] add default MVC route
- [x] add area route for `Admin`
- [ ] add route naming conventions for learner pages
- [ ] add route naming conventions for admin pages
- [x] add cache/output-caching policy per page type
- [x] add security headers and CSP baseline suitable for `htmx`

## 4. Learner-Facing Pages

- [x] landing page
- [x] browse by CEFR page
- [x] browse by topic page
- [x] search page
- [x] word detail page
- [x] favorites page
- [ ] recent activity page
- [x] account settings page
- [x] install-app guidance page

## 5. htmx Interaction Slices

- [x] live search results with debounced `htmx` requests
- [ ] favorite toggle without full page reload
- [ ] recent-activity panel refresh
- [x] CEFR word list pagination / load-more
- [ ] topic-filter chips with partial updates
- [ ] inline admin table filtering
- [ ] publish-history reload panel
- [ ] confirmation modal pattern for admin actions

## 6. Authentication and Identity

- [ ] add `ASP.NET Core Identity`
- [ ] add registration
- [ ] add sign-in / sign-out
- [ ] add forgot-password / reset-password baseline
- [ ] add learner role seeding
- [ ] add operator/admin role seeding
- [ ] add authorization policies for `Operator` and `Admin`
- [ ] add account-management pages

## 7. Shared Backend Integration

- [x] wire the web host to shared application modules
- [x] define read-side queries for:
- [x] browse by CEFR
- [x] browse by topic
- [x] search
- [x] word detail
- [ ] recent activity
- [x] favorite words
- [ ] keep MVC rendering on server-side application services, not mobile package endpoints

## 8. User State

- [ ] persist favorite words for authenticated web users
- [ ] persist last viewed words
- [ ] persist recent activity
- [x] persist preferred meaning languages
- [x] persist UI language preference when applicable
- [x] define anonymous-user fallback behavior

## 9. Admin Area

- [x] add `Areas/Admin` shell placeholder
- [ ] add admin layout and nav shell
- [ ] add content import dashboard
- [ ] add draft batch inspection
- [ ] add publish action screen
- [ ] add rollback action screen
- [ ] add publication history and audit view
- [ ] add package/manifest diagnostics view

## 10. Performance and UX

- [x] keep the first slice server-rendered and lightweight
- [ ] add output caching for high-read browse/detail pages
- [ ] add compression and asset caching review
- [ ] add image and icon optimization
- [x] add pagination / incremental rendering for large lists
- [ ] add UX telemetry for slow page loads and failed async actions
- [ ] validate install prompt flow on Android and desktop Chromium

## 11. Validation

- [x] add smoke tests for web host routing and area registration
- [ ] add UI structure tests for learner shell
- [ ] add UI structure tests for admin shell
- [ ] add manual validation worksheet for PWA installability
- [ ] add manual validation worksheet for auth flows
- [ ] add manual validation worksheet for browse/search/detail flows

## 12. Release Readiness

- [ ] environment-specific configuration for PostgreSQL and auth secrets
- [ ] deployment notes
- [ ] asset build pipeline notes for Tailwind
- [ ] production logging and diagnostics notes
- [ ] release checklist for learner web
- [ ] release checklist for admin web

---

## Execution Rule

Before each implementation slice:

- move selected items from `[ ]` to `[-]`
- finish the slice end-to-end
- mark completed items as `[x]`
- add newly discovered tasks in the correct section
