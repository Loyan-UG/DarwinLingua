# English A1 Foundation Expansion 02 Plan

Status: completed 2026-06-24.

## Purpose

Expand the current controlled English pilot from a small platform proof into a
slightly broader A1 foundation slice without making English public active.

This is still a pilot. It must remain small, native-English, target-scoped, and
reviewable. It must not become broad English generation and must not activate
`/learn/en/...` for public learners.

## Current State

Current package:

- `content/learning-portal/english/pilot/packages/english-a1-platform-pilot-01-v1.json`

Imported English pilot counts before Batch 02:

| Module | Count |
| --- | ---: |
| Course paths | 1 |
| Course modules | 1 |
| Course lessons | 5 |
| Grammar topics | 3 |
| Expressions | 10 |
| Exercises | 4 |
| Exercise sets | 1 |
| Writing templates | 2 |

English remains `pilot/content-importable`, not public active.

Completion evidence:

- `artifacts/validation/english-a1-expansion-02-import-20260624.md`
- `artifacts/validation/multi-target-phase9-closure-20260624.md`
- `X:\Projects\DarwinLingua.Backup\20260624-211627-english-a1-expansion-02-branch-checkpoint-pre-activation`

## Batch 02 Scope

The package remains cumulative and keeps the same package file. Batch 02 adds
the remaining first-contact A1 candidates:

Course lessons:

1. `en-a1-people-and-pronouns`
2. `en-a1-polite-you-and-basic-requests`
3. `en-a1-regular-verbs-in-daily-life`
4. `en-a1-build-a-short-introduction`
5. `en-a1-first-contact-review`

Grammar topics:

1. `en-a1-simple-questions-with-be`
2. `en-a1-basic-word-order`

Expressions:

1. `sorry`
2. `excuse-me`
3. `can-you-repeat-that`
4. `i-do-not-understand`
5. `can-you-help-me`
6. `where-is`
7. `how-much-is-it`
8. `see-you-later`
9. `have-a-nice-day`
10. `one-moment-please`

Exercises:

1. `en-a1-reorder-a-simple-question`
2. `en-a1-identify-singular-and-plural-nouns`
3. `en-a1-complete-a-short-introduction`
4. `en-a1-review-first-contact-phrases`

Writing templates:

1. `en-a1-simple-appointment-request`
2. `en-a1-short-apology-message`
3. `en-a1-simple-thank-you-message`

Expected cumulative counts after import:

| Module | Count |
| --- | ---: |
| Course paths | 1 |
| Course modules | 1 |
| Course lessons | 10 |
| Grammar topics | 5 |
| Expressions | 20 |
| Exercises | 8 |
| Exercise sets | 1 |
| Writing templates | 5 |

## Quality Rules

- English source content must be native English-learning content.
- Do not translate German source packages into English.
- Do not activate English publicly.
- Do not add Country Guidance yet.
- Keep A1 language short, practical, and inspectable.
- Helper translations must be semantic and learner-facing, not English fallback.
- Persian helper text must explain the learning action naturally, not word by
  word.
- Course activity links may target only English slugs in the cumulative package.
- Writing-template variables must match placeholders exactly.
- Exercise answer keys must stay deterministic and not expose answers before
  evaluation.

## Verification Plan

Preflight:

- JSON parse.
- `targetLearningLanguageCode = en`.
- Expected cumulative counts are present.
- All helper translation arrays cover `en, fa, ar, tr, ru, ckb, kmr, pl, ro,
  sq`.
- Activity target slugs exist inside the cumulative package.
- Writing-template variables match placeholders.

Validation/import:

- Run targeted ContentOps import validation.
- Import the cumulative package to `darwinlingua_shared`.
- Verify English counts and German counts remain present.

Smoke:

- Public `/learn/en/...` remains inactive.
- Admin preview can inspect at least one new English lesson with Persian helper
  text.
- Admin English search returns English target results only.
- Admin report for `targetLearningLanguageCode=en` has zero missing
  translations and zero unresolved links.

Backup:

- The package became a branch checkpoint.
- Restore-ready backup:
  `X:\Projects\DarwinLingua.Backup\20260624-211627-english-a1-expansion-02-branch-checkpoint-pre-activation`

## Non-Goals

- No public English activation.
- No English Country Guidance.
- No broad English generation beyond this small A1 foundation slice.
- No Spanish/French content.
- No MAUI/mobile work.
