# Web Asset Build Pipeline Notes

## Purpose

This document defines the intended asset pipeline for `DarwinLingua.Web`.

At the moment, the repository contains a CSS fallback plus Tailwind-ready config files. The final production direction remains Tailwind-driven once Node tooling is available in the active build environment.

---

## Current State

- `wwwroot/css/site.css` is the live fallback stylesheet
- `package.json` exists
- `tailwind.config.js` exists
- `postcss.config.js` exists
- `Styles/tailwind.css` exists as the future Tailwind input

The current Codex environment does not expose working Node/npm execution, so the Tailwind output is not yet the authoritative production stylesheet.

---

## Intended Final Pipeline

1. install Node/npm in the CI or local build environment
2. restore frontend dependencies with `npm install`
3. compile Tailwind from `Styles/tailwind.css`
4. emit the built stylesheet into `wwwroot/css/`
5. include the generated asset in the publish output
6. keep the fallback stylesheet only until the generated pipeline is proven stable

---

## Guardrails

- do not maintain two diverging design systems
- once Tailwind compilation is active, consolidate tokens and components into the Tailwind source
- keep generated assets out of manual editing workflows
- ensure cache-busting remains enabled through `asp-append-version`

---

## CI Recommendation

The web build job should eventually include:

1. restore .NET dependencies
2. restore Node dependencies
3. build Tailwind assets
4. run solution tests
5. publish the web host

Until Node is available, document that the fallback stylesheet is the deployed source of truth.
