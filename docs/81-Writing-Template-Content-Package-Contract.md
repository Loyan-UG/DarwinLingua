# Writing Template Content Package Contract

## Package Target Language

Every import package must declare package-level `targetLearningLanguageCode`. Current official German-learning packages use `"de"`.

`targetLearningLanguageCode` is the language being taught. It is separate from `defaultMeaningLanguages` and from all `...Translations` fields, which remain helper/meaning languages for learner support.

Levelled packages must declare `levelSystemCode`; current German packages use CEFR (`"cefr"`). Import validation rejects missing `levelSystemCode`, unsupported level systems, and missing or inactive target-learning languages before content is persisted.

## Purpose

Writing Templates teach learners how to produce practical German messages and exam-ready short texts. The German source is canonical. Helper translations explain the situation, structure, template, and sample in the learner's selected meaning language; they never replace the German template text as the target output.

## Root Array

- `writingTemplates`

## Required Source Fields

- `slug`
- `title`
- `shortDescription`
- `cefrLevel`
- `category`
- `situation`
- `register`
- `templateText`
- `explanation`
- `replaceableVariables`
- `sampleFilledVersion`
- `sortOrder`

All source fields above must be German-first, except controlled metadata values and slugs.

## Required Helper Translation Fields

Each template must include complete helper translations for active learner languages: `en`, `fa`, `ar`, `tr`, `ru`, `ckb`, `kmr`, `pl`, `ro`, `sq`.

- `titleTranslations`
- `shortDescriptionTranslations`
- `situationTranslations`
- `explanationTranslations`
- `templateTextTranslations`
- `sampleFilledVersionTranslations`

Translation rows use this shape:

```json
{ "language": "fa", "text": "..." }
```

## Optional Linked Content Fields

- `linkedGrammarTopicSlugs`
- `linkedWordSlugs`
- `linkedExpressionSlugs`
- `linkedExerciseSlugs`
- `linkedCourseLessonSlugs`

## Controlled Categories

- `email-to-school`
- `email-to-kindergarten`
- `message-to-landlord`
- `doctor-appointment-request`
- `appointment-reschedule`
- `sick-note-to-employer`
- `complaint`
- `application-email`
- `cancellation`
- `insurance-message`
- `government-office-message`
- `exam-email`
- `exam-opinion-text`

## Controlled Registers

- `formal`
- `informal`
- `neutral`
- `official`
- `workplace`
- `exam`

## Validation Rules

- slugs and linked slugs must be lowercase kebab-case.
- CEFR levels must be valid.
- category and register must use controlled values.
- every declared variable must appear in `templateText` as `{{variable-name}}`.
- every placeholder used in `templateText` must be declared in `replaceableVariables`.
- `sampleFilledVersion` must be concrete and must not be empty.
- translation languages must be active learner meaning languages.
- duplicate translation language rows are rejected.
- every required translation field must cover all active learner meaning languages.
- non-English translations must not reuse English helper text.

## Quality Rules

- Titles must not repeat metadata such as CEFR, provider, section, or exam type when those are already stored in metadata.
- Helper translations must be semantic and natural. Avoid word-by-word renderings when they obscure the learner-facing meaning.
- Keep A1/A2 templates short, usable, and explicit. Higher levels may use more complex structure, but should still remain a reusable template rather than a full lesson.
- Linked practice may be empty only when no genuinely related content exists; otherwise link to existing Course, Grammar, Expression, or Exercise slugs.
- Do not duplicate full grammar explanations, vocabulary entries, expression articles, or exercise answer keys inside a template.

Bulk generation must not start until this contract, validation, Web API, Web rendering, admin inspection, and a small validated pilot are stable.