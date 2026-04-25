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
- [x] replace temporary CSS fallback with compiled Tailwind output once working Node/npm tooling is available in the active environment

## 2. Frontend System

- [x] define shared design tokens for color, spacing, radius, elevation, and typography
- [x] define learner shell vs admin shell visual separation
- [x] add reusable Razor partials / view components for:
- [x] app header
- [x] mobile nav
- [x] section heading
- [x] metric card
- [x] word card
- [x] CEFR badge
- [x] favorite toggle
- [x] async status / empty state / failure state
- [x] add accessibility review checklist for focus order, landmarks, contrast, and keyboard interaction

## 3. Routing and Host Composition

- [x] add default MVC route
- [x] add area route for `Admin`
- [x] add route naming conventions for learner pages
- [x] add route naming conventions for admin pages
- [x] add cache/output-caching policy per page type
- [x] add security headers and CSP baseline suitable for `htmx`

## 4. Learner-Facing Pages

- [x] landing page
- [x] browse by CEFR page
- [x] browse by topic page
- [x] search page
- [x] word detail page
- [x] favorites page
- [x] recent activity page
- [x] account settings page
- [x] install-app guidance page

## 5. htmx Interaction Slices

- [x] live search results with debounced `htmx` requests
- [x] favorite toggle without full page reload
- [x] recent-activity panel refresh
- [x] CEFR word list pagination / load-more
- [x] topic-filter chips with partial updates
- [x] inline admin table filtering
- [x] publish-history reload panel
- [x] confirmation modal pattern for admin actions

## 6. Authentication and Identity

- [x] add `ASP.NET Core Identity`
- [x] add registration
- [x] add sign-in / sign-out
- [x] add forgot-password / reset-password baseline
- [x] assign `Learner` as the default role for newly registered learner accounts
- [x] add learner role seeding
- [x] add operator/admin role seeding
- [x] add authorization policies for `Operator` and `Admin`
- [x] add account-management pages
- [ ] add seeded non-production system admin account from environment-backed secrets
- [ ] add seeded non-production learner test account from environment-backed secrets
- [x] define a startup failure rule when required seed-account secrets are missing in local/dev bootstrap modes
- [x] add a shared auth contract for future mobile sign-in against the same account system

## 6.1 Cross-Platform Account Foundation

- [x] define the shared identity boundary used by both `DarwinLingua.Web` and `DarwinDeutsch.Maui`
- [x] keep cookie auth for web and add token-based auth planning for mobile against the same identity store
- [x] define the account API surface needed by mobile for register, sign-in, sign-out, refresh, and profile bootstrap
- [ ] define how anonymous learner state upgrades into authenticated learner state without data loss
- [ ] define device/session management expectations for low-friction learner sign-in

## 6.2 Entitlements and Monetization Foundation

- [ ] define the first entitlement model separate from roles
- [ ] define baseline entitlements such as `Free`, `Trial`, and `Premium` without locking final pricing
- [ ] add server-side entitlement checks for learner-facing premium features
- [ ] add web-side feature-gate presentation patterns for locked premium features
- [ ] define trial bootstrap rules for newly registered learners
- [ ] define subscription-state persistence and expiration handling
- [ ] define audit and diagnostics for entitlement changes
- [ ] ensure feature gating never blocks permanently free catalog browsing
- [ ] define the first premium-capable features as backlog placeholders:
- [ ] favorites
- [ ] dual-meaning-language mode
- [ ] future advanced practice capabilities

## 7. Shared Backend Integration

- [x] wire the web host to shared application modules
- [x] define read-side queries for:
- [x] browse by CEFR
- [x] browse by topic
- [x] search
- [x] word detail
- [x] recent activity
- [x] favorite words
- [x] keep MVC rendering on server-side application services, not mobile package endpoints

## 8. User State

- [x] persist favorite words for authenticated web users
- [x] persist last viewed words
- [x] persist recent activity
- [x] persist preferred meaning languages
- [x] persist UI language preference when applicable
- [x] define anonymous-user fallback behavior
- [ ] define authenticated-user upgrade flow for guest favorites/history where migration is worth keeping

## 9. Admin Area

- [x] add `Areas/Admin` shell placeholder
- [x] add admin layout and nav shell
- [x] add content import dashboard
- [x] add draft batch inspection
- [x] add publish action screen
- [x] add rollback action screen
- [x] add publication history and audit view
- [x] add package/manifest diagnostics view

## 10. Performance and UX

- [x] keep the first slice server-rendered and lightweight
- [x] add caching for high-read browse/detail pages without leaking personalized learner state
- [x] add compression and asset caching review
- [x] add image and icon optimization
- [x] add pagination / incremental rendering for large lists
- [x] add UX telemetry for slow page loads and failed async actions
- [x] validate install prompt flow on Android and desktop Chromium on real target browsers/devices

## 11. Validation

- [x] add smoke tests for web host routing and area registration
- [x] add UI structure tests for learner shell
- [x] add UI structure tests for admin shell
- [x] add manual validation worksheet for PWA installability
- [x] add manual validation worksheet for auth flows
- [x] add manual validation worksheet for browse/search/detail flows
- [ ] validate seeded admin login on a clean local environment
- [ ] validate seeded learner test login on a clean local environment
- [ ] validate default `Learner` role assignment after registration
- [ ] validate premium/free feature gating behavior with both learner and admin accounts
- [ ] validate web/mobile auth compatibility against the same server identity boundary

## 12. Release Readiness

- [x] environment-specific configuration for PostgreSQL and auth secrets
- [x] deployment notes
- [x] asset build pipeline notes for Tailwind
- [x] production logging and diagnostics notes
- [x] release checklist for learner web
- [x] release checklist for admin web

---

## Execution Rule

Before each implementation slice:

- move selected items from `[ ]` to `[-]`
- finish the slice end-to-end
- mark completed items as `[x]`
- add newly discovered tasks in the correct section
