# Web PWA Install Validation Worksheet

## Purpose

This worksheet validates the installability and app-like behavior of the learner web host.

Use it on Android Chrome and desktop Chromium-class browsers after the learner shell is already validated.

---

## Build Under Test

- Build commit:
- Validation date:
- Validator:
- Browser:
- OS:
- Deployment URL:

---

## Preconditions

- [ ] site is reachable over HTTPS
- [ ] manifest loads successfully
- [ ] service worker registers successfully
- [ ] icons are available
- [ ] browser supports install prompts for the tested platform

---

## A. Install Prompt

- [ ] load the learner home page
- [ ] verify the install banner appears when supported
- [ ] click `Install`
- [ ] verify the browser install prompt appears
- [ ] complete installation
- Result:
- Notes:

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

- [ ] load the app once while online
- [ ] disable network access
- [ ] relaunch the installed app
- [ ] verify the shell still loads
- [ ] verify previously cached assets do not break navigation
- Result:
- Notes:

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
- Follow-up tasks filed:
- Final PWA-install readiness recommendation:
