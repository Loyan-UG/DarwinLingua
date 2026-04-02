# Web Platform Architecture

## Purpose

This document defines the recommended architecture for the future ASP.NET Core web platform of Darwin Lingua.

It covers:

- the web delivery model
- the MVC and Area structure
- installable web-app direction
- authentication and user-account boundaries
- shared-backend reuse rules
- the initial web backlog slices

This document is planning guidance only.

No web implementation has started yet.

---

## 1. Goals

The web platform should:

- reuse the same shared content backend as the mobile apps
- provide a learner-facing website at the root of the site
- provide a separate `Admin` area for content and operational work
- remain compatible with the existing Web API and PostgreSQL direction
- support installability on desktop and mobile like an app where supported
- support authenticated user state such as favorites and recent activity
- keep content ownership and user-state ownership clearly separated

---

## 2. Recommended Technology Stack

The recommended stack is:

- `ASP.NET Core MVC`
- `Razor Views`
- `Bootstrap 5`
- `ASP.NET Core Identity`
- `Entity Framework Core`
- `PostgreSQL`
- `PWA manifest + service worker`

### Why This Is the Right Choice

This stack is the best fit because it:

- matches the current `.NET` platform and team direction
- works well with server-rendered content-heavy pages
- keeps complexity lower than introducing a JavaScript SPA too early
- supports clean Area separation for admin workflows
- supports cookie-based authentication and user accounts cleanly
- provides a practical path to installable web-app behavior without changing the whole frontend model

---

## 3. Delivery Hosts

The future web expansion should introduce these hosts:

- `DarwinLingua.Web`
- `DarwinLingua.Admin` is not a separate host in the first web slice

Instead, the first recommended shape is:

- one host: `DarwinLingua.Web`
- one MVC root site for learner-facing pages
- one MVC `Areas/Admin` section for administrative workflows

This is the simplest correct starting point.

If the admin experience later grows into a clearly separate product, it can split into its own host.

---

## 4. Site Shape

## 4.1 Root Site

The root MVC site should provide learner-facing experiences such as:

- landing page
- browse by CEFR
- browse by topic
- search words
- word detail pages
- login and registration
- favorites
- recent activity
- account settings
- install-app guidance

The root site should feel like the web equivalent of the mobile learner app, not like an admin console.

---

## 4.2 Admin Area

The same MVC host should contain:

- `Areas/Admin`

The `Admin` area should contain:

- content import screens
- draft batch inspection
- publish / rollback actions
- package history and audit views
- operator diagnostics
- later content moderation and review tools

The `Admin` area must:

- require stronger authorization than the learner-facing root site
- have a distinct navigation shell
- avoid leaking administrative concepts into public learner pages

---

## 5. Installable Web-App Direction

The website should be installable where browser/platform support allows it.

The recommended direction is:

- `manifest.webmanifest`
- service worker
- icons and theme metadata
- offline shell support for safe, limited scenarios

This means the web project should behave as a `PWA-capable MVC application`.

### Important Boundary

The website should not try to become a full offline clone of the MAUI app in the first slice.

Recommended first web behavior:

- installable shell
- cached static assets
- cached app chrome
- graceful offline messaging
- optional read-only caching for a few high-value learner screens later

Not recommended in the first slice:

- full client-side offline data sync
- browser-first package application like mobile SQLite
- duplicating the mobile local-first storage model inside the browser

---

## 6. Shared Backend Relationship

The web platform must reuse the same backend direction already established for mobile.

The shared backend remains:

- `PostgreSQL` as the server source of truth
- `DarwinLingua.WebApi` for mobile package/manifest distribution
- shared catalog and publishing model

The MVC website should access server data through application services and persistence owned by the server-side solution.

It should not call mobile package endpoints for normal page rendering.

### Rule

The Web API remains the mobile-distribution edge.

The MVC web site is a first-class server-rendered product host.

These two hosts may reuse the same backend modules and database, but they are different delivery surfaces.

---

## 7. Authentication and User Accounts

The recommended first implementation is:

- `ASP.NET Core Identity`
- cookie authentication
- email/password registration and sign-in

This is the simplest correct baseline.

Future federation can be added later if needed.

### Initial User Capabilities

Authenticated web users should later be able to persist:

- favorite words
- last viewed words
- recent activity
- selected meaning languages
- selected UI language where relevant

### Boundary Rule

Content state and user state must remain separate.

Examples:

- lexical entries, topics, and published packages belong to shared content
- favorites, activity, and learner preferences belong to user state

This is the same separation already used in the MAUI architecture and should remain consistent on the web.

---

## 8. Data and Module Boundaries

The future server-side web platform should keep these responsibilities:

- `Catalog`: shared lexical content
- `ContentOps`: import and package workflows
- `Learning`: user-specific favorites and state
- `Localization`: UI-language rules where shared
- `Practice`: later learner practice workflows
- `Publishing` / `Distribution`: server-side published-content lifecycle

The web host should compose these modules, not bypass them.

---

## 9. Recommended Project Structure

The recommended first structure is:

```text
src/
  Apps/
    DarwinLingua.Web/
      Areas/
        Admin/
      Controllers/
      Views/
      wwwroot/
      Pwa/
```

Key host responsibilities:

- MVC routing
- layout and Bootstrap-based UI composition
- Identity wiring
- cookie/auth policies
- PWA manifest and service-worker registration
- server-rendered page orchestration

Not host responsibilities:

- raw SQL business logic
- direct content import rules
- bypassing application services

---

## 10. UI and Frontend Direction

The first web UI should use:

- `Bootstrap 5`
- server-rendered Razor views
- limited progressive enhancement with small JavaScript helpers only where needed

This gives:

- faster delivery
- lower complexity
- better consistency with admin workflows
- easier SEO and accessibility for content pages

### Design Rule

Bootstrap should be a baseline, not the whole design language.

The site should still have:

- custom tokens
- product-specific branding
- consistent responsive layouts
- clear admin vs learner visual separation

---

## 11. PWA and Responsive Rules

The site must be designed mobile-first even though it is MVC.

Required baseline:

- responsive layouts
- touch-friendly controls
- installable manifest
- service worker for static assets
- tested install prompts on Android and desktop Chromium

Later optional enhancements:

- read-only cached browse pages
- lightweight sync reminders
- account-aware install onboarding

---

## 12. Authorization Model

Recommended first authorization split:

- anonymous users can browse basic public pages if product policy allows
- authenticated learner users can use favorites, history, and personalized state
- admin users can access `Areas/Admin`

Recommended roles:

- `Learner`
- `Operator`
- `Admin`

This is enough for the first implementation.

---

## 13. Initial Web Backlog Slices

The first recommended web implementation order is:

1. create `DarwinLingua.Web` MVC host
2. add Bootstrap-based shared layout and root navigation
3. add PWA manifest, icons, and service-worker baseline
4. add ASP.NET Core Identity with registration and sign-in
5. add root learner pages for browse/search/detail
6. add authenticated favorites and recent activity
7. add `Areas/Admin` shell and authorization boundary
8. add admin content batch inspection and publish-history views

This order keeps the architecture clean and avoids starting with admin-only workflows.

---

## 14. Non-Goals for the First Web Slice

The first web slice should not try to deliver:

- a JavaScript SPA rewrite
- browser-side full offline database sync
- payment/subscription systems
- advanced moderation workflows
- multi-tenant product separation
- real-time collaboration

Those can come later if the product proves the need.

---

## 15. Final Recommendation

The correct next-step web architecture for Darwin Lingua is:

- one ASP.NET Core MVC host
- Bootstrap-based responsive UI
- root learner-facing site plus `Areas/Admin`
- installable PWA baseline
- ASP.NET Core Identity for user accounts
- PostgreSQL shared backend reused from the current server direction
- strict separation between shared content and user state

This is the lowest-complexity architecture that still keeps a clean path for:

- web learners
- administrative workflows
- mobile/web backend reuse
- future platform growth
