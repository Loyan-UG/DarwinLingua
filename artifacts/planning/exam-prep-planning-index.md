# Exam Prep Planning Index

## Purpose

This file tracks the planning state before any further Exam Prep content generation. Exam Prep content must not be generated in bulk from infrastructure alone; each level and exam profile needs a reviewed candidate list first.

## Current Planning State

- `artifacts/planning/exam-prep-a1-a2-pilot-candidates.md` exists, but the first generated pilot was rejected and removed on 2026-06-08.
- Reviewed candidate lists now exist for A1/A2, B1, B2, C1, and Goethe C2.
- The next content step is small-batch generation from reviewed candidate lists, not bulk generation. Goethe C2 foundation generation is complete through `exam-prep-c2-foundation-05-v1.json`.

## Supported Profile Taxonomy

Initial production candidates may use these profile keys:

- A1/A2: `goethe-a1`, `goethe-a2`, `telc-a1`, `telc-a2`, `dtz-a2-b1`
- B1/B2: `goethe-b1`, `goethe-b2`, `telc-b1`, `telc-b2`, `berufssprache-b1`, `berufssprache-b2`
- C1 and university/professional: `goethe-c1`, `telc-c1`, `telc-c1-hochschule`, `c1-hochschule`, `testdaf`, `testdaf-b2-c1`, `dsh`, `berufssprache-c1`
- C2: `goethe-c2`

## Required Planning Documents

Create these before generating broad Exam Prep content:

- `exam-prep-a1-a2-title-candidates.md`
- `exam-prep-b1-title-candidates.md`
- `exam-prep-b2-title-candidates.md`
- `exam-prep-c1-title-candidates.md`
- `exam-prep-c2-title-candidates.md` (created; foundation packages 01-05 imported)

Each candidate row should include:

- `Order`
- `Suggested Slug`
- `German Title`
- `Exam Profile`
- `CEFR`
- `Section`
- `Task Type`
- `Skill Focus`
- `Core Learner Task`
- `Linked Practice Targets`
- `Quality Notes`

## Title Rules

- Do not put CEFR level, exam profile, or exam section into the title when those values are already stored in metadata.
- Use titles that read naturally as learning tasks, for example `Einen Termin klaeren`, not `Im A2-Sprechen einen Termin klaeren`.
- Slugs may include level/profile where needed for uniqueness, but learner-facing source titles should stay clean.

## Linked Practice Rules

- Link only to imported and reviewed content.
- Course lessons, Dialogue lessons, Roleplay scenarios, and Exercises currently exist and can be reviewed for links.
- Writing Templates are not imported yet; do not invent writing-template links.
- Empty linked fields require a reason in planning notes.

## Translation Quality Gate

- German source stays canonical.
- Helper translations must be natural in each learner language, not literal glosses.
- Persian and other non-English helper text must not copy English phrasing or awkward source-language word order.
- Review at least one non-English language sample before importing any package.
