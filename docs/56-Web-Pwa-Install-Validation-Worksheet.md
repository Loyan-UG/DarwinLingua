# Web PWA Install Validation Worksheet

## Purpose

This worksheet validates the installability and app-like behavior of the learner web host.

Use it on Android Chrome and desktop Chromium-class browsers after the learner shell is already validated.

Latest local validation evidence can be captured in:

- `artifacts/installability-report.json` for desktop Chromium and mobile-form-factor Chromium inspection
- `artifacts/installability-report-android.json` for Android Chrome via emulator + remote DevTools

---

## Build Under Test

- Build commit: current working tree after Web readiness/account/legal/email/PWA fixes
- Validation date: 2026-06-19
- Validator: local automated smoke
- Browser: Google Chrome desktop, headless Chromium via `puppeteer-core`
- OS: Windows local development machine
- Deployment URL: `https://localhost:7501`

Latest local evidence:

- `DarwinLingua.Web` build passed after the Web readiness changes.
- `tools/Web/New-WebPwaInstallabilityReport.ps1` regenerated `artifacts/installability-report.json` with `17` passed automated checks, `0` failed checks, and `2` manual checks.
- Local HTTPS smoke returned HTTP 200 for `/`, `/manifest.webmanifest`, `/sw.js`, `/offline.html`, and `/images/logo.png`.
- Desktop Chromium loaded the home page, parsed the manifest, registered the service worker at `https://localhost:7501/`, created cache `darwin-lingua-shell-v3`, and cached `/offline.html`.
- With the Web host intentionally stopped after online load, desktop Chromium navigated to `/offline-smoke-check` and rendered cached `Offline - Darwin Lingua`.
- 2026-06-19 follow-up fixed a homepage missing collection image (`/images/collections/basic-communication-daily-life.svg`) and disabled htmx inline indicator style injection through `htmx-config`; the refreshed PWA report no longer shows CSP inline-style errors or 404 asset errors.

---

## Preconditions

- [x] site is reachable over HTTPS
- [x] manifest loads successfully
- [x] service worker registers successfully
- [x] icons are available
- [x] browser supports install prompts for the tested platform
  - Evidence: the automated report observed the `beforeinstallprompt` event in desktop Chrome; real prompt acceptance still requires manual browser interaction.

---

## A. Install Prompt

- [x] load the learner home page
- [x] verify the install banner appears when supported
- [ ] click `Install`
- [ ] verify the browser install prompt appears
- [ ] complete installation
- Result: Automated prompt availability passed; manual prompt acceptance remains open.
- Notes: Headless Chromium cannot accept the real browser install prompt. Validate on desktop Chrome/Edge with a visible browser session.

---

## B. Installed Experience

- [ ] launch the installed app from home screen / app launcher
- [ ] verify standalone window/chrome behavior
- [ ] verify `Home`, `Browse`, `Search`, `Favorites`, `Recent`, and `Settings` open correctly
- [ ] verify the admin area remains reachable but visually separate
- Result:
- Notes:

---

## C. Offline And Cache Behavior

- [x] load the app once while online
- [x] disable network access
- [ ] relaunch the installed app
- [x] verify the shell still loads
- [x] verify previously cached assets do not break navigation
- Result: Passed for service-worker shell fallback in desktop Chromium.
- Notes: The local smoke stopped the Web host instead of completing a real installed-app relaunch. Install prompt acceptance and installed-window behavior remain part of sections A and B.

---

## D. Update Behavior

- [ ] deploy a fresh build or invalidate static assets
- [ ] refresh the browser session
- [ ] verify updated assets are served
- [ ] verify no stale install prompt or shell regression appears
- Result:
- Notes:

---

## E. Sign-Off

- Known issues accepted:
- Follow-up tasks filed: manual target-browser install acceptance on Desktop Chrome/Edge and Android Chrome.
- Final PWA-install readiness recommendation: automated PWA shell readiness is green; do not block Web tester onboarding on headless prompt acceptance, but complete the two manual install checks before treating PWA as production signed off.
