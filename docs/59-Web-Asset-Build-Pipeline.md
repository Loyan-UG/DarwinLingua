# Web Asset Build Pipeline Notes

## Purpose

This document defines the active asset pipeline for `DarwinLingua.Web`.

---

## Current State

- `Styles/tailwind.css` is the source stylesheet
- `wwwroot/css/tailwind.generated.css` is the generated runtime stylesheet
- `package.json` contains the executable Tailwind build scripts
- `tailwind.config.js` and `postcss.config.js` remain part of the build input
- the MVC layouts now reference the generated Tailwind output directly

---

## Active Pipeline

1. install Node/npm in the CI or local build environment
2. restore frontend dependencies with `npm install`
3. compile Tailwind from `Styles/tailwind.css`
4. emit the built stylesheet into `wwwroot/css/`
5. include the generated asset in the publish output
6. serve the generated asset from the MVC layouts and service worker shell cache

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

The generated stylesheet should be rebuilt whenever `Styles/tailwind.css` changes.
