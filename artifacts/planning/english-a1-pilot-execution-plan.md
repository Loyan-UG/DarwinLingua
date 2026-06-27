# English A1 Pilot Execution Plan

Status: Package 01 generated and imported as pilot/staging data; English is not public active.

Created: 2026-06-24.

Last updated: 2026-06-24.

## Purpose

This plan executes the first non-German target-language pilot without making English public-active. The pilot must prove that Darwin Lingua can store, import, search, diagnose, and later expose native English learning content without mixing it with the German baseline.

## Scope For Package 01

Target learning language:

- `en`

Level system:

- `cefr`

Public activation:

- English remains pilot/content-importable only.
- Public learner routes must still reject or hide English until a separate activation decision is made.

Package location:

- `content/learning-portal/english/pilot/packages/english-a1-platform-pilot-01-v1.json`

Package 01 content:

- Course: one A1 path, one module, five lessons.
- Grammar: three A1 topics.
- Expressions: ten A1 expressions.
- Exercises: four deterministic A1 exercises and one exercise set.
- Writing Templates: two A1 templates.

Import status:

- Package file generated at the planned location.
- Imported into `darwinlingua_shared` with zero warnings.
- Verification evidence: `artifacts/validation/english-a1-pilot-import-20260624.md`.
- English remains pilot/content-importable only; public learner routes still reject `/learn/en/...`.

Country Guidance:

- Not included in Package 01.
- `en|US` remains the first recommended country stream, but it starts only after the English learning-content pilot proves routing, import, search, helper projection, progress isolation, and admin diagnostics.

## Source Quality Rules

- English source content must be authored natively for learners of English.
- Do not translate German source packages into English source content.
- Use English-specific beginner issues: `be`, `have`, subject pronouns, `a/an`, plural nouns, simple questions, spelling, and everyday formulas.
- Keep A1 language short and practical.
- Helper translations explain the English source; they do not replace it.
- Persian and other helper translations must be semantic and natural, not word-by-word copies of English.

## Linking Rules

Package 01 may link only to English slugs included in the same package.

Allowed internal links:

- Course activity blocks can target grammar topics, exercises, the exercise set, and writing templates from this package.
- Writing templates can link to package grammar topics and exercises.
- Exercise owner slugs can point to package grammar, expression, course lesson, or writing template slugs.

Not allowed:

- Links to German content.
- Links to future English content not included in the package.
- Country Guidance links before the `en|US` pilot exists.

## Validation And Import Gates

Before import:

- JSON parse succeeds.
- `targetLearningLanguageCode = "en"`.
- `levelSystemCode = "cefr"`.
- All helper translation fields required by each module cover active helper languages.
- Activity blocks use only valid `kind`, `targetType`, `targetSlug`, and `sortOrder`.
- Exercise answer keys are deterministic and do not expose answers before evaluation.
- Writing template variables match placeholders in both directions.

After import:

- PostgreSQL has English counts for the imported modules.
- German counts remain unchanged.
- Unified Search can query English pilot content internally without returning German results for the English target.
- Admin reports show English pilot counts separately from German.
- Public learner routes remain non-active for English until activation.

Current post-import counts:

| Module | English count |
| --- | ---: |
| Course paths | 1 |
| Course modules | 1 |
| Course lessons | 5 |
| Grammar topics | 3 |
| Expressions | 10 |
| Exercises | 4 |
| Exercise sets | 1 |
| Writing templates | 2 |

Remaining before activation:

- internal/admin detail projection smoke for English pilot content
- target-isolated Unified Search smoke for the English pilot without public activation
- progress/recommendation isolation smoke with English pilot data
- admin report diagnostic smoke for `targetLearningLanguageCode=en`
- restore-ready backup after the activation gate is reviewed

## Package 01 Candidate List

Course lessons:

1. `en-a1-say-hello-and-give-your-name`
2. `en-a1-use-i-am-and-i-have`
3. `en-a1-ask-simple-questions`
4. `en-a1-numbers-times-and-short-details`
5. `en-a1-name-everyday-objects`

Grammar topics:

1. `en-a1-subject-pronouns-and-be`
2. `en-a1-have-and-simple-possession`
3. `en-a1-a-an-and-plural-nouns`

Expressions:

1. `hello`
2. `good-morning`
3. `nice-to-meet-you`
4. `my-name-is`
5. `i-am-from`
6. `i-live-in`
7. `how-are-you`
8. `i-am-fine`
9. `please`
10. `thank-you`

Exercises:

1. choose the correct greeting
2. complete `I am` sentences
3. match names and countries
4. choose `a` or `an`

Writing templates:

1. `en-a1-short-self-introduction-message`
2. `en-a1-simple-class-question`

## Non-Goals

- Do not generate broad English content.
- Do not activate English publicly.
- Do not create `en|US` Country Guidance yet.
- Do not edit MAUI/mobile.
- Do not reuse German slugs as if they belonged to English unless the slug is intentionally target-scoped and semantically correct for English.
